using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class KnowledgeService
    {
        public List<KnowledgeItem> Load()
        {
            string path = Path.Combine(AppContext.BaseDirectory,"Data","knowledge.json");

            string json = File.ReadAllText(path);
            // Ignore differences in uppercase and lowercase letters when matching JSON property names to C# property names.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var items = JsonSerializer.Deserialize<List<KnowledgeItem>>(json, options);

            return items;
        }

        public KnowledgeItem? Search(string question)
        {
            List<KnowledgeItem> items = Load();
            string cleanedQuestion = Regex.Replace(question.ToLower(), @"[^\w\s]", "");
            var words = cleanedQuestion
                        .ToLower()
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            KnowledgeItem? best = null;
            int bestScore = 0;

            foreach (var item in items)
            {
                int score = 0;

                foreach (var keyword in item.Keywords)
                {
                    if (words.Contains(keyword.ToLower()))
                    {
                        score++;
                    }
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = item;
                }
            }

            return best;
        }
    }
}
