using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests.Model.Service
{
    [TestClass]
    public class GameWindowHookerTests
    {
        private readonly AutoResetEvent _single = new(false);

        [TestMethod]
        // NOTE: Cause the service is depending on System.Windows.Application, this test may less meaningful
        public async Task WindowHookerTest()
        {
            // Arrange
            var notepad = Process.Start("notepad");
            var posCollect = new List<GameWindowPosition>();
            IGameWindowHooker hooker = new GameWindowHooker();

            // Act
            hooker.GamePosArea += pos =>
            {
                posCollect.Add(pos);
                if (pos == GameWindowHooker.HiddenPos)
                {
                    _single.Set();
                }
            };
            // Depend on MainProcess 
            await hooker.SetGameWindowHookAsync(notepad, new List<Process> { notepad });
            // Depend on GameProcesses
            //await hooker.ResetWindowHandler();

            // Assert
            Assert.AreEqual((0, 0), (posCollect[0].Left, posCollect[0].Top));
            notepad.Kill();
            _single.WaitOne();
            Assert.AreEqual((-32000, -32000), (posCollect[^1].Left, posCollect[^1].Top));
        }
    }
}