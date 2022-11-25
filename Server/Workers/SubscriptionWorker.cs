using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using System.Diagnostics;

namespace Server.Workers
{
    public class SubscriptionWorker : BackgroundService
    {
        private readonly IHubContext<ChatHub, IChatClient> _hubContext;
        private readonly string messageForSubscribers;
        public SubscriptionWorker(IHubContext<ChatHub, IChatClient> hubContext)
        {
            _hubContext = hubContext;
            messageForSubscribers = "This is secret message for subscribers only...";
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var newMessageForSubscribers = ChatMessage.Create("System", messageForSubscribers);
                //Debug.WriteLine($"DEBUG: {DateTime.Now.ToString("HH:mm:ss")} : {messageForSubscribers}");
                await _hubContext.Clients.Group("Subscribers").SendClientMessageToChat(newMessageForSubscribers);
                //Console.WriteLine($"CONSOLE: {DateTime.Now.ToString("HH:mm:ss")} : {messageForSubscribers}");
                /*
                var newMessageForAnonymous = ChatMessage.Create("System", "Please register");
                await _hubContext.Clients.User("Anonymous").SendClientMessageToChat(newMessageForAnonymous);
                */
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
