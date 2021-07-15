using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Model.Entity.Payload;
using ErogeHelper.Model.Entity.Response;
using ErogeHelper.Model.Factory.Interface;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;

namespace ErogeHelper.Model.Factory.Dictionary
{
    public class MojiDict : IDict
    {
        public MojiDict(string mojiSessionToken)
        {
            _mojiSessionToken = mojiSessionToken;
        }

        private readonly string _mojiSessionToken;
        private const string BaseAddress = "https://api.mojidict.com";
        private const string SearchApi = "parse/functions/search_v3";
        private const string FetchApi = "parse/functions/fetchWord_v2";

        public async Task<MojiSearchResponse> SearchAsync(string query)
        {
            var client = new RestClient(BaseAddress).UseSystemTextJson();

            var searchPayload = new MojiSearchPayload
            {
                LangEnv = "zh-CN_ja",
                NeedWords = "true",
                SearchText = query,
                ApplicationId = "E62VyFVLMiW7kvbtVq3p",
                ClientVersion = "js2.12.0",                              // no need
                InstallationId = "7d959a18-48c4-243c-7486-632147466544", // no need
                SessionToken = _mojiSessionToken
            };

            var request = new RestRequest(SearchApi, Method.POST)
                .AddJsonBody(searchPayload);

            var resp = await client.ExecutePostAsync<MojiSearchResponse>(request, CancellationToken.None);

            //resp.ResponseStatus
            //resp.StatusCode

            // NOTE: resp.Data may be null if deserialization failed or network etc.
            if (resp.Data is { } result)
            {
                // 如果没有结果，Words.Count == 0，MojiCollection[0].TarId 指向雅虎搜索网址
                return result;
            }

            return new MojiSearchResponse { Error = "An error occurred, it may be bad token, or bad net request" };
        }

        public async Task<MojiFetchResponse> FetchAsync(string tarId)
        {
            var client = new RestClient(BaseAddress).UseSystemTextJson();

            var fetchPayload = new MojiFetchPayload
            {
                WordId = tarId,
                ApplicationId = "E62VyFVLMiW7kvbtVq3p",                  // no need ?
                ClientVersion = "js2.12.0",                              // no need
                InstallationId = "7d959a18-48c4-243c-7486-632147466544", // no need
                SessionToken = _mojiSessionToken                         // no need ?
            };

            IRestRequest requestFetch = new RestRequest(FetchApi, Method.POST)
                .AddJsonBody(fetchPayload);

            var resp = await client.ExecutePostAsync<MojiFetchResponse>(requestFetch, CancellationToken.None);
            if (resp.Data is { } result)
            {
                return result;
            }

            return new MojiFetchResponse();
        }
    }
}
