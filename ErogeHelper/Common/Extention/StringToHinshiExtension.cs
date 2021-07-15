using System.Windows.Media;
using ErogeHelper.Common.Constraint;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.Common.Extention
{
    public static class HinshiColorExtension
    {
        public static JapanesePartOfSpeech ToHinshi(this string partOfSpeech)
        {
            return partOfSpeech switch
            {
                "名詞" => JapanesePartOfSpeech.Noun,
                "動詞" => JapanesePartOfSpeech.Verb,
                "形容詞" => JapanesePartOfSpeech.Adjective,
                "副詞" => JapanesePartOfSpeech.Adverb,
                "助詞" => JapanesePartOfSpeech.Auxiliary,
                "助動詞" => JapanesePartOfSpeech.AuxiliaryVerb,
                "感動詞" => JapanesePartOfSpeech.Interjection,
                "形状詞" => JapanesePartOfSpeech.Form,
                "代名詞" => JapanesePartOfSpeech.Pronoun,
                "連体詞" => JapanesePartOfSpeech.Conjunction,
                "接尾辞" => JapanesePartOfSpeech.Suffix,
                "補助記号" => JapanesePartOfSpeech.Mark,
                _ => JapanesePartOfSpeech.Undefined
            };
        }

        public static ImageSource ToColor(this JapanesePartOfSpeech partOfSpeech)
        {
            return partOfSpeech switch
            {
                JapanesePartOfSpeech.Noun or JapanesePartOfSpeech.Pronoun => StaticXamlBitmapImage.AquaGreenImage,
                JapanesePartOfSpeech.Verb or JapanesePartOfSpeech.AuxiliaryVerb or JapanesePartOfSpeech.Adverb => StaticXamlBitmapImage.GreenImage,
                JapanesePartOfSpeech.Adjective or JapanesePartOfSpeech.Interjection or JapanesePartOfSpeech.Form or JapanesePartOfSpeech.Conjunction or JapanesePartOfSpeech.Suffix => StaticXamlBitmapImage.PinkImage,
                _ => StaticXamlBitmapImage.TransparentImage, // Hinshi.助詞
            };
        }
    }
}