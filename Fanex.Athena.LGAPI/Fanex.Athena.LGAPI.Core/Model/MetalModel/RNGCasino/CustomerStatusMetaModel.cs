namespace Fanex.Athena.LGAPI.Core.Service.RNGCasino
{
    public class CustomerStatusMetaModel
    {
        public int CustId { get; set; }

        public bool IsOn { get; set; }

        public string Ip { get; set; }

        public int ModifyById { get; set; }

        public string ModifyByUsername { get; set; }
    }
}