using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class CommonService
    {
        public CommonService()
        {

        }
        public HashSet<string> stopWords =
           new()
           {
                "is","the","a","an","of","at","to","for","what","am","was","were","in"
           };
        private Dictionary<string, string> _dictionary;
        //=========Text normalization ===========
        /*
            Convert to lowercase
            Remove punctuation
            Remove extra spaces
            Remove special characters
         */
        public string NormalizeText(string text)
        {
            string cleaned = new string(
                text.ToLower()
                    .Select(c => char.IsPunctuation(c) ? ' ' : c)
                    .ToArray());
            cleaned = string.Join(" ", cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return cleaned;
        }

        //=========normalization => Tokenization ===========

        /*
         Purpose
            Split text into individual words (tokens).
            Computers process words individually.
         */

        public string[] Tokenization(string text)
        {
            string[] words = text.Split(' ',
                StringSplitOptions.RemoveEmptyEntries);
            return words;
        }


        //=========Tokenization => RemoveStopWord ===========

        /*
         Purpose
            Remove common words that add little meaning.
            These words appear everywhere.
            They don't help distinguish documents.
         */
        public List<string> RemoveStopWord(string[] words)
        {
            var wrds = words
                    .Where(w => !stopWords.Contains(w))
                    .ToList();
            return wrds;
        }


        //=========RemoveStopWord => Stemming/Lemmatizations ===========
        /*
       Purpose
         Reduce words to their root form by chopping off endings.
          playing,played,plays => play
          running => run
          studies => studi ,This isn't a real English word.
          fishing => fish

        Advantages
            Fast
            Simple

        Disadvantages
            Can produce incorrect words
       */
        public List<string> Stemming(List<string> words)
        {
            var stemWords = words
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

        /*
         Advantages
            More accurate
            Produces valid words

        Disadvantages
            Slower
            Needs a dictionary/model
         */
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

        //=========Stemming/Lemmatizations => Bag of Word ===========
        /*
        Purpose
         Convert text into numbers.
         Machine learning cannot understand text.
         It understands numbers.
        */
    }
}
