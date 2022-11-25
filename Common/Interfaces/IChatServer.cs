using Common.Models;
using System.Runtime.CompilerServices;

namespace Common.Interfaces
{
    public interface IChatServer
    {
        Task AddMessageToChat(string message);
        Task Subscribe();
        Task Unsubscribe();
        IAsyncEnumerable<string> DownloadStream([EnumeratorCancellation] CancellationToken cancellationToken);
        Task UploadStream(IAsyncEnumerable<string> asyncEnumerable);
    }
}
