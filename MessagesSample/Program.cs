using System;

namespace MessagesSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var toNumber = "TO_NUMBER";
            var fromNumber = "FROM_NUMBER";
            var url = @"IMAGE_URL";
            MessageSender.SendMessage(url, fromNumber, toNumber);
            Console.WriteLine("Hello World!");
        }
    }
}
