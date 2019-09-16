using System;

namespace Fanex.Athena.LGAPI.SSAS.Bridge.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CubeMappingAttribute : Attribute
    {
        public CubeMappingAttribute(string mapColumn)
        {
            MapColumn = mapColumn;
        }

        public CubeMappingAttribute()
        {
        }

        public string MapColumn { get; set; }
    }
}