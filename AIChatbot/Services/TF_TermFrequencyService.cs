using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIChatbot.Services
{
    public class TF_TermFrequencyService
    {
        //Term Frequency(TF) measures how many times a word appears in a document.
        //TF measures how important a word is within a single document
        /* Instead of only knowing whether a word exists(BoW), 
            TF tells us how important the word is inside that document based on how often it appears.
        */


        public List<string> tokens = new()
                            {
                                "love",
                                "ai",
                                "because",
                                "ai",
                                "future",
                                "ai"
                            };

        public Dictionary<string, double> CalculateTF()
        {
            Dictionary<string, double> tf = new();

            int totalWords = tokens.Count;

            var frequencies = tokens
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var item in frequencies)
            {
                tf[item.Key] = (double)item.Value / totalWords;
            }

            foreach (var item in tf)
            {
                Console.WriteLine($"{item.Key} : {item.Value:F3}");
            }

            return tf;
        }

    }
}
