using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Function;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using WK.Libraries.SharpClipboardNS;

namespace ErogeHelper.Model.Service
{
    public class GameDataService : IGameDataService, IHandle<RegExpChangedMessage>
    {
        public event Action<string>? SourceTextReceived;
        public event Action<BindableCollection<SingleTextItem>>? BindableTextItem;
        public event Action<string, string>? AppendTextReceived;
        public event Action<object>? AppendTextsRefresh;
        public void RefreshCurrentJapanese()
        {
            var text = _currentText;
            // User define RegExp 
            if (_pattern != string.Empty)
            {
                var list = Regex.Split(text, _pattern);
                text = string.Join("", list);
            }

            // Clear ascii control characters
            text = new string(text.Select(c => c < ' ' ? '_' : c).ToArray()).Replace("_", string.Empty);
            // Full-width space
            text = text.Replace("　", string.Empty);

            if (text.Length > 120)
            {
                BindableTextItem?.Invoke(new BindableCollection<SingleTextItem>());
                return;
            }

            // Process source japanese text
            if (_ehConfigRepository.EnableMeCab)
            {
                BindableTextItem?.Invoke(Utils.BindableTextMaker(
                    text,
                    _ehConfigRepository,
                    _meCabService.MeCabWordUniDicEnumerable,
                    _ehConfigRepository.TextTemplateConfig));
            }
        }

        public void SendNewText(string text) => ProcessDataText(new HookParam() { Text = text });

        public GameDataService(
            IMeCabService meCabService,
            ITextractorService textractorService,
            EhConfigRepository ehConfigRepository,
            ITranslatorFactory translatorFactory,
            ITermDataService termDataService,
            IDanmakuService danmakuService,
            IEventAggregator eventAggregator,
            EhDbRepository ehDbRepository,
            SharpClipboard sharpClipboard)
        {
            _meCabService = meCabService;
            _ehConfigRepository = ehConfigRepository;
            _translatorFactory = translatorFactory;
            _termDataService = termDataService;
            _danmakuService = danmakuService;
            _eventAggregator = eventAggregator;
            _pattern = ehDbRepository.GetGameInfo()?.RegExp ?? string.Empty;
            _sharpClipboard = sharpClipboard;

            _sharpClipboard.ClipboardChanged += (_, e) =>
            {
                if (_ehConfigRepository.MonitorClipboard && e.ContentType == SharpClipboard.ContentTypes.Text)
                    ProcessDataText(new HookParam() { Text = _sharpClipboard.ClipboardText });
            };
            _eventAggregator.SubscribeOnUIThread(this);
            textractorService.SelectedDataEvent += ProcessDataText;
            var meCabDicPath = Path.Combine(_ehConfigRepository.AppDataDir, "dic");
            if (Directory.Exists(meCabDicPath))
            {
                _meCabService.CreateTagger(meCabDicPath);
            }
        }


        private readonly IMeCabService _meCabService;
        private readonly EhConfigRepository _ehConfigRepository;
        private readonly ITranslatorFactory _translatorFactory;
        private readonly ITermDataService _termDataService;
        private readonly IDanmakuService _danmakuService;
        private readonly IEventAggregator _eventAggregator;
        private readonly SharpClipboard _sharpClipboard;

        private string _currentText = string.Empty;

        private string _pattern;

        private async void ProcessDataText(HookParam hp)
        {
            await Task.Run(() =>
            {
                _currentText = hp.Text;

                //if (_ehConfigRepository.EnableMeCab && _translatorFactory.GetEnabledTranslators().Count == 0)
                //    return ;

                // Refresh
                AppendTextsRefresh?.Invoke(nameof(GameDataService));

                // User define RegExp 
                if (_pattern != string.Empty)
                {
                    var list = Regex.Split(hp.Text, _pattern);
                    hp.Text = string.Join("", list);
                }

                // Clear ascii control characters
                hp.Text = new string(hp.Text.Select(c => c < ' ' ? '_' : c).ToArray()).Replace("_", string.Empty);
                // LineBreak 
                // Full-width space
                hp.Text = hp.Text.Replace("　", string.Empty);
                // Ruby like <.*?>

                if (hp.Text.Length > _ehConfigRepository.MaxAcceptTextLength)
                {
                    BindableTextItem?.Invoke(new BindableCollection<SingleTextItem>());
                    AppendTextReceived?.Invoke(Language.Strings.GameView_MaxLenthTip, string.Empty);
                    return;
                }

                //if (_ehConfigRepository.UseDanmaku)
                //{
                //    List<string> result = _danmakuService.QueryDanmaku(hp.Text);
                //    if (result.Count != 0)
                //        _eventAggregator.PublishOnUIThreadAsync(new DanmakuMessage { Danmaku = result});
                //}

                SourceTextReceived?.Invoke(hp.Text);

                // DeepL Extension
                if (_ehConfigRepository.PasteToDeepL)
                    SendTextToDeepL(hp.Text);

                // Process source japanese text
                if (_ehConfigRepository.EnableMeCab)
                {
                    BindableTextItem?.Invoke(Utils.BindableTextMaker(
                        hp.Text,
                        _ehConfigRepository,
                        _meCabService.MeCabWordUniDicEnumerable,
                        _ehConfigRepository.TextTemplateConfig));
                }

                //if (_ehConfigRepository.UseTermTable)
                //{ 
                //    hp.Text = _termDataService.ProcessText(hp.Text);
                //}

                // Process translations
                foreach (var translator in _translatorFactory.GetEnabledTranslators())
                {
                    Task.Run(async () =>
                    {
                        Stopwatch sw = new();
                        sw.Start();
                        var result = await translator.TranslateAsync(hp.Text, _ehConfigRepository.SrcTransLanguage,
                            _ehConfigRepository.TargetTransLanguage);
                        sw.Stop();

                        //if (_ehConfigRepository.UseTermTable)
                        //{
                        //    result = _termDataService.FinalText(result);
                        //}

                        if (result != string.Empty)
                        {
                            Log.Debug($"{translator.Name}: {result}");
                            AppendTextReceived?.Invoke(result, $"{translator.Name.I18N()} {sw.ElapsedMilliseconds}ms");
                        }
                    });
                }
            });
        }

        private static void SendTextToDeepL(string text)
        {
            Process[] temp = Process.GetProcessesByName("DeepL");
            if (temp.Length == 0)
                return;

            var deepLProc = temp[0];
            var handle = deepLProc.MainWindowHandle;
            NativeMethods.SwitchToThisWindow(handle);

            // Do SetText and Paste both
            new DeepLHelper(DataFormats.Text, text).Go();

            // Bring DeepL from taskbar tray to front
            if (NativeMethods.GetForegroundWindow() == handle) return;
            var deepLPath = deepLProc.MainModule?.FileName ?? string.Empty;
            if (deepLPath == string.Empty) return;
            Process.Start(deepLPath);
        }

        public Task HandleAsync(RegExpChangedMessage message, CancellationToken cancellationToken)
        {
            _pattern = message.RegExp;
            return Task.CompletedTask;
        }
    }
}