using Fanex.Athena.LGAPI.SSAS.Bridge.Attributes;
using System;
using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.SSAS.Bridge.Extensions
{
    public static class CubeMappingExtensions
    {
        public static TResult ToObject<TResult>(this IDictionary<string, object> source)
            where TResult : new()
        {
            var objDTO = Activator.CreateInstance<TResult>();
            var props = objDTO.GetType().GetProperties();
            foreach (var prop in props)
            {
                var cubeMappingAttr = Attribute.GetCustomAttribute(prop, typeof(CubeMappingAttribute)) as CubeMappingAttribute;
                if (cubeMappingAttr != null && !string.IsNullOrWhiteSpace(cubeMappingAttr.MapColumn))
                {
                    if (!source.ContainsKey(cubeMappingAttr.MapColumn))
                    {
                        throw new KeyNotFoundException($"{nameof(cubeMappingAttr.MapColumn)} {cubeMappingAttr.MapColumn}");
                    }
                    prop.SetValue(objDTO, Convert.ChangeType(source[cubeMappingAttr.MapColumn], prop.PropertyType));
                }
            }

            return objDTO;
        }
    }
}