using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.Core.Model.MetalModel.SSAS
{
    public class QueryMultipleMetaModel : BaseSSASMetaModel
    {
        public IEnumerable<string> Queries { get; set; }
    }
}