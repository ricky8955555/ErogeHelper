using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.View.Dialog;

namespace ErogeHelper.View.Window
{
    /// <summary>
    /// SelectProcessView.xaml 的交互逻辑
    /// </summary>
    public partial class SelectProcessView : IHandle<ViewActionMessage>
    {
        public SelectProcessView()
        {
            InitializeComponent();

            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.SubscribeOnUIThread(this);
        }

        private readonly IEventAggregator _eventAggregator;

        public async Task HandleAsync(ViewActionMessage message, CancellationToken cancellationToken)
        {
            if (message.WindowType == GetType())
            {
                switch (message.Action)
                {
                    case ViewAction.Hide:
                        Hide();
                        break;
                    case ViewAction.Close:
                        Close();
                        break;
                    case ViewAction.OpenDialog:
                        if (message.DialogType == ModernDialog.SelectProcessTip)
                            await new SelectProcessTipDialog(message.ExtraInfo).ShowAsync().ConfigureAwait(false);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        protected override void OnClosed(EventArgs e) => _eventAggregator.Unsubscribe(this);
    }
}
