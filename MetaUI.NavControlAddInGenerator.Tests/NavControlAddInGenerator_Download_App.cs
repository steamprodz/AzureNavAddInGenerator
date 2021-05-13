using MetaUI.NavControlAddInGenerator.Interface;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MetaUI.NavControlAddInGenerator.Tests
{
    public class NavControlAddInGenerator_Download_App : IClassFixture<StartupFixture>
    {
        private readonly ServiceProvider serviceProvider;

        public NavControlAddInGenerator_Download_App(StartupFixture fixture)
        {
            this.serviceProvider = fixture.ServiceProvider;
        }

        [Fact]
        public async Task DownloadApp()
        {
            // Emulating Azure function directory in local storage
            var functionDirectory = "";
            string appFolder = "App";

            try
            {
                if (Directory.Exists(appFolder))
                {
                    Directory.Delete(appFolder, true);
                }
            }
            catch { }
                
            // Get service
            var generator = serviceProvider.GetService<INavControlAddInGenerator>();
            // Execute
            appFolder = await generator.CreateAppFolderAsync(functionDirectory);
            await generator.DownloadSymbolsAsync();
            var appFileName = await generator.RunCompiler(functionDirectory);
            // Read .app file
            var result = await File.ReadAllBytesAsync(appFileName);

            result.ShouldNotBeEmpty();
        }
    }
}
