using Caliburn.Micro;

namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class MojiItem : PropertyChangedBase
    {
        private string _pron = string.Empty;
        private string _title = string.Empty;
        private BindableCollection<MojiExpanderItem> _expanderCollection = new();

        public string Header { get; set; } = string.Empty;
        public string TarId { get; set; } = string.Empty;
        public string Pron { get => _pron; set { _pron = value; NotifyOfPropertyChange(() => Pron); } }
        public string Title { get => _title; set { _title = value; NotifyOfPropertyChange(() => Title); } }
        public BindableCollection<MojiExpanderItem> ExpanderCollection
        { get => _expanderCollection; set { _expanderCollection = value; NotifyOfPropertyChange(() => ExpanderCollection); } }
    }
}