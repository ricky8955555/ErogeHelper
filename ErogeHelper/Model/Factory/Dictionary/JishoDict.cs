using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Model.Entity.Response;
using ErogeHelper.Model.Factory.Interface;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;

namespace ErogeHelper.Model.Factory.Dictionary
{
    public class JishoDict : IDict
    {
        private const string BaseAddress = "https://jisho.org";

        public async Task<JishoResponse> SearchWordAsync(string query)
        {
            var client = new RestClient(BaseAddress).UseSystemTextJson();

            var request = new RestRequest("api/v1/search/words", Method.GET)
                .AddParameter("keyword", query);

            var resp =
                await client.ExecuteGetAsync<JishoResponse>(request, CancellationToken.None).ConfigureAwait(false);

            if (resp.Data is { } result)
                return result;

            return new JishoResponse();
        }
    }
}