using RevolveUavcan.Dsdl.Fields;
using System;
using System.Collections.Generic;
using System.Text;

namespace RevolveUavcan.Uavcan.Interfaces
{
    public interface IUavcanSerializationGenerator
    {
        Dictionary<Tuple<uint, string>, List<UavcanChannel>> MessageSerializationRules { get; }
        Dictionary<Tuple<uint, string>, UavcanService> ServiceSerializationRules { get; }

        bool TryGetSerializationRuleForMessage(uint subjectId, out List<UavcanChannel> uavcanChannels);
        bool TryGetSerializationRuleForMessage(string messageName, out List<UavcanChannel> uavcanChannels);
        bool TryGetSerializationRuleForService(uint subjectId, out UavcanService service);
        bool TryGetSerializationRuleForService(string serviceName, out UavcanService service);
    }
}
