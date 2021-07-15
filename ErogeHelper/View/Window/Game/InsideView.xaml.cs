using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ModernWpf.Controls;

namespace ErogeHelper.View.Window.Game
{
    /// <summary>
    /// Interaction logic for InsideView.xaml
    /// </summary>
    public partial class InsideView : IHandle<ViewActionMessage>, IHandle<LoseFocusMessage>
    {
        public InsideView()
        {
            InitializeComponent();

            _eventAggregator = IoC.Get<IEventAggregator>();
            _gameWindowHooker = IoC.Get<IGameWindowHooker>();
            var isLoseFocus = IoC.Get<EhDbRepository>().GetGameInfo()?.IsLoseFocus ?? false;

            _eventAggregator.SubscribeOnUIThread(this);
            Visibility = Visibility.Collapsed;
            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            AssistiveTouch.Dpi = _dpi;
            _gameWindowHooker.GamePosArea += PositionChanged;
            Loaded += (_, _) =>
            {
                Utils.HideWindowInAltTab(this);
                HandleAsync(
                    isLoseFocus ? new LoseFocusMessage { Status = true } : new LoseFocusMessage { Status = false },
                    CancellationToken.None);
            };
        }

        private readonly IEventAggregator _eventAggregator;
        private readonly IGameWindowHooker _gameWindowHooker;
        private double _dpi;

        private void PositionChanged(GameWindowPosition pos)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Height = pos.Height / _dpi;
                Width = pos.Width / _dpi;
                Left = pos.Left / _dpi;
                Top = pos.Top / _dpi;
                ClientArea.Margin = new Thickness(
                    pos.ClientArea.Left / _dpi,
                    pos.ClientArea.Top / _dpi,
                    pos.ClientArea.Right / _dpi,
                    pos.ClientArea.Bottom / _dpi);
            });
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            AssistiveTouch.Dpi = _dpi;
            Log.Debug($"Current screen dpi {_dpi * 100}%");
        }

        public Task HandleAsync(ViewActionMessage message, CancellationToken cancellationToken)
        {
            if (message.WindowType == GetType())
            {
                switch (message.Action)
                {
                    case ViewAction.Show:
                        _gameWindowHooker.InvokeLastWindowPosition();
                        Show();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            return Task.CompletedTask;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Get GameView window handle
            var interopHelper = new WindowInteropHelper(this);
            _handler = interopHelper.Handle;

            // Set fullscreen application listener
            RegisterAppBar(false);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);

            // Always make window front
            _bringToTopTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(1000), // 50
            };
            _bringToTopTimer.Tick += (_, _) => NativeMethods.BringWindowToTop(_handler);
        }

        private DispatcherTimer? _bringToTopTimer;
        private IntPtr _desktopHandle;
        private IntPtr _shellHandle;
        private readonly int _appbarMsg = NativeMethods.RegisterWindowMessage("APPBARMSG_EROGE_HELPER");
        private IntPtr _handler;

        /// <summary>
        /// Listening to ABNotify.ABN_FULLSCREENAPP message for checking game window handle
        /// </summary>
        /// <param name="registered">false to enable</param>
        private void RegisterAppBar(bool registered)
        {
            var abd = new NativeMethods.AppbarData();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = _handler;

            _desktopHandle = NativeMethods.GetDesktopWindow();
            _shellHandle = NativeMethods.GetShellWindow();
            if (!registered)
            {
                abd.uCallbackMessage = _appbarMsg;
                _ = NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_NEW, ref abd);
            }
            else
            {
                _ = NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_REMOVE, ref abd);
            }
        }

        private AppBarButton? _fullScreenButton;

        private void FullScreenButton_Loaded(object sender, RoutedEventArgs e)
        {
            var appbarButton = sender as AppBarButton;
            _fullScreenButton = appbarButton;
        }

        /// <summary>
        /// Only use for checking game fullscreen status
        /// </summary>
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != _appbarMsg)
                return IntPtr.Zero;

            switch (wParam.ToInt32())
            {
                case (int)NativeMethods.ABNotify.ABN_FULLSCREENAPP:
                    var foregroundHWnd = NativeMethods.GetForegroundWindow();
                    if (foregroundHWnd.Equals(_desktopHandle) || foregroundHWnd.Equals(_shellHandle))
                    {
                        break;
                    }
                    // XXX: these two are same
                    //if (foregroundHWnd != IoC.Get<GameRuntimeDataRepo>().MainProcess.MainWindowHandle)
                    //    break;
                    //Log.Debug(Utils.IsGameForegroundFullScreen(IoC.Get<GameRuntimeDataRepo>().MainProcess.MainWindowHandle).ToString());

                    if ((int)lParam == 1)
                    {
                        Log.Debug("The game is being maxsize");

                        if (_fullScreenButton is not null)
                        {
                            _fullScreenButton.Icon = new SymbolIcon { Symbol = Symbol.BackToWindow };
                            _fullScreenButton.ToolTip = ErogeHelper.Language.Strings.GameView_SwitchWindow;
                        }
                        _eventAggregator.PublishOnUIThreadAsync(new FullScreenChangedMessage { IsFullScreen = true });
                        _bringToTopTimer?.Start();
                    }
                    else
                    {
                        Log.Debug("The game is being normalize or minimize");
                        if (_fullScreenButton is not null)
                        {
                            _fullScreenButton.Icon = new SymbolIcon { Symbol = Symbol.FullScreen };
                            _fullScreenButton.ToolTip = ErogeHelper.Language.Strings.GameView_SwitchFullScreen;
                        }
                        _eventAggregator.PublishOnUIThreadAsync(new FullScreenChangedMessage { IsFullScreen = false });
                        _bringToTopTimer?.Stop();
                    }
                    _gameWindowHooker.ResetWindowHandler();
                    break;
            }

            return IntPtr.Zero;
        }

        public Task HandleAsync(LoseFocusMessage lostFocus, CancellationToken cancellationToken)
        {
            if (lostFocus.Status)
            {
                var exStyle = NativeMethods.GetWindowLong(_handler, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(_handler,
                    NativeMethods.GWL_EXSTYLE,
                    exStyle | NativeMethods.WS_EX_NOACTIVATE);
            }
            else
            {
                var exStyle = NativeMethods.GetWindowLong(_handler, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(_handler,
                    NativeMethods.GWL_EXSTYLE,
                    exStyle & ~NativeMethods.WS_EX_NOACTIVATE);
            }
            return Task.CompletedTask;
        }

        protected override void OnClosed(EventArgs e)
        {
            RegisterAppBar(true);
            _eventAggregator.Unsubscribe(this);
        }

        private void DanmakuCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            DanmakuInputPopup.PlacementTarget = DanmakuButton;
        }

        private void OpenDanmakuPopup(object sender, RoutedEventArgs e)
        {
            DanmakuInputPopup.IsOpen = !DanmakuInputPopup.IsOpen;
        }

        #region Moveable Text Control

        private System.Windows.Controls.Control? _control;

        const int Band = 10;
        const int BtnMinWidth = 100;
        const int BtnMinHeight = 100;
        private MousePointPosition _enumMousePointPosition;
        private Point _point; //记录鼠标上次位置;


        private MousePointPosition GetMousePointPosition(System.Windows.Controls.Control control, MouseEventArgs e)
        {
            Size size = control.RenderSize;
            Point point = e.GetPosition(control);

            Point pointCanvas = e.GetPosition(CanvasContainer);
            _point.X = pointCanvas.X;
            _point.Y = pointCanvas.Y;

            if ((point.X >= -1 * Band) | (point.X <= size.Width) | (point.Y >= -1 * Band) | (point.Y <= size.Height))
            {
                if (point.X < Band)
                {
                    if (point.Y < Band)
                    {
                        return MousePointPosition.MouseSizeTopLeft;
                    }
                    else
                    {
                        if (point.Y > -1 * Band + size.Height)
                        {
                            return MousePointPosition.MouseSizeBottomLeft;
                        }
                        else
                        {
                            return MousePointPosition.MouseSizeLeft;
                        }
                    }
                }
                else
                {
                    if (point.X > -1 * Band + size.Width)
                    {
                        if (point.Y < Band)
                        {
                            return MousePointPosition.MouseSizeTopRight;
                        }
                        else
                        {
                            if (point.Y > -1 * Band + size.Height)
                            {
                                return MousePointPosition.MouseSizeBottomRight;
                            }
                            else
                            {
                                return MousePointPosition.MouseSizeRight;
                            }
                        }
                    }
                    else
                    {
                        if (point.Y < Band)
                        {
                            return MousePointPosition.MouseSizeTop;
                        }
                        else
                        {
                            if (point.Y > -1 * Band + size.Height)
                            {
                                return MousePointPosition.MouseSizeBottom;
                            }
                            else
                            {
                                return MousePointPosition.MouseDrag;
                            }
                        }
                    }
                }
            }
            else
            {
                return MousePointPosition.MouseSizeNone;
            }
        }

        private void SetControlLocation(System.Windows.Controls.Control control, Point point)
        {
            Canvas.SetLeft(control, point.X);
            Canvas.SetTop(control, point.Y);
        }

        #endregion

        private void MoveableControl_MouseMove(object sender, MouseEventArgs e)
        {
            _control = (System.Windows.Controls.Control)sender;
            double left = Canvas.GetLeft(_control);
            double top = Canvas.GetTop(_control);
            Point point = e.GetPosition(CanvasContainer);
            double height = _control.Height;
            double width = _control.Width;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                switch (_enumMousePointPosition)
                {
                    case MousePointPosition.MouseDrag:
                        SetControlLocation(_control, new Point(left + point.X - _point.X, top + point.Y - _point.Y));
                        break;
                    case MousePointPosition.MouseSizeBottom:
                        height += point.Y - _point.Y;
                        break;
                    case MousePointPosition.MouseSizeBottomRight:
                        width += point.X - _point.X;
                        height += point.Y - _point.Y;
                        break;
                    case MousePointPosition.MouseSizeRight:
                        width += point.X - _point.X;
                        break;
                    case MousePointPosition.MouseSizeTop:
                        SetControlLocation(_control, new Point(left, top + point.Y - _point.Y));
                        height -= (point.Y - _point.Y);
                        break;
                    case MousePointPosition.MouseSizeLeft:
                        SetControlLocation(_control, new Point(left + point.X - _point.X, top));
                        width -= (point.X - _point.X);
                        break;
                    case MousePointPosition.MouseSizeBottomLeft:
                        SetControlLocation(_control, new Point(left + point.X - _point.X, top));
                        width -= (point.X - _point.X);
                        height += point.Y - _point.Y;
                        break;
                    case MousePointPosition.MouseSizeTopRight:
                        SetControlLocation(_control, new Point(left, top + point.Y - _point.Y));
                        width += (point.X - _point.X);
                        height -= (point.Y - _point.Y);
                        break;
                    case MousePointPosition.MouseSizeTopLeft:
                        SetControlLocation(_control, new Point(left + point.X - _point.X, top + point.Y - _point.Y));
                        width -= (point.X - _point.X);
                        height -= (point.Y - _point.Y);
                        break;
                    default:
                        break;
                }

                //记录光标拖动到的当前点
                _point.X = point.X;
                _point.Y = point.Y;

                if (width < BtnMinWidth) width = BtnMinWidth;
                if (height < BtnMinHeight) height = BtnMinHeight;
                _control.Width = width;
                _control.Height = height;
            }
            else
            {
                _enumMousePointPosition = GetMousePointPosition(_control, e); //'判断光标的位置状态

                switch (_enumMousePointPosition) //'改变光标
                {
                    case MousePointPosition.MouseSizeNone:
                        _control.Cursor = Cursors.Arrow;       //'箭头
                        break;
                    case MousePointPosition.MouseDrag:
                        _control.Cursor = Cursors.Arrow;     //'四方向
                        break;
                    case MousePointPosition.MouseSizeBottom:
                        _control.Cursor = Cursors.SizeNS;      //'南北
                        break;
                    case MousePointPosition.MouseSizeTop:
                        _control.Cursor = Cursors.SizeNS;      //'南北
                        break;
                    case MousePointPosition.MouseSizeLeft:
                        _control.Cursor = Cursors.SizeWE;      //'东西
                        break;
                    case MousePointPosition.MouseSizeRight:
                        _control.Cursor = Cursors.SizeWE;      //'东西
                        break;
                    case MousePointPosition.MouseSizeBottomLeft:
                        _control.Cursor = Cursors.SizeNESW;    //'东北到南西
                        break;
                    case MousePointPosition.MouseSizeBottomRight:
                        _control.Cursor = Cursors.SizeNWSE;    //'东南到西北
                        break;

                    case MousePointPosition.MouseSizeTopLeft:
                        _control.Cursor = Cursors.SizeNWSE;    //'东南到西北
                        break;
                    case MousePointPosition.MouseSizeTopRight:
                        _control.Cursor = Cursors.SizeNESW;    //'东北到南西
                        break;
                    default:
                        break;
                }
            }
        }

        private void MoveableControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_control is null)
                return;
            _enumMousePointPosition = MousePointPosition.MouseSizeNone;
            _control.Cursor = Cursors.Arrow;
        }

        // UNDONE: 只能左右移动，上下高度由控件自动确定
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
    }
}
