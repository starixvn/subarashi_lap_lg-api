using Fanex.Athena.LGAPI.SSAS.Bridge.Attributes;
using System;
using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.SSAS.Bridge.Extensions
{
    public static class MappingExtensions
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

        public static IDictionary<string, object> GetParams(this object obj)
        {
            var paramsDic = new Dictionary<string, object>();
            var props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                var paramQueryAttribute = Attribute.GetCustomAttribute(prop, typeof(ParamQueryMappingAttribute)) as ParamQueryMappingAttribute;

                if (paramQueryAttribute == null)
                {
                    paramsDic.Add(prop.Name, prop.GetValue(obj));
                }
                else if (!paramQueryAttribute.NoneParam && !string.IsNullOrWhiteSpace(paramQueryAttribute.ParamName))
                {
                    paramsDic.Add(paramQueryAttribute.ParamName, prop.GetValue(obj));
                }
                else
                {
                    //
                }
            }
            return paramsDic;
        }

        public static void AddRange<T, S>(this IDictionary<T, S> source, IDictionary<T, S> collection)
        {
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    if (!source.ContainsKey(item.Key))
                    {
                        source.Add(item.Key, item.Value);
                    }
                }
            }
        }

    }
}