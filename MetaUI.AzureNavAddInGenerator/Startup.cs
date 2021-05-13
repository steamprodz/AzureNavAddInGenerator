using MetaUI.NavControlAddInGenerator.Interface;
using MetaUI.NavControlAddInGenerator.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(MetaUI.AzureNavAddInGenerator.Startup))]

namespace MetaUI.AzureNavAddInGenerator
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //var config = new ConfigurationBuilder().AddJsonFile("local.settings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();

            builder.Services.AddHttpClient();
            builder.Services.AddLogging();

            builder.Services.AddSingleton<ISettingsProvider, SettingsProvider>();
            builder.Services.AddTransient<ISymbolsDownloader, SymbolsDownloader>();
            builder.Services.AddTransient<INavControlAddInGenerator, NavControlAddInGenerator.Services.NavControlAddInGenerator>();
        }
    }
}
