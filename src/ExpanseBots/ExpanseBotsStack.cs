using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.CustomResources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;
using Construct = Constructs.Construct;

namespace ExpanseBotsCDK
{
    public class ExpanseBotsStack : Stack
    {
        const String functionPath = "src/ExpanseBotsLambda/bin/Debug/net6.0";
        internal ExpanseBotsStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var AwsAccessKeyId = new CfnParameter(this, "AwsAccessKeyId");
            AwsAccessKeyId.Type = "String";

            var AwsSecretAccessKey = new CfnParameter(this, "AwsSecretAccessKey");
            AwsSecretAccessKey.Type = "String";

            var ExpanseBotsApiKey = new CfnParameter(this, "ExpanseBotsApiKey");
            ExpanseBotsApiKey.Type = "String";

            var ExpanseBotsApiSecretKey = new CfnParameter(this, "ExpanseBotsApiSecretKey");
            ExpanseBotsApiSecretKey.Type = "String";

            var AvasaralaBotAccessToken = new CfnParameter(this, "AvasaralaBotAccessToken");
            AvasaralaBotAccessToken.Type = "String";

            var AvasaralaBotAccessTokenSecret = new CfnParameter(this, "AvasaralaBotAccessTokenSecret");
            AvasaralaBotAccessTokenSecret.Type = "String";

            var MillerBotAccessToken = new CfnParameter(this, "MillerBotAccessToken");
            MillerBotAccessToken.Type = "String";

            var MillerBotAccessTokenSecret = new CfnParameter(this, "MillerBotAccessTokenSecret");
            MillerBotAccessTokenSecret.Type = "String";

            var HoldenBotAccessToken = new CfnParameter(this, "HoldenBotAccessToken");
            HoldenBotAccessToken.Type = "String";

            var HoldenBotAccessTokenSecret = new CfnParameter(this, "HoldenBotAccessTokenSecret");
            HoldenBotAccessTokenSecret.Type = "String";

            var NaomiBotAccessToken = new CfnParameter(this, "NaomiBotAccessToken");
            NaomiBotAccessToken.Type = "String";

            var NaomiBotAccessTokenSecret = new CfnParameter(this, "NaomiBotAccessTokenSecret");
            NaomiBotAccessTokenSecret.Type = "String";

            var AmosBotAccessToken = new CfnParameter(this, "AmosBotAccessToken");
            AmosBotAccessToken.Type = "String";

            var AmosBotAccessTokenSecret = new CfnParameter(this, "AmosBotAccessTokenSecret");
            AmosBotAccessTokenSecret.Type = "String";

            var AlexBotAccessToken = new CfnParameter(this, "AlexBotAccessToken");
            AlexBotAccessToken.Type = "String";

            var AlexBotAccessTokenSecret = new CfnParameter(this, "AlexBotAccessTokenSecret");
            AlexBotAccessTokenSecret.Type = "String";

            var BobbieBotAccessToken = new CfnParameter(this, "BobbieBotAccessToken");
            BobbieBotAccessToken.Type = "String";

            var BobbieBotAccessTokenSecret = new CfnParameter(this, "BobbieBotAccessTokenSecret");
            BobbieBotAccessTokenSecret.Type = "String";

            var FredBotAccessToken = new CfnParameter(this, "FredBotAccessToken");
            FredBotAccessToken.Type = "String";

            var FredBotAccessTokenSecret = new CfnParameter(this, "FredBotAccessTokenSecret");
            FredBotAccessTokenSecret.Type = "String";

            var ClarissaBotAccessToken = new CfnParameter(this, "ClarissaBotAccessToken");
            ClarissaBotAccessToken.Type = "String";

            var ClarissaBotAccessTokenSecret = new CfnParameter(this, "ClarissaBotAccessTokenSecret");
            ClarissaBotAccessTokenSecret.Type = "String";

            var ElviBotAccessToken = new CfnParameter(this, "ElviBotAccessToken");
            ElviBotAccessToken.Type = "String";

            var ElviBotAccessTokenSecret = new CfnParameter(this, "ElviBotAccessTokenSecret");
            ElviBotAccessTokenSecret.Type = "String";

            var HavelockBotAccessToken = new CfnParameter(this, "HavelockBotAccessToken");
            HavelockBotAccessToken.Type = "String";

            var HavelockBotAccessTokenSecret = new CfnParameter(this, "HavelockBotAccessTokenSecret");
            HavelockBotAccessTokenSecret.Type = "String";

            var PraxBotAccessToken = new CfnParameter(this, "PraxBotAccessToken");
            PraxBotAccessToken.Type = "String";

            var PraxBotAccessTokenSecret = new CfnParameter(this, "PraxBotAccessTokenSecret");
            PraxBotAccessTokenSecret.Type = "String";

            // Lambda that handles ExpanseBots requests
            var expanseBotsLambda = new Function(this, "ExpanseBotsLambda", new FunctionProps
            {
                Description = "Expanse Bots that talk to each others",
                Runtime = Runtime.DOTNET_6,
                Code = Code.FromAsset(functionPath),
                Timeout = Duration.Seconds(30),
                Handler = "ExpanseBotsLambda::ExpanseBotsLambda.Function::Get", // Assembly::Type::Method
                Environment = new Dictionary<String, String>{
                    { BotDynamoDB.ConversationsTable.AwsAccessKeyId, AwsAccessKeyId.ValueAsString},
                    { BotDynamoDB.ConversationsTable.AwsSecretAccessKey, AwsSecretAccessKey.ValueAsString},
                    { ExpanseBotsLambda.Function.AppKey, ExpanseBotsApiKey.ValueAsString },
                    { ExpanseBotsLambda.Function.AppSecret, ExpanseBotsApiSecretKey.ValueAsString },
                    { ExpanseBotsLambda.Person.AvasaralaToken, AvasaralaBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.AvasaralaSecret, AvasaralaBotAccessTokenSecret.ValueAsString },
                    { ExpanseBotsLambda.Person.MillerToken, MillerBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.MillerSecret, MillerBotAccessTokenSecret.ValueAsString },
                    { ExpanseBotsLambda.Person.HoldenToken, HoldenBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.HoldenSecret, HoldenBotAccessTokenSecret.ValueAsString },
                    { ExpanseBotsLambda.Person.NaomiToken, NaomiBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.NaomiSecret, NaomiBotAccessTokenSecret.ValueAsString },
                    { ExpanseBotsLambda.Person.AmosToken, AmosBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.AmosSecret, AmosBotAccessTokenSecret.ValueAsString },
                    { ExpanseBotsLambda.Person.AlexToken, AlexBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.AlexSecret, AlexBotAccessTokenSecret.ValueAsString },

                    { ExpanseBotsLambda.Person.BobbieToken, BobbieBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.BobbieSecret, BobbieBotAccessTokenSecret.ValueAsString },
                    { ExpanseBotsLambda.Person.FredToken, FredBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.FredSecret, FredBotAccessTokenSecret.ValueAsString },
                    { ExpanseBotsLambda.Person.ClarissaToken, ClarissaBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.ClarissaSecret, ClarissaBotAccessTokenSecret.ValueAsString },
                    { ExpanseBotsLambda.Person.ElviToken, ElviBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.ElviSecret, ElviBotAccessTokenSecret.ValueAsString },
                    { ExpanseBotsLambda.Person.HavelockToken, HavelockBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.HavelockSecret, HavelockBotAccessTokenSecret.ValueAsString },
                    { ExpanseBotsLambda.Person.PraxToken, PraxBotAccessToken.ValueAsString },
                    { ExpanseBotsLambda.Person.PraxSecret, PraxBotAccessTokenSecret.ValueAsString },
                },
            });

            // REST API that receives ExpanseBots requests
            new LambdaRestApi(this, "ExpanseBotsEndpoint", new LambdaRestApiProps
            {
                Handler = expanseBotsLambda
            });

            // DynamoDB table
            string tableName = "ExpanseConversations";
            var table = new Table(this, tableName, new TableProps
            {
                TableName = tableName,
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = RemovalPolicy.DESTROY,
                PartitionKey = new Attribute
                {
                    Name = "Id",
                    Type = AttributeType.NUMBER
                },
            });

            // Let lambda function control database 
            table.GrantFullAccess(expanseBotsLambda);

            // Collection of AwsCustomResources that write the to table
            var customResources = new List<AwsCustomResource>();

            String conversationFileContents = ExpanseBotsStack.ReadConversationFile();
            BotDynamoDB.ConversationCollection collection = JsonConvert.DeserializeObject<BotDynamoDB.ConversationCollection>(conversationFileContents);

            // Set the number of conversations to some maximum
            const Int32 MAX_CONVERSATIONS = 200;
            collection.Conversations = collection.Conversations.Take(MAX_CONVERSATIONS).ToList();
            if (collection != null)
            {
                Int32 batchSize = 11;
                Int32 currentBatchSize = batchSize;
                Int32 totalConversations = collection.Conversations.Count();
                Int32 totalBatches = totalConversations / batchSize;
                Int32 stubbyBatchSize = totalConversations % batchSize;
                if (stubbyBatchSize > 0)
                    totalBatches++;

                var conversations = collection.Conversations.ToList();

                for (Int32 ii=0; ii<totalBatches; ii++)
                {
                    var requestBatch = new List<Dictionary<string, object>>();

                    if (ii+1 == totalBatches)
                        currentBatchSize = stubbyBatchSize;

                    for (Int32 jj = 0; jj < currentBatchSize; jj++)
                    {
                        Int32 index = jj + ii * batchSize;

                        if (index >= totalConversations)
                            break;

                        var conversation = conversations[index];
                        var recordBatch = CreateRecord(index, conversation);
                        requestBatch.Add(recordBatch);
                    }
                    AwsCustomResource customResource = GetData(this, table, ii, requestBatch.ToArray());
                    customResources.Add(customResource);
                }
            }
        }

        public static String ReadConversationFile()
        {
            const String conversationFileName = @"conversations.json";
            String DynamoDirectory = Directory.GetCurrentDirectory() + "/src/BotDynamoDB";

            var filePath = Path.Combine(DynamoDirectory, conversationFileName);

            var conversationFile = new FileInfo(filePath);
            String fileContents = File.ReadAllText(conversationFile.FullName);

            return fileContents;
        }

        public static Dictionary<string, object> CreateRecord(Int32 count, BotDynamoDB.Conversation conversation)
        {
            var listLines = new List<Dictionary<string, object>>();

            foreach (BotDynamoDB.Line line in conversation.Lines)
            {
                var dictLine = new Dictionary<string, Dictionary<string, object>>()
                {
                    {"Character", new Dictionary<string, object>() { { "S", line.Character } }},
                    {"Quote", new Dictionary<string, object>() { { "S", line.Quote } }},
                    {"Index", new Dictionary<string, object>() { { "N", line.Index.ToString() } }}
                };

                var lineItem = new Dictionary<string, object>() { { "M", dictLine } };
                listLines.Add(lineItem);
            }

            var fields = new Dictionary<string, Dictionary<string, object>>() {
                {"Id", new Dictionary<string, object>() { { "N", $"{count}" } }},
                {"Book", new Dictionary<string, object>() { { "S", conversation.Book } }},
                {"Chapter", new Dictionary<string, object>() { { "S", conversation.Chapter } }},
                {"Published", new Dictionary<string, object>() { { "S", conversation.Published } }},
                {"Active", new Dictionary<string, object>() { { "BOOL", conversation.Active } }},
                {"Lines", new Dictionary<string, object>() { { "L", listLines.ToArray() } }}
            };

            var item = new Dictionary<string, object>() { { "Item", fields } };
            var pr = new Dictionary<string, object>() { { "PutRequest", item } };

            return pr;
        }

        private AwsCustomResource GetData(Construct scope, Table table, Int32 index, Dictionary<String, Object>[] putRequest)
        {
            AwsSdkCall putItemCall = new()
            {
                Service = "DynamoDB",
                Action = "batchWriteItem",
                PhysicalResourceId = PhysicalResourceId.Of($"{table.TableName}_seed_{index}"),
                Parameters = new Dictionary<string, object> {
                {
                    "RequestItems", new Dictionary<string, object>{ { table.TableName, putRequest } }},
                }
            };

            var ddbTableSeed = new AwsCustomResource(scope, $"DdbTableSeed_{index}", new AwsCustomResourceProps
            {
                Policy = AwsCustomResourcePolicy.FromStatements(new PolicyStatement[] {
                    new PolicyStatement(new PolicyStatementProps{
                        Effect = Effect.ALLOW,
                        Actions = new string[] { "dynamodb:PutItem", "dynamodb:BatchWriteItem" },
                        Resources = new string[] { table.TableArn }
                    })
                }),
                OnCreate = putItemCall,
            });

            ddbTableSeed.Node.AddDependency(table);
            return ddbTableSeed;
        }
    }
}
