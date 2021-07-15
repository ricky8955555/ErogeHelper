using System;
using System.Collections.Generic;
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
    public class YeekitTranslator : ITranslator
    {
        public YeekitTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.Yeekit;

        public bool IsEnable { get => _ehConfigRepository.YeekitEnable; set => _ehConfigRepository.YeekitEnable = value; }

        public bool NeedEdit => false;

        public bool UnLock => true;

        public string IconPath => @"/assets/site_icon/yeekit.com.ico";

        // Supported languages https://www.yeekit.com/site/translate
        public TransLanguage[] SupportSrcLang { get; } = new TransLanguage[] { TransLanguage.Japanese, TransLanguage.English };

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
                TransLanguage.Japanese => "nja",
                TransLanguage.English => "nen",
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                TransLanguage.SimplifiedChinese => "nzh",
                _ => throw new Exception("Language not supported"),
            };

            var request = new RestRequest("site/dotranslate", Method.POST)
                .AddParameter("content[]", sourceText)
                .AddParameter("sourceLang", from)
                .AddParameter("targetLang", to);

            string result;
            try
            {
                RestClient client = new RestClient("https://www.yeekit.com");
                var resp = await client.ExecuteAsync(request, CancellationToken.None).ConfigureAwait(false);
                dynamic raw = JsonSerializer.Deserialize<dynamic>(resp.Content)!;
                string jsonString = raw[0].ToString();
                YeekitResponse obj = JsonSerializer.Deserialize<YeekitResponse>(jsonString)!;
                result = obj.Translation[0].Translated[0].Text;
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
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

        private class YeekitResponse
        {
            [JsonPropertyName("translation")]
            public List<YeekitTranslation> Translation { get; set; } = new List<YeekitTranslation>();

            public class YeekitTranslation
            {
                [JsonPropertyName("translationId")]
                public string TranslationId { get; set; } = string.Empty;

                [JsonPropertyName("translated")]
                public List<YeekitTranslated> Translated { get; set; } = new List<YeekitTranslated>();

                public class YeekitTranslated
                {
                    [JsonPropertyName("src-tokenized")]
                    public List<List<string>> SrcTokenized { get; set; } = new List<List<string>>();

                    [JsonPropertyName("score")]
                    public double Score { get; set; }

                    [JsonPropertyName("rank")]
                    public int Rank { get; set; }

                    [JsonPropertyName("text")]
                    public string Text { get; set; } = string.Empty;

                    [JsonPropertyName("translatetime")]
                    public double Translatetime { get; set; }

                    [JsonPropertyName("translationlist")]
                    public List<List<string>> Translationlist { get; set; } = new List<List<string>>();
                }
            }
        }
    }
}