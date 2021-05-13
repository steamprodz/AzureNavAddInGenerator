using System;
using System.Collections.Generic;

namespace MetaUI.NavControlAddInGenerator.Model
{
    public class NavControlAddInSettings
	{
		 public string DependenciesJson { get; set; }

		public string PlatformVersion { get; set; }

		public string ApplicationVersion { get; set; }

		public string IdRangesJson { get; set; }

		public string RuntimeVersion { get; set; }

		public List<SymbolsConfiguration> SymbolsToDownload { get; set; }

		public string EnvironmentName { get; set; }

		public string EnvironmentTenantId { get; set; }

		public string ClientId { get; set; }

		public string ClientSecret { get; set; }

		public string ExtensionPublisher { get; set; }

		public string ExtensionVersion { get; set; }

		public string InitEventName { get; set; }

		public string ControlHtmlContext { get; set; }

		public string PluginDetailsUrl { get; set; }

		public List<ExtensionPage> ExtensionPages { get; set; }

        public string ControlAddInName { get; set; }

        public Guid ControlId { get; set; }

        public List<string> Scripts { get; set; }

        public List<string> Styles { get; set; }

        public string CustomStylesString { get; set; }

		////public string NgRowSelecteEventName { get; set; }

		////public string NavRowSelectedEventName { get; set; }

		////public string NgOnValidateEventName { get; set; }

		////public string NavOnvalidateEventName { get; set; }

		////public string NavOnLookupEventName { get; set; }

		////public string NgOnLookupEventName { get; set; }
	}
}