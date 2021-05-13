using System.Threading.Tasks;

namespace MetaUI.NavControlAddInGenerator.Interface
{
    public interface ISymbolsDownloader
    {
        Task DownloadSymbolsAsync(string appFolderPath);
    }
}