using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.Model.Service
{
    public class GameWindowHooker : IGameWindowHooker
    {
        public event Action<GameWindowPosition>? GamePosArea;

        public event Action<GameWindowPositionChanged>? GamePosChanged;

        public event Action<WindowSize>? NewWindowSize;

        public async Task SetGameWindowHookAsync(Process gameProcess, List<Process> gameProcesses)
        {
            _gameProcList = gameProcesses;
            _gameProc = gameProcess;
            _gameHWnd = _gameProc.MainWindowHandle;
            await Task.Run(() =>
            {
                _winEventDelegate = WinEventCallback;
                _gcSafetyHandle = GCHandle.Alloc(_winEventDelegate);

                // Register game exit event
                _gameProc.EnableRaisingEvents = true;
                _gameProc.Exited += ApplicationExit;

                ResetWindowHandler();
                // For the first time. If MainHandle is okay set it directly
                if (_gameHWnd == _gameProc.MainWindowHandle)
                {
                    SetWindowHandler();
                }
            });
        }

        public void InvokeLastWindowPosition()
        {
            GamePosArea?.Invoke(_lastPos);
            Log.Debug(_lastPos.ToString());
        }

        public GameWindowPosition GetLastWindowPosition() => _lastPos;
        public WindowSize GetLastWindowSize() => new(_oldWidth, _oldHeight);

        /// <summary>
        /// Check and reset or do nothing
        /// </summary>
        public void ResetWindowHandler()
        {
            // First, get info from `GameProcess.MainWindowHandle` or last handle
            var clientRect = NativeMethods.GetClientRect(_gameHWnd);
            var realHandle = IntPtr.Zero;

            // InsideView命中的客户区窗口范围很小，说明不是正常的游戏窗口
            if (400 > clientRect.Bottom && 400 > clientRect.Right)
            {
                // NOTE: This would active even when game window get minimize
                var windowRect = NativeMethods.GetWindowRect(_gameHWnd);
                if ($"{windowRect.Left} {windowRect.Top} {windowRect.Right} {windowRect.Bottom}"
                    .Equals(MinimizedPosition))
                {
                    // Ignore minimized window situation
                    return;
                }

                // Start search handle
                realHandle = FindRealHandle();
            }

            if (realHandle != IntPtr.Zero)
            {
                // New handle found
                _gameHWnd = realHandle;
                SetWindowHandler();
            }
            else
            {
                // gameHWnd still be last handle
                UpdateLocation();
            }
        }

        private NativeMethods.WinEventDelegate? _winEventDelegate;
        private GCHandle _gcSafetyHandle;
        private IntPtr _hWinEventHook = IntPtr.Zero;
        private Process _gameProc = new();
        private List<Process> _gameProcList = new();
        private IntPtr _gameHWnd;
        private const string MinimizedPosition = "-32000 -32000 -31840 -31972";
        public static readonly GameWindowPosition HiddenPos = new()
        {
            ClientArea = new Thickness(),
            Left = -32000,
            Top = -32000,
            Height = 0,
            Width = 0,
        };

        private void SetWindowHandler()
        {
            Log.Info($"Set handle to 0x{Convert.ToString(_gameHWnd.ToInt64(), 16).ToUpper()} " +
                     $"Title: {_gameProc.MainWindowTitle}");
            var targetThreadId = NativeMethods.GetWindowThread(_gameHWnd);

            // NOTE: Application.Current made tightly coupled to wpf System.Windows.Application and hard to test
            // 如果使用await等待会导致GameWindowHooker一连串async变得繁琐起来..
            // 调用 SetWinEventHook 传入 WinEventDelegate 回调函数，必须在UI线程上执行启用
            Dispatcher.InvokeAsync(
                () => _hWinEventHook = NativeMethods.WinEventHookOne(
                    NativeMethods.SWEHEvents.EventObjectLocationChange,
                    _winEventDelegate!,
                    (uint)_gameProc.Id,
                    targetThreadId)
            );

            // Send game position first time
            UpdateLocation();
        }

        private void WinEventCallback(IntPtr hWinEventHook,
            NativeMethods.SWEHEvents eventType,
            IntPtr hWnd,
            NativeMethods.SWEH_ObjectId idObject,
            long idChild,
            uint dwEventThread,
            uint dwmsEventTime)
        {
            // 游戏窗口获取焦点时会调用
            //if (hWnd == GameInfoTable.Instance.hWnd &&
            //    eventType == Hook.SWEH_Events.EVENT_OBJECT_FOCUS)
            //{
            //    log.Info("Game window get focus");
            //}

            // Update game's position information
            if (hWnd == _gameHWnd &&
                eventType == NativeMethods.SWEHEvents.EventObjectLocationChange &&
                idObject == NativeMethods.SWEH_CHILDID_SELF)
            {
                UpdateLocation();
            }
        }

        private GameWindowPosition _lastPos = HiddenPos;

        private int _oldWidth = -1;
        private int _oldHeight = -1;

        private void UpdateLocation()
        {
            var rect = NativeMethods.GetWindowRect(_gameHWnd);
            var rectClient = NativeMethods.GetClientRect(_gameHWnd);

            var width = rect.Right - rect.Left;  // equal rectClient.Right + shadow*2
            var height = rect.Bottom - rect.Top; // equal rectClient.Bottom + shadow + title

            var winShadow = (width - rectClient.Right) / 2;

            var wholeHeight = rect.Bottom - rect.Top;
            var winTitleHeight = wholeHeight - rectClient.Bottom - winShadow;

            var clientArea = new Thickness(winShadow, winTitleHeight, winShadow, winShadow);

            GamePosChanged?.Invoke(new GameWindowPositionChanged(rect.Left - _lastPos.Left, rect.Top - _lastPos.Top));

            _lastPos = new GameWindowPosition
            {
                Height = height,
                Width = width,
                Left = rect.Left,
                Top = rect.Top,
                ClientArea = clientArea
            };
            GamePosArea?.Invoke(_lastPos);

            #region Change FloatButton Position
            // init
            if (_oldWidth == -1 && _oldHeight == -1)
            {
                _oldWidth = rectClient.Right;
                _oldHeight = rectClient.Bottom;
            }
            else if (_oldHeight != rectClient.Bottom || _oldWidth != rectClient.Right)
            {
                NewWindowSize?.Invoke(new WindowSize(rectClient.Right, rectClient.Bottom));
                _oldWidth = rectClient.Right;
                _oldHeight = rectClient.Bottom;
            }
            #endregion
        }

        private IntPtr FindRealHandle()
        {
            List<IntPtr> handleList = new();
            foreach (var process in _gameProcList)
            {
                handleList.AddRange(GetRootWindowsOfProcess(process.Id));
            }
            Log.Debug($"{handleList.Count} handles found");

            foreach (var handle in handleList)
            {
                var clientRect = NativeMethods.GetClientRect(handle);
                if (clientRect.Bottom > 400 && clientRect.Right > 400)
                {
                    return handle;
                }
            }
            Log.Info("Find failed, use last handle");
            return IntPtr.Zero;
        }

        private static IEnumerable<IntPtr> GetRootWindowsOfProcess(int pid)
        {
            IEnumerable<IntPtr> rootWindows = GetChildWindows(IntPtr.Zero);
            List<IntPtr> dsProcRootWindows = new();
            foreach (var hWnd in rootWindows)
            {
                NativeMethods.GetWindowThread(hWnd, out var lpdwProcessId);
                if (lpdwProcessId == pid)
                    dsProcRootWindows.Add(hWnd);
            }
            return dsProcRootWindows;
        }

        private static IEnumerable<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new();
            var listHandle = GCHandle.Alloc(result);
            try
            {
                NativeMethods.Win32Callback childProc = EnumWindow;
                NativeMethods.EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            var gch = GCHandle.FromIntPtr(pointer);
            if (gch.Target is not List<IntPtr> list)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        public static event Action<object>? ExitAction;

        private void ApplicationExit(object? sender, EventArgs e)
        {
            ExitAction?.Invoke(new object());
            Log.Info("Detected game quit event");
            GamePosArea?.Invoke(HiddenPos);
            _gcSafetyHandle.Free();
            NativeMethods.WinEventUnhook(_hWinEventHook);

            Dispatcher.InvokeShutdown();
        }

        // For running in the unit test
        private static Dispatcher Dispatcher => Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
    }
}