using System.Threading.Tasks;
using ErogeHelper.Model.Entity.Response;
using ErogeHelper.Model.Factory.Dictionary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Factory.Dictionary
{
    [TestClass()]
    public class JishoDictTests
    {
        [TestMethod]
        [DataRow("走った")]
        //[DataRow("もだありません")]
        public async Task SearchWordAsyncTest(string query)
        {
            var dict = new JishoDict();
            JishoResponse result = await dict.SearchWordAsync(query);

            // Expected, Actual
            Assert.AreEqual(200, result.Meta.Status);
        }

        [TestMethod]
        [DataRow("pettanko", "", "ぺちゃんこ", "crushed flat", "Na-adjective")]
        // [DataRow("原発性免疫不全症", "原発性免疫不全症", "げんぱつせいめんえきふぜんしょう", "primary immunodeficiency syndrome", "Noun")]
        [DataRow("にく", "肉", "にく", "flesh", "Noun")]
        public async Task SearchWordTest(string searchQuery, string firstWord, string firstReading, string firstEnglishDefinition, string firstPartOfSpeech)
        {
            var dict = new JishoDict();
            var result = await dict.SearchWordAsync(searchQuery);
            var dataList = result.DataList;

            Assert.AreNotEqual(0, dataList.Count);
            Assert.AreNotEqual(0, dataList[0].JapaneseList.Count);
            Assert.AreNotEqual(0, dataList[0].Senses.Count);
            Assert.AreNotEqual(0, dataList[0].Senses[0].EnglishDefinitions.Count);
            Assert.AreNotEqual(0, dataList[0].Senses[0].PartsOfSpeech.Count);
            Assert.AreEqual(firstWord, dataList[0].JapaneseList[0].Word);
            Assert.AreEqual(firstReading, dataList[0].JapaneseList[0].Reading);
            Assert.AreEqual(firstEnglishDefinition, dataList[0].Senses[0].EnglishDefinitions[0]);
            Assert.AreEqual(firstPartOfSpeech, dataList[0].Senses[0].PartsOfSpeech[0]);
        }

        [TestMethod]
        public async Task SearchWordFullAsync()
        {
            var dict = new JishoDict();
            var result = await dict.SearchWordAsync("rori");
            var data = result.DataList;

            Assert.AreNotEqual(0, data.Count);
            Assert.AreEqual("ロリ", data[0].Slug);
            Assert.IsFalse(data[0].IsCommon);
            Assert.AreEqual(0, data[0].Tags.Count);
            Assert.AreEqual(0, data[0].Jlpt.Count);
            Assert.AreNotEqual(0, data[0].Senses);
            Assert.AreEqual(1, data[0].Senses[0].Info.Count);
            Assert.AreEqual("also written as 炉裏", data[0].Senses[0].Info[0]);
            Assert.AreEqual(0, data[0].Senses[0].Links.Count);
            Assert.AreEqual(2, data[0].Senses[0].Tags.Count);
            Assert.AreEqual("Colloquialism", data[0].Senses[0].Tags[0].ToString());
            Assert.AreEqual("Abbreviation", data[0].Senses[0].Tags[1].ToString());
            Assert.AreEqual(0, data[0].Senses[0].Restrictions.Count);
            Assert.AreEqual(1, data[0].Senses[0].SeeAlso.Count);
            Assert.AreEqual("ロリータ", data[0].Senses[0].SeeAlso[0].ToString());
            Assert.AreEqual(0, data[0].Senses[0].Antonyms.Count);
            Assert.AreEqual(0, data[0].Senses[0].Source.Count);
            Assert.IsTrue(data[0].Attribution.Jmdict);
            Assert.IsFalse(data[0].Attribution.Jmnedict);
            Assert.AreEqual("False", data[0].Attribution.Dbpedia.ToString());
        }
    }
}