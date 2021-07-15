using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Entity.Payload;
using ErogeHelper.Model.Entity.Table;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.View.Dialog;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using ErogeHelper.ViewModel.Window;

namespace ErogeHelper.ViewModel.Page
{
    public class HookViewModel : PropertyChangedBase, IHandle<NewTextThreadSelectedMessage>
    {
        public HookViewModel(
            IHookDataService dataService,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            ITextractorService textractorService,
            EhDbRepository ehDbRepository,
            EhConfigRepository ehConfigRepository,
            IGameDataService gameDataService,
            IEhServerApiService ehServerApiService,
            GameRuntimeDataRepo gameRuntimeDataRepo)
        {
            _dataService = dataService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _textractorService = textractorService;
            _ehDbRepository = ehDbRepository;
            _ehConfigRepository = ehConfigRepository;
            _gameDataService = gameDataService;
            _ehServerApiService = ehServerApiService;
            _gameRuntimeDataRepo = gameRuntimeDataRepo;

            _eventAggregator.SubscribeOnUIThread(this);
            _textractorService.DataEvent += DataProcess;
            CurrentThreadsNames = _textractorService.Setting.Hookcode == string.Empty
                ? "None"
                : _textractorService.Setting.Hookcode + " with " + _textractorService.Setting.HookSettings.Count() + " threads";
            RegExp = _dataService.GetRegExp();
            ConsoleOutput = string.Join('\n', _textractorService.GetConsoleOutputInfo());
            SelectedText = Language.Strings.HookPage_SelectedTextInitTip;
        }

        private readonly IHookDataService _dataService;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITextractorService _textractorService;
        private readonly EhDbRepository _ehDbRepository;
        private readonly EhConfigRepository _ehConfigRepository;
        private readonly IGameDataService _gameDataService;
        private readonly IEhServerApiService _ehServerApiService;
        private readonly GameRuntimeDataRepo _gameRuntimeDataRepo;

        #region RegExp

        private string _currentThreadsNames = string.Empty;

        public string CurrentThreadsNames
        {
            get => _currentThreadsNames;
            set { _currentThreadsNames = value; NotifyOfPropertyChange(() => CurrentThreadsNames); }
        }

        private string? _regExp = string.Empty;

        // If user click 'x', it will turn to null
        public string? RegExp
        {
            get => _regExp ?? string.Empty;
            set
            {
                _regExp = value;
                if (value is null)
                {
                    SelectedText = _rawText ?? string.Empty;
                }
                else if (!InvalidRegExp)
                {
                    if (_rawText is not null)
                    {
                        SelectedText = Utils.TextEvaluateWithRegExp(_rawText, value);
                    }
                }
                NotifyOfPropertyChange(() => RegExp);
            }
        }

        private bool _invalidRegExp;

        public bool InvalidRegExp
        {
            get => _invalidRegExp;
            set { _invalidRegExp = value; NotifyOfPropertyChange(() => CanSubmitSetting); }
        }

        private string _rawText = string.Empty;

        private string _selectedText = string.Empty;
        public string SelectedText
        { get => _selectedText; set { _selectedText = value; NotifyOfPropertyChange(() => SelectedText); } }

        private const string Tag1 = ".*(?=[「|『])";
        private const string Tag2 = "(?<=[」|』]).*";
        private const string Tag3 = "<.*?>";
        private const string Tag4 = "_r|<br>|#n|\\n|\\\\n";
        private const string Tag5 = "[\\x00-\\xFF]";

        public void RegExpTag1() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag1 : $"{RegExp}|{Tag1}";
        public void RegExpTag2() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag2 : $"{RegExp}|{Tag2}";
        public void RegExpTag3() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag3 : $"{RegExp}|{Tag3}";
        public void RegExpTag4() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag4 : $"{RegExp}|{Tag4}";
        public void RegExpTag5() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag5 : $"{RegExp}|{Tag5}";

        #endregion

        #region Console

        private string _consoleOutput = string.Empty;
        public string ConsoleOutput
        { get => _consoleOutput; set { _consoleOutput = value; NotifyOfPropertyChange(() => ConsoleOutput); } }

        #endregion

        #region HookCode ReadCode

        private bool _canOpenDialog = true;
        private string _inputHCode = string.Empty;
        private bool _canSearchCode = true;

        public bool CanOpenDialog
        {
            get => _canOpenDialog;
            set { _canOpenDialog = value; NotifyOfPropertyChange(() => CanOpenDialog); }
        }

        public async void OpenHCodeDialog()
        {
            CanOpenDialog = false;
            await _eventAggregator.PublishOnUIThreadAsync(
                new ViewActionMessage(GetType(), ViewAction.OpenDialog, ModernDialog.HookCode, null, ViewType.Page))
                .ConfigureAwait(false);
            CanOpenDialog = true;
        }

        public string InputHCode
        {
            get => _inputHCode;
            set
            {
                // 因 validation 约束 InputHCode 只在值有效、null或empty时，xaml才会传值过来
                _inputHCode = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
                NotifyOfPropertyChange(() => InputHCode);
            }
        }

        public bool CanSearchCode
        {
            get => _canSearchCode;
            set { _canSearchCode = value; NotifyOfPropertyChange(() => CanSearchCode); }
        }

        public async void SearchHCode()
        {
            CanSearchCode = false;
            var hcode = await _dataService.QueryHCode().ConfigureAwait(false);
            if (hcode is not null)
            {
                InputHCode = hcode;
                Log.Info(hcode);
            }
            else
            {
                InputHCode = Language.Strings.HookPage_CodeSearchNoResult;
            }
            CanSearchCode = true;
        }

        public void InsertCode()
        {
            // Condition here cause "Enter" key will cross this
            if (InputHCode != string.Empty)
            {
                _textractorService.InsertHook(InputHCode);
            }
        }

        public async void OpenRCodeDialog()
        {
            CanOpenDialog = false;
            await _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(GetType(), ViewAction.OpenDialog, ModernDialog.ReadCode, null, ViewType.Page))
                .ConfigureAwait(false);
            CanOpenDialog = true;
        }

        public void SearchRCodeText(RCodeDialog dialog)
        {
            // Condition here cause "Enter" key will cross this
            if (dialog.IsPrimaryButtonEnabled)
            {
                _textractorService.SearchRCode(dialog.JapaneseText.Text);
            }
        }

        #endregion

        public void ClearHookMapData()
        {
            SelectedHookItem = null;
            HookComboSource.Clear();
            HookMapData.Clear();
            SelectedText = Language.Strings.HookPage_SelectedTextInitTip;
        }

        public async void ReInjectProcesses()
        {
            ClearHookMapData();
            await _textractorService.ReAttachProcesses();
        }

        public void RemoveHook(HookMapItem hookItem)
        {
            _textractorService.RemoveHook(hookItem.Address);
            HookComboSource.Remove(hookItem);
        }

        public HookBindingList<long, HookMapItem> HookComboSource { get; set; } = new(p => p.Address);

        public HookMapItem? SelectedHookItem
        {
            get => _selectedHookItem;
            set
            {
                if (_selectedHookItem is not null)
                {
                    var lastTextThreads = HookMapData.Where(it => it.Address == _selectedHookItem.Address);
                    foreach (var textThread in lastTextThreads)
                    {
                        textThread.Selected = false;
                    }
                }
                _selectedHookItem = value;
                SelectedAddressItems.Clear();
                if (value is not null)
                {
                    SelectedAddressItems.AddRange(
                        HookMapData.Where(it => it.Address == value.Address));
                }
                _selectedTextThreads.Clear();
                SelectedText = Language.Strings.HookPage_SelectedTextInitTip;

                NotifyOfPropertyChange(() => CanSubmitSetting);
                NotifyOfPropertyChange(() => SelectedHookItem);
            }
        }

        public BindableCollection<HookMapItem> SelectedAddressItems { get; set; } = new();

        private HookBindingList<long, HookMapItem> HookMapData { get; set; } = new(p => p.Handle);

        private readonly List<HookMapItem> _selectedTextThreads = new();

        private async void DataProcess(HookParam hp)
        {
            if (hp.Handle == 0)
            {
                // Console
                ConsoleOutput += "\n" + hp.Text;
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // All datas
                var targetItem = HookMapData.FastFind(hp.Handle);
                if (targetItem is null)
                {
                    targetItem = new HookMapItem
                    {
                        Handle = hp.Handle,
                        Address = hp.Address,
                        HookCode = hp.Hookcode,
                        ThreadContext = hp.Ctx,
                        SubThreadContext = hp.Ctx2,
                        TotalText = hp.Text,
                        Text = hp.Text,
                        EngineName = hp.Name
                    };
                    HookMapData.Insert(0, targetItem);
                }
                else
                {
                    string tmp = targetItem.TotalText + "\n\n" + hp.Text;

                    targetItem.TotalText = tmp;
                    targetItem.Text = hp.Text;
                }

                targetItem = HookComboSource.FastFind(hp.Address);
                if (targetItem is null)
                {
                    targetItem = new HookMapItem
                    {
                        Address = hp.Address,
                        EngineName = hp.Name
                    };
                    HookComboSource.Add(targetItem);
                }

                if (SelectedHookItem is null)
                {
                    // Set default hook combo item
                    SelectedHookItem = targetItem;
                }

                if (_selectedTextThreads.Any(thread => thread.Handle == hp.Handle))
                {
                    _rawText = hp.Text;
                    SelectedText = Utils.TextEvaluateWithRegExp(_rawText ?? string.Empty, RegExp);
                }
            });
        }

        public Task HandleAsync(NewTextThreadSelectedMessage message, CancellationToken cancellationToken)
        {
            if (message.Status)
            {
                _selectedTextThreads.Add(message.HookMapItem);

                _rawText = message.HookMapItem.Text;
                SelectedText = Utils.TextEvaluateWithRegExp(_rawText ?? string.Empty, RegExp);
            }
            else
            {
                _selectedTextThreads.Remove(message.HookMapItem);
            }

            if (!_selectedTextThreads.Any())
            {
                SelectedText = Language.Strings.HookPage_SelectedTextInitTip;
            }

            NotifyOfPropertyChange(() => CanSubmitSetting);

            return Task.CompletedTask;
        }

        public bool CanSubmitSetting => _selectedTextThreads.Any() && !InvalidRegExp && !_submitting;
        private bool _submitting;
        private HookMapItem? _selectedHookItem;

        public async void SubmitSetting()
        {
            _submitting = true;
            NotifyOfPropertyChange(() => CanSubmitSetting);

            var textPendingToSend = SelectedText
                .Replace("|~S~|", string.Empty)
                .Replace("|~E~|", string.Empty);

            if (!string.IsNullOrWhiteSpace(RegExp))
            {
                var list = Regex.Split(textPendingToSend, RegExp);
                textPendingToSend = string.Join("", list);
            }

            HookComboSource
                .Where(combo => !combo.Address.Equals(SelectedHookItem!.Address)).ToList()
                .ForEach(shouldRemoveThread => HookComboSource.Remove(shouldRemoveThread));
            _textractorService.RemoveUselessHooks();

            var hookSettings = new List<TextractorSetting.HookSetting>();
            _selectedTextThreads.ForEach(thread =>
            {
                hookSettings.Add(new TextractorSetting.HookSetting
                {
                    ThreadContext = thread.ThreadContext,
                    SubThreadContext = thread.SubThreadContext,
                });
            });
            var textractorSetting = new TextractorSetting
            {
                IsUserHook = _selectedTextThreads.First().EngineName.Contains("UserHook"),
                Hookcode = _selectedTextThreads.First().HookCode,
                HookSettings = hookSettings,
            };
            _textractorService.Setting = textractorSetting;
            CurrentThreadsNames = textractorSetting.Hookcode + ' ' + textractorSetting.HookSettings.Count() + " threads";

            _ = _eventAggregator.PublishOnUIThreadAsync(new RegExpChangedMessage { RegExp = RegExp ?? string.Empty });

            if (!_selectedTextThreads.First().EngineName.Equals("Search"))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var resp = await _ehServerApiService.SendGameSetting(new GameSettingPayload(
                            "guest",
                            "erogehelper",
                            _ehDbRepository.Md5,
                            Utils.GetGameNamesByProcess(_gameRuntimeDataRepo.MainProcess),
                            JsonSerializer.Serialize(textractorSetting),
                            RegExp ?? string.Empty)).ConfigureAwait(false);
                        if (!resp.IsSuccessStatusCode)
                        {
                            Log.Warn(resp.Error ?? throw new Exception("Refit unknown error"));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                });
            }

            var gameInfoTable =
                await _ehDbRepository.GetGameInfoAsync().ConfigureAwait(false);
            if (gameInfoTable is null)
            {
                // HookPage must in HookConfigView window
                // This happen when server has no data or user uses EH offline
                gameInfoTable = new GameInfoTable
                {
                    Md5 = _ehDbRepository.Md5,
                    RegExp = RegExp ?? string.Empty,
                    TextractorSettingJson = JsonSerializer.Serialize(textractorSetting),
                };
                await _ehDbRepository.SetGameInfoAsync(gameInfoTable).ConfigureAwait(false);
            }
            else
            {
                var hookSetting = gameInfoTable.TextractorSettingJson;
                if (hookSetting == string.Empty)
                {
                    // HookPage in HookConfigView window
                    await _ehDbRepository.UpdateGameInfoAsync(new GameInfoTable
                    {
                        Md5 = gameInfoTable.Md5,
                        GameIdList = gameInfoTable.GameIdList,
                        RegExp = RegExp ?? string.Empty,
                        TextractorSettingJson = JsonSerializer.Serialize(textractorSetting),
                        IsLoseFocus = gameInfoTable.IsLoseFocus,
                        IsEnableTouchToMouse = gameInfoTable.IsEnableTouchToMouse,
                    }).ConfigureAwait(false);
                }
                else
                {
                    // HookPage in Preference window
                    _gameDataService.SendNewText(textPendingToSend);
                    await _ehDbRepository.UpdateGameInfoAsync(new GameInfoTable
                    {
                        Md5 = gameInfoTable.Md5,
                        GameIdList = gameInfoTable.GameIdList,
                        RegExp = RegExp ?? string.Empty,
                        TextractorSettingJson = JsonSerializer.Serialize(textractorSetting),
                        IsLoseFocus = gameInfoTable.IsLoseFocus,
                        IsEnableTouchToMouse = gameInfoTable.IsEnableTouchToMouse,
                    }).ConfigureAwait(false);
                    await _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(),
                            ViewAction.OpenDialog, ModernDialog.HookSettingUpdatedTip, null, ViewType.Page))
                        .ConfigureAwait(false);

                    _submitting = false;
                    NotifyOfPropertyChange(() => CanSubmitSetting);
                    return;
                }
            }

            // 在HookViewModel(page vm)以HookConfigViewModel(window vm)的名义向HookConfigView(window)发消息
            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(typeof(HookConfigViewModel), ViewAction.Hide));

            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("InsideView").ConfigureAwait(false);
            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("OutsideView").ConfigureAwait(false);

            _gameDataService.SendNewText(textPendingToSend);

            if (_ehConfigRepository.UseMoveableTextControl)
            {
                await _eventAggregator.PublishOnUIThreadAsync(new UseMoveableTextMessage { UseMove = true });
            }
            else
            {
                await _eventAggregator.PublishOnUIThreadAsync(new UseMoveableTextMessage { UseMove = false });
            }

            await _eventAggregator.PublishOnUIThreadAsync(
                new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "InsideView"));

            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(typeof(HookConfigViewModel), ViewAction.Close));

            _submitting = false;
            NotifyOfPropertyChange(() => CanSubmitSetting);
        }

#pragma warning disable 8618
        public HookViewModel() { }
    }
}