using System.Threading.Tasks;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory;
using ErogeHelper.Model.Factory.Dictionary;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Factory.Dictionary
{
    [TestClass]
    public class MojiDictTests
    {
        [TestMethod]
        public async Task MojiDictTest()
        {
            IDictionaryFactory dictionaryFactory = new DictionaryFactory(new EhConfigRepository(TestEnvironmentValue.RoamingDir));
            var dict = dictionaryFactory.GetDictInstance(DictType.Moji);

            if (dict is MojiDict mojiDict)
            {
                var result = await mojiDict.SearchAsync("買う");
                Assert.AreEqual("買う", result.Result.OriginalSearchText);
                Assert.AreEqual("買う", result.Result.Words[0].Spell);
                Assert.AreEqual("◎", result.Result.Words[0].Accent);
                Assert.AreEqual("かう", result.Result.Words[0].Pron);
                Assert.AreEqual("19899595", result.Result.Words[0].ObjectId);

                var fetchResult = await mojiDict.FetchAsync(result.Result.Words[0].ObjectId);
                Assert.AreEqual("買う", fetchResult.Result.Word.Spell);
                Assert.AreEqual("かう", fetchResult.Result.Word.Pron);
                Assert.AreEqual("13363", fetchResult.Result.Subdetails[0].ObjectId);
                Assert.AreEqual("买。（品物や金と引き換えに、自分の望みの品物を得る。）", fetchResult.Result.Subdetails[0].Title);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}