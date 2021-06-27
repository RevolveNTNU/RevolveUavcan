using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl.Interfaces;
using System;
using System.Collections.Generic;

namespace RevolveUavcan.Uavcan.Interfaces
{
    public interface IUavcanSerializationGenerator
    {
        IDsdlParser DsdlParser { get; }
        Dictionary<Tuple<uint, string>, List<UavcanChannel>> MessageSerializationRules { get; }
        Dictionary<Tuple<uint, string>, UavcanService> ServiceSerializationRules { get; }

        /// <summary>
        /// Use the <see cref="DsdlParser"/> to initialize the <see cref="MessageSerializationRules"/> 
        /// and <see cref="ServiceSerializationRules"/>.
        /// </summary>
        /// <exception cref="UavcanException">The parsing of DsdlRules fails</exception>
        void Init();
        bool TryGetSerializationRuleForMessage(uint subjectId, out List<UavcanChannel> uavcanChannels);
        bool TryGetSerializationRuleForMessage(string messageName, out List<UavcanChannel> uavcanChannels);
        bool TryGetSerializationRuleForService(uint subjectId, out UavcanService service);
        bool TryGetSerializationRuleForService(string serviceName, out UavcanService service);
        public string GetMessageNameFromSubjectId(uint subjectId);
        public string GetServiceNameFromSubjectId(uint subjectId);

    }
}
