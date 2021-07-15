using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.Model.Service
{
    public class HookDataService : IHookDataService
    {
        public HookDataService(EhDbRepository ehDbRepository)
        {
            _ehDbRepository = ehDbRepository;
        }

        private readonly EhDbRepository _ehDbRepository;

        private static readonly string GameQuery = "http://vnr.aniclan.com/connection.php?go=game_query";

        private static readonly HttpClient HttpClient = new();

        public async Task<string?> QueryHCode()
        {
            var content = new StringContent($"md5={_ehDbRepository.Md5}");
            var response = await HttpClient.PostAsync(GameQuery, content);
            string result = await response.Content.ReadAsStringAsync();

            var document = XDocument.Parse(result);
            var game = document.Element("grimoire")?.Element("games")?.Element("game");

            if (game?.Element("hook") is not null)
            {
                return game?.Element("hook")?.Value;
            }

            return null;
        }

        public string? GetRegExp() => _ehDbRepository.GetGameInfo()?.RegExp;
    }
}
