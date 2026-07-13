using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot
{
    public class IntentBasedChatbot
    {
        List<Intent> intents = new List<Intent> { };
        public IntentBasedChatbot()
        {

            intents = new List<Intent>
                        {
                            new Intent
                            {
                                Name = "Greeting",
                                Patterns = new List<string>
                                {
                                    "hi",
                                    "hello",
                                    "hey",
                                    "good morning"
                                },
                                Responses = new List<string>
                                {
                                    "Hello!",
                                    "Hi there!",
                                    "Nice to meet you!"
                                }
                            },

                            new Intent
                            {
                                Name = "Goodbye",
                                Patterns = new List<string>
                                {
                                    "bye",
                                    "see you",
                                    "good night"
                                },
                                Responses = new List<string>
                                {
                                    "Goodbye!",
                                    "See you later!"
                                }
                            },

                            new Intent
                            {
                                Name = "AskName",
                                Patterns = new List<string>
                                {
                                    "what is your name",
                                    "who are you"
                                },
                                Responses = new List<string>
                                {
                                    "My name is LearningBot."
                                }
                            }
                        };
        }

        public static string Clean(string text)
        {
            text = text.ToLower();

            text = text.Replace("?", "")
                       .Replace(".", "")
                       .Replace(",", "");

            return text.Trim();
        }

        public Intent DetectIntent(string input)
        {
            input = Clean(input);

            Intent bestIntent = null;
            int bestScore = 0;

            foreach (var intent in intents)
            {
                int score = 0;

                foreach (var pattern in intent.Patterns)
                {
                    if (input.Contains(pattern))
                    {
                        score++;
                    }
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestIntent = intent;
                }
            }
            return bestIntent;
        }

        private readonly Random _random = new();

        public string GetResponse(string message)
        {
            var intent = DetectIntent(message);

            if (intent == null)
                return "Sorry, I didn't understand.";

            int index = _random.Next(intent.Responses.Count);

            return intent.Responses[index];
        }
    }
}
