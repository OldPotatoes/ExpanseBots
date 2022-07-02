using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToTwitter;

namespace ExpanseBotsLambda
{
    public enum TweetDerivation
    {
        Unknown,
        TweetFromAB,
        TweetAtAB,
        ReplyToAB,
        RetweetOfAB,
        ReplyToOther
    }

    public class TweetDerivationPair
    {
        public TweetDerivation Derivation { get; set; }
        public Status? Tweet { get; set; }
    }
}
