using Shouldly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MetaUI.NavControlAddInGenerator.Tests
{
    public class NavControlAddInGenerator_Download_App
    {
        [Fact]
        public async Task DownloadApp()
        {
            var client = new HttpClient();

            var body = new Dictionary<string, string>
            {
                { "InitEventName", "init"},
                { "ControlHtmlContext", "<meta-ui-grid></meta-ui-grid>"},
                { "ControlAddInName", "Customers"},
                { "PluginDetailsUrl", "test settings"},
                { "ExtensionPages", "[{\"PageExtensionId\": 50001, \"PageExtensionName\": \"Customer Grid\", \"PageToExtendName\": \"Customer List\"}]"},
                { "Scripts", "[\"https://dev-meta-ui-grid.azureedge.net/latest/meta-ui-grid.js\"]"},
                { "Styles", "[\"https://cdn.jsdelivr.net/npm/primeicons@1.0.0-beta.10/primeicons.min.css\", \"https://cdn.jsdelivr.net/npm/primeng@6/resources/primeng.min.css\", \"https://cdn.jsdelivr.net/npm/primeng@6.1.4/resources/themes/nova-light/theme.min.css\"]"},
                { "ControlId", "e6189b6a-b364-4c01-a121-164cc15d36a5"}
            };

            var requestUri = $@"https://generatenavextensionapp.azurewebsites.net/api/GenerateNavExtensionApp";

            var content = new FormUrlEncodedContent(body);

            var response = await client.PostAsync(requestUri, content);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.ShouldNotBeEmpty();
        }
    }
}
