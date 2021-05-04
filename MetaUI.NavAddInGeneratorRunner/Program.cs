using System;
using System.Collections.Generic;
using System.IO;
using MetaUI.NavControlAddInGenerator.Model;

namespace MetaUI.NavAddInGeneratorRunner
{
    class Program
    {
        static void Main(string[] args)
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
                    "https://dev-meta-ui-grid.azureedge.net/latest/meta-ui-grid.js"
                },
                Styles = new List<string>
                {
                    "https://cdn.jsdelivr.net/npm/primeicons@1.0.0-beta.10/primeicons.min.css",
                    "https://cdn.jsdelivr.net/npm/primeng@6/resources/primeng.min.css",
                    "https://cdn.jsdelivr.net/npm/primeng@6.1.4/resources/themes/nova-light/theme.min.css"
                },
                ControlId = Guid.NewGuid()
            };

            var generator = new NavControlAddInGenerator.NavControlAddInGenerator(settings);
            //generator.GenerateControl(settings);

            generator.CreateAppFolderAsync(Environment.CurrentDirectory);
            generator.RunCompiler(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName);
        }
    }
}
