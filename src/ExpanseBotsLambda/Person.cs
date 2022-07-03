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
        public const String MillerShortName = "Miller";
        public const String HoldenShortName = "Holden";
        public const String NaomiShortName = "Naomi";
        public const String AmosShortName = "Amos";
        public const String AlexShortName = "Alex";
        public const String BobbieShortName = "Bobbie";
        public const String FredShortName = "Fred";
        public const String ClarissaShortName = "Clarissa";
        public const String ElviShortName = "Elvi";
        public const String HavelockShortName = "Havelock";
        public const String PraxShortName = "Prax";

        public const String AvasaralaFullName = "Chrisjen Avasarala";
        public const String MillerFullName = "Josephus Miller";
        public const String HoldenFullName = "James Holden";
        public const String NaomiFullName = "Naomi Nagata";
        public const String AmosFullName = "Amos Burton";
        public const String AlexFullName = "Alex Kamal";
        public const String BobbieFullName = "Bobbie Draper";
        public const String FredFullName = "Fred Lucius Johnson";
        public const String ClarissaFullName = "Clarissa Melpomene Mao";
        public const String ElviFullName = "Elvi Okoye";
        public const String HavelockFullName = "Dimitri Havelock";
        public const String PraxFullName = "Praxidike Meng";

        public const String AvasaralaToken = "AvasaralaBot_AccessToken";
        public const String MillerToken = "MillerBot_AccessToken";
        public const String HoldenToken = "HoldenBot_AccessToken";
        public const String NaomiToken = "NaomiBot_AccessToken";
        public const String AmosToken = "AmosBot_AccessToken";
        public const String AlexToken = "AlexBot_AccessToken";
        public const String BobbieToken = "BobbieBot_AccessToken";
        public const String FredToken = "FredBot_AccessToken";
        public const String ClarissaToken = "ClarissaBot_AccessToken";
        public const String ElviToken = "ElviBot_AccessToken";
        public const String HavelockToken = "HavelockBot_AccessToken";
        public const String PraxToken = "PraxBot_AccessToken";

        public const String AvasaralaSecret = "AvasaralaBot_AccessTokenSecret";
        public const String MillerSecret = "MillerBot_AccessTokenSecret";
        public const String HoldenSecret = "HoldenBot_AccessTokenSecret";
        public const String NaomiSecret = "NaomiBot_AccessTokenSecret";
        public const String AmosSecret = "AmosBot_AccessTokenSecret";
        public const String AlexSecret = "AlexBot_AccessTokenSecret";
        public const String BobbieSecret = "BobbieBot_AccessTokenSecret";
        public const String FredSecret = "FredBot_AccessTokenSecret";
        public const String ClarissaSecret = "ClarissaBot_AccessTokenSecret";
        public const String ElviSecret = "ElviBot_AccessTokenSecret";
        public const String HavelockSecret = "HavelockBot_AccessTokenSecret";
        public const String PraxSecret = "PraxBot_AccessTokenSecret";

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
                {
                    "Bobbie",
                    new Person
                    {
                        ShortName = BobbieShortName,
                        FullName = BobbieFullName,
                        TwitterHandle = Tweeter.BobbieBot,
                        TwitterID = Tweeter.BobbieBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(BobbieToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(BobbieSecret) ?? String.Empty
                    }
                },
                {
                    "Fred",
                    new Person
                    {
                        ShortName = FredShortName,
                        FullName = FredFullName,
                        TwitterHandle = Tweeter.FredBot,
                        TwitterID = Tweeter.FredBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(FredToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(FredSecret) ?? String.Empty
                    }
                },
                {
                    "Clarissa",
                    new Person
                    {
                        ShortName = ClarissaShortName,
                        FullName = ClarissaFullName,
                        TwitterHandle = Tweeter.ClarissaBot,
                        TwitterID = Tweeter.ClarissaBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(ClarissaToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(ClarissaSecret) ?? String.Empty
                    }
                },
                {
                    "Elvi",
                    new Person
                    {
                        ShortName = ElviShortName,
                        FullName = ElviFullName,
                        TwitterHandle = Tweeter.ElviBot,
                        TwitterID = Tweeter.ElviBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(ElviToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(ElviSecret) ?? String.Empty
                    }
                },
                {
                    "Havelock",
                    new Person
                    {
                        ShortName = HavelockShortName,
                        FullName = HavelockFullName,
                        TwitterHandle = Tweeter.HavelockBot,
                        TwitterID = Tweeter.HavelockBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(HavelockToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(HavelockSecret) ?? String.Empty
                    }
                },
                {
                    "Prax",
                    new Person
                    {
                        ShortName = PraxShortName,
                        FullName = PraxFullName,
                        TwitterHandle = Tweeter.PraxBot,
                        TwitterID = Tweeter.PraxBotUserId,
                        TwitterToken = Environment.GetEnvironmentVariable(PraxToken) ?? String.Empty,
                        TwitterSecret = Environment.GetEnvironmentVariable(PraxSecret) ?? String.Empty
                    }
                },
            };
        }
    }
}
