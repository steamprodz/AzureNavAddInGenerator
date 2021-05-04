using MetaUI.NavControlAddInGenerator;
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
using System.Text;
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

                // Read request body params
                NameValueCollection formData;
                try
                {
                    logger.LogInformation("Reading http request form data");
                    formData = await httpRequest.Content.ReadAsFormDataAsync();
                }
                catch (Exception ex)
                {
                    return logger.LogErrorAndReturnHttpResponse("Failed to read http request form data", ex);
                }

                // Create settings for generator
                NavControlAddInSettings controlSettings;
                try
                {
                    controlSettings = new NavControlAddInSettings
                    {
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

                    // Set provider's settings
                    settingsProvider.ControlAddInSettings = controlSettings;

                    logger.LogInformation($"Created control settings: {controlSettings}");
                }
                catch (Exception ex)
                {
                    return logger.LogErrorAndReturnHttpResponse("Failed to create control settings", ex);
                }

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

                //// Compile app
                //string appFileName;
                //try
                //{
                //    logger.LogInformation($"Running AL compiler...");
                //    appFileName = generator.RunCompiler(functionAppDirectory);
                //}
                //catch (Exception ex)
                //{
                //    return logger.LogErrorAndReturnHttpResponse("Failed to compile the app", ex);
                //}

                //byte[] bytes;
                //try
                //{
                //    logger.LogInformation($"Reading generated .app file as byte array...");
                //    bytes = await File.ReadAllBytesAsync(appFileName);
                //}
                //catch (Exception ex)
                //{
                //    return logger.LogErrorAndReturnHttpResponse("Failed to read generated .app file", ex);
                //}

                //try
                //{
                //    logger.LogInformation($"Removed App directory from disk");
                //    Directory.Delete(appFolderPath, true);
                //}
                //catch (Exception ex)
                //{
                //    return logger.LogErrorAndReturnHttpResponse("Failed to delete App directory", ex);
                //}

                //try
                //{
                //    logger.LogInformation($"Returning http resonse");
                //    return new HttpResponseMessage(HttpStatusCode.OK)
                //    {
                //        Content = new StringContent("Hello World")
                //    };
                //}
                //catch (Exception ex)
                //{
                //    return logger.LogErrorAndReturnHttpResponse("Failed to return http response", ex);
                //}

                return new HttpResponseMessage(HttpStatusCode.OK);
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
                var appFolder = NavControlAddInGenerator.NavControlAddInGenerator.AppFolder;

                // Read request body params
                NameValueCollection formData;
                try
                {
                    logger.LogInformation("Reading http request form data");
                    formData = await httpRequest.Content.ReadAsFormDataAsync();
                }
                catch (Exception ex)
                {
                    return logger.LogErrorAndReturnHttpResponse("Failed to read http request form data", ex);
                }

                // Create settings for generator
                NavControlAddInSettings controlSettings;
                try
                {
                    controlSettings = new NavControlAddInSettings
                    {
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

                    // Set provider's settings
                    settingsProvider.ControlAddInSettings = controlSettings;

                    logger.LogInformation($"Created control settings: {controlSettings}");
                }
                catch (Exception ex)
                {
                    return logger.LogErrorAndReturnHttpResponse("Failed to create control settings", ex);
                }

                // Compile app
                string appFileName;
                try
                {
                    logger.LogInformation($"Running AL compiler...");
                    appFileName = generator.RunCompiler(functionAppDirectory);
                }
                catch (Exception ex)
                {
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

                string appFolderPath = Path.Combine(functionAppDirectory, appFolder);
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
            catch (Exception ex)
            {
                return logger.LogErrorAndReturnHttpResponse("Error", ex);
            }
        }
    }
}
