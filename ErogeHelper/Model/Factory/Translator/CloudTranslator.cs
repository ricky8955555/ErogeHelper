using System;
using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using RestSharp;

namespace ErogeHelper.Model.Factory.Translator
{
    class CloudTranslator : ITranslator
    {
        public CloudTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.Cloud;

        public string IconPath => @"/assets/image/yunyi-logo2.png";

        public bool IsEnable { get => _ehConfigRepository.CloudEnable; set => _ehConfigRepository.CloudEnable = value; }

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
                TransLanguage.SimplifiedChinese => "zh-cn",
                _ => throw new Exception("Language not supported"),
            };

            var client = new RestClient("https://sz-nmt-1.cloudtranslation.com/nmt");
            var request = new RestRequest()
            {
                Method = Method.GET
            };
            request.AddParameter("lang", from + '_' + to);
            request.AddParameter("src", sourceText);

            string result;
            try
            {
                var response = await client.ExecuteAsync(request, CancellationToken.None).ConfigureAwait(false);
                result = response.Content;
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
    }
}
