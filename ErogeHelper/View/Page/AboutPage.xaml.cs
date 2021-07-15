using System.Linq;
using System.Windows;
using ErogeHelper.View.Window;
using ErogeHelper.ViewModel.Window;

namespace ErogeHelper.View.Page
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage
    {
        public AboutPage()
        {
            InitializeComponent();
            DataContext =
                ((PreferenceViewModel)Application.Current.Windows.OfType<PreferenceView>().Single().DataContext)
                .AboutViewModel;
        }
    }
}
