using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Function;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.Model.Service
{
    public class TextractorService : ITextractorService
    {
        public event Action<HookParam>? DataEvent;
        public event Action<HookParam>? SelectedDataEvent;

        public TextractorSetting Setting { get; set; } = new();

        public void InjectProcesses(IEnumerable<Process> gameProcesses, TextractorSetting? setting = null)
        {
            _gameProcesses = gameProcesses.ToList();
            Setting = setting ?? new TextractorSetting();

            // Current texthook.dll version 4.15
            var textractorPath = Directory.GetCurrentDirectory() + @"\libs\texthost.dll";
            if (!File.Exists(textractorPath))
                throw new FileNotFoundException(textractorPath);

            _createThread = CreateThreadHandle;
            _output = OutputHandle;
            _removeThread = RemoveThreadHandle;
            _callback = OnConnectCallBackHandle;

            _ = TextHostDll.TextHostInit(_callback, _ => { }, _createThread, _removeThread, _output);

            foreach (Process p in _gameProcesses)
            {
                _ = TextHostDll.InjectProcess((uint)p.Id);
                Log.Info($"attach to PID {p.Id}.");
            }
        }

        private List<Process> _gameProcesses = new(2);

        public void InsertHook(string hookcode)
        {
            if (hookcode.StartsWith('/'))
                hookcode = hookcode[1..];

            string engineName;
            if (hookcode.StartsWith('R'))
            {
                // engineName = "READ";
                if (_threadHandleDict.Any(hcodeItem => hookcode.Equals(hcodeItem.Value.Hookcode)))
                {
                    DataEvent?.Invoke(new HookParam
                    {
                        Handle = 0,
                        Pid = 0,
                        Address = -1,
                        Ctx = -1,
                        Ctx2 = -1,
                        Name = "Console",
                        Hookcode = "HB0@0",
                        Text = "ErogeHelper: The Read-Code has already insert"
                    });
                    return;
                }
            }
            else
            {
                // HCode
                engineName = hookcode[(hookcode.LastIndexOf(':') + 1)..];

                // 重复插入相同的code(可能)会导致产生很高位的Context
                if (_threadHandleDict.Any(hcodeItem =>
                    engineName.Equals(hcodeItem.Value.Name) || hookcode.Equals(hcodeItem.Value.Hookcode)))
                {
                    DataEvent?.Invoke(new HookParam
                    {
                        Handle = 0,
                        Pid = 0,
                        Address = -1,
                        Ctx = -1,
                        Ctx2 = -1,
                        Name = "Console",
                        Hookcode = "HB0@0",
                        Text = "ErogeHelper: The Hook-Code has already insert"
                    });
                    return;
                }
            }

            foreach (Process p in _gameProcesses)
            {
                _ = TextHostDll.InsertHook((uint)p.Id, hookcode);
                Log.Info($"Try insert hook {hookcode} to PID {p.Id}");
            }
        }

        public void SearchRCode(string text)
        {
            foreach (Process p in _gameProcesses)
            {
                _ = TextHostDll.SearchForText((uint)p.Id, text, 932);
            }
        }

        public IEnumerable<string> GetConsoleOutputInfo() => _consoleOutput;

        public async Task ReAttachProcesses()
        {
            firstTimeInject = false;
            // Cause console thread only create once after TextHostInit();
            var consoleParam = _threadHandleDict.First().Value;
            _threadHandleDict.Clear();
            _threadHandleDict.Add(0, consoleParam);
            // May return `invalid process`
            _gameProcesses.ForEach(proc => _ = TextHostDll.DetachProcess((uint)proc.Id));
            DataEvent?.Invoke(new HookParam
            {
                Handle = 0,
                Pid = 0,
                Address = -1,
                Ctx = -1,
                Ctx2 = -1,
                Name = "Console",
                Hookcode = "HB0@0",
                Text = "ErogeHelper: Detach Processes " + string.Join(',', _gameProcesses.Select(proc => proc.Id))
            });

            // XXX: Too fast
            await Task.Run(async () =>
            {
                await Task.Delay(50);
                _gameProcesses.ForEach(proc => _ = TextHostDll.InjectProcess((uint)proc.Id));
            });
        }

        public void RemoveHook(long address) =>
            _gameProcesses.ForEach(proc => _ = TextHostDll.RemoveHook((uint)proc.Id, (ulong)address));

        public void RemoveUselessHooks()
        {
            // FIXME: Unload hooks when submit
            //_gameProcesses.ForEach(proc =>
            //{ 
            //_threadHandleDict.Values
            //    .Where(thread => !thread.Hookcode.Equals(Setting.Hookcode) && thread.Address != -1)
            //    .Select(thread => thread.Address)
            //    .Distinct().ToList()
            //    .ForEach(address => _ = TextHostDll.RemoveHook((uint)proc.Id, (ulong)address));
            //});
        }

        private readonly List<string> _consoleOutput = new();

        #region TextHost Callback Implement

        private TextHostDll.OnOutputText? _output;
        private TextHostDll.ProcessCallback? _callback;
        private TextHostDll.OnCreateThread? _createThread;
        private TextHostDll.OnRemoveThread? _removeThread;

        private readonly Dictionary<long, HookParam> _threadHandleDict = new();

        private bool firstTimeInject = true;

        private void CreateThreadHandle(
            long threadId,
            uint processId,
            ulong address,
            ulong context,
            ulong subContext,
            string name,
            string hookcode)
        {
            if (Setting.Hookcode != string.Empty && !Setting.Hookcode.Equals(hookcode) && firstTimeInject)
            {
                _gameProcesses.ForEach(proc =>
                    _ = TextHostDll.RemoveHook((uint)proc.Id, address));
            }

            _threadHandleDict[threadId] = new HookParam
            {
                Handle = threadId,
                Pid = processId,
                Address = (long)address,
                Ctx = (long)context,
                Ctx2 = (long)subContext,
                Name = name,
                Hookcode = hookcode
            };
        }

        private void OutputHandle(long threadId, string opData, uint length)
        {
            if (length > 500)
                return;

            HookParam hp = _threadHandleDict[threadId];
            hp.Text = opData;

            DataEvent?.Invoke(hp);

            if (threadId == 0)
            {
                _consoleOutput.Add(Common.Utils.ConsoleI18N(hp.Text));
                return;
            }

            foreach (var hookSetting in Setting.HookSettings)
            {
                if (Setting.Hookcode.Equals(hp.Hookcode)
                    && (hookSetting.ThreadContext & 0xFFFF) == (hp.Ctx & 0xFFFF)
                    && hookSetting.SubThreadContext == hp.Ctx2)
                {
                    Log.Debug(hp.Text);
                    SelectedDataEvent?.Invoke(hp);
                }
                // XXX: hp.Name `Search` `Read` is different
                else if (Setting.Hookcode.StartsWith('R')
                         && hp.Name.Equals("READ"))
                {
                    Log.Debug(hp.Text);
                    SelectedDataEvent?.Invoke(hp);
                }
            }
        }

        private void RemoveThreadHandle(long threadId) { }

        // This called when console thread created
        private void OnConnectCallBackHandle(uint processId)
        {
            if (Setting.IsUserHook)
            {
                InsertHook(Setting.Hookcode);
            }
        }
        #endregion
    }
}
