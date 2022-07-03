using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotDynamoDB
{
    public class ConversationsTable
    {
        private const String TableName = "ExpanseConversations";
        public const String AwsAccessKeyId = "AwsAccessKeyId";
        public const String AwsSecretAccessKey = "AwsSecretAccessKey";

        private readonly AmazonDynamoDBClient _client;

        public ConversationsTable()
        {
            String AwsAccessId = Environment.GetEnvironmentVariable(AwsAccessKeyId) ?? String.Empty;
            String AwsSecretKey = Environment.GetEnvironmentVariable(AwsSecretAccessKey) ?? String.Empty;

            _client = new AmazonDynamoDBClient(
                AwsAccessId, 
                AwsSecretKey,
                new AmazonDynamoDBConfig
                {
                    RegionEndpoint = RegionEndpoint.USEast1 // (N. Virginia)
                });
        }

        public async Task<List<Conversation>> GetAllConversations(Boolean unpublished)
        {
            ScanRequest request;
            String filterExpression = String.Empty;
            var attribValues = new Dictionary<string, AttributeValue>();

            if (unpublished)
            {
                attribValues[":published"] = new AttributeValue { S = "Yes" };
                attribValues[":active"] = new AttributeValue { BOOL = true };
                filterExpression = "Published <> :published AND Active = :active";
            }

            request = new ScanRequest
            {
                TableName = TableName,
                ExpressionAttributeValues = attribValues,
                FilterExpression = filterExpression
            };

            var conversations = new List<Conversation>();
            var resp = await _client.ScanAsync(request);

            foreach (Dictionary<string, AttributeValue> item in resp.Items)
            {
                var conversation = new Conversation
                {
                    Id = Int32.Parse(item["Id"].N),
                    Book = item["Book"].S,
                    Chapter = item["Chapter"].S,
                    Published = item["Published"].S,
                    Active = item["Active"].BOOL,
                    Lines = new List<Line>()
                };

                List<AttributeValue> lines = item["Lines"].L;
                foreach (AttributeValue line in lines)
                {
                    Line convoLine = new Line();

                    var someLine = line.M;
                    foreach (var thing in someLine)
                    {
                        if (thing.Key == "Character")
                            convoLine.Character = thing.Value.S;
                        if (thing.Key == "Index")
                            convoLine.Index = Int32.Parse(thing.Value.N);
                        if (thing.Key == "Quote")
                            convoLine.Quote = thing.Value.S;
                    }

                    conversation.Lines.Add(convoLine);
                }

                conversations.Add(conversation);
            }

            LambdaLogger.Log($"    Conversations Count: {conversations.Count}\n");

            return conversations;
        }

        public Conversation GetCurrentlyPublishedConversation(List<Conversation> convos)
        {
            var currentlyPublishing = new Conversation();
            foreach (Conversation conversation in convos)
            {
                if (conversation.Published != "Yes" && conversation.Published != "No" && conversation.Active == false)
                {
                    LambdaLogger.Log($"    There is a conversation being published: {conversation.Published}\n");
                    currentlyPublishing = conversation;

                    break;
                }
            }

            return currentlyPublishing;
        }

        public Int32 GetNextLineIndex(Conversation conversation)
        {
            if (Int32.TryParse(conversation.Published, out Int32 lineIndex))
            {
                return lineIndex;
            }
            else
            {
                return -1;
            }
        }

        public void SetPublishedLine(Int32 conversationId, String publishedLine, bool actuallySet)
        {
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();

            var request = new UpdateItemRequest
            {
                TableName = "ExpanseConversations",
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "Id", new AttributeValue { N = conversationId.ToString() } }
                },
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#P", "Published"}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":p",new AttributeValue {S = publishedLine}}
                },
                UpdateExpression = "SET #P = :p"
            };

            if (actuallySet)
            {
                //_ = client.UpdateItemAsync(request).Result;
                client.UpdateItemAsync(request);
            }
        }
    }
}
