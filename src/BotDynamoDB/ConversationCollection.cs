using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BotDynamoDB
{
    public class ConversationCollection
    {
        public IEnumerable<Conversation> Conversations { get; set; }
    }
}
