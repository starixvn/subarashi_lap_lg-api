namespace Fanex.Athena.LGAPI.Core.Utility
{
    using System;
    using ThirdParty.ExchangeProduct.Enums;

    public static class ExchangeHelper
    {
        private const string OneTwoBet = "12bet";

        public static int GetGroupModeBySiteName(string siteName)
        {
            if (string.Equals(siteName, OneTwoBet, StringComparison.OrdinalIgnoreCase))
            {
                return (int)GroupMode.OneTwoBet;
            }

            return (int)GroupMode.Other;
        }
    }
}