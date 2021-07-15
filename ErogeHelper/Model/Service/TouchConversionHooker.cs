using ErogeHelper.Common;
using ErogeHelper.Model.Service.Interface;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Service
{
    public class TouchConversionHooker : ITouchConversionHooker, IDisposable
    {
        public TouchConversionHooker()
        {
            _hookCallback = HookCallback;
        }

        private readonly NativeMethods.LowLevelMouseProc _hookCallback;

        public void Init()
        {
            var moduleHandle = NativeMethods.GetModuleHandle();

            _hookId = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _hookCallback, moduleHandle, 0);
            if (_hookId == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private IntPtr _hookId = IntPtr.Zero;

        public bool Enable { get; set; }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var obj = Marshal.PtrToStructure(lParam, typeof(NativeMethods.MSLLHook));
                if (obj is null) return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);

                var info = (NativeMethods.MSLLHook)obj;

                var extraInfo = (uint)info.DwExtraInfo.ToInt64();
                if ((extraInfo & NativeMethods.MOUSEEVENTF_FROMTOUCH) == NativeMethods.MOUSEEVENTF_FROMTOUCH
                    && Enable)
                {
                    switch ((int)wParam)
                    {
                        case 0x202:
                            Task.Run(async () =>
                            {
                                var pos = NativeMethods.GetCursorPosition();
                                NativeMethods.MouseLeftDown(pos);
                                await Task.Delay(50);
                                NativeMethods.MouseLeftUp(pos);
                            });
                            break;
                        case 0x205:
                            Task.Run(async () =>
                            {
                                var pos = NativeMethods.GetCursorPosition();
                                NativeMethods.MouseRightDown(pos);
                                await Task.Delay(50);
                                NativeMethods.MouseRightUp(pos);
                            });
                            break;
                    }

                    return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
                }
            }

            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            _ = NativeMethods.UnhookWindowsHookEx(_hookId);
            GC.SuppressFinalize(this);
        }
    }
}