using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using Jurassic;

namespace ErogeHelper.Model.Factory.Translator
{
    public class BaiduWebTranslator : ITranslator
    {
        public BaiduWebTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;

            _client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
            })
            {
                Timeout = TimeSpan.FromSeconds(6)
            };
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.BaiduWeb;

        public string IconPath => @"/assets/site_icon/baidu.com.ico";

        public bool IsEnable { get => _ehConfigRepository.BaiduWebEnable; set => _ehConfigRepository.BaiduWebEnable = value; }

        public bool NeedEdit => false;

        public bool UnLock => true;

        public TransLanguage[] SupportSrcLang { get; } = new TransLanguage[] { TransLanguage.Japanese, TransLanguage.English };

        public TransLanguage[] SupportDesLang { get; } = new TransLanguage[] { TransLanguage.SimplifiedChinese, TransLanguage.English };

        public async Task<string> TranslateAsync(string sourceText, TransLanguage srcLang, TransLanguage desLang)
        {
            try
            {
                var uri = new Uri(BaseUrl);

                string? gtk = null;
                string? token = null;

                if (_firstRequest)
                {
                    await _client.GetStringAsync(TransUrl);
                    _firstRequest = false;
                }

                string content = await _client.GetStringAsync(TransUrl);

                var tokenMatch = Regex.Match(content, "token: '(.*?)',");
                var gtkMatch = Regex.Match(content, "window.gtk = '(.*?)';");

                if (gtkMatch.Success && gtkMatch.Groups.Count > 1)
                    gtk = gtkMatch.Groups[1].Value;
                if (tokenMatch.Success && tokenMatch.Groups.Count > 1)
                    token = tokenMatch.Groups[1].Value;

                _jsEngine.Evaluate(TokenCalculatingJs);
                string sign = _jsEngine.CallGlobalFunction<string>("token", sourceText, gtk);

                var values = new Dictionary<string, string?>()
                {
                    { "from", GetLanguage(srcLang) },
                    { "to", GetLanguage(desLang) },
                    { "query", sourceText },
                    { "transtype", "translang" },
                    { "simple_means_flag", "3" },
                    { "sign", sign },
                    { "token", token },
                };

                var data = new FormUrlEncodedContent(values!);
                var response = await _client.PostAsync(ServiceUrl, data);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BaiduWebResponse>();

                if (result is null)
                {
                    throw new HttpRequestException();
                }

                return result.TransResult.Data[0].Dst;
            }
            catch (HttpRequestException ex)
            {
                Log.Warn(ex.ToString());
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

        private bool _firstRequest = true;

        private readonly HttpClient _client;

        private const string BaseUrl = @"https://www.baidu.com";
        private const string TransUrl = @"https://fanyi.baidu.com";
        private const string ServiceUrl = @"https://fanyi.baidu.com/v2transapi";

        private readonly ScriptEngine _jsEngine = new();

        private static readonly string TokenCalculatingJs = @"
function a(r, o) {
    for (var t = 0; t < o.length - 2; t += 3) {
        var a = o.charAt(t + 2);
        a = a >= 'a' ? a.charCodeAt(0) - 87 : Number(a),
        a = '+' === o.charAt(t + 1) ? r >>> a : r << a,
        r = '+' === o.charAt(t) ? r + a & 4294967295 : r ^ a
	}
    return r
}
var C = null;
var token = function( r, _gtk ) {
    var o = r.length;
	o > 30 && (r = '' + r.substr(0, 10) + r.substr(Math.floor(o / 2) - 5, 10) + r.substring(r.length, r.length - 10));
    var t = void 0,
    t = null !== C ? C: (C = _gtk || '') || '';
    for (var e = t.split('.'), h = Number( e[0]) || 0, i = Number( e[1]) || 0, d = [], f = 0, g = 0; g<r.length; g++) {
        var m = r.charCodeAt( g );
        128 > m ? d[f++] = m : (2048 > m ? d[f++] = m >> 6 | 192 : (55296 === (64512 & m) && g + 1 < r.length && 56320 === (64512 & r.charCodeAt(g + 1)) ? (m = 65536 + ((1023 & m) << 10) + (1023 & r.charCodeAt(++g)), d[f++] = m >> 18 | 240, d[f++] = m >> 12 & 63 | 128) : d[f++] = m >> 12 | 224, d[f++] = m >> 6 & 63 | 128), d[f++] = 63 & m | 128)
    }
    for (var S = h, u = '+-a^+6', l = '+-3^+b+-f', s = 0; s<d.length; s++)
		S += d[s], S = a( S, u);
    return S = a( S, l),
		S ^= i,
		0 > S && (S = (2147483647 & S) + 2147483648),
		S %= 1e6,
		S.toString() + '.' + (S ^ h)
}
";

        private class BaiduWebResponse
        {
            [JsonPropertyName("trans_result")]
            public BaiduTransResult TransResult { get; set; } = new();

            [JsonPropertyName("liju_result")]
            public BaiduLijuResult LijuResult { get; set; } = new();

            [JsonPropertyName("logid")]
            public long Logid { get; set; }

            public class BaiduTransResult
            {
                [JsonPropertyName("data")]
                public List<Datum> Data { get; set; } = new();
                public string from { get; set; } = string.Empty;
                public int status { get; set; }
                public string to { get; set; } = string.Empty;
                public int type { get; set; }
                public List<Phonetic> phonetic { get; set; } = new();

                public class Datum
                {
                    [JsonPropertyName("dst")]
                    public string Dst { get; set; } = string.Empty;
                    public int prefixWrap { get; set; }
                    public List<List<object>> result { get; set; } = new();
                    public string src { get; set; } = string.Empty;
                }

                public class Phonetic
                {
                    public string src_str { get; set; } = string.Empty;
                    public string trg_str { get; set; } = string.Empty;
                }
            }

            public class BaiduLijuResult
            {
                public string @double { get; set; } = string.Empty;
                public string single { get; set; } = string.Empty;
            }
        }
    }
}