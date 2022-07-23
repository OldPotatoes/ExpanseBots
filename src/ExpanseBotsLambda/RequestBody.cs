namespace ExpanseBotsLambda
{
    internal class RequestBody
    {
        public String TweetType { get; set; }
        public Boolean ActuallyTweet { get; set; }
        public Int32 Conversation { get; set; }
        public Int32 Line { get; set; }
        public String LastTweetId { get; set; }

        public RequestBody()
        {
            TweetType = "";
            ActuallyTweet = false;
            Conversation = 0;
            Line = 0;
            LastTweetId = "";
        }
    }
}
