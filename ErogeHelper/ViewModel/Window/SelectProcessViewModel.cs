﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Entity.Response;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace ErogeHelper.ViewModel.Window
{
    public class SelectProcessViewModel : PropertyChangedBase
    {
        public SelectProcessViewModel(
            ISelectProcessDataService dataService,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            ITextractorService textractorService,
            IGameWindowHooker gameWindowHooker,
            IEhServerApiService ehServerApiService,
            EhDbRepository ehDbRepository,
            EhConfigRepository ehConfigRepository,
            GameRuntimeDataRepo gameRuntimeDataRepo)
        {
            _dataService = dataService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _textractorService = textractorService;
            _gameWindowHooker = gameWindowHooker;
            _ehServerApiService = ehServerApiService;
            _ehDbRepository = ehDbRepository;
            _ehConfigRepository = ehConfigRepository;
            _gameRuntimeDataRepo = gameRuntimeDataRepo;

            _dataService.RefreshBindableProcComboBoxAsync(ProcItems);
        }

        private readonly ISelectProcessDataService _dataService;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITextractorService _textractorService;
        private readonly IGameWindowHooker _gameWindowHooker;
        private readonly IEhServerApiService _ehServerApiService;
        private readonly EhDbRepository _ehDbRepository;
        private readonly EhConfigRepository _ehConfigRepository;
        private readonly GameRuntimeDataRepo _gameRuntimeDataRepo;

        public BindableCollection<ProcComboBoxItem> ProcItems { get; } = new();

        private ProcComboBoxItem? _selectedProcItem;
        public ProcComboBoxItem? SelectedProcItem
        {
            get => _selectedProcItem;
            set
            {
                _selectedProcItem = value;
                NotifyOfPropertyChange(() => CanInject);
            }
        }

        public async void GetProcessAction() =>
            await _dataService.RefreshBindableProcComboBoxAsync(ProcItems).ConfigureAwait(false);

        public bool CanInject => SelectedProcItem is not null;

        public async void Inject()
        {
            if (SelectedProcItem!.Proc.HasExited)
            {
                // This would turn SelectedProcItem to null
                ProcItems.Remove(SelectedProcItem);
                await _eventAggregator
                    .PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.OpenDialog,
                        ModernDialog.SelectProcessTip, extraInfo: Language.Strings.SelectProcess_ProcessExit))
                    .ConfigureAwait(false);
                return;
            }

            if (SelectedProcItem!.Proc.Id == System.Environment.ProcessId)
            {
                await _eventAggregator
                    .PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.OpenDialog,
                        ModernDialog.SelectProcessTip, extraInfo: Language.Strings.SelectProcess_ProcessItself))
                    .ConfigureAwait(false);
                return;
            }

            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Hide));

            IEnumerable<Process> gameProcesses = Utils.ProcessCollect(SelectedProcItem.Proc.ProcessName);
            var (md5, gameProcess) = _gameRuntimeDataRepo.Init(gameProcesses);

            _ = _gameWindowHooker.SetGameWindowHookAsync(gameProcess, gameProcesses.ToList());

            _ehDbRepository.Md5 = md5;
            var settingJson = string.Empty;
            var gameInfo = await _ehDbRepository.GetGameInfoAsync().ConfigureAwait(false);
            if (gameInfo is not null)
            {
                settingJson = gameInfo.TextractorSettingJson;
            }
            else
            {
                try
                {
                    using var resp = await _ehServerApiService.GetGameSetting(md5).ConfigureAwait(true);
                    // For throw ApiException
                    //resp = await resp.EnsureSuccessStatusCodeAsync();
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        // TODO: 先完成服务器重建 Save game Id also
                        // TODO: 先完成服务器重建 Also need RegExp info _ehDbRepository
                        var content = resp.Content ?? new GameSettingResponse();
                        settingJson = content.GameSettingJson;
                    }
                    Log.Debug($"EHServer: {resp.StatusCode} {resp.Content}");
                }
                catch (HttpRequestException ex)
                {
                    Log.Warn("Can't connect to internet", ex);
                }
            }

            if (settingJson == string.Empty)
            {
                Log.Info("Not find game hook setting, open hook panel.");
                _textractorService.InjectProcesses(gameProcesses);
                await _windowManager.ShowWindowFromIoCAsync<HookConfigViewModel>().ConfigureAwait(false);
                _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Close));
                return;
            }

            var textractorSetting = JsonSerializer.Deserialize<TextractorSetting>(settingJson) ?? new TextractorSetting();
            _textractorService.InjectProcesses(gameProcesses, textractorSetting);

            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("InsideView").ConfigureAwait(false);
            await _windowManager.SilentStartWindowFromIoCAsync<GameViewModel>("OutsideView").ConfigureAwait(false);

            if (_ehConfigRepository.UseOutsideWindow)
            {
                _ = _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "OutsideView"));
            }

            _ = _eventAggregator.PublishOnUIThreadAsync(
                    new ViewActionMessage(typeof(GameViewModel), ViewAction.Show, null, "InsideView"));

            _ = _eventAggregator.PublishOnUIThreadAsync(new ViewActionMessage(GetType(), ViewAction.Close));
        }

#pragma warning disable 8618
        public SelectProcessViewModel() { }
    }
}