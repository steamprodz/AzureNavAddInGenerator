using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MetaUI.NavControlAddInGenerator.Model;
using MetaUI.NavControlAddInGenerator.Symbols;
using Microsoft.Extensions.Logging;

namespace MetaUI.NavControlAddInGenerator
{
    public class NavControlAddInGenerator : INavControlAddInGenerator
    {
        #region Path Consts

        // Consts
        private const string TemplateFolder = "Template";
        private const string AlPackagesFolder = ".alpackages";
        private const string CompilerFolder = "AlCompiler";
        private const string CompilerAppName = "alc.exe";
        private const string BinFolder = "bin";

        private string[] CopyExtensions = { ".png", ".app" };

        private const string HelloWorldDirPath = "TestHelloWorld";

        public const string AppFolder = "App";

        #endregion


        // Settings
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

            string templateFolderPath = Path.Combine(functionDirectory, TemplateFolder);

            var appFolderPath = Path.Combine(functionDirectory, AppFolder);

            Directory.CreateDirectory(appFolderPath);

            // Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(templateFolderPath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(TemplateFolder, AppFolder));
            }

            foreach (string filePath in Directory.GetFiles(templateFolderPath, "*", SearchOption.AllDirectories))
            {
                var extension = Path.GetExtension(filePath);
                if (CopyExtensions.Contains(extension))
                {
                    File.Copy(filePath, filePath.Replace(TemplateFolder, AppFolder), true);

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

        public async Task DownloadSymbolsAsync(string appFolder = AppFolder)
        {
            Directory.CreateDirectory(Path.Combine(appFolder, AlPackagesFolder));

            await symbolsDownloader.DownloadSymbolsAsync(appFolder);
        }

        public string RunCompiler(string rootPath)
        {
            var controlAddInSettings = settingsProvider.ControlAddInSettings;

            string compilerFileName = Path.Combine(rootPath, BinFolder, CompilerFolder, CompilerAppName);
            string appFolderPath = Path.Combine(rootPath, AppFolder);
            string symbolsDir = Path.Combine(appFolderPath, AlPackagesFolder);

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
            process.WaitForExit();

            if (!String.IsNullOrEmpty(processOutput))
            {
                logger.LogInformation($"Compiler output:\n{processOutput}");
            }

            if (!String.IsNullOrEmpty(processError))
            {
                logger.LogInformation($"Compiler error:\n{processError}");
            }

            var appFileName = $"Global Mediator_{controlAddInSettings.ControlAddInName}_1.0.20180917.1.app";
            //var appFileName = $"Global Mediator_ControlAddIn_1.0.20180917.1.app";
            var appFilePath = Path.Combine(appFolderPath, appFileName);

            return appFilePath;
        }


        private string ReplacePlaceholders(string content, NavControlAddInSettings settings)
        {
            foreach (var page in settings.ExtensionPages)
            {
                content = content
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
