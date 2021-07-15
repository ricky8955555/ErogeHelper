using Caliburn.Micro;
using ErogeHelper.Model.Repository;
using ModernWpf.Controls;

namespace ErogeHelper.View.Dialog
{
    /// <summary>
    /// TencentMtDialog.xaml 的交互逻辑
    /// </summary>
    public partial class TencentMtDialog : ContentDialog
    {
        public TencentMtDialog()
        {
            InitializeComponent();

            EhConfigRepository ehConfigRepository = IoC.Get<EhConfigRepository>();

            PrimaryButtonClick += (_, _) =>
            {
                // Condition here cause "Enter" key will cross this
                if (IsPrimaryButtonEnabled)
                {
                    ehConfigRepository.TencentMtSecretId = SecretId.Text;
                    ehConfigRepository.TencentMtSecretKey = SecretKey.Text;
                }
            };

            Closing += (_, args) =>
            {
                // If the PrimaryButton is disabled, block the "Enter" key
                if (args.Result == ContentDialogResult.Primary && !IsPrimaryButtonEnabled)
                {
                    args.Cancel = true;
                }
            };

            SecretId.Text = ehConfigRepository.TencentMtSecretId;
            SecretKey.Text = ehConfigRepository.TencentMtSecretKey;
        }
    }
}
