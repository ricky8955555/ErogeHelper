using System.Collections.Generic;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.Model.Factory.Interface
{
    public interface ITranslatorFactory
    {
        ITranslator[] Translators { get; }

        IEnumerable<ITranslator> GetEnabledTranslators();

        ITranslator GetTranslator(TranslatorName name);
    }
}