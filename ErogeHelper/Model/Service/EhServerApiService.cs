using System;
using System.Net.Http;
using System.Threading.Tasks;
using ErogeHelper.Model.Entity.Payload;
using ErogeHelper.Model.Entity.Response;
using ErogeHelper.Model.Service.Interface;
using Refit;

namespace ErogeHelper.Model.Service
{
    public class EhServerApiService : IEhServerApiService
    {
        private readonly IEhServerApiService _ehServerApiService;

        public EhServerApiService(string baseUrl)
        {
            // For debug using
            //var httpClient = new HttpClient(new HttpClientDiagnosticsHandler(new HttpClientHandler()))
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            _ehServerApiService = RestService.For<IEhServerApiService>(httpClient);
        }

        public async Task<ApiResponse<GameSettingResponse>> GetGameSetting(string md5)
        {
            return await _ehServerApiService.GetGameSetting(md5).ConfigureAwait(false);
        }

        public async Task<ApiResponse<SubmitSettingResponse>> SendGameSetting(GameSettingPayload payload)
        {
            return await _ehServerApiService.SendGameSetting(payload).ConfigureAwait(false);
        }
    }
}