using MetaUI.AzureNavAddInGenerator.Helpers;
using MetaUI.NavControlAddInGenerator.Consts;
using MetaUI.NavControlAddInGenerator.Interface;
using MetaUI.NavControlAddInGenerator.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MetaUI.AzureNavAddInGenerator
{
    public class GenerateNavExtensionApp
    {
        private readonly ISettingsProvider settingsProvider;
        private readonly INavControlAddInGenerator generator;
        private readonly ILogger<GenerateNavExtensionApp> logger;

        public GenerateNavExtensionApp(ISettingsProvider settingsProvider, INavControlAddInGenerator controlAddInGenerator, ILogger<GenerateNavExtensionApp> logger)
        {
            this.settingsProvider = settingsProvider;
            this.generator = controlAddInGenerator;
            this.logger = logger;
        }

        [FunctionName("GenerateAppFolder")]
        public async Task<HttpResponseMessage> GenerateAppFolder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage httpRequest,
            ExecutionContext context)
        {
            try
            {
                var functionAppDirectory = context.FunctionAppDirectory;

                await this.SetupSettingsAsync(httpRequest);
                return await this.GenerateAppFolderAsync(functionAppDirectory);
            }
            catch (Exception ex)
            {
                return logger.LogErrorAndReturnHttpResponse("Error", ex);
            }
        }


        [FunctionName("Compile")]
        public async Task<HttpResponseMessage> Compile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage httpRequest,
            ExecutionContext context)
        {
            try
            {
                var functionAppDirectory = context.FunctionAppDirectory;

                await this.SetupSettingsAsync(httpRequest);
                return await this.CompileAsync(functionAppDirectory);
            }
            catch (Exception ex)
            {
                return logger.LogErrorAndReturnHttpResponse("Error", ex);
            }
        }

        [FunctionName("GenerateApp")]
        public async Task<HttpResponseMessage> GenerateApp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage httpRequest,
            ExecutionContext context)
        {
            try
            {
                var functionAppDirectory = context.FunctionAppDirectory;

                await this.SetupSettingsAsync(httpRequest);
                await this.GenerateAppFolderAsync(functionAppDirectory);
                return await this.CompileAsync(functionAppDirectory);
            }
            catch (Exception ex)
            {
                return logger.LogErrorAndReturnHttpResponse("Error", ex);
            }
        }


        private async Task<HttpResponseMessage> GenerateAppFolderAsync(string functionAppDirectory)
        {
            string appFolderPath;
            try
            {
                logger.LogInformation($"Creating App folder");
                appFolderPath = await generator.CreateAppFolderAsync(functionAppDirectory);
            }
            catch (Exception ex)
            {
                return logger.LogErrorAndReturnHttpResponse("Failed to create App folder", ex);
            }

            // Download symbols
            try
            {
                logger.LogInformation($"Downloading App symbols...");
                await generator.DownloadSymbolsAsync(appFolderPath);
                logger.LogInformation($"Symbols downloaded");
            }
            catch (Exception ex)
            {
                return logger.LogErrorAndReturnHttpResponse("Failed to download symbols", ex);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private async Task<HttpResponseMessage> CompileAsync(string functionAppDirectory)
        {
            var appFolder = AppFolderConsts.AppFolder;

            // Compile app
            string appFileName;
            string appFolderPath = Path.Combine(functionAppDirectory, appFolder);
            try
            {
                logger.LogInformation($"Running AL compiler...");
                appFileName = await generator.RunCompiler(functionAppDirectory);
            }
            catch (Exception ex)
            {
                try
                {
                    if (Directory.Exists(appFolderPath))
                    {
                        logger.LogInformation($"Failed to generate app. Deleting App folder.");
                        Directory.Delete(appFolderPath, true);
                        logger.LogInformation($"App folder deleted.");
                    }
                }
                catch (Exception e)
                {
                    logger.LogErrorAndReturnHttpResponse("Error removing App folder", e);
                }

                return logger.LogErrorAndReturnHttpResponse("Failed to compile the app", ex);
            }

            byte[] bytes;
            try
            {
                logger.LogInformation($"Reading generated .app file as byte array...");
                bytes = await File.ReadAllBytesAsync(appFileName);
            }
            catch (Exception ex)
            {
                return logger.LogErrorAndReturnHttpResponse("Failed to read generated .app file", ex);
            }

            try
            {
                logger.LogInformation($"Removed App directory from disk");
                Directory.Delete(appFolderPath, true);
            }
            catch (Exception ex)
            {
                return logger.LogErrorAndReturnHttpResponse("Failed to delete App directory", ex);
            }

            try
            {
                logger.LogInformation($"Returning http response");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };
            }
            catch (Exception ex)
            {
                return logger.LogErrorAndReturnHttpResponse("Failed to return http response", ex);
            }
        }

        private async Task SetupSettingsAsync(HttpRequestMessage httpRequest)
        {
            // Read request body params
            NameValueCollection formData;
            try
            {
                logger.LogInformation("Reading http request form data");
                formData = await httpRequest.Content.ReadAsFormDataAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to read http request form data", ex?.InnerException);
            }

            // Create settings for generator
            try
            {
                var controlSettings = new NavControlAddInSettings
                {
                    ApplicationVersion = formData["ApplicationVersion"],
                    PlatformVersion = formData["PlatformVersion"],
                    RuntimeVersion = formData["RuntimeVersion"],
                    DependenciesJson = formData["DependenciesJson"],
                    IdRangesJson = formData["IdRangesJson"],

                    ClientId = formData["ClientId"],
                    ClientSecret = formData["ClientSecret"],
                    EnvironmentName = formData["EnvironmentName"],
                    EnvironmentTenantId = formData["EnvironmentTenantId"],
                    SymbolsToDownload = new List<SymbolsConfiguration>(),

                    ExtensionPublisher = formData["ExtensionPublisher"],
                    ExtensionVersion = formData["ExtensionVersion"],

                    ControlAddInName = formData["ControlAddInName"],
                    ControlHtmlContext = formData["ControlHtmlContext"],
                    ControlId = Guid.Parse(formData["ControlId"]),
                    CustomStylesString = formData["CustomStylesString"],
                    ExtensionPages = new List<ExtensionPage>(),
                    InitEventName = formData["InitEventName"],
                    PluginDetailsUrl = formData["PluginDetailsUrl"],
                    Scripts = new List<string>(),
                    Styles = new List<string>()
                };

                // Deserialize extra data
                controlSettings.ExtensionPages = JsonConvert.DeserializeObject<List<ExtensionPage>>(formData["ExtensionPages"]);
                controlSettings.Scripts = JsonConvert.DeserializeObject<List<string>>(formData["Scripts"]);
                controlSettings.Styles = JsonConvert.DeserializeObject<List<string>>(formData["Styles"]);
                controlSettings.SymbolsToDownload = JsonConvert.DeserializeObject<List<SymbolsConfiguration>>(formData["SymbolsToDownload"]);

                // Set provider's settings
                settingsProvider.ControlAddInSettings = controlSettings;

                logger.LogInformation($"Initialized control settings.");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create control settings", ex?.InnerException);
            }
        }
    }
}
