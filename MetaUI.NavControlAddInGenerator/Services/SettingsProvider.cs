using MetaUI.NavControlAddInGenerator.Interface;
using MetaUI.NavControlAddInGenerator.Model;

namespace MetaUI.NavControlAddInGenerator.Services
{
    public class SettingsProvider : ISettingsProvider
    {
        public NavControlAddInSettings ControlAddInSettings { get; set; }
    }
}
