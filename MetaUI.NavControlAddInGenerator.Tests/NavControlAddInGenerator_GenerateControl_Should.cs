using System.Threading.Tasks;
using MetaUI.NavControlAddInGenerator.Interface;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

using Xunit;

namespace MetaUI.NavControlAddInGenerator.Tests
{
    public class NavControlAddInGenerator_GenerateControl_Should : IClassFixture<StartupFixture>
    {
        private readonly ServiceProvider serviceProvider;

        public NavControlAddInGenerator_GenerateControl_Should(StartupFixture fixture)
        {
            this.serviceProvider = fixture.ServiceProvider;
        }

        [Fact]
        public async Task GenerateZipArchive()
        {
            // Get service
            var generator = serviceProvider.GetService<INavControlAddInGenerator>();
            // Execute
            var result = await generator.GenerateControlAsync();

            result.ShouldNotBeNull();
        }
    }
}
