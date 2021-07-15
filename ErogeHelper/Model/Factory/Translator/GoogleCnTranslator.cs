using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using Jurassic;
using RestSharp;

namespace ErogeHelper.Model.Factory.Translator
{
    public class GoogleCnTranslator : ITranslator
    {
        public GoogleCnTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.GoogleCn;

        public string IconPath => @"/assets/site_icon/translate.google.cn.ico";

        public bool IsEnable
        {
            get => _ehConfigRepository.GoogleCnEnable;
            set => _ehConfigRepository.GoogleCnEnable = value;
        }

        public bool NeedEdit => false;

        public bool UnLock => true;

        public TransLanguage[] SupportSrcLang { get; } = new TransLanguage[] { TransLanguage.Japanese };

        public TransLanguage[] SupportDesLang { get; } = new TransLanguage[] { TransLanguage.SimplifiedChinese };

        public async Task<string> TranslateAsync(string sourceText, TransLanguage srcLang, TransLanguage desLang)
        {
            // Set CancelToken
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            var cancelToken = _cts.Token;

            // Define Support Language
            string sl = srcLang switch
            {
                TransLanguage.Japanese => "ja",
                _ => throw new Exception("Language not supported"),
            };
            string tl = desLang switch
            {
                TransLanguage.SimplifiedChinese => "zh-CN",
                _ => throw new Exception("Language not supported"),
            };


            _jsEngine.Evaluate(TkkJs);

            //string fun = $@"TL('{sourceText}')";
            var tk = _jsEngine.CallGlobalFunction<string>("TL", sourceText);

            StringBuilder builder = new();
            builder
                .Append("http://translate.google.cn/translate_a/single?client=webapp")
                .Append("&sl=").Append(sl)
                .Append("&tl=").Append(tl)
                .Append("&hl=zh-CN&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t")
                .Append("&ie=UTF-8&oe=UTF-8&clearbtn=1&otf=1&pc=1&srcrom=0&ssel=0&tsel=0&kc=2")
                .Append("&tk=").Append(tk)
                .Append("&q=").Append(Uri.EscapeDataString(sourceText));
            string googleTransUrl = builder.ToString();

            string result = string.Empty;
            try
            {
                var client = new RestClient();
                var request = new RestRequest(googleTransUrl);
                var resultHtml = await client.ExecuteGetAsync(request, CancellationToken.None);

                var tempResult = JsonSerializer.Deserialize<JsonElement>(resultHtml.Content);

                var array = tempResult.EnumerateArray().ToArray()[0].EnumerateArray().ToArray();
                for (var i = 0; i < array.Length; i++)
                {
                    result += array[i][0];
                }
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

        private readonly ScriptEngine _jsEngine = new();

        private const string TkkJs = @"
function TL(a) { 
    var k = ''; 
    var b = 406644; 
    var b1 = 3293161072; 
    var jd = '.'; 
    var $b = '+-a^+6'; 
    var Zb = '+-3^+b+-f'; 
    for (var e = [], f = 0, g = 0; g < a.length; g++) { 
        var m = a.charCodeAt(g); 
        128 > m ? e[f++] = m : (2048 > m ? e[f++] = m >> 6 | 192 : (55296 == (m & 64512) && g + 1 < a.length && 56320 == (a.charCodeAt(g + 1) & 64512) ? (m = 65536 + ((m & 1023) << 10) + (a.charCodeAt(++g) & 1023), 
        e[f++] = m >> 18 | 240, 
        e[f++] = m >> 12 & 63 | 128) : e[f++] = m >> 12 | 224, 
        e[f++] = m >> 6 & 63 | 128), 
        e[f++] = m & 63 | 128) 
    } 
    a = b; 
    for (f = 0; f < e.length; f++) a += e[f], 
    a = RL(a, $b); 
    a = RL(a, Zb); 
    a ^= b1 || 0; 
    0 > a && (a = (a & 2147483647) + 2147483648); 
    a %= 1E6; 
    return a.toString() + jd + (a ^ b) 
}; 
function RL(a, b) { 
    var t = 'a'; 
    var Yb = '+'; 
    for (var c = 0; c < b.length - 2; c += 3) { 
        var d = b.charAt(c + 2), 
        d = d >= t ? d.charCodeAt(0) - 87 : Number(d), 
        d = b.charAt(c + 1) == Yb ? a >>> d: a << d; 
        a = b.charAt(c) == Yb ? a + d & 4294967295 : a ^ d 
    } 
    return a 
}
";
    }
}
