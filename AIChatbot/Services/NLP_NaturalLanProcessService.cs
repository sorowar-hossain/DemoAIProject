using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AIChatbot.Services
{
    public class NLP_NaturalLanProcessService
    {
        public HashSet<string> stopWords =
              new()
              {
                "is","the","a","an","of","at","to","for","what"
              };
        private Dictionary<string, string> _dictionary;
        public string NormalizeText(string text)
        {
            string cleaned = new string(
                text.ToLower()
                    .Select(c => char.IsPunctuation(c) ? ' ' : c)
                    .ToArray());
            cleaned = string.Join(" ", cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var r=Tokenization(cleaned);
            var r2 = RemoveStopWord(r);
            var r22 = Lemmatize(r2);
            var r222 = FeatureExtraction(r22, r22);
            
            return cleaned;
        }

        public string[] Tokenization(string text)
        {
            string[] words = text.Split(' ',
                StringSplitOptions.RemoveEmptyEntries);
            return words;
        }
        public List<string> RemoveStopWord(string[] words)
        {
            var wrds = words
                    .Where(w => !stopWords.Contains(w))
                    .ToList();
            return wrds;
        }

        //Reduce words to their root.
        // playing -> play
        //working -> work
        //opened -> open
        public List<string> Stemming(List<string> words)
        {
            var stemWords= words
                        .Select(Stem)
                        .ToList();
            return stemWords;
        }

        public string Stem(string word)
        {
            word = word.ToLower();

            if (word.EndsWith("ing") && word.Length > 5)
                return word[..^3];

            if (word.EndsWith("ed") && word.Length > 4)
                return word[..^2];

            if (word.EndsWith("es") && word.Length > 4)
                return word[..^2];

            if (word.EndsWith("s") && word.Length > 3)
                return word[..^1];

            return word;
        }

        //Stemming simply removes letters.
        //Lemmatization understands language.
        public List<string> Lemmatize(List<string> words)
        {
            List<string> result = new();
            string path = Path.Combine(AppContext.BaseDirectory, "Data", "lemma.json");
            string json = File.ReadAllText(path);
            _dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            foreach (var word in words)
            {
                if (_dictionary.ContainsKey(word))
                    result.Add(_dictionary[word]);
                else
                    result.Add(word);
            }

            return result;
        }

        //Feature Extraction is the process of converting text into numerical features
        //that a machine learning model can use.

        public int[] FeatureExtraction(List<string> lemmas, List<string> vocabulary)
        {
            vocabulary = vocabulary
                        .Distinct()
                        .ToList();
            int[] featureVector = new int[vocabulary.Count];

            var frequencies = lemmas
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());

            for (int i = 0; i < vocabulary.Count; i++)
            {
                featureVector[i] = frequencies.TryGetValue(vocabulary[i], out int count)
                    ? count
                    : 0;
            }

            return featureVector;
        }
    }
}
