using MetaUI.NavControlAddInGenerator.Interface;
using MetaUI.NavControlAddInGenerator.Model;
using MetaUI.NavControlAddInGenerator.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MetaUI.NavControlAddInGenerator.Tests
{
    public class StartupFixture
    {
        private NavControlAddInSettings settings;

        public StartupFixture()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            // Register http client and service
            serviceCollection.AddHttpClient<ISymbolsDownloader, SymbolsDownloader>();

            serviceCollection.AddSingleton<ISettingsProvider, SettingsProvider>();
            serviceCollection.AddTransient<INavControlAddInGenerator, Services.NavControlAddInGenerator>();
            serviceCollection.AddTransient<ExecutionContext>();

            // Create service provider
            this.ServiceProvider = serviceCollection.BuildServiceProvider();

            // Generate settings
            this.GenerateSettings();

            // Set up settings
            var settingsProvider = this.ServiceProvider.GetService<ISettingsProvider>();
            settingsProvider.ControlAddInSettings = this.settings;
        }

        public ServiceProvider ServiceProvider { get; private set; }

        private void GenerateSettings()
        {
            this.settings = new NavControlAddInSettings
            {
                ExtensionVersion = "1.0.0.5",
                ApplicationVersion = "17.0.0.0",
                PlatformVersion = "17.0.0.0",
                RuntimeVersion = "6.0",
                DependenciesJson = "[{\"id\": \"aa674118-6bda-4ffb-9c00-2fa79a1d13d5\", \"name\": \"Meta UI Wizard\", \"publisher\": \"Global Mediator\", \"version\": \"1.0.0.4\"}]",
                IdRangesJson = "[{\"from\": 50000, \"to\": 50100}]",
                ClientId = "02aab7a9-5e57-44e1-900e-546a34d4940e",
                ClientSecret = @"-qq_~Ciq31FoQXu475CrZ54-Us3GTfpA1-",
                EnvironmentName = "test",
                EnvironmentTenantId = "d25c5a7b-54fb-4863-88b9-5ccf8190a323",
                ExtensionPublisher = "Global Mediator",
                SymbolsToDownload = new List<SymbolsConfiguration>
                {
                    new SymbolsConfiguration { Publisher = "Microsoft", AppName = "Application", VersionText = "18.0.0.0" },
                    new SymbolsConfiguration { Publisher = "Microsoft", AppName = "Base Application", VersionText = "18.0.0.0" },
                    new SymbolsConfiguration { Publisher = "Microsoft", AppName = "System Application", VersionText = "18.0.0.0" },
                    new SymbolsConfiguration { Publisher = "Microsoft", AppName = "System", VersionText = "18.0.0.0" },
                    new SymbolsConfiguration { Publisher = "Global Mediator", AppName = "Meta UI Wizard", VersionText = "1.0.0.4" }
                },
                ControlId = Guid.NewGuid(),
                CustomStylesString = "",
                InitEventName = "init",
                ControlHtmlContext = "<meta-ui-grid></meta-ui-grid>",
                ControlAddInName = "Customers",
                PluginDetailsUrl = "test settings",
                ExtensionPages = new List<ExtensionPage>
                {
                    new ExtensionPage { PageExtensionId = "50001", PageExtensionName = "Customer Grid", PageToExtendName = "Customer List" }
                },
                Scripts = new List<string>
                {
                    "https://dev-navnxt-angular.azureedge.net/dist/meta-ui-two/meta-ui-grid.js"
                },
                Styles = new List<string>
                {
                    "https://cdn.jsdelivr.net/npm/primeicons@1.0.0-beta.10/primeicons.min.css",
                    "https://cdn.jsdelivr.net/npm/primeng@6/resources/primeng.min.css",
                    "https://cdn.jsdelivr.net/npm/primeng@6.1.4/resources/themes/nova-light/theme.min.css"
                }
            };
        }
    }
}
