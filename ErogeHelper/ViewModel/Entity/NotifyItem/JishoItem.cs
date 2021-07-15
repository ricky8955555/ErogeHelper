using Caliburn.Micro;

namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class JishoItem
    {
        public string? Ruby { get; set; }
        public string? Word { get; set; }
        public string? CommonWord { get; set; }
        public string? JlptLevel { get; set; }
        public string? WanikanaLevel { get; set; }

        public BindableCollection<Detail>? Details { get; set; }

        public class Detail
        {
            public string? PartOfSpeech { get; set; }
            public string? Explanation { get; set; }
            public BindableCollection<Link>? Links { get; set; }

            public class Link
            {
                public string? Text { get; set; }
                public string? HyperLink { get; set; }
            }
        }
    }
}