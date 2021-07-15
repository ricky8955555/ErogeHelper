using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using ErogeHelper.ViewModel.Window;

namespace ErogeHelper.ViewModel.Page
{
    public class TransViewModel : PropertyChangedBase, IHandle<RefreshTranslatorEnableSwitch>
    {
        public TransViewModel(
            IEventAggregator eventAggregator,
            EhConfigRepository ehConfigRepository,
            ITranslatorFactory translatorFactory,
            IWindowManager windowManager)
        {
            _eventAggregator = eventAggregator;
            _ehConfigRepository = ehConfigRepository;
            _translatorFactory = translatorFactory;
            _windowManager = windowManager;

            _eventAggregator.SubscribeOnUIThread(this);
            SrcLanguageList = SrcLanguageListInit();
            SelectedSrcLang = _ehConfigRepository.SrcTransLanguage;
            TargetLanguageList = TargetLanguageListRefresh(out _);
            SelectedTarLang = _ehConfigRepository.TargetTransLanguage;
            RefreshTranslatorList();
        }

        private readonly IEventAggregator _eventAggregator;
        private readonly EhConfigRepository _ehConfigRepository;
        private readonly ITranslatorFactory _translatorFactory;
        private readonly IWindowManager _windowManager;

        private TransLanguage _selectedSrcLang;
        private TransLanguage _selectedTarLang;
        private BindableCollection<LanguageComboBoxItem> _targetLanguageList = new();

        public bool UserTermStatus
        {
            get => _ehConfigRepository.UseTermTable;
            set => _ehConfigRepository.UseTermTable = value;
        }

        public async void OpenTermList() => await _windowManager.ShowDialogFromIoCAsync<TermViewModel>();

        public BindableCollection<LanguageComboBoxItem> SrcLanguageList { get; }

        public TransLanguage SelectedSrcLang
        {
            get => _selectedSrcLang;
            set { _selectedSrcLang = value; NotifyOfPropertyChange(() => SelectedSrcLang); }
        }

        public void SrcLanguageChanged()
        {
            _ehConfigRepository.SrcTransLanguage = SelectedSrcLang;

            var markTarLang = SelectedTarLang;
            TargetLanguageList = TargetLanguageListRefresh(out Dictionary<TransLanguage, bool> hackLangDict);
            // To avoid if last target language not include in new target language list
            if (!hackLangDict.ContainsKey(markTarLang))
            {
                using var enumerator = hackLangDict.GetEnumerator();
                enumerator.MoveNext();
                var firstLang = enumerator.Current.Key;
                Log.Debug(firstLang.ToString());
                SelectedTarLang = firstLang;
                _ehConfigRepository.TargetTransLanguage = SelectedTarLang;
            }
            else
            {
                SelectedTarLang = markTarLang;
            }

            RefreshTranslatorList(true);
        }

        public BindableCollection<LanguageComboBoxItem> TargetLanguageList
        {
            get => _targetLanguageList;
            set { _targetLanguageList = value; NotifyOfPropertyChange(() => TargetLanguageList); }
        }

        public TransLanguage SelectedTarLang
        {
            get => _selectedTarLang;
            set { _selectedTarLang = value; NotifyOfPropertyChange(() => SelectedTarLang); }
        }

        public void TargetLanguageChanged()
        {
            _ehConfigRepository.TargetTransLanguage = SelectedTarLang;
            RefreshTranslatorList(true);
        }

        public BindableCollection<TranslatorItem> TranslatorList { get; set; } = new();

        public async void OpenTranslatorDialog(TranslatorItem translator) =>
            await _eventAggregator.PublishOnUIThreadAsync(new TranslatorDialogMessage(translator.NameEnum));

        private void RefreshTranslatorList(bool reset = false)
        {
            if (reset)
            {
                _translatorFactory.Translators.ForEach(translator => translator.IsEnable = false);
            }
            TranslatorList.Clear();

            foreach (var translatorItem in
                from translator in _translatorFactory.Translators
                where translator.SupportSrcLang.Contains(SelectedSrcLang) && translator.SupportDesLang.Contains(SelectedTarLang)
                select new TranslatorItem
                {
                    CanBeEnable = translator.UnLock,
                    Enable = translator.IsEnable,
                    IconPath = translator.IconPath,
                    TransName = translator.Name.I18N(),
                    NameEnum = translator.Name,
                    CanEdit = !translator.NeedEdit,
                })
            {
                TranslatorList.Add(translatorItem);
            }
        }

        private BindableCollection<LanguageComboBoxItem> SrcLanguageListInit()
        {
            BindableCollection<LanguageComboBoxItem> langList = new();
            foreach (var lang in new HashSet<TransLanguage>(
                _translatorFactory.Translators.SelectMany(translator => translator.SupportSrcLang)))
            {
                langList.Add(new LanguageComboBoxItem(lang));
            }
            return langList;
        }

        private BindableCollection<LanguageComboBoxItem> TargetLanguageListRefresh(out Dictionary<TransLanguage, bool> tmpMark)
        {
            BindableCollection<LanguageComboBoxItem> langList = new();
            tmpMark = new Dictionary<TransLanguage, bool>();
            foreach (var lang in
                _translatorFactory.Translators
                    .Where(translator => translator.SupportSrcLang.Contains(SelectedSrcLang))
                    .SelectMany(translator => translator.SupportDesLang))
            {
                if (!tmpMark.ContainsKey(lang) && lang != SelectedSrcLang)
                {
                    langList.Add(new LanguageComboBoxItem(lang));
                    tmpMark[lang] = true;
                }
            }
            return langList;
        }

        public Task HandleAsync(RefreshTranslatorEnableSwitch message, CancellationToken cancellationToken)
        {
            var translator = _translatorFactory.GetTranslator(message.Name);
            TranslatorList.Single(it => it.NameEnum == message.Name).CanBeEnable = translator.UnLock;

            return Task.CompletedTask;
        }

#pragma warning disable CS8618
        public TransViewModel() { }
    }
}