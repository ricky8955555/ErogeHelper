using Caliburn.Micro;
using ErogeHelper.Common.Messenger;

namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class HookMapItem : PropertyChangedBase
    {
        private string _totalText = string.Empty;
        private string _text = string.Empty;
        private bool _selected;

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                IoC.Get<IEventAggregator>().PublishOnUIThreadAsync(new NewTextThreadSelectedMessage(this, value));
                NotifyOfPropertyChange(() => Selected);
            }
        }

        public long Address { get; set; }

        public long Handle { get; set; }

        public string Text { get => _text; set { _text = value; NotifyOfPropertyChange(() => Text); } }

        public string TotalText
        {
            get => _totalText;
            set
            {
                _totalText = value;
                NotifyOfPropertyChange(() => TotalText);
            }
        }

        public string HookCode { get; set; } = string.Empty;

        public string EngineName { get; set; } = string.Empty;

        public long ThreadContext { get; set; }

        public long SubThreadContext { get; set; }
    }
}