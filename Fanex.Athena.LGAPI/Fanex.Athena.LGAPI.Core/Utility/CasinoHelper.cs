namespace Fanex.Athena.LGAPI.Core.Utility
{
    public static class CasinoHelper
    {
        public static string GetColumnNameByLanguage(string columnName, string languageCode)
        {
            string suffix;

            switch (languageCode)
            {
                case "zh-CN":
                case "zh-Hans":
                    suffix = "_Cs";
                    break;

                case "zh-TW":
                    suffix = "_Ct";
                    break;

                default:
                    suffix = "_En";
                    break;
            }

            return columnName + suffix;
        }
    }
}
