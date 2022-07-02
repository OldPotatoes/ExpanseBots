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
    public const Boolean ActuallyTweet = false;
    public const String AppKey = "ExpanseBots_ApiKey";
    public const String AppSecret = "ExpanseBots_ApiSecretKey";

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

        (RequestBody body, Boolean ok) = ReadRequestBody(context, request.Body);
        if (!ok)
        {
            message = "ExpanseBots needs a request to action. Goodbye\n";
            return ReturnResponse(context, message, HttpStatusCode.BadRequest);
        }

        if (body.ActuallyTweet == true)
        {
            message = "We're not ready to actually tweet. Goodbye\n";
            return ReturnResponse(context, message, HttpStatusCode.BadRequest);
        }

        if (body.TweetType == "Conversation")
        {
            return Conversation(context);
        }

        message = "I am Expanse Bots, give me something to do.\n";
        return ReturnResponse(context, message, HttpStatusCode.OK);
    }

    private APIGatewayProxyResponse Conversation(ILambdaContext context)
    {
        context.Logger.LogInformation("Beginning a conversation...\n");

        var dbConvos = new ConversationsTable();
        var taskConvos = dbConvos.GetAllConversations(true);
        List<Conversation> convos = taskConvos.Result;

        Conversation convo = dbConvos.GetCurrentlyPublishedConversation(convos);
        Int32 previousLine = dbConvos.GetNextLineIndex(convo);
        if (previousLine == -1)
        {
            convo = convos[(new Random()).Next(convos.Count)];
        }

        Int32 thisLine = previousLine+1;
        Boolean isFinalLine = (1 + thisLine == convo.Lines.Count);
        String tweetText = MakeTweetString(context, convo, thisLine, isFinalLine);
        Person speaker = People[convo.Lines[thisLine].Character];

        context.Logger.LogInformation($"Line pub: {previousLine}, next line: {thisLine}, actual: {convo.Lines[thisLine].Quote}\n");
        Tweeter? tweeter = InitializeTweeter(context, speaker);
        if (tweeter == null)
        {
            String message = $"Failed to initialize the tweeter for {thisLine}";
            return ReturnResponse(context, message, HttpStatusCode.InternalServerError);
        }

        tweeter.MaybeTweet(tweetText, ActuallyTweet);

        // Update the published number in the database
        if (isFinalLine)
            dbConvos.SetPublishedLine(convo.Id, "Yes", true);
        else
            dbConvos.SetPublishedLine(convo.Id, thisLine.ToString(), true);

        return ReturnResponse(context, tweetText, HttpStatusCode.OK);
    }

    private static (RequestBody, Boolean) ReadRequestBody(ILambdaContext context, String boddy)
    {
        Boolean Ok = false;
        context.Logger.LogInformation($"Request: {boddy}\n");

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
}