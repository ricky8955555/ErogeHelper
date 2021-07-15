using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.ViewModel.Page
{
    public class MeCabViewModel : PropertyChangedBase
    {
        public MeCabViewModel(
            IEventAggregator eventAggregator,
            IMeCabService meCabService,
            EhConfigRepository ehConfigRepository,
            IGameDataService gameDataService)
        {
            _eventAggregator = eventAggregator;
            _meCabService = meCabService;
            _ehConfigRepository = ehConfigRepository;
            _gameDataService = gameDataService;

            _kanaDefault = _ehConfigRepository.KanaDefault;
            _kanaTop = _ehConfigRepository.KanaTop;
            _kanaBottom = _ehConfigRepository.KanaBottom;
            _romaji = _ehConfigRepository.Romaji;
            _hiragana = _ehConfigRepository.Hiragana;
            _katakana = _ehConfigRepository.Katakana;
            CanEnableMecab = File.Exists(Path.Combine(_ehConfigRepository.AppDataDir, "dic", "char.bin"));
        }

        private readonly IEventAggregator _eventAggregator;
        private readonly EhConfigRepository _ehConfigRepository;
        private readonly IMeCabService _meCabService;
        private readonly IGameDataService _gameDataService;

        private bool _kanaDefault;
        private bool _kanaTop;
        private bool _kanaBottom;
        private bool _romaji;
        private bool _hiragana;
        private bool _katakana;
        private bool _canEnableMecab;
        private bool _chooseDicButtonEnabled = true;

        public bool CanEnableMecab
        {
            get => _canEnableMecab;
            set { _canEnableMecab = value; NotifyOfPropertyChange(() => CanEnableMecab); }
        }

        public bool ChooseDicButtonEnabled
        {
            get => _chooseDicButtonEnabled;
            set { _chooseDicButtonEnabled = value; NotifyOfPropertyChange(() => ChooseDicButtonEnabled); }
        }

        public async void ChooseMecabDic()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".eh",
                Filter = "IpaDic.eh file (*.eh) | *.eh"
            };

            var result = dlg.ShowDialog();

            if (result is not true)
                return;

            ChooseDicButtonEnabled = false;
            string filename = dlg.FileName;
            var dicPath = Path.Combine(_ehConfigRepository.AppDataDir, "dic");

            #region Make Dialog
            var progress = new ModernWpf.Controls.ProgressRing
            {
                IsActive = true,
                Width = 80,
                Height = 80,
            };

            var dialogCanClose = false;
            var dialog = new ModernWpf.Controls.ContentDialog
            {
                Content = progress,
            };
            dialog.Closing += (sender, args) =>
            {
                // Block Enter key and PrimaryButton, SecondaryButton, Escape key
                if (args.Result == ModernWpf.Controls.ContentDialogResult.Primary ||
                    args.Result == ModernWpf.Controls.ContentDialogResult.Secondary ||
                    args.Result == ModernWpf.Controls.ContentDialogResult.None && !dialogCanClose)
                {
                    args.Cancel = true;
                }
                // Only let CloseButton and dialog.Hide() method go
                //if (args.Result == ContentDialogResult.None)
            };
            #endregion

            var dialogTask = dialog.ShowAsync();

            await Task.Run(() => ZipFile.ExtractToDirectory(filename, dicPath)).ConfigureAwait(false);
            _meCabService.CreateTagger(dicPath);
            File.Delete(filename);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                progress.IsActive = false;
                dialogCanClose = true;
                dialog.Hide();
            });

            CanEnableMecab = true;
            Log.Info("Loaded mecab-dic");
        }

        public bool MeCabToggle
        {
            get => _ehConfigRepository.EnableMeCab;
            set
            {
                _ehConfigRepository.EnableMeCab = value;

                _gameDataService.RefreshCurrentJapanese();
                _eventAggregator.PublishOnUIThreadAsync(value is true
                    ? new JapaneseVisibleMessage { IsShowed = true }
                    : new JapaneseVisibleMessage { IsShowed = false });

                NotifyOfPropertyChange(() => MeCabToggle);
            }
        }


        public bool KanaDefault
        {
            get => _kanaDefault;
            set
            {
                // Same value return
                if (value == KanaDefault)
                    return;
                // Set memory value
                _kanaDefault = value;
                // React change in view and save to local
                if (value)
                {
                    ChangeSourceTextTemplate(TextTemplateType.OutLineDefault);
                    _ehConfigRepository.KanaDefault = true;
                    _ehConfigRepository.KanaTop = false;
                    _ehConfigRepository.KanaBottom = false;
                }
            }
        }
        public bool KanaTop
        {
            get => _kanaTop;
            set
            {
                if (value == KanaTop)
                    return;
                _kanaTop = value;
                if (value)
                {
                    ChangeSourceTextTemplate(TextTemplateType.OutLineKanaTop);
                    _ehConfigRepository.KanaDefault = false;
                    _ehConfigRepository.KanaTop = true;
                    _ehConfigRepository.KanaBottom = false;
                }
            }
        }
        public bool KanaBottom
        {
            get => _kanaBottom;
            set
            {
                if (value == _kanaBottom)
                    return;
                _kanaBottom = value;
                if (value)
                {
                    ChangeSourceTextTemplate(TextTemplateType.OutLineKanaBottom);
                    _ehConfigRepository.KanaDefault = false;
                    _ehConfigRepository.KanaTop = false;
                    _ehConfigRepository.KanaBottom = true;
                }
            }
        }
        public bool MojiVertical { get; set; }

        private void ChangeSourceTextTemplate(TextTemplateType type)
        {
            _ehConfigRepository.TextTemplateConfig = type;
            _gameDataService.RefreshCurrentJapanese();
        }

        public bool Romaji
        {
            get => _romaji;
            set
            {
                if (value == _romaji)
                    return;
                _romaji = value;
                if (value)
                {
                    _ehConfigRepository.Romaji = true;
                    _ehConfigRepository.Hiragana = false;
                    _ehConfigRepository.Katakana = false;
                    _gameDataService.RefreshCurrentJapanese();
                }
            }
        }
        public bool Hiragana
        {
            get => _hiragana;
            set
            {
                if (value == _hiragana)
                    return;
                _hiragana = value;
                if (value)
                {
                    _ehConfigRepository.Romaji = false;
                    _ehConfigRepository.Hiragana = true;
                    _ehConfigRepository.Katakana = false;
                    _gameDataService.RefreshCurrentJapanese();
                }
            }
        }
        public bool Katakana
        {
            get => _katakana;
            set
            {
                if (value == _katakana)
                    return;
                _katakana = value;
                if (value)
                {
                    _ehConfigRepository.Romaji = false;
                    _ehConfigRepository.Hiragana = false;
                    _ehConfigRepository.Katakana = true;
                    _gameDataService.RefreshCurrentJapanese();
                }
            }
        }

        public bool MojiDictToggle
        {
            get => _ehConfigRepository.MojiDictEnable;
            set { _ehConfigRepository.MojiDictEnable = value; NotifyOfPropertyChange(() => MojiDictToggle); }
        }

        public bool JishoDictToggle
        {
            get => _ehConfigRepository.JishoDictEnable;
            set { _ehConfigRepository.JishoDictEnable = value; NotifyOfPropertyChange(() => JishoDictToggle); }
        }

#pragma warning disable 8618
        public MeCabViewModel() { }
    }
}