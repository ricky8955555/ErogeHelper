using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.View.Window;
using ErogeHelper.ViewModel.Control;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WindowsInput.Events;
using ErogeHelper.Model.Entity.Table;
using ErogeHelper.Common.Enum;
using System.Windows.Controls;
using WPFDanmakuLib;
using Utils = ErogeHelper.Common.Utils;

namespace ErogeHelper.ViewModel.Window
{
    public class GameViewModel : PropertyChangedBase,
        IHandle<UseMoveableTextMessage>, IHandle<JapaneseVisibleMessage>, IHandle<FullScreenChangedMessage>, IHandle<DanmakuVisibleMessage>, IHandle<DanmakuMessage>,
        IDisposable
    {
        public GameViewModel(
            IGameDataService dataService,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            ITouchConversionHooker touchConversionHooker,
            EhConfigRepository ehConfigRepository,
            GameRuntimeDataRepo gameRuntimeDataRepo,
            TextViewModel textViewModel,
            IGameWindowHooker gameWindowHooker,
            EhDbRepository ehDbRepository)
        {
            _touchHooker = touchConversionHooker;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _ehConfigRepository = ehConfigRepository;
            _gameRuntimeDataRepo = gameRuntimeDataRepo;
            TextControl = textViewModel;
            _gameWindowHooker = gameWindowHooker;
            _ehDbRepository = ehDbRepository;

            _eventAggregator.SubscribeOnUIThread(this);
            _touchHooker.Init();
            DanmakuVisibility = _ehConfigRepository.UseDanmaku ? Visibility.Visible : Visibility.Collapsed;
            if (_ehDbRepository.GetGameInfo()?.IsEnableTouchToMouse ?? false)
                TouchToMouseToggle();
            _fontSize = _ehConfigRepository.FontSize;
            AppendTextList.Add(new AppendTextItem(Language.Strings.GameViewModel_TextWaitingMessage));
            dataService.SourceTextReceived += text =>
            {
                SourceTextArchiver.Enqueue(text);
                TextControl.CardControl.TotalText = text;
            };
            dataService.BindableTextItem += textItem => TextControl.SourceTextCollection = textItem;
            dataService.AppendTextReceived += (text, tip) => AppendTextList.Add(new AppendTextItem(text, tip));
            dataService.AppendTextsRefresh += _ => AppendTextList.Clear();
        }

        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly EhConfigRepository _ehConfigRepository;
        private readonly GameRuntimeDataRepo _gameRuntimeDataRepo;
        private readonly ITouchConversionHooker _touchHooker;
        private readonly IGameWindowHooker _gameWindowHooker;
        private readonly EhDbRepository _ehDbRepository;

        private bool _isSourceTextPined = true;
        private Visibility _danmakuVisibility;
        private bool _assistiveTouchIsVisible = true;
        private double _fontSize;
        private Visibility _triggerBarVisibility = Visibility.Collapsed;
        private Visibility _textControlVisibility = Visibility.Visible;
        private Visibility _insideTextVisibility;
        private Visibility _insideMoveableTextVisibility;
        private Visibility _outsideJapaneseVisible;
        
        public TextViewModel TextControl { get; set; }
        public BindableCollection<AppendTextItem> AppendTextList { get; set; } = new();

        // Not much use for the moment
        public ConcurrentCircularBuffer<string> SourceTextArchiver = new(30);

        private WPFDanmakuEngine? _damakuEngine;
        private DanmakuStyle _danmaku = new();
        private Canvas? _danmakuContainer;

        public void DanmakuEngineInit(Canvas danmakuContainer)
        {
            _danmakuContainer = danmakuContainer;
            var engineBehavior = new EngineBehavior(DrawMode.WPF, CollisionPrevention.Enabled);
            _danmaku.FontSize = (int)FontSize;
            _damakuEngine = new WPFDanmakuEngine(engineBehavior, _danmaku, danmakuContainer);
        }

        public Visibility DanmakuVisibility
        {
            get => _danmakuVisibility;
            set { _danmakuVisibility = value; NotifyOfPropertyChange(() => DanmakuVisibility); }
        }

        public void SendDanmaku(string danmakuText)
        {
            if (_danmakuContainer is null || string.IsNullOrWhiteSpace(danmakuText))
                return;

            if (danmakuText.Length > 100)
            {
                ModernWpf.MessageBox.Show("Text length must lower than 100", "Eroge Helper");
                return;
            }

            // XXX: Net submit, only return normal can toast danmaku
            ToastDanmaku(danmakuText);
        }

        public bool AssistiveTouchIsVisible
        {
            get => _assistiveTouchIsVisible;
            set { _assistiveTouchIsVisible = value; NotifyOfPropertyChange(() => AssistiveTouchIsVisible); }
        }

        public double FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                _ehConfigRepository.FontSize = value;
                NotifyOfPropertyChange(() => FontSize);
            }
        }

        public void ZoomIn()
        {
            FontSize += 2;
            NotifyOfPropertyChange(() => CanZoomOut);
        }

        public bool CanZoomOut => FontSize > 3;
        public void ZoomOut()
        {
            FontSize -= 2;
            NotifyOfPropertyChange(() => CanZoomOut);
        }

        public static async void VolumeUp() => await WindowsInput.Simulate.Events()
            .Click(KeyCode.VolumeUp).Invoke().ConfigureAwait(false);
        public static async void VolumeDown() => await WindowsInput.Simulate.Events()
            .Click(KeyCode.VolumeDown).Invoke().ConfigureAwait(false);

        public async void SwitchGameScreen()
        {
            var handle = _gameRuntimeDataRepo.MainProcess.MainWindowHandle;
            NativeMethods.BringWindowToTop(handle);
            await WindowsInput.Simulate.Events()
                .ClickChord(KeyCode.Alt, KeyCode.Enter)
                .Invoke()
                .ConfigureAwait(false);
        }

        #region InsideView TextControl Pin

        private Visibility _pinSourceTextToggleVisibility;
        public Visibility PinSourceTextToggleVisibility
        {
            get => _pinSourceTextToggleVisibility;
            set
            {
                _pinSourceTextToggleVisibility = value;
                NotifyOfPropertyChange(() => PinSourceTextToggleVisibility);
            }
        }

        public bool IsSourceTextPined
        {
            get => _isSourceTextPined;
            set { _isSourceTextPined = value; NotifyOfPropertyChange(() => IsSourceTextPined); }
        }

        // Should worked in InsideView only
        public void PinSourceTextToggle()
        {
            if (IsSourceTextPined)
            {
                TriggerBarVisibility = Visibility.Collapsed;
                TextControlVisibility = Visibility.Visible;
                if (!_ehConfigRepository.UseMoveableTextControl)
                    TextControl.Background = new SolidColorBrush();
            }
            else
            {
                TriggerBarVisibility = Visibility.Visible;
                TextControlVisibility = Visibility.Collapsed;
                if (!_ehConfigRepository.UseMoveableTextControl)
                    TextControl.Background = new SolidColorBrush(Colors.Black) { Opacity = 0.5 };
            }
        }

        public Visibility TriggerBarVisibility
        {
            get => _triggerBarVisibility;
            set
            {
                _triggerBarVisibility = value;
                NotifyOfPropertyChange(() => TriggerBarVisibility);
            }
        }

        public Visibility TextControlVisibility
        {
            get => _textControlVisibility;
            set
            {
                _textControlVisibility = value;
                NotifyOfPropertyChange(() => TextControlVisibility);
            }
        }

        public void TriggerBarEnter()
        {
            if (IsSourceTextPined)
                return;

            TextControlVisibility = Visibility.Visible;
            TriggerBarVisibility = Visibility.Collapsed;
        }

        public void TextControlLeave()
        {
            if (IsSourceTextPined)
                return;

            TextControlVisibility = Visibility.Collapsed;
            TriggerBarVisibility = Visibility.Visible;
        }

        #endregion

        public bool IsLoseFocus
        {
            get => _ehDbRepository.GetGameInfo()?.IsLoseFocus ?? false;
            set
            {
                var gameInfo = _ehDbRepository.GetGameInfo() ?? new GameInfoTable();
                gameInfo.IsLoseFocus = value;
                _ehDbRepository.UpdateGameInfo(gameInfo);
                NotifyOfPropertyChange(() => IsLoseFocus);
            }
        }
        public async void FocusToggle()
        {
            if (IsLoseFocus)
            {
                await _eventAggregator.PublishOnUIThreadAsync(new LoseFocusMessage { Status = true });
            }
            else
            {
                await _eventAggregator.PublishOnUIThreadAsync(new LoseFocusMessage { Status = false });
            }
        }

        public bool IsTouchToMouse
        {
            get => _ehDbRepository.GetGameInfo()?.IsEnableTouchToMouse ?? false;
            set
            {
                var gameInfo = _ehDbRepository.GetGameInfo() ?? new GameInfoTable();
                gameInfo.IsEnableTouchToMouse = value;
                _ehDbRepository.UpdateGameInfo(gameInfo);
                NotifyOfPropertyChange(() => IsTouchToMouse);
            }
        }
        public void TouchToMouseToggle() => _touchHooker.Enable = IsTouchToMouse;

        public static async void TaskbarNotifyArea() => await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.A).Invoke().ConfigureAwait(false);

        public static async void TaskView() => await WindowsInput.Simulate.Events()
            .ClickChord(KeyCode.LWin, KeyCode.Tab).Invoke().ConfigureAwait(false);

        public async void ScreenShot()
        {
            AssistiveTouchIsVisible = false;

            await WindowsInput.Simulate.Events()
                .Click(KeyCode.Escape).Invoke().ConfigureAwait(false);

            // Wait for CommandBarFlyout hide
            await Task.Delay(500).ConfigureAwait(false);

            await WindowsInput.Simulate.Events()
                .ClickChord(KeyCode.LWin, KeyCode.Shift, KeyCode.S).Invoke().ConfigureAwait(false);

            await Task.Delay(3000).ConfigureAwait(false);

            AssistiveTouchIsVisible = true;
        }

        public void ResetInsideView() => _gameWindowHooker.ResetWindowHandler();

        //public async void OpenDanmaku()
        //{
        //    var window = Application.Current.Windows.OfType<DanmakuWindow>().SingleOrDefault();
        //    if (window is null)
        //    {
        //        await _windowManager.ShowWindowFromIoCAsync<PreferenceViewModel>().ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        window.Activate();
        //    }
        //}

        public async void OpenPreference()
        {
            var window = Application.Current.Windows.OfType<PreferenceView>().SingleOrDefault();
            if (window is null)
            {
                // XXX: Memory leak if add PreferenceViewModel as transition
                await _windowManager.ShowWindowFromIoCAsync<PreferenceViewModel>().ConfigureAwait(false);
            }
            else
            {
                window.Activate();
            }
        }

        public static async void PressSkip() => await WindowsInput.Simulate.Events()
            .Hold(KeyCode.Control).Invoke().ConfigureAwait(false);
        public static async void PressSkipRelease() => await WindowsInput.Simulate.Events()
            .Release(KeyCode.Control).Invoke().ConfigureAwait(false);

        public Visibility InsideTextVisibility
        {
            get => _insideTextVisibility;
            set { _insideTextVisibility = value; NotifyOfPropertyChange(() => InsideTextVisibility); }
        }

        public Visibility InsideMoveableTextVisibility
        {
            get => _insideMoveableTextVisibility;
            set { _insideMoveableTextVisibility = value; NotifyOfPropertyChange(() => InsideMoveableTextVisibility); }
        }

        public async Task HandleAsync(UseMoveableTextMessage message, CancellationToken cancellationToken)
        {
            if (message.UseMove)
            {
                if (Utils.IsGameForegroundFullScreen(_gameRuntimeDataRepo.MainProcess.MainWindowHandle))
                {
                    InsideTextVisibility = Visibility.Collapsed;
                    InsideMoveableTextVisibility = Visibility.Visible;
                    await _eventAggregator.PublishOnUIThreadAsync(
                        new ViewActionMessage(typeof(GameViewModel), ViewAction.Hide, null, "OutsideView"),
                        cancellationToken);
                }
                else
                {
                    InsideTextVisibility = Visibility.Collapsed;
                    InsideMoveableTextVisibility = Visibility.Collapsed;
                    await _eventAggregator.PublishOnUIThreadAsync(
                        new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "OutsideView"),
                        cancellationToken);
                }
            }
            else
            {
                InsideTextVisibility = Visibility.Visible;
                InsideMoveableTextVisibility = Visibility.Collapsed;
                await _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(typeof(GameViewModel), ViewAction.Hide, null, "OutsideView"),
                    cancellationToken);
            }
        }

        public async Task HandleAsync(FullScreenChangedMessage message, CancellationToken cancellationToken)
        {
            if (_ehConfigRepository.UseMoveableTextControl)
            {
                if (message.IsFullScreen)
                {
                    InsideTextVisibility = Visibility.Collapsed;
                    InsideMoveableTextVisibility = Visibility.Visible;
                    await _eventAggregator.PublishOnUIThreadAsync(
                        new ViewActionMessage(typeof(GameViewModel), ViewAction.Hide, null, "OutsideView"),
                        cancellationToken);
                }
                else
                {
                    InsideTextVisibility = Visibility.Collapsed;
                    InsideMoveableTextVisibility = Visibility.Collapsed;
                    await _eventAggregator.PublishOnUIThreadAsync(
                        new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "OutsideView"),
                        cancellationToken);
                }
            }
        }

        public Visibility OutsideJapaneseVisible
        {
            get => _outsideJapaneseVisible;
            set { _outsideJapaneseVisible = value; NotifyOfPropertyChange(() => OutsideJapaneseVisible); }
        }

        public Task HandleAsync(JapaneseVisibleMessage message, CancellationToken cancellationToken)
        {
            if (message.IsShowed)
            {
                // Reset TextPin button
                IsSourceTextPined = true;
                PinSourceTextToggle();
                PinSourceTextToggleVisibility = Visibility.Visible;
                OutsideJapaneseVisible = Visibility.Visible;
            }
            else
            {
                TextControlVisibility = Visibility.Collapsed;
                PinSourceTextToggleVisibility = Visibility.Collapsed;
                OutsideJapaneseVisible = Visibility.Collapsed;
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync(DanmakuVisibleMessage message, CancellationToken cancellationToken)
        {
            DanmakuVisibility = message.Status ? Visibility.Visible : Visibility.Collapsed;
            return Task.CompletedTask;
        }

        public Task HandleAsync(DanmakuMessage message, CancellationToken cancellationToken)
        {
            message.Danmaku.ForEach(d => ToastDanmaku(d));
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _eventAggregator.Unsubscribe(this);
            GC.SuppressFinalize(this);
        }

        private void ToastDanmaku(string danmakuText)
        { 
            if (_danmakuContainer is null || string.IsNullOrWhiteSpace(danmakuText))
            {
                return;
            }

            _danmaku.PositionX = _danmakuContainer.ActualWidth;
            _danmaku.OutlineEnabled = false;
            _danmaku.ShadowEnabled = true;
            var kanjiLength = danmakuText.Length;
            var duration = _danmakuContainer.ActualWidth / (120 + kanjiLength);
            _danmaku.Duration = (int)duration;
            // override default danmaku style
            _damakuEngine?.DrawDanmaku(danmakuText, _danmaku);
        }

#pragma warning disable CS8618
        public GameViewModel() { }
    }
}