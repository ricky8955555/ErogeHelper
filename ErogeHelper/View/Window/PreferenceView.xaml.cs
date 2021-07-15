using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;
using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.View.Page;
using ModernWpf.Controls;
using WindowPage = System.Windows.Controls.Page;

namespace ErogeHelper.View.Window
{
    /// <summary>
    /// PreferenceView.xaml 的交互逻辑
    /// </summary>
    public partial class PreferenceView
    {
        public PreferenceView()
        {
            InitializeComponent();

            _eventAggregator = IoC.Get<IEventAggregator>();

            ContentFrame.Navigated += OnNavigated;
            Loaded += (_, _) =>
            {
                _pagesList = new List<(string Tag, WindowPage PageInstance)>
                {
                    ("general", new GeneralPage()),
                    ("mecab", new MeCabPage()),
                    ("hook", new HookPage()),
                    ("trans", new TransPage()),

                    ("about", new AboutPage()),
                };

                _pagesList.ForEach(item => _eventAggregator.SubscribeOnUIThread(item.PageInstance));

                PageNavigate("general");
            };
        }

        private readonly IEventAggregator _eventAggregator;

        private List<(string Tag, WindowPage PageInstance)>? _pagesList;

        private readonly List<(string Tag, Type PageType, PageName PageName)> _pages = new()
        {
            ("general", typeof(GeneralPage), PageName.General),
            ("mecab", typeof(MeCabPage), PageName.MeCab),
            ("hook", typeof(HookPage), PageName.Hook),
            ("trans", typeof(TransPage), PageName.Trans),

            ("about", typeof(AboutPage), PageName.About),
        };

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is null)
                return;

            var navItemTag = args.SelectedItemContainer.Tag.ToString()!;
            PageNavigate(navItemTag);
        }

        private void PageNavigate(string navItemTag)
        {
            var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            var instanceItem = _pagesList?.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            Type pageType = item.PageType;
            WindowPage pageInstance = (instanceItem ?? throw new InvalidOperationException()).PageInstance;

            // if not same page
            if (pageType != null && ContentFrame.SourcePageType != pageType)
            {
                // Clear Journal info
                while (ContentFrame.CanGoBack)
                {
                    ContentFrame.RemoveBackEntry();
                }
                // https://github.com/Kinnara/ModernWpf/issues/329
                ContentFrame.Navigate(pageInstance, null);
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;
            Type sourcePageType = ContentFrame.SourcePageType;
            // Set header text
            if (sourcePageType is not null)
            {
                var (tag, _, pageName) = _pages.FirstOrDefault(p => p.PageType == sourcePageType);

                NavView.SelectedItem = NavView.FooterMenuItems
                    .OfType<NavigationViewItem>().
                    FirstOrDefault(n => n.Tag.Equals(tag)) ??
                    NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(n => n.Tag.Equals(tag));

                HeaderBlock.Text =
                    ((NavigationViewItem)NavView.SelectedItem!).Content?.ToString();

                _eventAggregator.PublishOnUIThreadAsync(new PageNavigatedMessage(pageName));
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _pagesList?.ForEach(item => _eventAggregator.Unsubscribe(item.PageInstance));
            // only use when PreferenceViewModel register as transition
            //_eventAggregator.Unsubscribe((DataContext as PreferenceViewModel)?.GeneralViewModel);
            //_eventAggregator.Unsubscribe((DataContext as PreferenceViewModel)?.TransViewModel);
            //_eventAggregator.Unsubscribe((DataContext as PreferenceViewModel)?.HookViewModel);
        }
    }
}
