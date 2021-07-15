using Caliburn.Micro;
using ErogeHelper.Model.Repository;
using ModernWpf.Controls;

namespace ErogeHelper.View.Dialog
{
    /// <summary>
    /// NiuTransDialog.xaml 的交互逻辑
    /// </summary>
    public partial class NiuTransDialog : ContentDialog
    {
        public NiuTransDialog()
        {
            InitializeComponent();

            EhConfigRepository ehConfigRepository = IoC.Get<EhConfigRepository>();
            ApiKey.Text = ehConfigRepository.NiuTransApiKey;

            PrimaryButtonClick += (_, _) =>
            {
                // Condition here cause "Enter" key will cross this
                if (IsPrimaryButtonEnabled)
                {
                    ehConfigRepository.NiuTransApiKey = ApiKey.Text.Trim();
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
        }
    }
}
