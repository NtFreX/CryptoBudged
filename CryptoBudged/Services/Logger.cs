using System;
using System.Linq;

namespace CryptoBudged.Services
{
    public class Logger
    {
        private Logger() { }

        public void Log(string message)
            => Console.WriteLine("CryptoBudged.Services.Logger => " + message);
        public void Log(Exception exce)
            => Log(StringifyException(exce));

        private string StringifyException(Exception exce)
            => StringifyException(exce, string.Empty, 0);
        private string StringifyException(Exception exce, string currentMessage, int tabCount)
        {
            if (exce is AggregateException aggregateException)
            {
                currentMessage += aggregateException.GetType().Name + " : " + aggregateException.Message;
                foreach (var innerExce in aggregateException.InnerExceptions)
                {
                    currentMessage += Environment.NewLine + string.Join(string.Empty, Enumerable.Repeat("\t", tabCount)) + StringifyException(innerExce, currentMessage, tabCount + 1);
                }
                return currentMessage;
            }

            currentMessage += exce.GetType().Name + " : " + exce.Message;
            if (exce.InnerException != null)
            {
                currentMessage += Environment.NewLine + string.Join(string.Empty, Enumerable.Repeat("\t", tabCount)) + StringifyException(exce.InnerException, currentMessage, tabCount + 1);
            }
            return currentMessage;
        }

        public static Logger Instance { get; } = new Logger();
    }
}
