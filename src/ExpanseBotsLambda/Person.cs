using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotTweeter;


namespace ExpanseBotsLambda
{
    public class Person
    {
        public const String AvasaralaShortName = "Avasarala";
        public const String AvasaralaFullName = "Chrisjen Avasarala";

        public const String MillerShortName = "Miller";
        public const String MillerFullName = "Josephus Miller";

        public const String HoldenShortName = "Holden";
        public const String HoldenFullName = "James Holden";

        public const String NaomiShortName = "Naomi";
        public const String NaomiFullName = "Naomi Nagata";

        public const String AmosShortName = "Amos";
        public const String AmosFullName = "Amos Burton";

        public const String AlexShortName = "Alex";
        public const String AlexFullName = "Alex Kamal";

        public const String AvasaralaToken = "AvasaralaBot_AccessToken";
        public const String AvasaralaSecret = "AvasaralaBot_AccessTokenSecret";
        public const String MillerToken = "MillerBot_AccessToken";
        public const String MillerSecret = "MillerBot_AccessTokenSecret";
        public const String HoldenToken = "HoldenBot_AccessToken";
        public const String HoldenSecret = "HoldenBot_AccessTokenSecret";
        public const String NaomiToken = "NaomiBot_AccessToken";
        public const String NaomiSecret = "NaomiBot_AccessTokenSecret";
        public const String AmosToken = "AmosBot_AccessToken";
        public const String AmosSecret = "AmosBot_AccessTokenSecret";
        public const String AlexToken = "AlexBot_AccessToken";
        public const String AlexSecret = "AlexBot_AccessTokenSecret";

        public String ShortName { get; set; }
        public String FullName { get; set; }
        public String TwitterHandle { get; set; }
        public Int64 TwitterID { get; set; }
        public String TwitterToken { get; set; }
        public String TwitterSecret { get; set; }

        public Person()
        {
            ShortName = "";
            FullName = "";
            TwitterHandle = "";
            TwitterToken = "";
            TwitterSecret = "";
        }

        public static Dictionary<String, Person> PopulatePeople()
        {
            return new Dictionary<String, Person>
            {
                {
                    "Avasarala",
                    new Person
                    {
                        ShortName = AvasaralaShortName,
                        FullName = AvasaralaFullName,
                        TwitterHandle = Tweeter.AvasaralaBot,
                        TwitterID = Tweeter.AvasaralaBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(AvasaralaToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(AvasaralaSecret) ?? String.Empty
                    }
                },
                {
                    "Miller",
                    new Person
                    {
                        ShortName = MillerShortName,
                        FullName = MillerFullName,
                        TwitterHandle = Tweeter.MillerBot,
                        TwitterID = Tweeter.MillerBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(MillerToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(MillerSecret) ?? String.Empty
                    }
                },
                {
                    "Holden",
                    new Person
                    {
                        ShortName = HoldenShortName,
                        FullName = HoldenFullName,
                        TwitterHandle = Tweeter.HoldenBot,
                        TwitterID = Tweeter.HoldenBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(HoldenToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(HoldenSecret) ?? String.Empty
                    }
                },
                {
                    "Naomi",
                    new Person
                    {
                        ShortName = NaomiShortName,
                        FullName = NaomiFullName,
                        TwitterHandle = Tweeter.NaomiBot,
                        TwitterID = Tweeter.NaomiBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(NaomiToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(NaomiSecret) ?? String.Empty
                    }
                },
                {
                    "Amos",
                    new Person
                    {
                        ShortName = AmosShortName,
                        FullName = AmosFullName,
                        TwitterHandle = Tweeter.AmosBot,
                        TwitterID = Tweeter.AmosBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(AmosToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(AmosSecret) ?? String.Empty
                    }
                },
                {
                    "Alex",
                    new Person
                    {
                        ShortName = AlexShortName,
                        FullName = AlexFullName,
                        TwitterHandle = Tweeter.AlexBot,
                        TwitterID = Tweeter.AlexBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(AlexToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(AlexSecret) ?? String.Empty
                    }
                },
            };
        }
    }
}
