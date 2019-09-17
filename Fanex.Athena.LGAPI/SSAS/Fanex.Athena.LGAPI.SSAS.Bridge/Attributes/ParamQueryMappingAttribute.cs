using System;

namespace Fanex.Athena.LGAPI.SSAS.Bridge.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ParamQueryMappingAttribute : Attribute
    {
        public ParamQueryMappingAttribute(string paramName, bool noneParam = false)
        {
            ParamName = paramName;
            NoneParam = noneParam;
        }

        public ParamQueryMappingAttribute()
        {
        }

        public string ParamName { get; set; }
        public bool NoneParam { get; set; }
    }
}