using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using RestSharp;

namespace ErogeHelper.Model.Factory.Translator
{
    public class YoudaoTranslator : ITranslator
    {
        public YoudaoTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.Youdao;

        public string IconPath => @"/assets/site_icon/youdao.com.ico";

        public bool IsEnable { get => _ehConfigRepository.YoudaoEnable; set => _ehConfigRepository.YoudaoEnable = value; }

        public bool NeedEdit => false;

        public bool UnLock => true;

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
                TransLanguage.SimplifiedChinese => "zh_cn",
                _ => throw new Exception("Language not supported"),
            };

            string transType = from + "2" + to;
            string q = sourceText;
            string url = "https://fanyi.youdao.com/translate?&doctype=json&type=" + transType + "&i=" + q;


            string result;
            try
            {
                var client = new RestClient();
                var request = new RestRequest(url);
                var resp = await client.GetAsync<YoudaoResponse>(request, CancellationToken.None);

                if (resp.errorCode == 0)
                {
                    if (resp.translateResult.Count == 1)
                    {
                        result = string.Join("", resp.translateResult[0].Select(x => x.tgt));
                    }
                    else
                    {
                        result = "Error translateResultList";
                    }
                }
                else
                {
                    result = "Error code: " + resp.errorCode;
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Log.Warn(ex.Message);
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

        class YoudaoResponse
        {
            public string type { get; set; } = string.Empty;
            public int errorCode { get; set; }
            public int elapsedTime { get; set; }
            public List<List<YoudaoTransData>> translateResult { get; set; } = new();

            public class YoudaoTransData
            {
                public string src { get; set; } = string.Empty;
                public string tgt { get; set; } = string.Empty;
            }
        }
    }
}