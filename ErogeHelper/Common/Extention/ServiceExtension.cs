using Caliburn.Micro;
using ErogeHelper.Model.Factory;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Repository.Migration;
using ErogeHelper.Model.Service;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Window;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.IO;
using System.Linq;
using Serilog;
using WK.Libraries.SharpClipboardNS;

namespace ErogeHelper.Common.Extention
{
    public static class ServiceExtension
    {
        /// <summary>
        /// Update the database
        /// </summary>
        public static void UpdateEhDatabase(this IServiceProvider serviceProvider)
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // https://github.com/fluentmigrator/fluentmigrator/issues/1450
            Log.Info("Fine FileNotFoundExceptions in CLR");
            runner.MigrateUp(); 
        }

        public static IServiceCollection AddViewModels(this IServiceCollection services, Type types)
        {
            types.Assembly.GetTypes()
                .Where(type => type.IsClass)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .ToList()
                .ForEach(viewModelType => services.TryAddTransient(
                    viewModelType, viewModelType));

            services.ReAddSingleton<GameViewModel>();
            services.ReAddSingleton<PreferenceViewModel>();

            return services;
        }

        public static IServiceCollection AddCaliburnMicroTools(this IServiceCollection services)
        {
            services.TryAddSingleton<IEventAggregator, EventAggregator>();
            services.TryAddSingleton<IWindowManager, WindowManager>();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            var roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbFile = Path.Combine(roamingPath, "ErogeHelper", "eh.db");
            var connectString = $"Data Source={dbFile}";

            services.TryAddSingleton<GameRuntimeDataRepo>();
            services.TryAddSingleton(new EhConfigRepository(roamingPath));
            services.TryAddSingleton(_ => new EhDbRepository(connectString));

            // XXX: FluentMigrator has too many dependencies... https://github.com/fluentmigrator/fluentmigrator/issues/982
            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(connectString)
                    .ScanIn(typeof(AddGameInfoTable).Assembly)
                    .ScanIn(typeof(AddUserTermTable).Assembly)
                    .For.Migrations())
                .AddLogging(lb => lb.AddSerilog());

            return services;
        }

        public static IServiceCollection AddEhServer(this IServiceCollection services)
        {
            services.TryAddScoped<IEhServerApiService>(p =>
            {
                var url = p.GetRequiredService<EhConfigRepository>().EhServerBaseUrl;
                return new EhServerApiService(url);
            });

            return services;
        }

        public static IServiceCollection AddOtherModules(this IServiceCollection services)
        {
            // Services
            services.TryAddSingleton<ITextractorService, TextractorService>();
            services.TryAddSingleton<IGameWindowHooker, GameWindowHooker>();
            services.TryAddSingleton<IGameDataService, GameDataService>();
            services.TryAddSingleton<IMeCabService, MeCabService>();
            services.TryAddSingleton<ITermDataService, TermDataService>();
            services.TryAddSingleton<IDanmakuService, DanmakuService>();

            services.TryAddTransient<ISelectProcessDataService, SelectProcessDataService>();
            services.TryAddTransient<IHookDataService, HookDataService>();
            services.TryAddTransient<IDictionaryFactory, DictionaryFactory>();
            services.TryAddTransient<ITouchConversionHooker, TouchConversionHooker>();
            services.TryAddTransient<ITranslatorFactory, TranslatorFactory>();
            services.TryAddTransient<SharpClipboard>();

            return services;
        }

        private static void ReAddSingleton<T>(this IServiceCollection services)
            where T : class
            => services.Remove<T>().TryAddSingleton<T>();

        private static IServiceCollection Remove<T>(this IServiceCollection services)
        {
            var serviceDescriptors = services.Where(descriptor => descriptor.ServiceType == typeof(T));

            foreach (var serviceDescriptor in serviceDescriptors.ToList())
            {
                services.Remove(serviceDescriptor);
            }

            return services;
        }
    }
}