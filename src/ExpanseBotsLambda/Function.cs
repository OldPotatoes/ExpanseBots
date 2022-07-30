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
    public const String AppKeyName = "ExpanseBots_ApiKey";
    public const String AppSecretName = "ExpanseBots_ApiSecretKey";

    private Dictionary<String, Person> People { get; set; }

    public String AppKey { get; set; }
    public String AppKeySecret { get; set; }

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Function()
    {
        AppKey = "";
        AppKeySecret = "";
        People = Person.PopulatePeople();
    }

    /// <summary>
    /// A Lambda function to respond to HTTP Get methods from API Gateway
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The API Gateway response.</returns>
    public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Get Request\n");
        context.Logger.LogInformation($"Http Method: {request.HttpMethod} \n");
        context.Logger.LogInformation($"Path: {request.Path} \n");
        context.Logger.LogInformation($"Resource: {request.Resource} \n");
        context.Logger.LogInformation($"Body: {request.Body} \n");

        String tweetType;
        (RequestBody body, Boolean ok) = ReadRequestBody(context, request.Body);
        if (!ok)
        {
            context.Logger.LogInformation($"Request is empty, going with the defaults");
            tweetType = "TestTweet";
        }
        else
        {
            tweetType = body.TweetType;
        }

        if (tweetType == "Conversation")
        {
            return Conversation(context);
        }
        else if (tweetType == "SpecificConversation")
        {
            return Conversation(context, body.Conversation);
        }
        else if (tweetType == "SpecificConversationLine")
        {
            return Conversation(context, body.Conversation, body.Line);
        }
        else if (tweetType == "TestTweet")
        {
            return TestTweet(context);
        }
        else
        {
            // For shame, this is where my EventBridge triggered calls end up, and I don't know why
            return Conversation(context);
        }

        return ReturnResponse(context, "I am Expanse Bots, give me something to do.\n", HttpStatusCode.OK);
    }

    private APIGatewayProxyResponse Conversation(ILambdaContext context, Int32 conversationId=-1, Int32 lineId=-1)
    {
        context.Logger.LogInformation("Beginning a conversation...\n");

        if (conversationId > 0)
            context.Logger.LogInformation($"Testing conversation {conversationId}\n");

        if (lineId > 0)
            context.Logger.LogInformation($"Testing line {lineId}\n");

        // Check if today's conversation has ended
        var dbConvos = new ConversationsTable();
        var taskHasFinishedToday = dbConvos.PublishedToday();
        Boolean hasFinishedToday = taskHasFinishedToday.Result;
        if (hasFinishedToday)
        {
            return ReturnResponse(context, "Finished for today", HttpStatusCode.OK);
        }

        Conversation convo;
        if (conversationId > -1)
        {
            // Get the specific conversation
            var taskConvo = dbConvos.GetSpecificConversation(conversationId);
            convo = taskConvo.Result;
        }
        else
        {
            // Get the current conversation, or a new one
            var taskConvos = dbConvos.GetAllConversations(true);
            List<Conversation> convos = taskConvos.Result;

            convo = dbConvos.GetCurrentlyPublishedConversation(convos);
            if (!convo.Active)
                convo = convos[(new Random()).Next(convos.Count)];
        }

        // Prepare the tweet text from the conversation
        Int32 thisLineId;
        if (lineId > -1)
        {
            thisLineId = lineId;
        }
        else
        {
            Int32 publishedLine = dbConvos.GetPublishedLineIndex(convo);
            thisLineId = publishedLine + 1;
        }
        Boolean isFinalLine = (1 + thisLineId == convo.Lines.Count);

        String tweetText = MakeTweetString(context, convo, thisLineId, isFinalLine);
        context.Logger.LogInformation($"Upcoming line: {thisLineId}, tweet: {tweetText}\n");

        // Get the speakers
        Person speaker = People[convo.Lines[thisLineId].Character];
        Boolean ok = GetCredentials(context, speaker);
        if (!ok)
            return ReturnResponse(context, "Failure getting the credentials", HttpStatusCode.InternalServerError);

        String lastSpeakerHandle = "";
        if (thisLineId > 0)
        {
            String lastSpeakerShortName = convo.Lines[thisLineId - 1].Character;
            lastSpeakerHandle = People[lastSpeakerShortName].TwitterHandle;
        }

        // Tweet it
        String tweetId;
        if (String.IsNullOrEmpty(convo.LastTweetId))
        {
            tweetId = Tweeter.SendTweet(tweetText, AppKey, AppKeySecret,
                speaker.TwitterToken, speaker.TwitterSecret);
        }
        else
        {
            tweetId = Tweeter.ReplyToTweet(tweetText, lastSpeakerHandle, convo.LastTweetId, AppKey, AppKeySecret,
                speaker.TwitterToken, speaker.TwitterSecret);
        }

        // Update the published number in the database
        String published = thisLineId.ToString();
        if (isFinalLine)
            published = DateTime.Today.ToString();

        dbConvos.SetPublishedLine(convo.Id, published, true);
        dbConvos.SetLastTweeted(convo.Id, tweetId, true);

        return ReturnResponse(context, $"{speaker.ShortName}: {tweetText}", HttpStatusCode.OK);
    }

    private APIGatewayProxyResponse TestTweet(ILambdaContext context)
    {
        context.Logger.LogInformation("Test tweet...\n");

        Person holden = People[Person.HoldenShortName];

        Boolean ok = GetCredentials(context, holden);
        if (!ok)
            return ReturnResponse(context, "Failure getting the credentials", HttpStatusCode.InternalServerError);

        String message = "I never said Mars did it.";
        Tweeter.SendTweet(message, AppKey, AppKeySecret, holden.TwitterToken, holden.TwitterSecret);

        return ReturnResponse(context, $"{holden.ShortName}: {message}", HttpStatusCode.OK);
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

    private Boolean GetCredentials(ILambdaContext context, Person perp)
    {
        Boolean ok = true;
        String? apiKey = Environment.GetEnvironmentVariable(AppKeyName);
        if (String.IsNullOrEmpty(apiKey))
        {
            context.Logger.LogInformation("No ApiKey");
            ok = false;
        }
        else
        {
            AppKey = apiKey;
            context.Logger.LogInformation($"ApiKey starts with {AppKey[0]}");
        }

        String? apiSecretKey = Environment.GetEnvironmentVariable(AppSecretName);
        if (String.IsNullOrEmpty(apiSecretKey))
        {
            context.Logger.LogInformation("No ApiSecretKey");
            ok = false;
        }
        else
        {
            AppKeySecret = apiSecretKey;
            context.Logger.LogInformation($"ApiSecretKey starts with {AppKeySecret[0]}");
        }

        if (String.IsNullOrEmpty(perp.TwitterToken))
        {
            context.Logger.LogInformation("No AccessToken");
            ok = false;
        }
        else
            context.Logger.LogInformation($"AccessToken starts with {perp.TwitterToken[0]}");

        if (String.IsNullOrEmpty(perp.TwitterSecret))
        {
            context.Logger.LogInformation("No AccessTokenSecret");
            ok = false;
        }
        else
            context.Logger.LogInformation($"AccessTokenSecret starts with {perp.TwitterSecret[0]}");

        return ok;
    }

    //private static Tweeter? InitializeTweeter(ILambdaContext context, Person perp)
    //{
    //    context.Logger.LogInformation($"Person shortname: {perp.ShortName}, token: {perp.TwitterToken}, secret: {perp.TwitterSecret}");

    //    String? ApiKey = Environment.GetEnvironmentVariable(AppKeyName);
    //    if (String.IsNullOrEmpty(ApiKey))
    //        context.Logger.LogInformation("No ApiKey");
    //    else
    //        context.Logger.LogInformation($"ApiKey starts with {ApiKey[0]}");

    //    String? ApiSecretKey = Environment.GetEnvironmentVariable(AppSecretName);
    //    if (String.IsNullOrEmpty(ApiSecretKey))
    //        context.Logger.LogInformation("No ApiSecretKey");
    //    else
    //        context.Logger.LogInformation($"ApiSecretKey starts with {ApiSecretKey[0]}");

    //    if (String.IsNullOrEmpty(perp.TwitterToken))
    //        context.Logger.LogInformation("No AccessToken");
    //    else
    //        context.Logger.LogInformation($"AccessToken starts with {perp.TwitterToken[0]}");

    //    if (String.IsNullOrEmpty(perp.TwitterSecret))
    //        context.Logger.LogInformation("No AccessTokenSecret");
    //    else
    //        context.Logger.LogInformation($"AccessTokenSecret starts with {perp.TwitterSecret[0]}");

    //    Tweeter? tweeter = null;
    //    if (String.IsNullOrEmpty(ApiKey) || String.IsNullOrEmpty(ApiSecretKey) || String.IsNullOrEmpty(perp.TwitterToken) || String.IsNullOrEmpty(perp.TwitterSecret))
    //    {
    //        context.Logger.LogError("Twitter environment variable is missing");
    //        return tweeter;
    //    }

    //    tweeter = new Tweeter(ApiKey, ApiSecretKey, perp.TwitterToken, perp.TwitterSecret);
    //    return tweeter;
    //}

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

    //private APIGatewayProxyResponse TestConversation(ILambdaContext context, Int32 conversationId, Int32 lineId, String lastTweetId)
    //{
    //    context.Logger.LogInformation($"Testing conversation {conversationId}, line {lineId}\n");

    //    // Get the specific conversation
    //    var dbConvos = new ConversationsTable();
    //    var taskConvo = dbConvos.GetSpecificConversation(conversationId);
    //    Conversation convo = taskConvo.Result;

    //    // Tweet a line from the conversation
    //    Boolean isFinalLine = (1 + lineId == convo.Lines.Count);
    //    String tweetText = MakeTweetString(context, convo, lineId, isFinalLine);
    //    Person speaker = People[convo.Lines[lineId].Character];

    //    String lastSpeakerHandle = "";
    //    if (lineId > 0)
    //    {
    //        String lastSpeakerShortName = convo.Lines[lineId - 1].Character;
    //        lastSpeakerHandle = People[lastSpeakerShortName].TwitterHandle;
    //    }

    //    context.Logger.LogInformation($"Tweet text: {tweetText}\n");

    //    Boolean ok = GetCredentials(context, speaker);
    //    if (!ok)
    //        return ReturnResponse(context, "Failure getting the credentials", HttpStatusCode.InternalServerError);

    //    //String? ApiKey = Environment.GetEnvironmentVariable(AppKeyName);
    //    //if (String.IsNullOrEmpty(ApiKey))
    //    //{
    //    //    context.Logger.LogInformation("No ApiKey");
    //    //    return ReturnResponse(context, "No ApiKey", HttpStatusCode.InternalServerError);
    //    //}

    //    //String? ApiSecretKey = Environment.GetEnvironmentVariable(AppSecretName);
    //    //if (String.IsNullOrEmpty(ApiSecretKey))
    //    //{
    //    //    context.Logger.LogInformation("No ApiSecretKey");
    //    //    return ReturnResponse(context, "No ApiSecretKey", HttpStatusCode.InternalServerError);
    //    //}

    //    //if (String.IsNullOrEmpty(speaker.TwitterToken))
    //    //{
    //    //    context.Logger.LogInformation("No AccessToken");
    //    //    return ReturnResponse(context, "No AccessToken", HttpStatusCode.InternalServerError);
    //    //}

    //    //if (String.IsNullOrEmpty(speaker.TwitterSecret))
    //    //{
    //    //    context.Logger.LogInformation("No AccessTokenSecret");
    //    //    return ReturnResponse(context, "No AccessTokenSecret", HttpStatusCode.InternalServerError);
    //    //}

    //    String tweetId;
    //    if (String.IsNullOrEmpty(lastTweetId))
    //    {
    //        tweetId = Tweeter.SendTweet(tweetText, AppKey, AppKeySecret,
    //            speaker.TwitterToken, speaker.TwitterSecret);
    //    }
    //    else
    //    {
    //        LambdaLogger.Log($"tweetText: {tweetText}");
    //        tweetId = Tweeter.ReplyToTweet(tweetText, lastSpeakerHandle, lastTweetId, AppKey, AppKeySecret,
    //            speaker.TwitterToken, speaker.TwitterSecret);
    //    }

    //    return ReturnResponse(context, $"{speaker.ShortName}: {tweetText}, ID: {tweetId}", HttpStatusCode.OK);
    //}
}
