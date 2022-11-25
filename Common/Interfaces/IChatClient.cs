using Common.Models;

namespace Common.Interfaces
{
    public interface IChatClient
    {
        Task SendClientMessageToChat(ChatMessage message);
    }
}
