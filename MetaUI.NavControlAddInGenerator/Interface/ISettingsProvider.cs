using MetaUI.NavControlAddInGenerator.Model;

namespace MetaUI.NavControlAddInGenerator.Interface
{
    public interface ISettingsProvider
    {
        NavControlAddInSettings ControlAddInSettings { get; set; }
    }
}