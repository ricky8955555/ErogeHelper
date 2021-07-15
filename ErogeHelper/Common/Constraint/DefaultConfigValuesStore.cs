using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.Common.Constraint
{
    internal static class DefaultConfigValuesStore
    {
        public const string EhServerUrl = "https://eh.nya.run"; // "http://106.15.88.80";

        public static string AssistiveTouchPostion = System.Text.Json.JsonSerializer.Serialize<AssistiveTouchPosition>(new(TouchButtonCorner.UpperLeft));

        public const bool MonitorClipboard = false;

        public const int MaxAcceptTextLength = 120;

        public const double FontSize = 28;

        public const bool EnableMeCab = false;

        public const bool UseDanmaku = false;

        public const bool UseOutsideWindow = false;

        public const bool ShowAppendText = true;

        public const bool PasteToDeepL = false;

        public const TextTemplateType TextTemplate = TextTemplateType.OutLineKanaTop;

        public const bool KanaDefault = false;
        public const bool KanaTop = true;
        public const bool KanaBottom = false;

        public const bool Romaji = false;
        public const bool Hiragana = true;
        public const bool Katakana = false;

        // Account: erogehelper@github.com
        // Password: ******
        public const string MojiSessionToken = "r:02625329e868e96b5eb65bca9dead47e";
        public const bool MojiDictEnable = false;
        public const bool JishoDictEnable = false;

        public const bool UseTermTable = false;

        public const TransLanguage TransSrcLanguage = TransLanguage.日本語;
        public const TransLanguage TransTargetLanguage = TransLanguage.English;

        public const bool AlapiEnable = false;

        public const bool BaiduApiEnable = false;
        public const string BaiduApiAppid = "";
        public const string BaiduApiSecretKey = "";

        public const bool YeekitEnable = false;

        public const bool BaiduWebEnable = false;

        public const bool CaiyunEnable = false;
        public const string CaiyunDefaultToken = "3975l6lr5pcbvidl6jl2";

        public const bool YoudaoEnable = false;

        public const bool NiuTransEnable = false;
        public const string NiuTransApiKey = "";

        public const bool GoogleCnEnable = false;

        public const bool TencentApiEnable = false;
        public const string TencentApiSecretId = "";
        public const string TencentApiSecretKey = "";

        public const bool CloudEnable = false;
    }
}