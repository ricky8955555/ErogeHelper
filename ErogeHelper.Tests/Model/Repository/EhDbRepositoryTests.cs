using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Extention;
using ErogeHelper.Model.Entity.Table;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Repository.Migration;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Repository
{
    [TestClass]
    public class EhDbRepositoryTests
    {
        [TestMethod]
        public void EhDbRepositoryBasicQueryTest()
        {
            // Arrange 
            var dbFile = Path.Combine(TestEnvironmentValue.RoamingDir, "ErogeHelper", "eh.db");
            var connectString = $"Data Source={dbFile}";
            var ehDbRepo = new EhDbRepository(connectString) { Md5 = "0123456789ABCDEF0123456789ABCDEF" };
            if (!File.Exists(dbFile))
                CreateDb(connectString);

            // Act
            // 26ms
            var result = ehDbRepo.GetGameInfoAsync().Result;

            // Assert
            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public async Task EhDatabaseGameInfoTableCrud()
        {
            // Arrange 
            var dbFile = Path.Combine(TestEnvironmentValue.RoamingDir, "ErogeHelper", "eh.db");
            var connectString = $"Data Source={dbFile}";
            var fakeMd5 = "0123456789ABCDEF0123456789ABCDEF";
            var ehDbRepo = new EhDbRepository(connectString) { Md5 = fakeMd5 };
            var fakeIdList = "1,2,3";
            var textractorSetting = new TextractorSetting
            {
                IsUserHook = true,
                Hookcode = "HA8@168460:cs2_open.exe",
                HookSettings = new List<TextractorSetting.HookSetting>
                {
                    new() {ThreadContext = 0, SubThreadContext = 0}
                }
            };
            var fakeJson = JsonSerializer.Serialize(textractorSetting);
            var fakeGameInfo = new GameInfoTable
            {
                Md5 = fakeMd5,
                GameIdList = fakeIdList,
                RegExp = string.Empty,
                TextractorSettingJson = fakeJson,
                IsLoseFocus = false,
                IsEnableTouchToMouse = false,
            };
            var newGameInfo = new GameInfoTable
            {
                Md5 = fakeMd5,
                GameIdList = fakeIdList,
                TextractorSettingJson = string.Empty,
            };
            if (!File.Exists(dbFile))
                CreateDb(connectString);

            // Act
            var result = await ehDbRepo.GetGameInfoAsync().ConfigureAwait(false);
            if (result is not null)
            {
                await ehDbRepo.DeleteGameInfoAsync().ConfigureAwait(false);
            }

            // Act and Assert
            await ehDbRepo.SetGameInfoAsync(fakeGameInfo).ConfigureAwait(false);
            result = await ehDbRepo.GetGameInfoAsync().ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(fakeMd5, result!.Md5);
            Assert.AreEqual(fakeIdList, result!.GameIdList);
            Assert.AreEqual(fakeJson, result!.TextractorSettingJson);
            // Clear the TextractorSettingJson field
            await ehDbRepo.UpdateGameInfoAsync(newGameInfo).ConfigureAwait(false);
            result = await ehDbRepo.GetGameInfoAsync().ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result!.TextractorSettingJson);
            await ehDbRepo.DeleteGameInfoAsync().ConfigureAwait(false);
            result = await ehDbRepo.GetGameInfoAsync().ConfigureAwait(false);
            Assert.IsNull(result);
        }

        private static void CreateDb(string connectString)
        {
            var serviceCollection = new ServiceCollection();
            var provider = serviceCollection.AddFluentMigratorCore()
                    .ConfigureRunner(rb => rb
                        .AddSQLite()
                        .WithGlobalConnectionString(connectString)
                        .ScanIn(typeof(AddGameInfoTable).Assembly).For.Migrations())
                    .BuildServiceProvider();
            provider.UpdateEhDatabase();
        }
    }
}