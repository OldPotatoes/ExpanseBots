using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using BotDynamoDB;
using BotTweeter;
using Newtonsoft.Json;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ExpanseBotsLambda;

public class Function
{
    public const String AppKey = "ExpanseBots_ApiKey";
    public const String AppSecret = "ExpanseBots_ApiSecretKey";
    public Boolean ActuallyTweet = false;

    private Dictionary<String, Person> People { get; set; }

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Function()
    {
        People = Person.PopulatePeople();
    }

    /// <summary>
    /// A Lambda function to respond to HTTP Get methods from API Gateway
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The API Gateway response.</returns>
    public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
    {
        String message = String.Empty;
        context.Logger.LogInformation("Get Request\n");
        context.Logger.LogInformation($"Http Method: {request.HttpMethod} \n");
        context.Logger.LogInformation($"Path: {request.Path} \n");
        context.Logger.LogInformation($"Resource: {request.Resource} \n");
        context.Logger.LogInformation($"Body: {request.Body} \n");

        String tweetType = String.Empty;
        Int32 conversation = 0;
        Int32 line = 0;
        String lastTweetId = String.Empty;

        (RequestBody body, Boolean ok) = ReadRequestBody(context, request.Body);
        if (!ok)
        {
            context.Logger.LogInformation($"Request is empty, going with the defaults");

            // Set values to defaults
            // This is a horrible hack, as I can't get the EventBridge constant to populate the request
            ActuallyTweet = true;
            //tweetType = "Conversation";
            tweetType = "TestTweet";
        }
        else
        {
            ActuallyTweet = body.ActuallyTweet;
            tweetType = body.TweetType;
            conversation = body.Conversation;
            line = body.Line;
            lastTweetId = body.LastTweetId;
        }

        //if (ActuallyTweet == true)
        //{
        //    //message = "We're not ready to actually tweet. Goodbye\n";
        //    //return ReturnResponse(context, message, HttpStatusCode.BadRequest);
        //    context.Logger.LogInformation($"We're really going to do this? OK...");
        //}

        if (tweetType == "TestTweet")
        {
            return TestTweet(context);
        }
        else if (tweetType == "TestConversation")
        {
            return TestConversation(context, conversation, line, lastTweetId);
        }
        else if (tweetType == "Conversation")
        {
            return Conversation(context);
        }

        message = "I am Expanse Bots, give me something to do.\n";
        return ReturnResponse(context, message, HttpStatusCode.OK);
    }

    private APIGatewayProxyResponse TestTweet(ILambdaContext context)
    {
        context.Logger.LogInformation("Test tweet...\n");

        Person holden = People[Person.HoldenShortName];

        String? apiKey = Environment.GetEnvironmentVariable(AppKey);
        if (String.IsNullOrEmpty(apiKey))
            return ReturnResponse(context, $"Failed to find apiKey", HttpStatusCode.InternalServerError);

        String? apiSecretKey = Environment.GetEnvironmentVariable(AppSecret);
        if (String.IsNullOrEmpty(apiSecretKey))
            return ReturnResponse(context, $"Failed to find apiSecretKey", HttpStatusCode.InternalServerError);

        if (String.IsNullOrEmpty(holden.TwitterToken))
            return ReturnResponse(context, $"Failed to find holden.TwitterToken", HttpStatusCode.InternalServerError);

        if (String.IsNullOrEmpty(holden.TwitterSecret))
            return ReturnResponse(context, $"Failed to find holden.TwitterSecret", HttpStatusCode.InternalServerError);

        String message = "I never said Mars did it.";
        Tweeter.SendTweet(message, apiKey, apiSecretKey, holden.TwitterToken, holden.TwitterSecret);


        return ReturnResponse(context, $"{holden.ShortName}: {message}", HttpStatusCode.OK);
    }

    private APIGatewayProxyResponse Conversation(ILambdaContext context)
    {
        context.Logger.LogInformation("Beginning a conversation...\n");

        // Check if today's conversation has ended
        var dbConvos = new ConversationsTable();
        var taskHasFinishedToday = dbConvos.PublishedToday();
        Boolean hasFinishedToday = taskHasFinishedToday.Result;
        if (hasFinishedToday)
        {
            return ReturnResponse(context, "Finished for today", HttpStatusCode.OK);
        }

        // Get the current conversation, or a new one
        var taskConvos = dbConvos.GetAllConversations(true);
        List<Conversation> convos = taskConvos.Result;

        Conversation convo = dbConvos.GetCurrentlyPublishedConversation(convos);
        Int32 previousLine = dbConvos.GetNextLineIndex(convo);
        if (previousLine == -1)
        {
            convo = convos[(new Random()).Next(convos.Count)];
        }

        // Tweet a line from the conversation
        Int32 thisLine = previousLine+1;
        Boolean isFinalLine = (1 + thisLine == convo.Lines.Count);
        String tweetText = MakeTweetString(context, convo, thisLine, isFinalLine);
        Person speaker = People[convo.Lines[thisLine].Character];

        context.Logger.LogInformation($"Line pub: {previousLine}, next line: {thisLine}, actual: {convo.Lines[thisLine].Quote}\n");
        Tweeter? tweeter = InitializeTweeter(context, speaker);
        if (tweeter == null)
        {
            String message = $"Failed to initialize the tweeter for {speaker.FullName}";
            return ReturnResponse(context, message, HttpStatusCode.InternalServerError);
        }

        String tweetId;
        if (String.IsNullOrEmpty(convo.LastTweetId))
            tweetId = tweeter.MaybeTweet(tweetText, ActuallyTweet);
        else
            tweetId = tweeter.MaybeReply(tweetText, convo.LastTweetId, ActuallyTweet);

        // Update the published number in the database
        String published = thisLine.ToString();
        if (isFinalLine)
            published = DateTime.Today.ToString();

        dbConvos.SetPublishedLine(convo.Id, published, true);

        // Set the LastTweetID
        dbConvos.SetLastTweeted(convo.Id, tweetId, true);

        return ReturnResponse(context, $"{speaker.ShortName}: {tweetText}", HttpStatusCode.OK);
    }

    private static (RequestBody, Boolean) ReadRequestBody(ILambdaContext context, String boddy)
    {
        Boolean Ok = false;
        context.Logger.LogInformation($"Request: {boddy}\n");
        if (String.IsNullOrWhiteSpace(boddy))
        {
            context.Logger.LogError("Request body is empty\n");
            return (new RequestBody(), Ok);
        }

        RequestBody? body = JsonConvert.DeserializeObject<RequestBody>(boddy);
        if (body == null)
        {
            context.Logger.LogError("Failed to deserialize request body\n");
            return (new RequestBody(), Ok);
        }

        Ok = true;
        return (body, Ok);
    }

    private static APIGatewayProxyResponse ReturnResponse(ILambdaContext context, String message, HttpStatusCode statusCode)
    {
        context.Logger.LogInformation($"Returning response: {message}");

        return new APIGatewayProxyResponse
        {
            StatusCode = (Int32)statusCode,
            Body = message,
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };
    }

    private static Tweeter? InitializeTweeter(ILambdaContext context, Person perp)
    {
        context.Logger.LogInformation($"Person shortname: {perp.ShortName}, token: {perp.TwitterToken}, secret: {perp.TwitterSecret}");

        String? ApiKey = Environment.GetEnvironmentVariable(AppKey);
        if (String.IsNullOrEmpty(ApiKey))
            context.Logger.LogInformation("No ApiKey");
        else
            context.Logger.LogInformation($"ApiKey starts with {ApiKey[0]}");

        String? ApiSecretKey = Environment.GetEnvironmentVariable(AppSecret);
        if (String.IsNullOrEmpty(ApiSecretKey))
            context.Logger.LogInformation("No ApiSecretKey");
        else
            context.Logger.LogInformation($"ApiSecretKey starts with {ApiSecretKey[0]}");

        if (String.IsNullOrEmpty(perp.TwitterToken))
            context.Logger.LogInformation("No AccessToken");
        else
            context.Logger.LogInformation($"AccessToken starts with {perp.TwitterToken[0]}");

        if (String.IsNullOrEmpty(perp.TwitterSecret))
            context.Logger.LogInformation("No AccessTokenSecret");
        else
            context.Logger.LogInformation($"AccessTokenSecret starts with {perp.TwitterSecret[0]}");

        Tweeter? tweeter = null;
        if (String.IsNullOrEmpty(ApiKey) || String.IsNullOrEmpty(ApiSecretKey) || String.IsNullOrEmpty(perp.TwitterToken) || String.IsNullOrEmpty(perp.TwitterSecret))
        {
            context.Logger.LogError("Twitter environment variable is missing");
            return tweeter;
        }

        tweeter = new Tweeter(ApiKey, ApiSecretKey, perp.TwitterToken, perp.TwitterSecret);
        return tweeter;
    }

    private String MakeTweetString(ILambdaContext context, Conversation convo, Int32 lineIndex, Boolean isFinalLine)
    {
        if (lineIndex >= convo.Lines.Count)
        {
            context.Logger.LogError($"Tried to read line {lineIndex} from {convo.Lines.Count} lines.");
        }

        Person perp = People[convo.Lines[lineIndex].Character];
        String hashTags = MakeHashTagFromName(perp.ShortName);
        String suffixes = "";
        if (isFinalLine)
            suffixes = $"{convo.Book}: {convo.Chapter}\n{hashTags}";

        String tweet = $"{convo.Lines[lineIndex].Quote}\n\n{suffixes}";
        return tweet;
    }

    private String MakeHashTagFromName(String fullName)
    {
        return "#" + fullName.Replace(" ", "") + " #TheExpanse";
    }

    private APIGatewayProxyResponse TestConversation(ILambdaContext context, Int32 conversationId, Int32 lineId, String lastTweetId)
    {
        context.Logger.LogInformation($"Testing conversation {conversationId}, line {lineId}\n");

        // Get the current conversation, or a new one
        var dbConvos = new ConversationsTable();
        var taskConvo = dbConvos.GetSpecificConversation(conversationId);
        Conversation convo = taskConvo.Result;

        // Tweet a line from the conversation
        Boolean isFinalLine = (1 + lineId == convo.Lines.Count);
        String tweetText = MakeTweetString(context, convo, lineId, isFinalLine);
        Person speaker = People[convo.Lines[lineId].Character];

        String lastSpeakerHandle = "";
        if (lineId > 0)
            lastSpeakerHandle = People[convo.Lines[lineId - 1].Character].TwitterHandle;

        context.Logger.LogInformation($"Tweet text: {tweetText}\n");

        String? ApiKey = Environment.GetEnvironmentVariable(AppKey);
        if (String.IsNullOrEmpty(ApiKey))
        {
            context.Logger.LogInformation("No ApiKey");
            return ReturnResponse(context, "No ApiKey", HttpStatusCode.InternalServerError);
        }

        String? ApiSecretKey = Environment.GetEnvironmentVariable(AppSecret);
        if (String.IsNullOrEmpty(ApiSecretKey))
        {
            context.Logger.LogInformation("No ApiSecretKey");
            return ReturnResponse(context, "No ApiSecretKey", HttpStatusCode.InternalServerError);
        }

        if (String.IsNullOrEmpty(speaker.TwitterToken))
        {
            context.Logger.LogInformation("No AccessToken");
            return ReturnResponse(context, "No AccessToken", HttpStatusCode.InternalServerError);
        }

        if (String.IsNullOrEmpty(speaker.TwitterSecret))
        {
            context.Logger.LogInformation("No AccessTokenSecret");
            return ReturnResponse(context, "No AccessTokenSecret", HttpStatusCode.InternalServerError);
        }

        String tweetId;
        if (String.IsNullOrEmpty(lastTweetId))
        {
            tweetId = Tweeter.SendTweet(tweetText, ApiKey, ApiSecretKey,
                speaker.TwitterToken, speaker.TwitterSecret);
        }
        else
        {
            LambdaLogger.Log($"tweetText: {tweetText}");
            tweetId = Tweeter.ReplyToTweet(tweetText, lastSpeakerHandle, lastTweetId, ApiKey, ApiSecretKey,
                speaker.TwitterToken, speaker.TwitterSecret);
        }

        return ReturnResponse(context, $"{speaker.ShortName}: {tweetText}, ID: {tweetId}", HttpStatusCode.OK);
    }
}
