using System.Collections.Generic;

namespace RevolveUavcan.Dsdl.Fields
{
    public class UavcanService
    {
        public List<UavcanChannel> RequestFields { get; }
        public List<UavcanChannel> ResponseFields { get; }

        public uint DataTypeID { get; }
        public string Name { get; }

        public UavcanService(List<UavcanChannel> requestFields, List<UavcanChannel> responseFields, uint dataTypeID, string name)
        {
            RequestFields = requestFields;
            ResponseFields = responseFields;
            DataTypeID = dataTypeID;
            Name = name;
        }

    }
}
