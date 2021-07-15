using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.View.Page;
using ErogeHelper.ViewModel.Window;


namespace ErogeHelper.View.Window
{
    /// <summary>
    /// HookConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class HookConfigView : IHandle<ViewActionMessage>
    {
        public HookConfigView()
        {
            InitializeComponent();

            _eventAggregator = IoC.Get<IEventAggregator>();

            _eventAggregator.SubscribeOnUIThread(this);
            HookPageFrame.LoadCompleted += (_, _) => _eventAggregator.SubscribeOnUIThread(HookPageFrame.Content as HookPage);
        }

        private readonly IEventAggregator _eventAggregator;

        public Task HandleAsync(ViewActionMessage message, CancellationToken cancellationToken)
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
                    default:
                        throw new InvalidOperationException();
                }
            }

            return Task.CompletedTask;
        }

        protected override void OnClosed(EventArgs e)
        {
            _eventAggregator.Unsubscribe(HookPageFrame.Content as HookPage);
            _eventAggregator.Unsubscribe(this);
            _eventAggregator.Unsubscribe((DataContext as HookConfigViewModel)?.HookPageFrame);
        }
    }
}
