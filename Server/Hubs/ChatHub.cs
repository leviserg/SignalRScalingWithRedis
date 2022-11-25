using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Server.Hubs
{
    public class ChatHub : Hub<IChatClient>, IChatServer
    {

        private readonly string subscribersGroupName = "Subscribers";
        public Task AddMessageToChat(string message)
        {
            var caller = Context.UserIdentifier; // implemented in CustomUserProvider : IUserIdProvider
            var messageForClient = ChatMessage.Create(caller, message);
            Console.WriteLine(message);
            return Clients.Others.SendClientMessageToChat(messageForClient);
        }

        // Streaming - server -> clients
        public async IAsyncEnumerable<string> DownloadStream([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            int iteration = 0;
            while (iteration < 10 && !cancellationToken.IsCancellationRequested)
            {
                yield return $"Server talks : {iteration.ToString().PadLeft(2,'0')} : {DateTime.Now.ToString("HH:mm:ss")}";

                iteration += 1;

                await Task.Delay(1000, cancellationToken);
            }
        }


        // Streaming - clients -> server
        public async Task UploadStream(IAsyncEnumerable<string> asyncEnumerable)
        {
            await foreach (string element in asyncEnumerable)
            {
                Debug.WriteLine(element);
            }

            Debug.WriteLine("Stream from client completed");
        }





        public override async Task OnConnectedAsync()
        {
            await AddMessageToChat("joined chat");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await AddMessageToChat("left chat");
        }

        public Task Subscribe()
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, subscribersGroupName);
        }

        public Task Unsubscribe()
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, subscribersGroupName);
        }
    }
}
