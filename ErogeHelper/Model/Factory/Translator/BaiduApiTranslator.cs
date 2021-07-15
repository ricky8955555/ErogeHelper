using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using ErogeHelper.Common;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;

namespace ErogeHelper.Model.Factory.Translator
{
    public class BaiduApiTranslator : ITranslator
    {
        private static readonly string ApiUri = "http://api.fanyi.baidu.com/api/trans/vip/translate";
        private static readonly HttpClient Client = new();
        private static readonly Random Random = new();

        public BaiduApiTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;

            AppId = _ehConfigRepository.BaiduApiAppid;
            SecretKey = _ehConfigRepository.BaiduApiSecretKey;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.BaiduApi;

        public bool IsEnable { get => _ehConfigRepository.BaiduApiEnable; set => _ehConfigRepository.BaiduApiEnable = value; }

        public bool NeedEdit => true;

        public bool UnLock => _ehConfigRepository.BaiduApiSecretKey != string.Empty;

        public string IconPath => @"/assets/site_icon/baidu.com.ico";

        public TransLanguage[] SupportSrcLang { get; } = new TransLanguage[] { TransLanguage.Japanese, TransLanguage.English };

        public TransLanguage[] SupportDesLang { get; } = new TransLanguage[] { TransLanguage.SimplifiedChinese, TransLanguage.English };

        public async Task<string> TranslateAsync(string sourceText, TransLanguage srcLang, TransLanguage desLang)
        {
            if (AppId == string.Empty)
            {
                AppId = _ehConfigRepository.BaiduApiAppid;
                SecretKey = _ehConfigRepository.BaiduApiSecretKey;
            }

            int salt = Random.Next(int.MaxValue);
            string sign = Utils.Md5Calculate(AppId + sourceText + salt + SecretKey);

            string query = $"q={HttpUtility.UrlEncode(sourceText)}&from={GetLanguage(srcLang)}&to={GetLanguage(desLang)}&appid={AppId}&salt={salt}&sign={sign}";
            var uri = new UriBuilder(ApiUri) { Query = query }.Uri;

            try
            {
                var response = await Client.GetAsync(uri);
                var result = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<BaiduApiResponse>();

                return string.IsNullOrWhiteSpace(result!.ErrorCode) ? result.TransResult[0].Dst : result.ErrorCode;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            static string GetLanguage(TransLanguage language) => language switch
            {
                TransLanguage.English => "en",
                TransLanguage.SimplifiedChinese => "zh",
                TransLanguage.Japanese => "jp",
                _ => throw new NotSupportedException("Language not supported.")
            };
        }

        public string AppId { get; set; }

        public string SecretKey { get; set; }

        private class BaiduApiResponse
        {
            [JsonPropertyName("error_code")]
            public string ErrorCode { get; set; } = string.Empty;

            [JsonPropertyName("from")]
            public string From { get; set; } = string.Empty;

            [JsonPropertyName("to")]
            public string To { get; set; } = string.Empty;

            [JsonPropertyName("trans_result")]
            public List<BaiduTransResult> TransResult { get; set; } = new List<BaiduTransResult>();

            internal class BaiduTransResult
            {
                [JsonPropertyName("src")]
                public string Src { get; set; } = string.Empty;

                [JsonPropertyName("dst")]
                public string Dst { get; set; } = string.Empty;
            }
        }
    }
}