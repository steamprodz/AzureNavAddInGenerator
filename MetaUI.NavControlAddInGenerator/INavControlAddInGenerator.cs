using System.Threading.Tasks;

namespace MetaUI.NavControlAddInGenerator
{
    public interface INavControlAddInGenerator
    {
        Task<string> CreateAppFolderAsync(string functionDirectory);
        Task DownloadSymbolsAsync(string appFolder = "App");
        Task<byte[]> GenerateControlAsync();
        string RunCompiler(string rootPath);
    }
}