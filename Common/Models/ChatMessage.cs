using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class ChatMessage
    {
        public string? Caller { get; set; }
        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }

        public ChatMessage()
        {

        }
        public static ChatMessage Create(string? caller, string text)
        {
            return new ChatMessage
            {
                Caller = string.IsNullOrWhiteSpace(caller) ? "Anonymous" : caller,
                Text = text,
                CreatedAt = DateTime.Now
            };
        }
    }
}
