namespace ExpanseBotsLambda
{
    internal class RequestBody
    {
        public String TweetType { get; set; }
        public Boolean ActuallyTweet { get; set; }

        public RequestBody()
        {
            TweetType = "";
            ActuallyTweet = false;
        }
    }
}
