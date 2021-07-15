using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Service
{
    [TestClass]
    public class MeCabServiceTests
    {
        [TestMethod]
        public void MeCabWordUniDicEnumerableTest()
        {
            var repo = new EhConfigRepository(TestEnvironmentValue.RoamingDir);
            IMeCabService meCabService = new MeCabService();
            var sentence = "ヒルダは井戸から水を汲み取るのをやめなかったのテキスト。";

            // mecab-unidic-neologd get error when produce "スマホ"

            meCabService.CreateTagger(Path.Combine(repo.AppDataDir, "dic"));
            List<MeCabWord> list = meCabService.MeCabWordUniDicEnumerable(sentence, repo).ToList();

            Assert.AreEqual("ヒルダ", list[0].Word);
            Assert.AreEqual(" ", list[0].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Noun, list[0].PartOfSpeech);
            Assert.AreEqual("は", list[1].Word);
            Assert.AreEqual(" ", list[1].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Auxiliary, list[1].PartOfSpeech);
            Assert.AreEqual("井戸", list[2].Word);
            // may be different kana type in config
            //Assert.AreEqual("いど", list[2].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Noun, list[2].PartOfSpeech);
            Assert.AreEqual("から", list[3].Word);
            Assert.AreEqual(" ", list[3].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Auxiliary, list[3].PartOfSpeech);
            Assert.AreEqual("水", list[4].Word);
            //Assert.AreEqual("みず", list[4].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Noun, list[4].PartOfSpeech);
            Assert.AreEqual("を", list[5].Word);
            Assert.AreEqual(" ", list[5].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Auxiliary, list[5].PartOfSpeech);
            Assert.AreEqual("汲み取る", list[6].Word);
            //Assert.AreEqual("くみとる", list[6].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Verb, list[6].PartOfSpeech);
            Assert.AreEqual("の", list[7].Word);
            Assert.AreEqual(" ", list[7].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Auxiliary, list[7].PartOfSpeech);
            Assert.AreEqual("を", list[8].Word);
            Assert.AreEqual(" ", list[8].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Auxiliary, list[8].PartOfSpeech);
            Assert.AreEqual("やめ", list[9].Word);
            Assert.AreEqual(" ", list[9].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Verb, list[9].PartOfSpeech);
            Assert.AreEqual("なかっ", list[10].Word);
            Assert.AreEqual(" ", list[10].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.AuxiliaryVerb, list[10].PartOfSpeech);
            Assert.AreEqual("た", list[11].Word);
            Assert.AreEqual(" ", list[11].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.AuxiliaryVerb, list[11].PartOfSpeech);
            Assert.AreEqual("の", list[12].Word);
            Assert.AreEqual(" ", list[12].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Auxiliary, list[12].PartOfSpeech);
            Assert.AreEqual("テキスト", list[13].Word);
            Assert.AreEqual("text", list[13].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Noun, list[13].PartOfSpeech);
            Assert.AreEqual("。", list[14].Word);
            Assert.AreEqual(" ", list[14].Kana);
            Assert.AreEqual(JapanesePartOfSpeech.Mark, list[14].PartOfSpeech);
        }

        [TestMethod]
        public void MeCabWordUniDicSpecificWordTest()
        {
            var repo = new EhConfigRepository(TestEnvironmentValue.RoamingDir);
            IMeCabService meCabService = new MeCabService();
            var word = "スマホ";

            meCabService.CreateTagger(Path.Combine(repo.AppDataDir, "dic"));
            var list = meCabService.MeCabWordUniDicEnumerable(word, repo);
            foreach (var meCabWord in list)
            {
                Trace.WriteLine($"{meCabWord.Kana} {meCabWord.Word} {meCabWord.PartOfSpeech}");
            }
        }

        // Surface  0    1       2    3 4 5  6      7              8      9        10      11     12 13141516
        // テキスト  名詞, 普通名詞, 一般,*,*,*,テキスト,テキスト - text,テキスト,テキスト,テキスト,テキスト,外,*,*,*,*
        // やめ     "動詞,一般,*,*,下一段-マ行,未然形-一般,ヤメル,止める,やめ,ヤメ,やめる,ヤメル,和,*,*,*,*"
        //          0    1   2 3 4          5          6     7     8   9   10     11
        // なかっ   "助動詞,*,*,*,助動詞-ナイ,連用形-促音便,ナイ,ない,なかっ,ナカッ,ない,ナイ,和,*,*,*,*"
        //          0     1 2 3 4           5           6    7   8    9    10     11  12
        // GetCForm *
        // GetCType *
        // GetFForm *
        // GetFType *
        // GetIForm *
        // GetIType *
        // GetLForm     テキスト

        // GetGoshu     外
        // GetLemma     テキスト - text
        // GetPron テキスト
        // GetOrth テキスト
        // GetOrthBase テキスト
        // GetPronBase テキスト
        // GetPos1 名詞
        // GetPos2 普通名詞
        // GetPos3 一般
        // GetPos4 *

        // 書き言葉    名詞,普通名詞,一般,*,*,*,カキコトバ,書き言葉,書き言葉,カキコトバ,書き言葉,カキコトバ,和,*,*,*,*
        // GetGoshu 	 和
        // GetLemma 書き言葉
        // GetPron カキコトバ
        // GetOrth 書き言葉
        // GetOrthBase 書き言葉
        // GetPronBase カキコトバ
        // GetPos1 名詞
        // GetPos2 普通名詞
        // GetPos3 一般
        // GetPos4*
    }
}