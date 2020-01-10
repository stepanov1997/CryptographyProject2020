using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptographyProject2019.Model
{
    public class Message
    {
        public string TextMessage { get; set; }
        public DateTime Time { get; set; }
        public MessageProperty MessageProperty { get; set; }

        public Message(string textMessage, DateTime time, MessageProperty messageProperty)
        {
            TextMessage = textMessage;
            Time = time;
            MessageProperty = messageProperty;
        }
    }

    public enum MessageProperty
    {
        Sent, Received
    }
}
