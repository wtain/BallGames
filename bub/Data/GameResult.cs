
using System;
using bub.Extensions;

namespace bub.Data
{
    public class GameResult
    {
        public string UserName { get; private set; }
        public int Score { get; private set; }
        public DateTime DateWhen { get; private set; }

        public GameResult(string userName, int score, DateTime dateWhen)
        {
            UserName = userName;
            Score = score;
            DateWhen = dateWhen;
        }

        public GameResult(string userName, int score, string dateWhen)
            : this(userName, score, parseDate(dateWhen))
        {
            
        }
        
        private static DateTime parseDate(string dateWhen)
        {
            if (!dateWhen.IsNullOrEmpty())
            {
                DateTime result;
                if (DateTime.TryParse(dateWhen, out result))
                    return result;
            }
            return DateTime.MinValue;
        }
    }
}
