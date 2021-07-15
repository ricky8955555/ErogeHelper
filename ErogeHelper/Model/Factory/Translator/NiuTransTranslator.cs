using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using RestSharp;

namespace ErogeHelper.Model.Factory.Translator
{
    public class NiuTransTranslator : ITranslator
    {
        public NiuTransTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.NiuTrans;

        public string IconPath => @"/assets/site_icon/niutrans.com.ico";

        public bool IsEnable { get => _ehConfigRepository.NiuTransEnable; set => _ehConfigRepository.NiuTransEnable = value; }

        public bool NeedEdit => true;

        public bool UnLock => _ehConfigRepository.NiuTransApiKey != string.Empty;

        public TransLanguage[] SupportSrcLang { get; } = new TransLanguage[] { TransLanguage.Japanese };

        public TransLanguage[] SupportDesLang { get; } = new TransLanguage[] { TransLanguage.SimplifiedChinese };

        public async Task<string> TranslateAsync(string sourceText, TransLanguage srcLang, TransLanguage desLang)
        {
            // SetCancelToken
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            var cancelToken = _cts.Token;

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

            var sb = new StringBuilder("https://free.niutrans.com/NiuTransServer/translation?")
                .Append("&from=").Append(from)
                .Append("&to=").Append(to)
                .Append("&apikey=").Append(_ehConfigRepository.NiuTransApiKey)
                .Append("&src_text=").Append(Uri.EscapeDataString(sourceText));

            string url = sb.ToString();

            string result;
            try
            {
                var client = new RestClient();
                var request = new RestRequest(url);
                var response = await client.ExecuteGetAsync(request, CancellationToken.None);

                var resp = JsonSerializer.Deserialize<NiuTransResponse>(response.Content)!;
                result = resp.tgt_text == string.Empty ? resp.error_code + ": " + resp.error_msg : resp.tgt_text;
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

        class NiuTransResponse
        {
            public string from { get; set; } = string.Empty;
            public string to { get; set; } = string.Empty;
            public string src_text { get; set; } = string.Empty;
            public string tgt_text { get; set; } = string.Empty;

            //https://niutrans.com/documents/develop/develop_text/free#error
            public int error_code { get; set; }
            public string error_msg { get; set; } = string.Empty;
        }
    }
}