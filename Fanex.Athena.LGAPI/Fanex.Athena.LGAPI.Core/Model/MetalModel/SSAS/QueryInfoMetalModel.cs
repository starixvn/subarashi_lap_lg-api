using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.Core.Model.MetalModel.SSAS
{
    public class QueryInfoMetalModel
    {
        public string Query { get; set; }
        public string ConnectionKey { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
    }
}