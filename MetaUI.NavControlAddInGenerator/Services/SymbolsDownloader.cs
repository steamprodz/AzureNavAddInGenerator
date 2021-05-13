using MetaUI.NavControlAddInGenerator.Consts;
using MetaUI.NavControlAddInGenerator.Interface;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MetaUI.NavControlAddInGenerator.Services
{
    public class SymbolsDownloader : ISymbolsDownloader
    {
        private readonly HttpClient client;
        private readonly ISettingsProvider settingsProvider;

        public SymbolsDownloader(ISettingsProvider settingsProvider, HttpClient client)
        {
            this.settingsProvider = settingsProvider;
            this.client = client;
        }

        public async Task DownloadSymbolsAsync(string appFolderPath)
        {
            var settings = settingsProvider.ControlAddInSettings;

            var bearerToken = await GetBearerTokenAsync();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            foreach (var symbolsConfig in settings.SymbolsToDownload)
            {
                var requestUri = string.Format(SymbolsDownloadConsts.BaseUrl, settings.EnvironmentName, symbolsConfig.Publisher, symbolsConfig.AppName, symbolsConfig.VersionText);
                var response = await client.GetAsync(requestUri);

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    var filename = $"{symbolsConfig.Publisher}_{symbolsConfig.AppName}_{symbolsConfig.VersionText}.app";

                    using (var fs = new FileStream(Path.Combine(appFolderPath, AppFolderConsts.AlPackagesFolder, filename), FileMode.Create))
                    {
                        await responseStream.CopyToAsync(fs);
                    }
                }
            }
        }


        private async Task<string> GetBearerTokenAsync()
        {
            var settings = settingsProvider.ControlAddInSettings;

            client.DefaultRequestHeaders.Authorization = null;

            var body = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "scope", SymbolsDownloadConsts.Scopes },
                { "client_id", settings.ClientId },
                { "client_secret", settings.ClientSecret },
            };

            var requestUri = $@"{SymbolsDownloadConsts.LoginUrl}/{settings.EnvironmentTenantId}/{SymbolsDownloadConsts.TokenEndpoint}";

            var content = new FormUrlEncodedContent(body);

            var response = await client.PostAsync(requestUri, content);

            var responseString = await response.Content.ReadAsStringAsync();

            var responsDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

            return responsDict["access_token"];
        }
    }
}
