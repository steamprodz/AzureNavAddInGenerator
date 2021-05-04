using System.Collections.Generic;

using MetaUI.NavControlAddInGenerator.Model;

using Shouldly;

using Xunit;

namespace MetaUI.NavControlAddInGenerator.Tests
{
    public class NavControlAddInGenerator_GenerateControl_Should
    {
        [Fact]
        public void GenerateZipArchive()
        {
            var settings = new NavControlAddInSettings
            {
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

            var generator = new NavControlAddInGenerator(settings);
            var result = generator.GenerateControlAsync();

            result.ShouldNotBeNull();
        }
    }
}
