using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Dictionary;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using ErogeHelper.ViewModel.Entity.NotifyItem;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ErogeHelper.ViewModel.Control
{
    public class CardViewModel : PropertyChangedBase
    {
        public CardViewModel(EhConfigRepository ehConfigRepository, IDictionaryFactory dictionaryFactory)
        {
            _ehConfigRepository = ehConfigRepository;

            _mojiTabItemVisible = _ehConfigRepository.MojiDictEnable ? Visibility.Visible : Visibility.Collapsed;
            _jishoTabItemVisible = _ehConfigRepository.JishoDictEnable ? Visibility.Visible : Visibility.Collapsed;
            _mojiDict = dictionaryFactory.GetDictInstance(DictType.Moji) as MojiDict ?? throw new Exception();
            _jishoDict = dictionaryFactory.GetDictInstance(DictType.Jisho) as JishoDict ?? throw new Exception();
        }

        private readonly EhConfigRepository _ehConfigRepository;
        private readonly MojiDict _mojiDict;
        private readonly JishoDict _jishoDict;

        private string _word = string.Empty;
        private string _totalText = string.Empty;
        private int _mojiSelectedIndex;
        private Visibility _dictTabVisibility = Visibility.Collapsed;
        private Visibility _mojiTabItemVisible;
        private Visibility _jishoTabItemVisible;
        private bool _isOpen;
        private UIElement? _placementTarget;
        private CancellationTokenSource _cancelSource = new();

        /// <summary>
        /// The main text in the TextBox
        /// </summary>
        public string Word
        {
            get => _word;
            set { _word = value; NotifyOfPropertyChange(() => Word); }
        }

        /// <summary>
        /// Total sentence
        /// </summary>
        public string TotalText
        {
            get => _totalText;
            set { _totalText = value; NotifyOfPropertyChange(() => TotalText); }
        }

        /// <summary>
        /// Send selected text to current word
        /// </summary>
        public void SendSelectedText(TextBox textBox)
        {
            // * use Attach Property binding to SelectionLength, SelectedText
            // * use System.Windows.Controls.TextBox
            // * send message to CardControl.cs (give TextBox a Name, operation in View, do Search() in VM)
            if (textBox.SelectedText == string.Empty)
                return;

            Word = textBox.SelectedText;
            Search();
            textBox.SelectionLength = 0;
        }

        public UIElement? PlacementTarget
        {
            get => _placementTarget;
            set { _placementTarget = value; NotifyOfPropertyChange(() => PlacementTarget); }
        }

        /// <summary>
        /// Search entry point
        /// </summary>
        public async void Search()
        {
            SetupSearch();

            _cancelSource.Cancel();
            _cancelSource = new CancellationTokenSource();
            await StartSearchWordTasksAsync(_cancelSource.Token);
        }

        public Visibility DictTabVisibility
        {
            get => _dictTabVisibility;
            set { _dictTabVisibility = value; NotifyOfPropertyChange(() => DictTabVisibility); }
        }
        public Visibility MojiTabItemVisible
        {
            get => _mojiTabItemVisible;
            set { _mojiTabItemVisible = value; NotifyOfPropertyChange(() => MojiTabItemVisible); }
        }
        public Visibility JishoTabItemVisible
        {
            get => _jishoTabItemVisible;
            set { _jishoTabItemVisible = value; NotifyOfPropertyChange(() => JishoTabItemVisible); }
        }

        private void SetupSearch()
        {
            MojiCollection.Clear();
            JishoCollection.Clear();

            MojiTabItemVisible = _ehConfigRepository.MojiDictEnable ? Visibility.Visible : Visibility.Collapsed;
            JishoTabItemVisible = _ehConfigRepository.JishoDictEnable ? Visibility.Visible : Visibility.Collapsed;
            DictTabVisibility = _ehConfigRepository.MojiDictEnable || _ehConfigRepository.JishoDictEnable
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // XXX: Is this weird?
        private async Task StartSearchWordTasksAsync(CancellationToken token)
        {
            if (_ehConfigRepository.MojiDictEnable)
            {
                await Task.Run(async () => await MojiSearchAsync(token), token);
            }
            if (_ehConfigRepository.JishoDictEnable)
            {
                await Task.Run(async () => await JishoSearchAsync(token), token);
            }
        }

        #region MojiDict

        public BindableCollection<MojiItem> MojiCollection { get; set; } = new();

        public int MojiSelectedIndex
        {
            get => _mojiSelectedIndex;
            set
            {
                _mojiSelectedIndex = value;
                NotifyOfPropertyChange(() => MojiSelectedIndex);
                if (value != -1 && value != 0 && MojiCollection[value].ExpanderCollection.Count == 0)
                {
                    _ = MojiFetchWord(MojiCollection[value].TarId);
                }
            }
        }

        internal async Task MojiSearchAsync(CancellationToken token)
        {
            var searchResponse = await _mojiDict.SearchAsync(Word);

            if (token.IsCancellationRequested)
                return;

            if (searchResponse.Error != string.Empty)
            {
                // Exception happened
                MojiCollection.Add(new MojiItem
                {
                    Header = "Error",
                    TarId = string.Empty
                });
                MojiCollection[0].ExpanderCollection.Add(
                    new MojiExpanderItem
                    {
                        Header = "",
                        ExampleCollection = new BindableCollection<MojiExpanderItem.Example>()
                    }
                );
                MojiSelectedIndex = 0;

                return;
            }

            if (searchResponse.Result.Words.Count == 0)
            {
                // Moji no result
                MojiCollection.Add(new MojiItem
                {
                    Header = "None",
                    TarId = string.Empty
                });
                MojiCollection[0].ExpanderCollection.Add(new MojiExpanderItem
                {
                    Header = "No Search Result",
                    ExampleCollection = new BindableCollection<MojiExpanderItem.Example>()
                });

                MojiSelectedIndex = 0;

                return;
            }

            Log.Debug($"Received search result of {searchResponse.Result.OriginalSearchText}");

            foreach (var item in searchResponse.Result.Words)
            {
                // XXX: CM error occurred here
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MojiCollection.Add(new MojiItem
                    {
                        //Header = $"{item.spell} | {item.pron}{item.accent}",
                        Header = $"{item.Spell}{item.Accent}",
                        TarId = item.ObjectId
                    });
                });
            }

            MojiSelectedIndex = 0;
            // Fetch first page
            await MojiFetchWord(string.Empty, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch word, also do mess work to MojiCollection
        /// </summary>
        /// <param name="wordId">Empty means fetch first page</param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async Task MojiFetchWord(string wordId, CancellationToken token = default)
        {
            // XXX: Dirty(?) check with `token.IsCancellationRequested` or `MojiCollection.Count` to avoid thread not
            // safe
            if (token.IsCancellationRequested)
                return;

            if (string.IsNullOrWhiteSpace(wordId))
            {
                wordId = MojiCollection[0].TarId;
            }

            var wordDetail = await _mojiDict.FetchAsync(wordId).ConfigureAwait(false);

            if (token.IsCancellationRequested) // and error?
                return;

            // 遍历 找wordDetail对应Word所在序列
            // QUESTION: MojiCollection 在遍历期间仍然被其他线程所修改?
            foreach (var mojiItem in MojiCollection)
            {
                if (token.IsCancellationRequested)
                    return;
                if (!mojiItem.TarId.Equals(wordDetail.Result.Word.ObjectId))
                    continue;

                // process information
                mojiItem.Pron = wordDetail.Result.Word.Pron;
                if (token.IsCancellationRequested)
                    return;
                mojiItem.Title = wordDetail.Result.Details[0].Title;

                foreach (var subDetail in wordDetail.Result.Subdetails)
                {
                    var header = subDetail.Title;
                    BindableCollection<MojiExpanderItem.Example> examples = new();
                    foreach (var example in
                        wordDetail.Result.Examples.Where(example => subDetail.ObjectId == example.SubdetailsId))
                    {
                        examples.Add(new MojiExpanderItem.Example { Title = example.Title, Trans = example.Trans });
                    }
                    if (token.IsCancellationRequested)
                        return;
                    mojiItem.ExpanderCollection.Add(new MojiExpanderItem { Header = header, ExampleCollection = examples });
                }
            }
        }

        #endregion

        public BindableCollection<JishoItem> JishoCollection { get; set; } = new();

        internal async Task JishoSearchAsync(CancellationToken token = default)
        {
            var result = await _jishoDict.SearchWordAsync(Word);

            if (token.IsCancellationRequested)
                return;

            foreach (var jishoItem in result.DataList!)
            {
                BindableCollection<JishoItem.Detail> details = new();
                var senseCount = 1;
                foreach (var sense in jishoItem.Senses!)
                {
                    BindableCollection<JishoItem.Detail.Link> links = new();
                    foreach (var link in sense.Links!)
                    {
                        links.Add(new JishoItem.Detail.Link { Text = link.Text, HyperLink = link.Url });
                    }

                    details.Add(new JishoItem.Detail
                    {
                        PartOfSpeech = string.Join(", ", sense.PartsOfSpeech!),
                        Explanation = $"{senseCount++}. " + string.Join("; ", sense.EnglishDefinitions!),
                        Links = links
                    });
                }

                JishoCollection.Add(new JishoItem
                {
                    Ruby = '(' + jishoItem.JapaneseList![0].Reading + ')',
                    Word = jishoItem.JapaneseList[0].Word,
                    CommonWord = jishoItem.IsCommon ? "Common" : string.Empty,
                    JlptLevel = jishoItem.Jlpt!.Length != 0 ? jishoItem.Jlpt[0] : string.Empty,
                    WanikanaLevel = jishoItem.Tags!.Length != 0 ? jishoItem.Tags[0] : string.Empty,
                    Details = details
                });
            }
        }

        public bool IsOpen
        {
            get => _isOpen;
            set { _isOpen = value; NotifyOfPropertyChange(() => IsOpen); }
        }

        public void CloseCard() => IsOpen = false;

        public void OpenWeblioLink()
        {
            Utils.OpenUrl($@"https://www.weblio.jp/content/{Word}");
            IsOpen = false;
        }

        public void OpenKotobankLink()
        {
            Utils.OpenUrl($@"https://kotobank.jp/gs/?q={Word}");
            IsOpen = false;
        }

#pragma warning disable CS8618
        public CardViewModel() { }
    }
}