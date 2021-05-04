using MetaUI.NavControlAddInGenerator.Model;

namespace MetaUI.NavControlAddInGenerator
{
    public interface ISettingsProvider
    {
        NavControlAddInSettings ControlAddInSettings { get; set; }
    }
}