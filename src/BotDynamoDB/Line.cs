using Amazon.DynamoDBv2.DataModel;
using System;

namespace BotDynamoDB
{
    [DynamoDBTable("Conversation")]
    public class Line
    {
        public Int32 Index { get; set; }
        public String Character { get; set; }
        public String Quote { get; set; }
    }
}
