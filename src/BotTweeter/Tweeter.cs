using Amazon.Lambda.Core;
using LinqToTwitter;
using LinqToTwitter.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotTweeter
{
    public class Tweeter
    {
        public const Int64 AvasaralaBotUserId = 1284552017403420675;
        public const Int64 MillerBotUserId = 1518314290910048256;
        public const Int64 HoldenBotUserId = 1518312573137043456;
        public const Int64 NaomiBotUserId = 1518315541030653953;
        public const Int64 AmosBotUserId = 1518316485818687493;
        public const Int64 AlexBotUserId = 1518317034022572033;
        public const String AvasaralaBot = "@AvasaralaBot";
        public const String MillerBot = "@JosephusBot";
        public const String HoldenBot = "@bot_holden";
        public const String NaomiBot = "@NaomiNagataBot";
        public const String AmosBot = "@AmosBurtonBot";
        public const String AlexBot = "@BotAlexKamal";

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
            LambdaLogger.Log($"    Get {numberOfTweets} tweets with for user ID {avasaralaBotUserId}, from date {lastReplyTime}.\n");

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
            LambdaLogger.Log($"GetTweetsFrom()\n");
            LambdaLogger.Log($"    Get {numberOfTweets} tweets from {tweeter}.\n");
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
            LambdaLogger.Log($"GetTweetWithId()\n");
            LambdaLogger.Log($"    Get tweet with ID {tweetId}.\n");

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
            LambdaLogger.Log($"    TweetID: {tweet.ID}\n");

            LambdaLogger.Log($"        Author ID: {tweet.AuthorID}\n");
            LambdaLogger.Log($"        Conversation Tweet ID: {tweet.ConversationID}\n");
            LambdaLogger.Log($"        Created at: {tweet.CreatedAt} UTC\n");
            LambdaLogger.Log($"        In Reply to user: {tweet.InReplyToUserID}\n");
            LambdaLogger.Log($"        Text: {tweet.Text}\n");

            if (tweet.Entities != null && tweet.Entities.Mentions != null)
            {
                LambdaLogger.Log($"        Mentions: {String.Join(", ", tweet.Entities.Mentions.Select(t => t.Username))}\n");
            }

            if (tweet.ReferencedTweets != null)
            {
                LambdaLogger.Log($"        References: {String.Join(", ", tweet.ReferencedTweets.Select(t => $"{t.ID} is a {t.Type}"))}\n");
            }
        }

        public String MaybeTweet(String tweetText, Boolean actuallyTweet)
        {
            LambdaLogger.Log($"MaybeTweet()\n");
            LambdaLogger.Log($"    Maybe ({actuallyTweet}) tweet: {tweetText}.\n");
            String id = "";

            if (actuallyTweet)
            {
                id = Tweet(tweetText).Result;
                LambdaLogger.Log($"    Actually tweeted: {tweetText}\n");
            }
            else
            {
                LambdaLogger.Log($"    Not actually tweeted: {tweetText}\n");
            }

            return id;
        }

        public async Task<String> Tweet(String tweetText)
        {
            LambdaLogger.Log($"Tweet() {tweetText}.\n");
            String id = "";

            try
            {
                var tweet = await _context.TweetAsync(tweetText);
                if (tweet == null)
                {
                    LambdaLogger.Log("    Tweet failed to process, but API did not report an error\n");
                    return id;
                }
                
                if (tweet.ID == null)
                {
                    LambdaLogger.Log("    Tweet ID is null, don't know why\n");
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

        public String MaybeReply(String tweetText, String tweetId, Boolean actuallyTweet)
        {
            LambdaLogger.Log($"MaybeReply()\n");
            LambdaLogger.Log($"    Maybe reply ({actuallyTweet}) to tweet ID {tweetId}, tweet: {tweetText}.\n");
            String id = "";

            if (actuallyTweet)
            {
                id = Reply(tweetText, tweetId).Result;
                LambdaLogger.Log($"    Actually replied: {tweetText}\n");
            }
            else
            {
                LambdaLogger.Log($"    Not actually replied: {tweetText}\n");
            }

            return id;
        }


        public async Task<String> Reply(String tweetText, String tweetId)
        {
            LambdaLogger.Log($"    Reply() to tweet ID {tweetId}, tweet: {tweetText}.\n");
            String id = "";

            try
            {
                var tweet = await _context.ReplyAsync(tweetText, tweetId);
                if (tweet == null)
                {
                    LambdaLogger.Log("    Reply failed to process, but API did not report an error\n");
                    return id;
                }

                if (tweet.ID == null)
                {
                    LambdaLogger.Log("    Reply ID is null, don't know why\n");
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
    }
}
