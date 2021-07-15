using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Translator;
using ErogeHelper.Model.Repository;
using ModernWpf.Controls;

namespace ErogeHelper.View.Dialog
{
    /// <summary>
    /// BaiduApiDialog.xaml 的交互逻辑
    /// </summary>
    public partial class BaiduApiDialog : ContentDialog
    {
        public BaiduApiDialog()
        {
            InitializeComponent();

            _ehConfigRepository = IoC.Get<EhConfigRepository>();

            Appid.Text = _ehConfigRepository.BaiduApiAppid;
            SecretKey.Text = _ehConfigRepository.BaiduApiSecretKey;
            IsPrimaryButtonEnabled = false;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        private async void TestApiButton_Click(object sender, RoutedEventArgs e)
        {
            TestApiButton.IsEnabled = false;
            Appid.IsEnabled = false;
            SecretKey.IsEnabled = false;

            var translator = new BaiduApiTranslator(_ehConfigRepository)
            {
                AppId = Appid.Text,
                SecretKey = SecretKey.Text
            };
            string result = await translator.TranslateAsync("頼りになる", TransLanguage.Japanese, TransLanguage.English);
            if (result.Equals("Rely on"))
            {
                IsPrimaryButtonEnabled = true;
                TestStatus.Text = "√";
                TestStatus.Foreground = Brushes.Green;
            }
            else
            {
                IsPrimaryButtonEnabled = false;
                TestStatus.Text = "X";
                TestStatus.Foreground = Brushes.Red;
                Appid.IsEnabled = true;
                SecretKey.IsEnabled = true;
            }

            TestApiButton.IsEnabled = true;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (IsPrimaryButtonEnabled)
            {
                _ehConfigRepository.BaiduApiAppid = Appid.Text;
                _ehConfigRepository.BaiduApiSecretKey = SecretKey.Text;
            }
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary && !IsPrimaryButtonEnabled)
            {
                args.Cancel = true;
            }
        }
    }
}
