using Amazon;
using Amazon.DynamoDBv2;
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

        public async Task<Boolean> PublishedToday()
        {
            Boolean alreadyPublished = false;
            ScanRequest request;
            var attribValues = new Dictionary<string, AttributeValue>
            {
                [":published"] = new AttributeValue { S = DateTime.Today.ToString() },
                [":active"] = new AttributeValue { BOOL = true }
            };
            String filterExpression = "Published = :published AND Active = :active";

            request = new ScanRequest
            {
                TableName = TableName,
                ExpressionAttributeValues = attribValues,
                FilterExpression = filterExpression
            };

            var resp = await _client.ScanAsync(request);
            if (resp.Items.Count > 0)
            {
                LambdaLogger.Log($"    Conversation already published today\n");
                alreadyPublished = true;
            }

            return alreadyPublished;
        }

        public async Task<List<Conversation>> GetAllConversations(Boolean unpublished)
        {
            ScanRequest request;
            String filterExpression = String.Empty;
            var attribValues = new Dictionary<string, AttributeValue>();

            if (unpublished)
            {
                attribValues[":active"] = new AttributeValue { BOOL = true };
                attribValues[":published"] = new AttributeValue { S = "No" };
                filterExpression = "Published = :published AND Active = :active";
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
                    LastTweetId = item["LastTweetID"].S,
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

        public async Task<Conversation> GetSpecificConversation(Int32 conversationId)
        {
            var attribValues = new Dictionary<string, AttributeValue>();
            attribValues[":id"] = new AttributeValue { N = conversationId.ToString() };
            String filterExpression = "Id = :id";

            ScanRequest request = new ScanRequest
            {
                TableName = TableName,
                ExpressionAttributeValues = attribValues,
                FilterExpression = filterExpression
            };

            var resp = await _client.ScanAsync(request);

            if (resp.Items.Count != 1)
            {
                LambdaLogger.Log($"ERROR: Found {resp.Items.Count} conversations instead of 1\n");
            }

            var item = resp.Items[0];
            var conversation = new Conversation
            {
                Id = Int32.Parse(item["Id"].N),
                Book = item["Book"].S,
                Chapter = item["Chapter"].S,
                Published = item["Published"].S,
                LastTweetId = item["LastTweetId"].S,
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

            return conversation;
        }

        public Conversation GetCurrentlyPublishedConversation(List<Conversation> convos)
        {
            var currentlyPublishing = new Conversation();
            foreach (Conversation conversation in convos)
            {
                if (conversation.Published != "Yes" && conversation.Published != "No" && conversation.Active == true)
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
            LambdaLogger.Log($"    SetPublishedLine, convo {conversationId} to {publishedLine}\n");
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
                LambdaLogger.Log($"    SetPublishedLine, request {request}\n");
                client.UpdateItemAsync(request);
            }
        }

        public void SetLastTweeted(Int32 conversationId, String tweetId, bool actuallySet)
        {
            LambdaLogger.Log($"    SetLastTweeted, convo {conversationId} to {tweetId}\n");
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
                    {"#T", "LastTweetID"}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":t",new AttributeValue {S = tweetId}}
                },
                UpdateExpression = "SET #T = :t"
            };

            if (actuallySet)
            {
                LambdaLogger.Log($"    SetLastTweeted, request {request}\n");
                client.UpdateItemAsync(request);
            }
        }
    }
}
