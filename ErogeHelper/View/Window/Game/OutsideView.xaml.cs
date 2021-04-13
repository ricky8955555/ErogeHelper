﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Common.Extention;

namespace ErogeHelper.View.Window.Game
{
    /// <summary>
    /// OutsideView.xaml 的交互逻辑
    /// </summary>
    public partial class OutsideView : IHandle<ViewActionMessage>
    {
        public OutsideView()
        {
            InitializeComponent();

            IoC.Get<IEventAggregator>().SubscribeOnUIThread(this);
            Visibility = Visibility.Collapsed;
            Loaded += (_, _) => { Utils.HideWindowInAltTab(this); };
        }

        public Task HandleAsync(ViewActionMessage message, CancellationToken cancellationToken)
        {
            if (message.WindowType == GetType())
            {
                switch (message.Action)
                {
                    case ViewAction.Hide:
                        Hide();
                        break;
                    case ViewAction.Show:
                        this.MoveToCenter();
                        Show();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            return Task.CompletedTask;
        }
        protected override void OnClosed(EventArgs e) => IoC.Get<IEventAggregator>().Unsubscribe(this);
    }
}
