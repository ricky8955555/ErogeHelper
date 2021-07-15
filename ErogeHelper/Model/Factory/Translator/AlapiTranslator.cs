using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using RestSharp;

namespace ErogeHelper.Model.Factory.Translator
{
    [Obsolete("翻译接口关闭")]
    public class AlapiTranslator : ITranslator
    {
        public AlapiTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.Alapi;

        public string IconPath => @"/assets/text_background/transparent.png";

        public bool IsEnable { get => _ehConfigRepository.AlapiEnable; set => _ehConfigRepository.AlapiEnable = value; }

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
            // Doc: https://www.alapi.cn/doc/show/32.html
            string from = srcLang switch
            {
                TransLanguage.Japanese => "jp",
                _ => throw new Exception("Language not supported"),
            };
            string to = desLang switch
            {
                TransLanguage.SimplifiedChinese => "zh",
                _ => throw new Exception("Language not supported"),
            };

            string q = sourceText;
            string result;

            string url = "https://v1.alapi.cn/api/fanyi?q=" + q + "&from=" + from + "&to=" + to;
            // https://v2.alapi.cn/api/fanyi v2 with token, free version 1000/ per day
            // http://www.alapi.cn/api/view/42 10 rmb/per month 

            try
            {
                var client = new RestClient();
                var request = new RestRequest(url);

                var resp = await client.ExecuteGetAsync<AliapiResponse>(request, CancellationToken.None);
                /*
                 * {
                 *      "code": 422,
                 *      "msg": "翻译接口关闭",
                 *      "data": "",
                 *      "author": {
                 *          "name": "Alone88",
                 *          "desc": "由Alone88提供的免费API 服务，官方文档：www.alapi.cn"
                 *      }
                 *  }
                 */
                var content = resp.Data;

                if (content.msg.Equals("success"))
                {
                    if (content.data.trans_result.Count == 1)
                    {
                        result = content.data.trans_result[0].dst;
                    }
                    else
                    {
                        result = "Unknown Error";
                    }
                }
                else
                {
                    result = content.msg;
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

        class AliapiResponse
        {
            public int code { get; set; }
            public string msg { get; set; } = string.Empty;
            public AliapiTransData data { get; set; } = new();

            public class AliapiTransData
            {
                public string from { get; set; } = string.Empty;
                public string to { get; set; } = string.Empty;
                public List<AliapiTransResData> trans_result { get; set; } = new();

                public class AliapiTransResData
                {
                    public string src { get; set; } = string.Empty;
                    public string dst { get; set; } = string.Empty;
                }
            }
        }
    }
}