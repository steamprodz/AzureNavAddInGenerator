using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MetaUI.NavControlAddInGenerator.Consts;
using MetaUI.NavControlAddInGenerator.Helpers;
using MetaUI.NavControlAddInGenerator.Interface;
using MetaUI.NavControlAddInGenerator.Model;
using Microsoft.Extensions.Logging;

namespace MetaUI.NavControlAddInGenerator.Services
{
    public class NavControlAddInGenerator : INavControlAddInGenerator
    {
        private readonly ILogger<NavControlAddInGenerator> logger;
        private readonly ISettingsProvider settingsProvider;
        private readonly ISymbolsDownloader symbolsDownloader;

        public NavControlAddInGenerator(ISymbolsDownloader symbolsDownloader,
            ISettingsProvider settingsProvider, ILogger<NavControlAddInGenerator> logger)
        {
            this.symbolsDownloader = symbolsDownloader;
            this.settingsProvider = settingsProvider;
            this.logger = logger;
        }

        public async Task<byte[]> GenerateControlAsync()
        {
            var controlAddInSettings = settingsProvider.ControlAddInSettings;

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var templateFolderPath = Path.Combine(Path.GetDirectoryName(path), "Template");

            byte[] result;
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (string filePath in Directory.GetFiles(templateFolderPath, "*", SearchOption.AllDirectories))
                    {
                        string contents = await File.ReadAllTextAsync(filePath);
                        contents = ReplacePlaceholders(contents, controlAddInSettings);
                        var fileName = ReplacePlaceholders(Path.GetRelativePath(templateFolderPath, filePath), controlAddInSettings);
                        var newFile = archive.CreateEntry(fileName);

                        using (var entryStream = newFile.Open())
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            await streamWriter.WriteAsync(contents);
                        }
                    }
                }

                result = memoryStream.ToArray();
            }

            return result;
        }

        public async Task<string> CreateAppFolderAsync(string functionDirectory)
        {
            var controlAddInSettings = settingsProvider.ControlAddInSettings;

            string templateFolderPath = Path.Combine(functionDirectory, AppFolderConsts.TemplateFolder);

            var appFolderPath = Path.Combine(functionDirectory, AppFolderConsts.AppFolder);

            Directory.CreateDirectory(appFolderPath);

            // Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(templateFolderPath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(AppFolderConsts.TemplateFolder, AppFolderConsts.AppFolder));
            }

            foreach (string filePath in Directory.GetFiles(templateFolderPath, "*", SearchOption.AllDirectories))
            {
                var extension = Path.GetExtension(filePath);
                if (AppFolderConsts.CopyExtensions.Contains(extension))
                {
                    File.Copy(filePath, filePath.Replace(AppFolderConsts.TemplateFolder, AppFolderConsts.AppFolder), true);

                    continue;
                }

                string relativeFilePath = Path.GetRelativePath(templateFolderPath, filePath);

                string contents = await File.ReadAllTextAsync(filePath);
                contents = ReplacePlaceholders(contents, controlAddInSettings);
                var fileName = ReplacePlaceholders(relativeFilePath, controlAddInSettings);
                var newFilePath = Path.Combine(appFolderPath, fileName);

                await File.WriteAllTextAsync(newFilePath, contents);
            }

            return appFolderPath;
        }

        public async Task DownloadSymbolsAsync(string appFolder = AppFolderConsts.AppFolder)
        {
            Directory.CreateDirectory(Path.Combine(appFolder, AppFolderConsts.AlPackagesFolder));

            await symbolsDownloader.DownloadSymbolsAsync(appFolder);
        }

        public async Task<string> RunCompiler(string rootPath)
        {
            var settings = settingsProvider.ControlAddInSettings;

            string compilerFileName = Path.Combine(rootPath, AppFolderConsts.BinFolder, AppFolderConsts.CompilerFolder, AppFolderConsts.CompilerAppName);
            string appFolderPath = Path.Combine(rootPath, AppFolderConsts.AppFolder);
            string symbolsDir = Path.Combine(appFolderPath, AppFolderConsts.AlPackagesFolder);

            //var projectDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent;

            string args = @$"/project:{appFolderPath} /packagecachepath:{symbolsDir}";

            var processInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = compilerFileName,
                Arguments = args
            };

            var process = Process.Start(processInfo);
            var processOutput = process.StandardOutput.ReadToEnd();
            var processError = process.StandardError.ReadToEnd();
            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(processOutput))
            {
                if (processOutput.Contains("error"))
                {
                    throw new Exception($"Compiler error:\n{processOutput}");
                }

                logger.LogInformation($"Compiler output:\n{processOutput}");
            }

            if (!string.IsNullOrEmpty(processError))
            {
                throw new Exception($"Compiler error:\n{processError}");
            }

            //Example: Global Mediator_ControlAddIn_1.0.20180917.1.app
            var appFileName = $"{settings.ExtensionPublisher}_{settings.ControlAddInName}_{settings.ExtensionVersion}.app";
            var appFilePath = Path.Combine(appFolderPath, appFileName);

            return appFilePath;
        }


        private string ReplacePlaceholders(string content, NavControlAddInSettings settings)
        {
            foreach (var page in settings.ExtensionPages)
            {
                content = content
                    .Replace("{{Version}}", settings.ExtensionVersion)
                    .Replace("{{DependenciesJson}}", settings.DependenciesJson)
                    .Replace("{{PlatformVersion}}", settings.PlatformVersion)
                    .Replace("{{ApplicationVersion}}", settings.ApplicationVersion)
                    .Replace("{{IdRangesJson}}", settings.IdRangesJson)
                    .Replace("{{RuntimeVersion}}", settings.RuntimeVersion)
                    .Replace("{{InitEventName}}", settings.InitEventName)
                    .Replace("{{ControlHtmlContext}}", settings.ControlHtmlContext)
                    .Replace("{{ControlAddInName}}", settings.ControlAddInName)
                    .Replace("{{PluginDetailsUrl}}", settings.PluginDetailsUrl)
                    .Replace("{{PageExtensionId}}", page.PageExtensionId)
                    .Replace("{{PageExtensionName}}", page.PageExtensionName)
                    .Replace("{{PageToExtendName}}", page.PageToExtendName)
                    .Replace("{{ScriptsPlaceholder}}", $"'{settings.Scripts.Aggregate((x, y) => x == string.Empty ? y : $"{y}', '{x}")}'")
                    .Replace("{{StylesPlaceholder}}", $"'{settings.Styles.Aggregate((x, y) => x == string.Empty ? y : $"{y}', '{x}")}'")
                    .Replace("{{CustomStylesString}}", settings.CustomStylesString)
                    .Replace("{{ControlId}}", settings.ControlId.ToString());
            }

            return content;
        }
    }
}
