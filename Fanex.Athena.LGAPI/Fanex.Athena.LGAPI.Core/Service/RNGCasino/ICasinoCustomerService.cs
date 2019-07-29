namespace Fanex.Athena.LGAPI.Core.Service.RNGCasino
{
    public interface ICasinoCustomerService
    {
        bool UpdateClosed(CustomerStatusMetaModel metaModel);

        bool UpdateSuspend(CustomerStatusMetaModel metaModel);

        bool UpdateCommission(RNGCasinoUpdateMemberComMetaModel metaModel);
    }
}