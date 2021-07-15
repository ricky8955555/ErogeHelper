using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.View.Control
{
    /// <summary>
    /// AssistiveTouch.xaml 的交互逻辑
    /// </summary>
    public partial class AssistiveTouch
    {
        public new event EventHandler? ClickEvent;
        private FrameworkElement _parent = null!;

        private bool _move;

        private const double OpacityValue = 0.4;
        private const double OpacityNormal = 1;
        private const double ButtonSpace = 2;

        // positioning button absorb field
        /// <summary>
        /// The diameter of button
        /// </summary>
        private double _distance;
        private double _halfDistance;
        private double _oneThirdDistance;
        private double _twoThirdDistance;

        private Point _lastPos;

        /// <summary>
        /// The position of the button after the left mouse button is released
        /// </summary>
        private Point _newPos;
        /// <summary>
        /// The position of the button
        /// </summary>
        private Point _oldPos;

        private bool _isFromUpdateButtonPosEvent;
        private int _newGameViewHeight;
        private int _newGameViewWidth;

        public double Dpi { private get; set; } = 1;

        public AssistiveTouch()
        {
            InitializeComponent();

            GameWindowHooker.ExitAction += _ =>
            {
                IoC.Get<EhConfigRepository>().AssistiveTouchPostion =
                    JsonSerializer.Serialize(_touchPosMemory);
            };
            IoC.Get<IGameWindowHooker>().NewWindowSize += (windowSize) =>
            {
                _move = true;
                _isFromUpdateButtonPosEvent = true;
                _newGameViewHeight = (int)(windowSize.Height / Dpi);
                _newGameViewWidth = (int)(windowSize.Width / Dpi);
                ResetButtonPostion();
                RaiseMouseUpEventInCode();
            };
        }

        private void RaiseMouseUpEventInCode()
        {
            var timestamp = new TimeSpan(DateTime.Now.Ticks).Milliseconds;

            var mouseUpEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, timestamp, MouseButton.Left)
            {
                RoutedEvent = PreviewMouseUpEvent,
                Source = this,
            };

            RaiseEvent(mouseUpEvent);
        }

        private void RegisterParentPreviewEvent()
        {
            _parent.PreviewMouseMove += (_, mouseEvent) =>
            {
                if (!_move)
                    return;

                // 获取相对于左上角的坐标
                var pos = mouseEvent.GetPosition(_parent);
                // 相对左上坐标 + 新位置与旧位置的差值 = 新坐标
                var left = Margin.Left + pos.X - _lastPos.X;
                var top = Margin.Top + pos.Y - _lastPos.Y;
                // 依靠Margin来实现移动的效果
                Margin = new Thickness(left, top, 0, 0);

                _lastPos = pos;

                // 拖拽时超出边框边缘直接释放
                if (left < -_oneThirdDistance || top < -_oneThirdDistance ||
                    left > _parent.ActualWidth - _twoThirdDistance || top > _parent.ActualHeight - _twoThirdDistance)
                {
                    RaiseMouseUpEventInCode();
                }
            };

            _parent.PreviewMouseUp += (_, mouseEvent) =>
            {
                if (!_move)
                    return;

                double parentActualWidth;
                double parentActualHeight;
                double curMarginLeft;
                double curMarginTop;

                if (_isFromUpdateButtonPosEvent)
                {
                    parentActualWidth = _newGameViewWidth;
                    parentActualHeight = _newGameViewHeight;
                    curMarginLeft = _immediateMargin.Left;
                    curMarginTop = _immediateMargin.Top;

                    _isFromUpdateButtonPosEvent = false;
                }
                else
                {
                    var pos = mouseEvent.GetPosition(_parent);
                    _newPos = pos;
                    parentActualHeight = _parent.ActualHeight;
                    parentActualWidth = _parent.ActualWidth;
                    curMarginLeft = Margin.Left;
                    curMarginTop = Margin.Top;
                }

                var left = curMarginLeft + _newPos.X - _lastPos.X;
                var top = curMarginTop + _newPos.Y - _lastPos.Y;
                // button 距离右边缘距离
                var right = parentActualWidth - left - ActualWidth;
                // button 距离下边缘距离
                var bottom = parentActualHeight - top - ActualHeight;
                // FIXME: Button release position not really middle
                var verticalMiddleLine = parentActualHeight - ActualHeight - ButtonSpace;

                //Log.Info($"鼠标位置 {_newPos.X} {_newPos.Y}");
                //Log.Info($"释放点与四边距离 {left} {top} {right} {bottom}");

                // 根据button所处屏幕位置来确定button之后应该动画移动到的位置
                if (left < _halfDistance && top < _twoThirdDistance) // button 距离左上角边距同时小于 distance
                {
                    left = ButtonSpace;
                    top = ButtonSpace;
                    _touchPosMemory = new AssistiveTouchPosition(TouchButtonCorner.UpperLeft);
                }
                else if (left < _halfDistance && bottom < _twoThirdDistance) // 左下
                {
                    left = ButtonSpace;
                    top = parentActualHeight - ActualHeight - ButtonSpace * 2;
                    _touchPosMemory = new AssistiveTouchPosition(TouchButtonCorner.LowerLeft);
                }
                else if (right < _halfDistance && top < _twoThirdDistance) // 右上
                {
                    left = parentActualWidth - ActualWidth - ButtonSpace * 2;
                    top = ButtonSpace;
                    _touchPosMemory = new AssistiveTouchPosition(TouchButtonCorner.UpperRight);
                }
                else if (right < _halfDistance && bottom < _twoThirdDistance) // 右下
                {
                    left = parentActualWidth - ActualWidth - ButtonSpace * 2;
                    top = parentActualHeight - ActualHeight - ButtonSpace * 2;
                    _touchPosMemory = new AssistiveTouchPosition(TouchButtonCorner.LowerRight);
                }
                else if (top < _twoThirdDistance) // 上
                {
                    left = curMarginLeft;
                    top = ButtonSpace;
                    var scale = (curMarginLeft + ButtonSpace + Width / 2) / parentActualWidth;
                    _touchPosMemory = new AssistiveTouchPosition(TouchButtonCorner.Top, scale);
                }
                else if (bottom < _twoThirdDistance) // 下
                {
                    left = curMarginLeft;
                    top = parentActualHeight - ActualHeight - ButtonSpace * 2;
                    var scale = (curMarginLeft + ButtonSpace + Width / 2) / parentActualWidth;
                    _touchPosMemory = new AssistiveTouchPosition(TouchButtonCorner.Bottom, scale);
                }
                else if (left < verticalMiddleLine) // 左
                {
                    left = ButtonSpace;
                    top = curMarginTop;
                    var scale = (curMarginTop + ButtonSpace + Height / 2) / parentActualHeight;
                    _touchPosMemory = new AssistiveTouchPosition(TouchButtonCorner.Left, scale);
                }
                else if (right < verticalMiddleLine) // 右
                {
                    left = parentActualWidth - ActualWidth - ButtonSpace * 2;
                    top = curMarginTop;
                    var scale = (curMarginTop + ButtonSpace + Height / 2) / parentActualHeight;
                    _touchPosMemory = new AssistiveTouchPosition(TouchButtonCorner.Right, scale);
                }
                else
                {
                    throw new InvalidOperationException("Should never happen!");
                }

                // 元素的某个属性，在开始值和结束值之间逐步增加，是一种线性插值的过程
                //Log.Info($"最终button移动位置 {left} {top}");
                SmoothMoveAnimation(left, top);
                _move = false;
            };
        }

        private void SmoothMoveAnimation(double left, double top)
        {
            var marginAnimation = new ThicknessAnimation
            {
                From = Margin,
                To = new Thickness(left, top, 0, 0),
                Duration = TimeSpan.FromMilliseconds(300)
            };

            Storyboard story = new()
            {
                FillBehavior = FillBehavior.Stop
            };
            story.Children.Add(marginAnimation);
            Storyboard.SetTargetName(marginAnimation, nameof(FloatButton));
            Storyboard.SetTargetProperty(marginAnimation, new PropertyPath("(0)", Border.MarginProperty));

            story.Begin(this);

            Margin = new Thickness(left, top, 0, 0);
        }

        private AssistiveTouchPosition _touchPosMemory =
            System.Text.Json.JsonSerializer.Deserialize<AssistiveTouchPosition>(IoC.Get<EhConfigRepository>().AssistiveTouchPostion) ?? new(TouchButtonCorner.UpperLeft);

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FloatButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (Parent is null && Parent is not FrameworkElement)
            {
                throw new InvalidOperationException("Control's parent must be FrameworkElement type");
            }

            _parent = (Parent as FrameworkElement)!;

            _distance = Width;
            _halfDistance = _distance / 2;
            _oneThirdDistance = _distance / 3;
            _twoThirdDistance = _oneThirdDistance * 2;

            RegisterParentPreviewEvent();

            // 初始化时Button位置在左上角
            //double left = _parent.ActualWidth - ActualWidth - distanceNew;
            //double top = _parent.ActualHeight - ActualHeight - distanceNew;

            var gamePos = IoC.Get<IGameWindowHooker>().GetLastWindowSize();
            _newGameViewHeight = (int)(gamePos.Height / Dpi);
            _newGameViewWidth = (int)(gamePos.Width / Dpi);
            ResetButtonPostion();
            Margin = _immediateMargin;
            RaiseMouseUpEventInCode();

            // for opacity the button first time loaded
            FloatButton_Click(FloatButton, new RoutedEventArgs());
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FloatButton.Opacity = OpacityNormal;
            _move = true;
            _lastPos = e.GetPosition(_parent);
            _oldPos = _lastPos;
            //Log.Info($"lastPos and oldPos {_lastPos.X} {_lastPos.Y}");
        }

        private CancellationTokenSource _tokenSource = new();
        private const int ButtonOpacityTimeout = 5000;

        // After mouse released
        private async void FloatButton_Click(object sender, RoutedEventArgs? e)
        {
            if (_newPos.Equals(_oldPos))
            {
                // Fire MouseClickEvent, no use for me
                ClickEvent?.Invoke(sender, e ?? new RoutedEventArgs());
            }
            else
            {
                // Disable flyout popup
                if (e is not null)
                    e.Handled = true;
            }

            _tokenSource.Cancel();
            _tokenSource = new CancellationTokenSource();
            var cancelToken = _tokenSource.Token;

            await Task.Run(async () =>
            {
                await Task.Delay(ButtonOpacityTimeout, CancellationToken.None).ConfigureAwait(false);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (_move == false && !cancelToken.IsCancellationRequested)
                    {
                        FloatButton.Opacity = OpacityValue;
                    }
                });
            }, cancelToken);
        }

        private Thickness _immediateMargin = new();

        private void ResetButtonPostion()
        {
            switch (_touchPosMemory.Corner)
            {
                case TouchButtonCorner.UpperLeft:
                    _immediateMargin = new Thickness(ButtonSpace, ButtonSpace, 0, 0);
                    //Margin = _immediateMargin;
                    break;
                case TouchButtonCorner.UpperRight:
                    _immediateMargin = new Thickness(
                        _newGameViewWidth - ActualWidth - ButtonSpace * 2,
                        ButtonSpace, 0, 0);
                    //Margin = _immediateMargin;
                    break;
                case TouchButtonCorner.LowerLeft:
                    _immediateMargin = new Thickness(
                        ButtonSpace,
                        _newGameViewHeight - ActualHeight - ButtonSpace * 2, 0, 0);
                    //Margin = _immediateMargin;
                    break;
                case TouchButtonCorner.LowerRight:
                    _immediateMargin = new Thickness(
                        _newGameViewWidth - ActualWidth - ButtonSpace * 2,
                        _newGameViewHeight - ActualHeight - ButtonSpace * 2, 0, 0);
                    //Margin = _immediateMargin;
                    break;
                case TouchButtonCorner.Left:
                    _immediateMargin = new Thickness(
                        ButtonSpace,
                        _touchPosMemory.Scale * _newGameViewHeight - ButtonSpace - Height / 2, 0, 0);
                    //Margin = _immediateMargin;
                    break;
                case TouchButtonCorner.Top:
                    _immediateMargin = new Thickness(
                        _touchPosMemory.Scale * _newGameViewWidth - ButtonSpace - Width / 2,
                        ButtonSpace, 0, 0);
                    //Margin = _immediateMargin;
                    break;
                case TouchButtonCorner.Right:
                    _immediateMargin = new Thickness(
                        _newGameViewWidth - ActualWidth - ButtonSpace * 2,
                        _touchPosMemory.Scale * _newGameViewHeight - ButtonSpace - Height / 2, 0, 0);
                    //Margin = _immediateMargin;
                    break;
                case TouchButtonCorner.Bottom:
                    _immediateMargin = new Thickness(
                        _newGameViewHeight - ActualHeight - ButtonSpace * 2,
                        _touchPosMemory.Scale * _newGameViewWidth - ButtonSpace - Width / 2, 0, 0);
                    //Margin = _immediateMargin;
                    break;
            }
        }
    }
}
