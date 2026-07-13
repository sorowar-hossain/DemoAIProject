using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot
{
    public class RuleBasedChatbot
    {
        public void StartChat()
        {
            var responses = new Dictionary<string, string>()
                            {
                                { "hello", "Hi! How can I help you?" },
                                { "hi", "Hello!" },
                                { "what is ai", "AI stands for Artificial Intelligence." },
                                { "who are you","I am a machine"},
                                { "bye", "Goodbye!" }
                            };

            while (true)
            {
                Console.Write("You: ");
                string input = Console.ReadLine()?.ToLower() ?? "";

                if (responses.ContainsKey(input))
                {
                    var res= responses[input];
                    Console.WriteLine($"Bot: {responses[input]}");

                    if (input == "bye")
                        break;
                }
                else
                {
                    Console.WriteLine("Bot: I don't understand.");
                }
            }
        }
        public void StopChat()
        {

        }
    }
}
