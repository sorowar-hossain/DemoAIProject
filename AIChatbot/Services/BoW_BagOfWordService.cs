using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    //In classical NLP, Bag of Words is one type of feature extraction.
    //Bag of Words converts a sentence into a vector of word counts.
    //That vector is called the Bag of Words feature vector.
    public class BoW_BagOfWordService
    {
        public List<List<string>> documents = new()
                                {
                                    new() { "learn", "c#" },
                                    new() { "sql", "database" },
                                    new() { "ai", "learn" }
                                };

        //Build the vocabulary:
        public List<string> BuildVocabulary(List<List<string>> documents)
        {
            return documents
                .SelectMany(x => x)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        //This is exactly what ML models use.
        public int[] BagOfWords_FeatureExtraction(List<string> lemmas) 
        {
            var vocabulary = BuildVocabulary(documents);
                
            int[] vector = new int[vocabulary.Count];

            Dictionary<string, int> frequencies = lemmas
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());

            for (int i = 0; i < vocabulary.Count; i++)
            {
                if (frequencies.TryGetValue(vocabulary[i], out int count))
                    vector[i] = count;
                else
                    vector[i] = 0;
            }

            return vector;
        }
    }
}
