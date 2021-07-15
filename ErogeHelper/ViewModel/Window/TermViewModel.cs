using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Entity.NotifyItem;

namespace ErogeHelper.ViewModel.Window
{
    public class TermViewModel : PropertyChangedBase
    {
        public TermViewModel(
            ITermDataService termDataService,
            ITranslatorFactory translatorFactory,
            EhConfigRepository ehConfigRepository)
        {
            _termDataService = termDataService;
            _translatorFactory = translatorFactory;
            _sourceLanguage = ehConfigRepository.SrcTransLanguage;
            _targetLanguage = ehConfigRepository.TargetTransLanguage;

            TermList = _termDataService.GetBindableTermItems();
            _translatorFactory.AllInstance.ForEach(translator =>
            {
                if (translator.UnLock &&
                    translator.SupportSrcLang.Contains(_sourceLanguage) &&
                    translator.SupportDesLang.Contains(_targetLanguage))
                {
                    TranslatorItems.Add(new TranslatorComboBoxItem()
                    {
                        TranslatorName = translator.Name,
                        Icon = Utils.LoadBitmapFromResource(translator.IconPath),
                        Title = translator.Name.I18N()
                    });
                }
            });
            SourceLanguageLabel = _sourceLanguage.ToString();
            TargetLanguageLabel = _targetLanguage.ToString();
        }

        private readonly ITermDataService _termDataService;
        private readonly ITranslatorFactory _translatorFactory;
        private readonly TransLanguage _sourceLanguage;
        private readonly TransLanguage _targetLanguage;

        private bool _translating;
        private TranslatorComboBoxItem? _selectedTranslator;
        private string _translatedResult = string.Empty;
        private string _finalResult = string.Empty;

        public string SourceWord { get; set; } = string.Empty;

        public string TargetWord { get; set; } = string.Empty;

        public void AddWord()
        {
            if (_termDataService.GetDictionary().ContainsKey(SourceWord))
            {
                new ModernWpf.Controls.ContentDialog()
                {
                    Title = "Word already exist",
                    CloseButtonText = Language.Strings.Common_Close,
                    DefaultButton = ModernWpf.Controls.ContentDialogButton.Close
                }.ShowAsync();
                return;
            }

            TermList.Add(new TermItem(SourceWord, TargetWord));
            _termDataService.AddTermToDatabase(SourceWord, TargetWord);
        }

        public BindableCollection<TermItem> TermList { get; set; }

        public async void DeleteWord(TermItem term)
        {
            var target = TermList.Single(it => it.SourceWord.Equals(term.SourceWord));
            TermList.Remove(target);
            await _termDataService.DeleteTermInDatabaseAsync(term);
        }

        public string PendingToTranslateText { get; set; } = "【爽】「悠真くんを攻略すれば２１０円か。なるほどなぁ…」";

        public BindableCollection<TranslatorComboBoxItem> TranslatorItems { get; set; } = new();

        public TranslatorComboBoxItem? SelectedTranslator
        {
            get => _selectedTranslator;
            set { _selectedTranslator = value; NotifyOfPropertyChange(() => CanTestTranslate); }
        }

        public string SourceLanguageLabel { get; set; }
        public string TargetLanguageLabel { get; set; }

        public bool CanTestTranslate => SelectedTranslator is not null && !_translating;
        public async void TestTranslate()
        {
            _translating = true;
            NotifyOfPropertyChange(() => CanTestTranslate);

            if (SelectedTranslator is not null)
            {
                TranslatedResult = await _translatorFactory.GetTranslator(SelectedTranslator.TranslatorName)
                    .TranslateAsync(PendingToTranslateText, _sourceLanguage, _targetLanguage);

                var count = 1234;
                Dictionary<int, string> countSourceWordDic = new();
                var tmpStr = PendingToTranslateText;
                TermList
                    .Where(term => tmpStr.Contains(term.SourceWord))
                    .ForEach(term => 
                    {
                        tmpStr = tmpStr.Replace(term.SourceWord, $"{{{count}}}");
                        countSourceWordDic.Add(count, term.SourceWord);
                        count--;
                    });

                var tmpStrTranslatedResult = await _translatorFactory.GetTranslator(SelectedTranslator.TranslatorName)
                    .TranslateAsync(tmpStr, _sourceLanguage, _targetLanguage);
                try
                {
                    for(var i = 0; i < countSourceWordDic.Count; i++)
                    {
                        tmpStrTranslatedResult = tmpStrTranslatedResult.Replace(
                            $"{{{++count}}}", 
                            _termDataService.GetDictionary()[countSourceWordDic[count]]);
                    }
                    FinalResult = tmpStrTranslatedResult;
                }
                catch(Exception ex)
                {
                    Log.Error(ex);
                }
            }

            _translating = false;
            NotifyOfPropertyChange(() => CanTestTranslate);
        }

        public string TranslatedResult
        {
            get => _translatedResult;
            set { _translatedResult = value; NotifyOfPropertyChange(() => TranslatedResult); }
        }

        public string FinalResult 
        { 
            get => _finalResult; 
            set { _finalResult = value; NotifyOfPropertyChange(() => FinalResult); }
        }

#pragma warning disable CS8618
        public TermViewModel() { }
    }
}