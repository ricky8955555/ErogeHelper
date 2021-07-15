using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using RestSharp;

namespace ErogeHelper.Model.Factory.Translator
{
    public class CaiyunTranslator : ITranslator
    {
        public CaiyunTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.Caiyun;

        public string IconPath => @"/assets/site_icon/caiyunapp.com.ico";

        public bool IsEnable { get => _ehConfigRepository.CaiyunEnable; set => _ehConfigRepository.CaiyunEnable = value; }

        public bool NeedEdit => true;

        public bool UnLock => _ehConfigRepository.CaiyunToken != string.Empty;

        public TransLanguage[] SupportSrcLang { get; } = new TransLanguage[] { TransLanguage.Japanese };

        public TransLanguage[] SupportDesLang { get; } = new TransLanguage[] { TransLanguage.SimplifiedChinese };

        public async Task<string> TranslateAsync(string sourceText, TransLanguage srcLang, TransLanguage desLang)
        {
            // SetCancelToken
            _cancelToken.Cancel();
            _cancelToken = new CancellationTokenSource();
            var token = _cancelToken.Token;

            // Define Support Language
            string from = srcLang switch
            {
                TransLanguage.Japanese => "ja",
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                TransLanguage.SimplifiedChinese => "zh",
                _ => throw new Exception("Language not supported"),
            };

            string q = sourceText;
            string transType = from + '2' + to;
            string caiyunToken = "token " + _ehConfigRepository.CaiyunToken;

            string jsonBody = "{\"source\": [\"" + q + "\"], \"trans_type\": \"" + transType + "\", \"request_id\": \"demo\", \"detect\": true}";

            var client = new RestClient("https://api.interpreter.caiyunai.com");
            var request = new RestRequest("v1/translator")
            {
                Method = Method.POST
            };
            request.AddHeader("ContentType", "application/json;charset=UTF-8");
            request.AddHeader("X-Authorization", caiyunToken);
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

            string result;
            try
            {
                var response = await client.ExecuteAsync(request, CancellationToken.None).ConfigureAwait(false);
                CaiyunResponse resp = JsonSerializer.Deserialize<CaiyunResponse>(response.Content)!;
                result = resp.Target.Count > 0
                    ? string.Join("", resp.Target.Select(System.Text.RegularExpressions.Regex.Unescape))
                    : resp.Message;
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message);
                result = ex.Message;
            }

            // Insert CancelAssert Before Return
            if (token.IsCancellationRequested)
            {
                Log.Debug($"{Name} Canceled");
                return string.Empty;
            }

            return result;
        }

        private static CancellationTokenSource _cancelToken = new();

        class CaiyunResponse
        {
            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;
            public double confidence { get; set; }
            public int rc { get; set; }

            [JsonPropertyName("target")]
            public List<string> Target { get; set; } = new();
        }
    }
}