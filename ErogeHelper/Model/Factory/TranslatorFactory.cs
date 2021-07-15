using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;

namespace ErogeHelper.Model.Factory
{
    public class TranslatorFactory : ITranslatorFactory
    {
        public TranslatorFactory(EhConfigRepository ehConfigRepository)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            var interfaceType = typeof(ITranslator);
            var translatorTypes = types.Where(
                type => type.IsClass &&
                interfaceType.IsAssignableFrom(type) &&
                !type.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ObsoleteAttribute)));

            var translators = translatorTypes
                .Select(type => type.GetConstructors()[0].Invoke(new object[] { ehConfigRepository }))
                .Cast<ITranslator>();

            Translators = translators.ToArray();
        }

        public ITranslator[] Translators { get; }

        public IEnumerable<ITranslator> GetEnabledTranslators() => Translators.Where(translator => translator.IsEnable);

        public ITranslator GetTranslator(TranslatorName name) =>
            Translators.FirstOrDefault(translator => translator.Name == name) ??
            throw new InvalidOperationException($"No translator {name}.");
    }
}