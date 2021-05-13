using System.Threading.Tasks;

namespace MetaUI.NavControlAddInGenerator.Interface
{
    public interface INavControlAddInGenerator
    {
        Task<string> CreateAppFolderAsync(string functionDirectory);
        Task DownloadSymbolsAsync(string appFolder = "App");
        Task<byte[]> GenerateControlAsync();
        Task<string> RunCompiler(string rootPath);
    }
}