using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;

namespace BotDynamoDB
{
    [DynamoDBTable("Conversation")]
    public class Conversation
    {
        public Int32 Id { get; set; }
        public String Book { get; set; }
        public String Chapter { get; set; }
        public String Published { get; set; }
        public String LastTweetId { get; set; }
        public List<Line> Lines { get; set; }
        public Boolean Active { get; set; }
    }
}
