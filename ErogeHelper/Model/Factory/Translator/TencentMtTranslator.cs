using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using RestSharp;

namespace ErogeHelper.Model.Factory.Translator
{
    public class TencentMtTranslator : ITranslator
    {
        public TencentMtTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.TencentMt;

        public string IconPath => @"/assets/site_icon/tencent.com.ico";

        public bool IsEnable { get => _ehConfigRepository.TencentMtEnable; set => _ehConfigRepository.TencentMtEnable = value; }

        public bool NeedEdit => true;

        public bool UnLock => _ehConfigRepository.TencentMtSecretId != string.Empty;

        public TransLanguage[] SupportSrcLang { get; } = new TransLanguage[] { TransLanguage.Japanese };

        public TransLanguage[] SupportDesLang { get; } = new TransLanguage[] { TransLanguage.SimplifiedChinese };

        public async Task<string> TranslateAsync(string sourceText, TransLanguage srcLang, TransLanguage desLang)
        {
            // SetCancelToken
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            var cancelToken = _cts.Token;

            // Define Support Language
            string source = srcLang switch
            {
                TransLanguage.Japanese => "ja",
                _ => throw new Exception("Language not supported"),
            };
            string target = desLang switch
            {
                TransLanguage.SimplifiedChinese => "zh",
                _ => throw new Exception("Language not supported"),
            };

            #region TencentCloud namespace reference

            //Credential cred = new()
            //{
            //    SecretId = _ehConfigRepository.TencentMtSecretId,
            //    SecretKey = _ehConfigRepository.TencentMtSecretKey
            //};

            //ClientProfile clientProfile = new();
            //HttpProfile httpProfile = new()
            //{
            //    Endpoint = "tmt.tencentcloudapi.com"
            //};
            //clientProfile.HttpProfile = httpProfile;

            //TmtClient client = new(cred, "ap-chengdu", clientProfile);
            //TextTranslateRequest req = new()
            //{
            //    SourceText = sourceText,
            //    Source = source,
            //    Target = target,
            //    ProjectId = 0
            //};

            #endregion

            string salt = new Random().Next(100000).ToString();

            var requestBuilder = new StringBuilder()
                .Append("Action=TextTranslate")
                .Append("&Nonce=").Append(salt)
                .Append("&ProjectId=0")
                .Append("&Region=ap-chengdu")
                .Append("&SecretId=").Append(_ehConfigRepository.TencentMtSecretId)
                .Append("&Source=").Append(source)
                .Append("&SourceText=").Append(sourceText)
                .Append("&Target=").Append(target)
                .Append("&Timestamp=").Append(DateTimeOffset.Now.ToUnixTimeSeconds())
                .Append("&Version=2018-03-21");
            string requestUrl = requestBuilder.ToString();
            HMACSHA1 hmac = new()
            {
                Key = Encoding.UTF8.GetBytes(_ehConfigRepository.TencentMtSecretKey)
            };
            byte[] data = Encoding.UTF8.GetBytes("GETtmt.tencentcloudapi.com/?" + requestUrl);
            var dataHash = hmac.ComputeHash(data);

            string baseUrl = "https://tmt.tencentcloudapi.com/?";
            requestUrl = requestUrl + "&Signature=" + HttpUtility.UrlEncode(Convert.ToBase64String(dataHash));

            string result;
            try
            {
                var client = new RestClient();
                var request = new RestRequest(baseUrl + requestUrl);
                var response = await client.ExecuteGetAsync(request, CancellationToken.None);
                var resp = System.Text.Json.JsonSerializer.Deserialize<TencentMtResponseWrapper>(response.Content);
                result = resp?.Response.TargetText ?? "Unknown error";
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Log.Warn(ex);
            }

            // Insert CancelAssert Before Return
            if (cancelToken.IsCancellationRequested)
            {
                Log.Debug($"{Name} Canceled");
                return string.Empty;
            }

            return result;
        }

        private static CancellationTokenSource _cts = new();

        class TencentMtResponse
        {
            public string TargetText { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public string Target { get; set; } = string.Empty;
            public string RequestId { get; set; } = string.Empty;
            public TencentMtErrorMessage Error { get; set; } = new();
        }

        class TencentMtResponseWrapper
        {
            public TencentMtResponse Response { get; set; } = new();
        }

        class TencentMtErrorMessage
        {
            public string Code { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }
    }
}