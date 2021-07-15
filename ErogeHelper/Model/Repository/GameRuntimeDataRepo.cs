using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ErogeHelper.Common;

namespace ErogeHelper.Model.Repository
{
    public class GameRuntimeDataRepo
    {
        public (string, Process) Init(IEnumerable<Process> gameProcesses)
        {
            GameProcesses = gameProcesses;

            MainProcess = GameProcesses.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero) ??
                           throw new InvalidOperationException();
            GamePath = MainProcess.MainModule?.FileName ?? string.Empty;
            Md5 = Utils.Md5Calculate(File.ReadAllBytes(GamePath));

            return (Md5, MainProcess);
        }

        public string Md5 { get; private set; } = string.Empty;

        public IEnumerable<Process> GameProcesses { get; private set; } = new List<Process>();

        public Process MainProcess { get; private set; } = new();

        /// <summary>
        /// For helping find game information, with <see cref="Utils.GetGameNamesByPath"/>
        /// </summary>
        public string GamePath { get; private set; } = string.Empty;
    }
}