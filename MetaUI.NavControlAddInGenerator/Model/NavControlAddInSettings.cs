using System;
using System.Collections.Generic;

namespace MetaUI.NavControlAddInGenerator.Model
{
	public class ExtensionPage
    {
		public string PageExtensionId { get; set; }

		public string PageExtensionName { get; set; }

		public string PageToExtendName { get; set; }
	}

	public class NavControlAddInSettings
	{
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