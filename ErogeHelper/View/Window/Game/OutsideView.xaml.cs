using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.View.Window.Game
{
    /// <summary>
    /// OutsideView.xaml 的交互逻辑
    /// </summary>
    public partial class OutsideView : IHandle<ViewActionMessage>
    {
        public OutsideView()
        {
            InitializeComponent();

            _eventAggregator = IoC.Get<IEventAggregator>();
            _gameWindowHooker = IoC.Get<IGameWindowHooker>();

            _eventAggregator.SubscribeOnUIThread(this);
            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            _gameWindowHooker.GamePosChanged += pos =>
            {
                Left += pos.HorizontalChange / _dpi;
                Top += pos.VerticalChange / _dpi;
            };
            Visibility = Visibility.Collapsed;
            Loaded += (_, _) =>
            {
                Utils.HideWindowInAltTab(this);
                //EnableBlur();
            };
        }

        private readonly IEventAggregator _eventAggregator;
        private readonly IGameWindowHooker _gameWindowHooker;
        private double _dpi;

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
        }

        private void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new NativeMethods.AccentPolicy
            {
                AccentState = NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND
            };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new NativeMethods.WindowCompositionAttributeData
            {
                Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            NativeMethods.SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        public Task HandleAsync(ViewActionMessage message, CancellationToken cancellationToken)
        {
            if (message.WindowType == GetType())
            {
                switch (message.Action)
                {
                    case ViewAction.Hide:
                        Hide();
                        break;
                    case ViewAction.Show:
                        MoveToGameCenter();
                        Show();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            return Task.CompletedTask;
        }

        private void MoveToGameCenter()
        {
            var gamePos = _gameWindowHooker.GetLastWindowPosition();
            Top = (gamePos.Top + gamePos.Height / 2) / _dpi;
            Left = (gamePos.Left + (gamePos.Width - Width) / 2) / _dpi;
        }

        protected override void OnClosed(EventArgs e) => _eventAggregator.Unsubscribe(this);

        // MouseDown="OutsideView_OnMouseDown" would cover the CardPopup 
        private void OutsideView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void ResizeGripper_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Width + e.HorizontalChange >= 100 && Width >= 100)
            {
                Width += e.HorizontalChange;
            }

            if (Height + e.VerticalChange >= 100 && Height >= 100)
            {
                Height += e.VerticalChange;
            }
        }

        // MouseDown
        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(new WindowInteropHelper(this).Handle, 0xA1, (IntPtr)0x2, (IntPtr)0);
        }
        // PreviewMouseLeftButtonDown
        private void WindowResize(object sender, MouseButtonEventArgs e)
        {
            if (PresentationSource.FromVisual((Visual)sender) is not HwndSource hWndSource)
                throw new ArgumentNullException($"hWndSource can not be null", nameof(hWndSource));
            NativeMethods.SendMessage(hWndSource.Handle, 0x112, (IntPtr)61448, IntPtr.Zero);
        }
    }
}
