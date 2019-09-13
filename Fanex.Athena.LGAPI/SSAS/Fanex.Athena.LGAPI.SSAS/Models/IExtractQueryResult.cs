using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.SSAS.Models
{
    public interface IExtractQueryResult<T>
    {
        T ToObject(IDictionary<string, object> record);
    }
}