using System;
using System.Collections.Generic;
using NLog;
using RevolveUavcan.Telemetry;

namespace RevolveUavcan.Uavcan
{
    public class FrameStorage
    {

        private readonly object _lock = new object();
        private readonly ILogger _logger;

        public event EventHandler<UavcanFrame> UavcanPacketReceived;

        /// <summary>
        /// Contains all transfer IDs (0 - 31)
        /// </summary>
        private readonly Dictionary<byte, Dictionary<uint, UavcanFrame>> _transferIdBuffer;

        public FrameStorage(ILogger logger)
        {
            _logger = logger;

            _transferIdBuffer = new Dictionary<byte, Dictionary<uint, UavcanFrame>>();

            // Populate dictionary
            for (byte i = 0; i <= 31; i++)
            {
                _transferIdBuffer.Add(i, new Dictionary<uint, UavcanFrame>());
            }
        }


        /// <summary>
        /// Subscribes FrameStorage to UavcanFrame event sent from modules UDP, PCAN, KCAN, etc...)
        /// </summary>
        /// <param name="module"></param>
        public void RegisterOnDataEvent(ITelemetryModule module) => module.UavcanFrameReceived += StoreFrame;

        /// <summary>
        /// Stores a frame when a UavcanFrame is received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="frame">An instance of the UavcanFrame class</param>
        private void StoreFrame(object sender, UavcanFrame frame)
        {
            lock (_lock)
            {
                AddFrameToDictionary(frame);

                // This will only run after evaluating that we have either a frame type of
                // SingleFrame or MultiFrameEnd
                if (!_transferIdBuffer.TryGetValue(frame.TransferId, out var subjectIdDictionary))
                {
                    // If we dont have a frame in the storage with the transation ID of the frame,
                    // something must have gone wrong during storage. Cannot pass it on then.
                    _logger
                        .Warn(
                            $"Error occured in frame storage. No matching frame for Subject ID {frame.SubjectId} was found.");
                    return;
                }

                if (subjectIdDictionary.TryGetValue(frame.SubjectId, out var frameFromSubjectId))
                {
                    if (!frameFromSubjectId.IsCompleted)
                    {
                        return;
                    }

                    UavcanPacketReceived?.Invoke(this, frameFromSubjectId);
                    subjectIdDictionary.Remove(frameFromSubjectId.SubjectId);
                }
                else
                {
                    // If we dont have a frame in the storage with the Subject ID of the frame,
                    // something must have gone wrong during storage. Cannot pass it on then.
                    _logger
                        .Warn(
                            $"Error occured in frame storage. No matching frame for Subject ID {frame.SubjectId} was found.");
                }
            }
        }

        /// <summary>
        /// Adds or removes from dictionaries based on the type of the frame
        /// </summary>
        /// <param name="frame">An instance of the UavcanFrame class</param>
        private void AddFrameToDictionary(UavcanFrame frame)
        {
            // See documentation, table 4.4: https://uavcan.org/specification/UAVCAN_Specification_v1.0-beta.pdf
            // to better understand handling of frame type.
            switch (frame.Type)
            {
                // If the subject ID dictionary already exists, it was not completed.
                // Thus, we have lost a frame. Restart with the received frame
                case UavcanFrame.FrameType.SingleFrame:
                case UavcanFrame.FrameType.MultiFrameStart:
                    {
                        Dictionary<uint, UavcanFrame> subjectIdDictionary = _transferIdBuffer[frame.TransferId];

                        if (subjectIdDictionary.ContainsKey(frame.SubjectId))
                        {
                            subjectIdDictionary.Remove(frame.SubjectId);
                        }

                        subjectIdDictionary.Add(frame.SubjectId, frame);

                        break;
                    }

                // If we're in the middle of a MultiFrame, we need to validate the toggle bit before appending ro removing the frame
                case UavcanFrame.FrameType.MultiFrameMiddle:
                    {
                        var subjectIdDictionary = _transferIdBuffer[frame.TransferId];

                        if (!subjectIdDictionary.TryGetValue(frame.SubjectId, out var frameFromSubjectId))
                        {
                            _logger
                                .Warn($"No startframe found for subject ID {frame.SubjectId}. Frame is discarded");
                            subjectIdDictionary.Remove(frame.SubjectId);
                            return;
                        }

                        if (ToggleBitIsValid(frame, frameFromSubjectId))
                        {
                            frameFromSubjectId?.AppendFrame(frame);
                        }
                        else
                        {
                            subjectIdDictionary.Remove(frame.SubjectId);
                        }

                        break;
                    }

                // If we have a MultiFrame end, we validate the toggle bit to decide to append / remove the frame.
                case UavcanFrame.FrameType.MultiFrameEnd:
                    {
                        // Transfer ID does not exist in the corresponding buffer
                        if (!_transferIdBuffer.TryGetValue(frame.TransferId, out var subjectIdDictionary))
                        {
                            return;
                        }

                        // Subject ID does not exist in the corresponding buffer
                        if (!subjectIdDictionary.TryGetValue(frame.SubjectId, out var frameFromSubjectId))
                        {
                            return;
                        }

                        // If we have a valid toggle bit, the frame data is appended
                        if (ToggleBitIsValid(frame, frameFromSubjectId))
                        {
                            frameFromSubjectId.AppendFrame(frame);
                        }
                        // Else, we remove the UavcanFrame
                        else
                        {
                            subjectIdDictionary.Remove(frame.SubjectId);
                        }

                        break;
                    }
            }
        }

        private bool ToggleBitIsValid(UavcanFrame frame, UavcanFrame frameFromSubjectId) =>
            frameFromSubjectId.ToggleBit != frame.ToggleBit;
    }
}
