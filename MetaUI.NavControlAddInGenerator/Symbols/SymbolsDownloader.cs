using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MetaUI.NavControlAddInGenerator.Symbols
{
    public class SymbolsDownloader : ISymbolsDownloader
    {
        #region Parameters

        // This parameter will be passed from BC
        private const string ClientId = "02aab7a9-5e57-44e1-900e-546a34d4940e";
        // This parameter will be passed from BC
        private const string ClientSecret = "-qq_~Ciq31FoQXu475CrZ54-Us3GTfpA1-";
        private const string loginURL = @"https://login.microsoftonline.com";
        // This parameter will be passed from BC
        private const string tenantId = "d25c5a7b-54fb-4863-88b9-5ccf8190a323";
        private const string scopes = @"https://api.businesscentral.dynamics.com/.default";
        // This parameter will be passed from BC
        private const string environment = "test";
        private const string baseUrl = @"https://api.businesscentral.dynamics.com/v2.0/{0}/dev/packages?publisher={1}&appName={2}&versionText={3}";
        // This parameter will be passed from BC
        private const string publisher = "Microsoft";
        // This parameter will be passed from BC
        private static readonly string[] appNames = { "Application", "Base Application", "System Application", "System" };
        // This parameter will be passed from BC
        private const string versionText = "17.0.0.0";

        // OAuth settings
        private const string grant_type = "client_credentials";
        private const string tokenEndpoint = @"oauth2/v2.0/token";

        #endregion

        private const string AlPackagesFolder = ".alpackages";

        private readonly HttpClient client;
        private readonly ILogger logger;

        public SymbolsDownloader(HttpClient client, ILogger logger)
        {
            this.client = client;
            this.logger = logger;
        }


        public async Task DownloadSymbolsAsync(string appFolderPath)
        {
            var bearerToken = await GetBearerTokenAsync();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            foreach (var appName in appNames)
            {
                var requestUri = String.Format(baseUrl, environment, publisher, appName, versionText);
                var response = await client.GetAsync(requestUri);

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    var filename = $"{publisher}_{appName}_{versionText}.app";

                    using (var fs = new FileStream(Path.Combine(appFolderPath, AlPackagesFolder, filename), FileMode.Create))
                    {
                        await responseStream.CopyToAsync(fs);
                    }
                }
            }
        }


        private async Task<string> GetBearerTokenAsync()
        {
            client.DefaultRequestHeaders.Authorization = null;

            var body = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "scope", scopes },
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
            };

            var requestUri = $@"{loginURL}/{tenantId}/{tokenEndpoint}";

            var content = new FormUrlEncodedContent(body);

            var response = await client.PostAsync(requestUri, content);

            var responseString = await response.Content.ReadAsStringAsync();

            var responsDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

            return responsDict["access_token"];
        }
    }
}
