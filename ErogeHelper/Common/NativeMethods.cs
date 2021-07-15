using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable InconsistentNaming XXX: allowed uppercase naming
// ReSharper disable IdentifierTypo XXX: allowed all (uppercase) naming

namespace ErogeHelper.Common
{
    public static class NativeMethods
    {
        public const long SWEH_CHILDID_SELF = 0;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int GWL_EXSTYLE = -20;
        public const int WH_MOUSE_LL = 14;
        public const uint MOUSEEVENTF_FROMTOUCH = 0xFF515700;
        public const int WM_CLIPBOARDUPDATE = 0x031D;

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        public delegate void WinEventDelegate(IntPtr hWinEventHook,
                                              SWEHEvents eventType,
                                              IntPtr hwnd,
                                              SWEH_ObjectId idObject,
                                              long idChild,
                                              uint dwEventThread,
                                              uint dwmsEventTime);

        public static IntPtr WinEventHookRange(SWEHEvents eventFrom,
                                               SWEHEvents eventTo,
                                               WinEventDelegate @delegate,
                                               uint idProcess, uint idThread)
        {
            return UnsafeNativeMethods.SetWinEventHook(eventFrom, eventTo,
                                                       IntPtr.Zero, @delegate,
                                                       idProcess, idThread,
                                                       WinEventHookInternalFlags);
        }

        public static IntPtr WinEventHookOne(SWEHEvents @event, WinEventDelegate @delegate, uint idProcess, uint idThread)
        {
            return UnsafeNativeMethods.SetWinEventHook(@event, @event,
                                                       IntPtr.Zero, @delegate,
                                                       idProcess, idThread,
                                                       WinEventHookInternalFlags);
        }

        public static bool WinEventUnhook(IntPtr hWinEventHook)
        {
            return UnsafeNativeMethods.UnhookWinEvent(hWinEventHook);
        }

        /// <summary>
        /// 返回 hWnd 窗口的线程标识
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static uint GetWindowThread(IntPtr hWnd)
        {
            return UnsafeNativeMethods.GetWindowThreadProcessId(hWnd, IntPtr.Zero);
        }

        /// <summary>
        /// 传入 out 参数，通过 hWnd 获取 PID
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="processId"></param>
        /// <returns></returns>
        public static uint GetWindowThread(IntPtr hWnd, out uint processId)
        {
            return UnsafeNativeMethods.GetWindowThreadProcessId(hWnd, out processId);
        }

        /// <summary>
        /// 返回窗口左上和右下的坐标(left, top, right, bottom)
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static Rect GetWindowRect(IntPtr hWnd)
        {
            var rect = new Rect();
            _ = SafeNativeMethods.GetWindowRect(hWnd, ref rect);
            return rect;
        }

        /// <summary>
        /// 返回窗口客户区大小, Right as Width, Bottom as Height 其余两项只会是0
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static Rect GetClientRect(IntPtr hWnd)
        {
            var rect = new Rect();
            _ = SafeNativeMethods.GetClientRect(hWnd, ref rect);
            return rect;
        }

        /// <summary>
        /// 取得前台窗口句柄函数
        /// </summary>
        /// <returns></returns>
        public static IntPtr GetForegroundWindow()
        {
            return SafeNativeMethods.GetForegroundWindow();
        }

        public static bool DestroyWindow(IntPtr hWnd)
        {
            return SafeNativeMethods.DestroyWindow(hWnd);
        }

        public static int GetWindowLong(IntPtr hWnd, int index)
        {
            return SafeNativeMethods.GetWindowLong(hWnd, index);
        }

        public static int SetWindowLong(IntPtr hWnd, int index, int newStyle)
        {
            return SafeNativeMethods.SetWindowLong(hWnd, index, newStyle);
        }

        public static bool SetForegroundWindow(IntPtr hWnd)
        {
            return SafeNativeMethods.SetForegroundWindow(hWnd);
        }

        public static bool BringWindowToTop(IntPtr hWnd)
        {
            return SafeNativeMethods.BringWindowToTop(hWnd);
        }

        public static IntPtr GetWindow(IntPtr parentHWnd, GW uCmd)
        {
            return SafeNativeMethods.GetWindow(parentHWnd, uCmd);
        }

        public static int GetWindowText(IntPtr hWnd, StringBuilder title, int length)
        {
            return SafeNativeMethods.GetWindowText(hWnd, title, length);
        }

        public static void SwitchToThisWindow(IntPtr hWnd, bool fAltTab = true)
        {
            SafeNativeMethods.SwitchToThisWindow(hWnd, fAltTab);
        }

        /// <summary>
        /// 取得桌面窗口句柄函数
        /// </summary>
        /// <returns></returns>
        public static IntPtr GetDesktopWindow()
        {
            return SafeNativeMethods.GetDesktopWindow();
        }

        /// <summary>
        /// 取得Shell窗口句柄函数
        /// </summary>
        /// <returns></returns>
        public static IntPtr GetShellWindow()
        {
            return SafeNativeMethods.GetShellWindow();
        }

        public static int RegisterWindowMessage(string msg)
        {
            return SafeNativeMethods.RegisterWindowMessage(msg);
        }

        public static uint SHAppBarMessage(int dwMessage, ref AppbarData pData)
        {
            // ExecutionEngineException???
            return SafeNativeMethods.SHAppBarMessage(dwMessage, ref pData);
        }

        public static IntPtr GetModuleHandle()
        {
            return UnsafeNativeMethods.GetModuleHandle(null);
        }

        public static IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod,
            uint dwThreadId)
        {
            return UnsafeNativeMethods.SetWindowsHookEx(idHook, lpfn, hMod, dwThreadId);
        }

        public static IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam)
        {
            return UnsafeNativeMethods.CallNextHookEx(hhk, nCode, wParam, lParam);
        }

        public static bool UnhookWindowsHookEx(IntPtr hhk)
        {
            return UnsafeNativeMethods.UnhookWindowsHookEx(hhk);
        }

        public static void MoveCursorToPoint(int x, int y)
        {
            SafeNativeMethods.SetCursorPos(x, y);
        }

        public static IntPtr GetMessageExtraInfo()
        {
            return SafeNativeMethods.GetMessageExtraInfo();
        }

        public static bool PostMessage(IntPtr hWnd, WMessages Msg, int wParam, int lParam)
        {
            return SafeNativeMethods.PostMessage((int)hWnd, (uint)Msg, wParam, lParam);
        }

        public static void MouseEventClick()
        {
            SafeNativeMethods.MouseEvent(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static void MouseEventRight()
        {
            SafeNativeMethods.MouseEvent(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        // https://stackoverflow.com/questions/2416748/how-do-you-simulate-mouse-click-in-c
        public static MousePoint GetCursorPosition()
        {
            var gotPoint = SafeNativeMethods.GetCursorPos(out var currentMousePoint);
            if (!gotPoint)
            {
                currentMousePoint = new MousePoint(0, 0);
            }
            return currentMousePoint;
        }

        public static void MouseLeftDown(MousePoint point)
        {
            SafeNativeMethods.MouseEvent(MOUSEEVENTF_LEFTDOWN, point.X, point.Y, 0, 0);
        }
        public static void MouseLeftUp(MousePoint point)
        {
            SafeNativeMethods.MouseEvent(MOUSEEVENTF_LEFTUP, point.X, point.Y, 0, 0);
        }
        public static void MouseRightDown(MousePoint point)
        {
            SafeNativeMethods.MouseEvent(MOUSEEVENTF_RIGHTDOWN, point.X, point.Y, 0, 0);
        }
        public static void MouseRightUp(MousePoint point)
        {
            SafeNativeMethods.MouseEvent(MOUSEEVENTF_RIGHTUP, point.X, point.Y, 0, 0);
        }

        public static bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam)
        {
            return UnsafeNativeMethods.EnumChildWindows(parentHandle, callback, lParam);
        }

        public static int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data)
        {
            return SafeNativeMethods.SetWindowCompositionAttribute(hwnd, ref data);
        }

        public static IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            return SafeNativeMethods.SendMessage(hWnd, msg, wParam, lParam);
        }

        public static bool ReleaseCapture()
        {
            return SafeNativeMethods.ReleaseCapture();
        }

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private static readonly SWEH_dwFlags WinEventHookInternalFlags = SWEH_dwFlags.WINEVENT_OUTOFCONTEXT |
                                                                         SWEH_dwFlags.WINEVENT_SKIPOWNPROCESS |
                                                                         SWEH_dwFlags.WINEVENT_SKIPOWNTHREAD;

        // https://www.jetbrains.com/help/resharper/MemberHidesStaticFromOuterClass.html
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetWindowRect(IntPtr hWnd, ref Rect lpRect);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetClientRect(IntPtr hWnd, ref Rect lpRect);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetActiveWindow();

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int pid);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
            public static extern bool DestroyWindow(IntPtr hWnd);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int index);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int SetWindowLong(IntPtr hWnd, int index, int newStyle);

            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern bool BringWindowToTop(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindow(IntPtr parentHWnd, GW uCmd);

            [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport("user32.dll")]
            public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

            [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
            public static extern void KeybdEvent(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

            [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
            public static extern uint SHAppBarMessage(int dwMessage, ref AppbarData pData);

            [DllImport("User32.dll", CharSet = CharSet.Unicode)]
            public static extern int RegisterWindowMessage(string msg);

            [DllImport("user32.dll")]
            public static extern IntPtr GetShellWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern bool SetCursorPos(int X, int Y);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

            [DllImport("user32.dll")]
            public static extern IntPtr GetMessageExtraInfo();

            [DllImport("user32.dll", EntryPoint = "mouse_event", CharSet = CharSet.Auto,
                CallingConvention = CallingConvention.StdCall)]
            public static extern void MouseEvent(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool PostMessage(int hWnd, uint Msg, int wParam, int lParam);

            [DllImport("user32.dll")]
            public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetCursorPos(out MousePoint lpMousePoint);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern bool ReleaseCapture();
        }

        [SuppressUnmanagedCodeSecurity]
        private static class UnsafeNativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            [DllImport("user32.dll")]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr voidProcessId);

            [DllImport("user32.dll", SetLastError = false)]
            public static extern IntPtr SetWinEventHook(SWEHEvents eventMin, SWEHEvents eventMax,
                                                        IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc,
                                                        uint idProcess, uint idThread, SWEH_dwFlags dwFlags);

            [DllImport("user32.dll", SetLastError = false)]
            public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod,
                uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string? lpModuleName);

            [DllImport("user32.Dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public enum AccentState
        {
            ACCENT_DISABLED = 1,
            ACCENT_ENABLE_GRADIENT = 0,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        public enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        public enum WMessages : uint
        {
            WM_LBUTTONDOWN = 0x201,
            WM_LBUTTONUP = 0x202,

            WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101,

            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14,
        }

        [Flags]
        public enum MouseEventF
        {
            Absolute = 0x8000,
            HWheel = 0x01000,
            Move = 0x0001,
            MoveNoCoalesce = 0x2000,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            VirtualDesk = 0x4000,
            Wheel = 0x0800,
            XDown = 0x0080,
            XUp = 0x0100
        }

        [Flags]
        public enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        public struct Input
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public HardwareInput hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int Dx;
            public int Dy;
            public uint MouseData;
            public uint DwFlags;
            public uint Time;
            public IntPtr DwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHook
        {
            public Point Point;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr DwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public override string ToString() => $"{Left} {Top} {Right} {Bottom}";
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AppbarData
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public Rect rc;
            public IntPtr lParam;
        }

        public enum SC : uint
        {
            MAXIMIZE = 0xF030,
            MINIMIZE = 0xF020,
            RESTORE = 0xF120,
        }

        public enum WM : uint
        {
            INACTIVE = 0x0000,
            SETFOCUS = 0x0007,
            KILLFOCUS = 0x0008,
            ACTIVATE = 0x0006,

            SYSCOMMAND = 0x0112,
        }

        public enum ABMsg : int
        {
            ABM_NEW = 0,
            ABM_REMOVE,
            ABM_QUERYPOS,
            ABM_SETPOS,
            ABM_GETSTATE,
            ABM_GETTASKBARPOS,
            ABM_ACTIVATE,
            ABM_GETAUTOHIDEBAR,
            ABM_SETAUTOHIDEBAR,
            ABM_WINDOWPOSCHANGED,
            ABM_SETSTATE
        }

        public enum ABNotify : int
        {
            ABN_STATECHANGE = 0,
            ABN_POSCHANGED,
            ABN_FULLSCREENAPP,
            ABN_WINDOWARRANGE
        }

        public enum ABEdge : int
        {
            ABE_LEFT = 0,
            ABE_TOP,
            ABE_RIGHT,
            ABE_BOTTOM
        }

        /// <summary>
        /// <see cref="UnsafeNativeMethods.SetWinEventHook"/> flags
        /// </summary>
        public enum SWEH_dwFlags : uint
        {
            WINEVENT_OUTOFCONTEXT = 0x0000,     // Events are ASYNC
            WINEVENT_SKIPOWNTHREAD = 0x0001,    // Don't call back for events on installer's thread
            WINEVENT_SKIPOWNPROCESS = 0x0002,   // Don't call back for events on installer's process
            WINEVENT_INCONTEXT = 0x0004         // Events are SYNC, this causes your dll to be injected into every process
        }

        /// <summary>
        /// <see cref="UnsafeNativeMethods.SetWinEventHook"/> Object Ids
        /// </summary>
        public enum SWEH_ObjectId : long
        {
            OBJID_WINDOW = 0x00000000,
            OBJID_SYSMENU = 0xFFFFFFFF,
            OBJID_TITLEBAR = 0xFFFFFFFE,
            OBJID_MENU = 0xFFFFFFFD,
            OBJID_CLIENT = 0xFFFFFFFC,
            OBJID_VSCROLL = 0xFFFFFFFB,
            OBJID_HSCROLL = 0xFFFFFFFA,
            OBJID_SIZEGRIP = 0xFFFFFFF9,
            OBJID_CARET = 0xFFFFFFF8,
            OBJID_CURSOR = 0xFFFFFFF7,
            OBJID_ALERT = 0xFFFFFFF6,
            OBJID_SOUND = 0xFFFFFFF5,
            OBJID_QUERYCLASSNAMEIDX = 0xFFFFFFF4,
            OBJID_NATIVEOM = 0xFFFFFFF0
        }

        /// <summary>
        /// <see cref="UnsafeNativeMethods.SetWinEventHook"/> events
        /// </summary>
        public enum SWEHEvents : uint
        {
            EventMin = 0x00000001,
            EventMax = 0x7FFFFFFF,
            EventSystemSound = EventMin,
            EventSystemAlert = 0x0002,
            EventSystemForeground = 0x0003,
            EventSystemMenuStart = 0x0004,
            EventSystemMenuEnd = 0x0005,
            EventSystemMenuPopupStart = 0x0006,
            EventSystemMenuPopupEnd = 0x0007,
            EventSystemCaptureStart = 0x0008,
            EventSystemCaptureEnd = 0x0009,
            EventSystemMoveSizeStart = 0x000A,
            EventSystemMoveSizeEnd = 0x000B,
            EventSystemContextHelpStart = 0x000C,
            EventSystemContextHelpEnd = 0x000D,
            EventSystemDragDropStart = 0x000E,
            EventSystemDragDropEnd = 0x000F,
            EventSystemDialogStart = 0x0010,
            EventSystemDialogEnd = 0x0011,
            EventSystemScrollingStart = 0x0012,
            EventSystemScrollingEnd = 0x0013,
            EventSystemSwitchStart = 0x0014,
            EventSystemSwitchEnd = 0x0015,
            EventSystemMinimizeStart = 0x0016,
            EventSystemMinimizeEnd = 0x0017,
            EventSystemDesktopSwitch = 0x0020,
            EventSystemEnd = 0x00FF,
            EventOemDefinedStart = 0x0101,
            EventOemDefinedEnd = 0x01FF,
            EventUiaEventIdStart = 0x4E00,
            EventUiaEventIdEnd = 0x4EFF,
            EventUiaPropIdStart = 0x7500,
            EventUiaPropIdEnd = 0x75FF,
            EventConsoleCaret = 0x4001,
            EventConsoleUpdateRegion = 0x4002,
            EventConsoleUpdateSimple = 0x4003,
            EventConsoleUpdateScroll = 0x4004,
            EventConsoleLayout = 0x4005,
            EventConsoleStartApplication = 0x4006,
            EventConsoleEndApplication = 0x4007,
            EventConsoleEnd = 0x40FF,
            EventObjectCreate = 0x8000,               // hWnd ID idChild is created item
            EventObjectDestroy = 0x8001,              // hWnd ID idChild is destroyed item
            EventObjectShow = 0x8002,                 // hWnd ID idChild is shown item
            EventObjectHide = 0x8003,                 // hWnd ID idChild is hidden item
            EventObjectReorder = 0x8004,              // hWnd ID idChild is parent of z-ordering children
            EventObjectFocus = 0x8005,                // * hWnd ID idChild is focused item
            EventObjectSelection = 0x8006,            // hWnd ID idChild is selected item (if only one), or idChild is OBJID_WINDOW if complex
            EventObjectSelectionAdd = 0x8007,         // hWnd ID idChild is item added
            EventObjectSelectionRemove = 0x8008,      // hWnd ID idChild is item removed
            EventObjectSelectionWithin = 0x8009,      // hWnd ID idChild is parent of changed selected items
            EventObjectStateChange = 0x800A,          // hWnd ID idChild is item w/ state change
            EventObjectLocationChange = 0x800B,       // * hWnd ID idChild is moved/sized item
            EventObjectNameChange = 0x800C,           // hWnd ID idChild is item w/ name change
            EventObjectDescriptionChange = 0x800D,    // hWnd ID idChild is item w/ desc change
            EventObjectValueChange = 0x800E,          // hWnd ID idChild is item w/ value change
            EventObjectParentChange = 0x800F,         // hWnd ID idChild is item w/ new parent
            EventObjectHelpChange = 0x8010,           // hWnd ID idChild is item w/ help change
            EventObjectDefactionChange = 0x8011,      // hWnd ID idChild is item w/ def action change
            EventObjectAcceleratorChange = 0x8012,    // hWnd ID idChild is item w/ keybd accel change
            EventObjectInvoked = 0x8013,              // hWnd ID idChild is item invoked
            EventObjectTextSelectionChanged = 0x8014, // hWnd ID idChild is item w? test selection change
            EventObjectContentScrolled = 0x8015,
            EventSystemArrangementPreview = 0x8016,
            EventObjectEnd = 0x80FF,
            EventAiaStart = 0xA000,
            EventAiaEnd = 0xAFFF
        }

        /// <summary>
        /// uCmd optional values
        /// </summary>
        public enum GW : uint
        {
            /// <summary>
            /// 同级别第一个
            /// </summary>
            HWNDFIRST = 0,
            /// <summary>
            /// 同级别最后一个
            /// </summary>
            HWNDLAST = 1,
            /// <summary>
            /// 同级别下一个
            /// </summary>
            HWNDNEXT = 2,
            /// <summary>
            /// 同级别上一个
            /// </summary>
            HWNDPREV = 3,
            /// <summary>
            /// 属主窗口
            /// </summary>
            OWNER = 4,
            /// <summary>
            /// 子窗口
            /// </summary>
            CHILD = 5,
        }
    }
}