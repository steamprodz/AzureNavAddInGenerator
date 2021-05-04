using System.Threading.Tasks;

namespace MetaUI.NavControlAddInGenerator.Symbols
{
    public interface ISymbolsDownloader
    {
        Task DownloadSymbolsAsync(string appFolderPath);
    }
}