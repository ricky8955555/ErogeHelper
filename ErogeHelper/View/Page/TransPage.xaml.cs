using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.View.Dialog;
using ErogeHelper.View.Window;
using ErogeHelper.ViewModel.Window;
using ModernWpf.Controls;

namespace ErogeHelper.View.Page
{
    /// <summary>
    /// TransPage.xaml 的交互逻辑
    /// </summary>
    public partial class TransPage : IHandle<TranslatorDialogMessage>
    {
        public TransPage()
        {
            InitializeComponent();
            DataContext =
                ((PreferenceViewModel)Application.Current.Windows.OfType<PreferenceView>().Single().DataContext)
                .TransViewModel;
            _eventAggregator = IoC.Get<IEventAggregator>();
        }

        private readonly IEventAggregator _eventAggregator;

        public async Task HandleAsync(TranslatorDialogMessage transDialog, CancellationToken cancellationToken)
        {
            ContentDialogResult? result = null;

            switch (transDialog.Name)
            {
                case TranslatorName.BaiduApi:
                    result = await new BaiduApiDialog().ShowAsync();
                    break;
                case TranslatorName.Caiyun:
                    result = await new CaiyunDialog().ShowAsync();
                    break;
                case TranslatorName.NiuTrans:
                    result = await new NiuTransDialog().ShowAsync();
                    break;
                case TranslatorName.TencentMt:
                    result = await new TencentMtDialog().ShowAsync();
                    break;
                default: throw new InvalidOperationException($"No target dialog {transDialog.Name}");
            }

            if (result == ContentDialogResult.Primary)
            {
                await _eventAggregator.PublishOnUIThreadAsync(
                    new RefreshTranslatorEnableSwitch(transDialog.Name), CancellationToken.None);
            }
        }
    }
}
