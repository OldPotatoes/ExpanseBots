namespace ExpanseBotsLambda
{
    internal class RequestBody
    {
        public String TweetType { get; set; }
        public Int32 Conversation { get; set; }
        public Int32 Line { get; set; }

        public RequestBody()
        {
            TweetType = "";
            Conversation = 0;
            Line = 0;
        }
    }
}
