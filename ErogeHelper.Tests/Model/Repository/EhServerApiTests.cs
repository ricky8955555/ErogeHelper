using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ErogeHelper.Model.Entity.Payload;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Repository
{
    [TestClass]
    public class EhServerApiTests
    {
        [TestMethod]
        public async Task GetExistGameTest()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configRepo = new EhConfigRepository(appDataDir);
            var ehServerApi = new EhServerApiService(configRepo.EhServerBaseUrl);

            // Act
            var resp = await ehServerApi.GetGameSetting("1ef3cdc2e666091bda2dc828872d597b");
            await resp.EnsureSuccessStatusCodeAsync();
            var gameSettingContent = resp.Content;

            // Assert
            if (resp.IsSuccessStatusCode && gameSettingContent is not null && resp.StatusCode == HttpStatusCode.OK)
            {
                Assert.AreEqual(13455, gameSettingContent.GameId);
                Assert.AreEqual(string.Empty, gameSettingContent.GameSettingJson);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task SendGameSettingTest()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configRepo = new EhConfigRepository(appDataDir);
            //const string? localTestUrl = "https://localhost:49161";
            var ehServerApi = new EhServerApiService(configRepo.EhServerBaseUrl);

            // Act
            var resp = await ehServerApi.SendGameSetting(
                new GameSettingPayload(
                    "guest",
                    "erogehelper",
                    "1C4B90AFFDAA1548A3399235971598B4",
                    new Dictionary<string, string>()
                    {
                        {"File", "cs2_open.exe"},
                        {"Dir", "恋ニ_甘味ヲソエテ２"},
                        {"Title", "恋ニ、甘味ヲソエテ２（DL版）"},
                        {"FileNoExt", "cs2_open"}
                    },
                    "{\"IsUserHook\":true,\"Hookcode\":\"HA8@168460:cs2_open.exe\",\"HookSettings\":[{\"ThreadContext\":5671568,\"SubThreadContext\":0}],\"UselessAddresses\":[]}",
                    ""));
            await resp.EnsureSuccessStatusCodeAsync();
            var submitSetting = resp.Content;

            // Assert
            if (resp.IsSuccessStatusCode && submitSetting is not null && resp.StatusCode == HttpStatusCode.OK)
            {
                Assert.AreEqual(98042, submitSetting.Id);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}