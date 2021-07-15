using Caliburn.Micro;
using ErogeHelper.Common.Constraint;
using ErogeHelper.Model.Repository;
using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;

namespace ErogeHelper.View.Dialog
{
    /// <summary>
    /// CaiyunDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CaiyunDialog : ContentDialog
    {
        public CaiyunDialog()
        {
            InitializeComponent();

            _ehConfigRepository = IoC.Get<EhConfigRepository>();

            Token.Text = _ehConfigRepository.CaiyunToken;
            Token_TextChanged(null!, null!);
        }

        private readonly EhConfigRepository _ehConfigRepository;

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (IsPrimaryButtonEnabled)
            {
                _ehConfigRepository.CaiyunToken = Token.Text;
            }
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary && !IsPrimaryButtonEnabled)
            {
                args.Cancel = true;
            }
        }

        private void Token_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsPrimaryButtonEnabled = Token.Text != string.Empty;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Token.Text = DefaultConfigValuesStore.CaiyunDefaultToken;
        }
    }
}
