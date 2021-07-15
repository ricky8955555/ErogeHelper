using ErogeHelper.Common.Enum;

namespace ErogeHelper.Common.Entity
{
    public class MeCabWord
    {
        public string Word { get; set; } = string.Empty;

        public string Kana { get; set; } = string.Empty;

        public JapanesePartOfSpeech PartOfSpeech { get; set; }
    }
}