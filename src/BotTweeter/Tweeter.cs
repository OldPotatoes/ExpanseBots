using Amazon.Lambda.Core;
using LinqToTwitter;
using LinqToTwitter.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BotTweeter
{
    public class Tweeter
    {
        private const String oAuthVersion = "1.0";
        private const String oAuthSignatureMethod = "HMAC-SHA1";

        public const Int64 AvasaralaBotUserId = 1284552017403420675;
        public const Int64 MillerBotUserId = 1518314290910048256;
        public const Int64 HoldenBotUserId = 1518312573137043456;
        public const Int64 NaomiBotUserId = 1518315541030653953;
        public const Int64 AmosBotUserId = 1518316485818687493;
        public const Int64 AlexBotUserId = 1518317034022572033;
        public const Int64 BobbieBotUserId = 1541187581093838849;
        public const Int64 FredBotUserId = 1541189422347177987;
        public const Int64 ClarissaBotUserId = 1541191039460966403;
        public const Int64 ElviBotUserId = 1541192486303023106;
        public const Int64 HavelockBotUserId = 1541193366330871817;
        public const Int64 PraxBotUserId = 1541195708157526020;
        public const String AvasaralaBot = "@AvasaralaBot";
        public const String MillerBot = "@JosephusBot";
        public const String HoldenBot = "@JamesHoldenBot";
        public const String NaomiBot = "@NaomiNagataBot";
        public const String AmosBot = "@AmosBurtonBot";
        public const String AlexBot = "@AlexKamalBot";
        public const String BobbieBot = "@BobbieDraperBot";
        public const String FredBot = "@FredJohnsonBot";
        public const String ClarissaBot = "@ClarissaMaoBot";
        public const String ElviBot = "@ElviOkoyeBot";
        public const String HavelockBot = "@HavelockBot";
        public const String PraxBot = "@PraxidikeBot";

        private readonly TwitterContext _context;

        public Tweeter(String apiKey, String apiSecretKey, String accessToken, String accessTokenSecret)
        {
            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = apiKey,
                    ConsumerSecret = apiSecretKey,
                    AccessToken = accessToken,
                    AccessTokenSecret = accessTokenSecret
                }
            };

            _context = new TwitterContext(auth);
        }


        public async Task<List<Tweet>> GetMentions(Int64 avasaralaBotUserId, DateTime lastReplyTime, Int32 numberOfTweets = 100)
        {
            // This does not pick up retweets
            LambdaLogger.Log($"GetMentions()\n");
            LambdaLogger.Log($"Get {numberOfTweets} tweets with for user ID {avasaralaBotUserId}, from date {lastReplyTime}.\n");

            var tweetResponse = await (from tweetQuery in _context.Tweets
                                       where tweetQuery.Type == TweetType.MentionsTimeline &&
                                       tweetQuery.ID == avasaralaBotUserId.ToString() &&
                                       tweetQuery.StartTime == lastReplyTime
                                       select tweetQuery).SingleOrDefaultAsync();

            var tweets = new List<Tweet>();
            if (tweetResponse != null && tweetResponse.Tweets != null)
            {
                tweets = tweetResponse.Tweets;
                LambdaLogger.Log("Mentions:");
                tweets.ForEach(tweet => LambdaLogger.Log($"User ID: {tweet.ID} tweeted: {tweet.Text}"));
            }
            else
                LambdaLogger.Log("No mentions found.");

            return tweets;
        }

        public List<Status> GetTweetsFrom(String tweeter, Int32 numberOfTweets = 100)
        {
            LambdaLogger.Log($"Get {numberOfTweets} tweets from {tweeter}.\n");
            var tweets =
                (from search in _context.Status
                 where search.Type == StatusType.User &&
                       search.ScreenName == tweeter &&
                       search.Count == numberOfTweets
                 select search).ToList();

            return tweets;
        }

        public async Task<Tweet> GetTweetWithId(String tweetId)
        {
            LambdaLogger.Log($"Get tweet with ID {tweetId}.\n");

            Tweet tweet = new Tweet();
            var tweetQuery = await (from t in _context.Tweets
                                    where t.Type == TweetType.Lookup && t.Ids == tweetId
                                    select t).SingleOrDefaultAsync();

            if (tweetQuery == null)
            {
                LambdaLogger.Log("No tweet found.");
                return tweet;
            }

            if (tweetQuery.Tweets == null)
            {
                LambdaLogger.Log("Tweet has no tweets.");
                return tweet;
            }

            tweet = tweetQuery.Tweets.Single();
            LambdaLogger.Log($"Found tweet: User ID: {tweet.ID} tweeted: {tweet.Text}");

            return tweet;
        }

        public void PrintTweet(String tweetId)
        {
            LambdaLogger.Log($"PrintTweet()\n");
            Tweet tweet = GetTweetWithId(tweetId).Result;

            PrintTweet(tweet);
        }

        public void PrintTweet(Tweet tweet)
        {
            LambdaLogger.Log($"TweetID: {tweet.ID}\n");

            LambdaLogger.Log($"Author ID: {tweet.AuthorID}\n");
            LambdaLogger.Log($"Conversation Tweet ID: {tweet.ConversationID}\n");
            LambdaLogger.Log($"Created at: {tweet.CreatedAt} UTC\n");
            LambdaLogger.Log($"In Reply to user: {tweet.InReplyToUserID}\n");
            LambdaLogger.Log($"Text: {tweet.Text}\n");

            if (tweet.Entities != null && tweet.Entities.Mentions != null)
            {
                LambdaLogger.Log($"Mentions: {String.Join(", ", tweet.Entities.Mentions.Select(t => t.Username))}\n");
            }

            if (tweet.ReferencedTweets != null)
            {
                LambdaLogger.Log($"References: {String.Join(", ", tweet.ReferencedTweets.Select(t => $"{t.ID} is a {t.Type}"))}\n");
            }
        }

        public String MaybeTweet(String tweetText, Boolean actuallyTweet)
        {
            LambdaLogger.Log($"Maybe ({actuallyTweet}) tweet: {tweetText}");
            String id = "";

            if (actuallyTweet)
            {
                id = Tweet(tweetText).Result;
                LambdaLogger.Log($"Actually tweeted: {tweetText}");
            }
            else
            {
                LambdaLogger.Log($"Not actually tweeted: {tweetText}");
            }

            return id;
        }

        public async Task<String> Tweet(String tweetText)
        {
            LambdaLogger.Log($"Tweet() {tweetText}");
            String id = "";

            try
            {
                var tweet = await _context.TweetAsync(tweetText);
                if (tweet == null)
                {
                    LambdaLogger.Log("Tweet failed to process, but API did not report an error\n");
                    return id;
                }
                
                if (tweet.ID == null)
                {
                    LambdaLogger.Log("Tweet ID is null, don't know why\n");
                    return id;
                }

                id = tweet.ID;
            }
            catch (Exception ex)
            {
                //_context.
                LambdaLogger.Log($"Exception: {ex.Message}\n");
                LambdaLogger.Log($"Stack: {ex.StackTrace}\n");
                LambdaLogger.Log($"Data: {ex.Data}\n");
                LambdaLogger.Log($"Inner: {ex.InnerException}\n");
            }

            return id;
        }

        public String MaybeReply(String tweetText, String tweetId, Boolean actuallyTweet)
        {
            LambdaLogger.Log($"Maybe reply ({actuallyTweet}) to tweet ID {tweetId}, tweet: {tweetText}.\n");
            String id = "";

            if (actuallyTweet)
            {
                id = Reply(tweetText, tweetId).Result;
                LambdaLogger.Log($"Actually replied: {tweetText}\n");
            }
            else
            {
                LambdaLogger.Log($"Not actually replied: {tweetText}\n");
            }

            return id;
        }


        public async Task<String> Reply(String tweetText, String tweetId)
        {
            LambdaLogger.Log($"Reply() to tweet ID {tweetId}, tweet: {tweetText}.\n");
            String id = "";

            try
            {
                var tweet = await _context.ReplyAsync(tweetText, tweetId);
                if (tweet == null)
                {
                    LambdaLogger.Log("Reply failed to process, but API did not report an error\n");
                    return id;
                }

                if (tweet.ID == null)
                {
                    LambdaLogger.Log("Reply ID is null, don't know why\n");
                    return id;
                }

                id = tweet.ID;
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"Exception: {ex.Message}\n");
            }

            return id;
        }

        public static String SendTweet(String message,
                                       String apiKey,
                                       String apiSecretKey,
                                       String accessToken,
                                       String accessTokenSecret)
        {
            return Tweet(message, apiKey, apiSecretKey, accessToken, accessTokenSecret);
        }

        public static String ReplyToTweet(String message,
                                          String replyingToPerson,
                                          String replyingToTweet,
                                          String apiKey,
                                          String apiSecretKey,
                                          String accessToken,
                                          String accessTokenSecret)
        {
            message = $"{replyingToPerson} {message}";
            //message = $"{message}";
            return Tweet(message, apiKey, apiSecretKey, accessToken, accessTokenSecret, replyingToTweet);
        }

        public static String Tweet(String message,
                                   String apiKey,
                                   String apiSecretKey,
                                   String accessToken,
                                   String accessTokenSecret,
                                   String replyToId = "")
        {
            String url = @"https://api.twitter.com/1.1/statuses/update.json";

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //ServicePointManager.Expect100Continue = false;

            HttpClient httpClient = new HttpClient();
            String authHeader = GenerateAuthorizationHeader(url, message, apiKey, apiSecretKey, accessToken, accessTokenSecret, replyToId);
            httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
            //httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            //httpClient.DefaultRequestHeaders.Host = "api.twitter.com";

            string postBody = "status=" + Uri.EscapeDataString(message);
            byte[] byteBody = Encoding.UTF8.GetBytes(postBody);
            ByteArrayContent body = new ByteArrayContent(byteBody);
            body.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            if (replyToId != "")
            {
                url = $"{url}?in_reply_to_status_id={replyToId}";
            }

            LambdaLogger.Log($"OauthHeader: {authHeader}");
            LambdaLogger.Log($"Post Body: {postBody}");
            LambdaLogger.Log($"URL: {url}");

            // Send the POST
            HttpResponseMessage response = httpClient.PostAsync(url, body).Result;
            String responseContent = response.Content.ReadAsStringAsync().Result;
            LambdaLogger.Log($"Response Content: {responseContent}\n");

            String id = FindIdInResponse(responseContent);
            LambdaLogger.Log($"Tweet ID: {id}");

            // Check for error status
            response.EnsureSuccessStatusCode();

            return id;
        }

        private static String FindIdInResponse(String responseContent)
        {
            String id = "";
            Int32 count = 0;
            var parts = responseContent.Split('"');
            foreach (String part in parts)
            {
                if (count + 3 > parts.Length)
                {
                    break;
                }

                if (part == "id_str")
                {
                    id = parts[count + 2];
                    break;
                }
                count++;
            }

            return id;
        }

        private static String GenerateAuthorizationHeader(String url,
                                                   String tweetMessage,
                                                   String functionAppKey,
                                                   String functionAppSecret,
                                                   String personToken,
                                                   String personSecret,
                                                   String replyToId="")
        {
            Double unixTimestamp = NowInUnixEpoch();
            String nonce = GenerateNonce();
            String oAuthSignature = GenerateOauthSignature(unixTimestamp, url, nonce, tweetMessage,
                                                           functionAppKey, functionAppSecret,
                                                           personToken, personSecret, replyToId);

            String a1 = $"oauth_consumer_key=\"{Uri.EscapeDataString(functionAppKey)}\"";
            String a2 = $"oauth_nonce=\"{Uri.EscapeDataString(nonce)}\"";
            String a3 = $"oauth_signature=\"{Uri.EscapeDataString(oAuthSignature)}\"";
            String a4 = $"oauth_signature_method=\"{Uri.EscapeDataString(oAuthSignatureMethod)}\"";
            String a5 = $"oauth_timestamp=\"{unixTimestamp}\"";
            String a6 = $"oauth_token=\"{Uri.EscapeDataString(personToken)}\"";
            String a7 = $"oauth_version=\"{Uri.EscapeDataString(oAuthVersion)}\"";

            String header = $"OAuth {a1}, {a2}, {a3}, {a4}, {a5}, {a6}, {a7}";
            return header;
        }

        private static String GenerateNonce()
        {
            String nonce = String.Empty;
            Random rando = new();
            for (Int32 count = 0; count < 32; count++)
            {
                Int32 ascii = rando.Next(65, 90);
                Char c = Convert.ToChar(ascii);
                nonce += c;
            }

            return nonce;
        }

        private static Double NowInUnixEpoch()
        {
            TimeSpan timePassed = DateTime.Now.ToUniversalTime() - DateTime.UnixEpoch;
            return Math.Floor(timePassed.TotalSeconds);
        }

        private static String GenerateOauthSignature(Double unixTimestamp,
                                                     String url,
                                                     String nonce,
                                                     String tweetMessage,
                                                     String functionAppKey,
                                                     String functionAppSecret,
                                                     String personToken,
                                                     String personSecret,
                                                     String replyToId="")
        {
            String a1 = $"in_reply_to_status_id={replyToId}";
            String a2 = $"oauth_consumer_key={functionAppKey}";
            String a3 = $"oauth_nonce={nonce}";
            String a4 = $"oauth_signature_method={oAuthSignatureMethod}";
            String a5 = $"oauth_timestamp={unixTimestamp}";
            String a6 = $"oauth_token={personToken}";
            String a7 = $"oauth_version={oAuthVersion}";
            String a8 = $"status={Uri.EscapeDataString(tweetMessage)}";

            String queryParams = $"{a2}&{a3}&{a4}&{a5}&{a6}&{a7}&{a8}";

            if (!String.IsNullOrEmpty(replyToId))
            {
                queryParams = $"{a1}&{queryParams}";
            }

            // This (among other things) double percent-encodes the status, which is needed
            // Note the two un-percent-encoded ampersands, which are needed
            String post = $"POST&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(queryParams)}";
            Console.WriteLine($"OauthSignature: {post}");

            HMACSHA1 hmac = new HMACSHA1();
            String signingKey = $"{Uri.EscapeDataString(functionAppSecret)}&{Uri.EscapeDataString(personSecret)}";
            hmac.Key = Encoding.UTF8.GetBytes(signingKey);

            Byte[] dataBuff = Encoding.UTF8.GetBytes(post);
            Byte[] hashBytes = hmac.ComputeHash(dataBuff);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
