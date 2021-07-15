using System.Diagnostics;
using System.Linq;
using ErogeHelper.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Common
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void GetGameNamesByPathTest()
        {
            var notepadPath = @"C:\WINDOWS\system32\notepad.exe";
            var dict = Utils.GetGameNamesByPath(notepadPath);

            Assert.AreEqual("notepad.exe", dict["File"]);
            Assert.AreEqual("system32", dict["Dir"]);
            Assert.AreEqual("notepad", dict["FileNoExt"]);
        }

        [TestMethod]
        public void GetGameNamesByProcess()
        {
            var processes = Process.GetProcessesByName("RuntimeBroker");
            var dict = Utils.GetGameNamesByProcess(processes.First());

            Assert.AreEqual("RuntimeBroker.exe", dict["File"]);
            Assert.AreEqual("System32", dict["Dir"]);
            Assert.AreEqual("RuntimeBroker", dict["FileNoExt"]);
            Assert.AreEqual(string.Empty, dict["Title"]);
        }

        [TestMethod()]
        public void TextEvaluateWithRegExpTest_Normal()
        {
            var rawText = "this is raw text";
            var expectText = "th|~S~|is|~E~| |~S~|is|~E~| raw text";
            var regExpPattern = "is";

            string wrapperWithSpecialString = Utils.TextEvaluateWithRegExp(rawText, regExpPattern);

            Assert.AreEqual(expectText, wrapperWithSpecialString);
        }

        [TestMethod()]
        public void TextEvaluateWithRegExpTest_Wildcard()
        {
            var rawText = "草薙の読み方は<ruby>くさなぎ<ruby/>草薙と言っています";
            var expectText = "草薙の読み方は|~S~|<ruby>|~E~|くさなぎ|~S~|<ruby/>|~E~|草薙と言っています";
            var regExpPattern = "<.*?>";

            string wrapperWithSpecialString = Utils.TextEvaluateWithRegExp(rawText, regExpPattern);

            Assert.AreEqual(expectText, wrapperWithSpecialString);
        }

        [TestMethod()]
        public void TextEvaluateWithRegExpTest_WildcardSecond()
        {
            var rawText = "また……会";
            var expectText = "|~S~|ま|~E~||~S~|た|~E~||~S~|…|~E~||~S~|…|~E~||~S~|会|~E~|";
            var regExpPattern = ".";

            string wrapperWithSpecialString = Utils.TextEvaluateWithRegExp(rawText, regExpPattern);

            Assert.AreEqual(expectText, wrapperWithSpecialString);
        }

        [TestMethod()]
        public void TextEvaluateWithRegExpTest_Special()
        {
            var rawText = "また……会";
            var expectText = "また……会";
            var regExpPattern = "ま|";
            // if pattern over with '|', just return text itself

            var wapperWithSpecialString = Utils.TextEvaluateWithRegExp(rawText, regExpPattern);

            Assert.AreEqual(expectText, wapperWithSpecialString);
        }
    }
}