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
                 // Articles
                    "a", "an", "the",
                 // Pronouns
                    "i", "me", "my", "mine",
                    "you", "your", "yours",
                    "he", "him", "his",
                    "she", "her", "hers",
                    "it", "its",
                    "we", "us", "our", "ours",
                    "they", "them", "their", "theirs",
               // Be verbs
                    "am", "is", "are", "was", "were", "be", "been", "being",

                // Auxiliary verbs
                    "do", "does", "did",
                    "have", "has", "had",

                // Modal verbs
                "can", "could", "will", "would",
                "shall", "should",
                "may", "might", "must",

                // Common question words
                "how", "what", "when", "where",
                "which", "who", "whom", "whose",
                "why",

                // Prepositions
                "of", "to", "for", "at", "in", "on",
                "by", "with", "from", "into", "onto",
                "over", "under", "between", "through",

                // Conjunctions
                "and", "or", "but", "if", "than", "then", "as"
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

        public List<string> Tokenization(string text)
        {
            List<string> words = text.Split(' ',
                StringSplitOptions.RemoveEmptyEntries).ToList();
            return words;
        }


        //=========Tokenization => RemoveStopWord ===========

        /*
         Purpose
            Remove common words that add little meaning.
            These words appear everywhere.
            They don't help distinguish documents.
         */
        public List<string> RemoveStopWord(List<string> words)
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
        public List<string> Lemmatization(List<string> words) 
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

        //=========Stemming/Lemmatizations => N-Gram ===========
        /*
        Purpose
        understanding N-Grams is important because they help capture phrases instead of 
        only individual words.
        Without N-Grams, the sentence is treated as separate words, and it breaks relationship
        
        What is an N-Gram?
        An N-Gram is a sequence of N consecutive words (or characters) from a piece of text.
        N = 1 → Unigram
        N = 2 → Bigram
        N = 3 → Trigram
        N = 4 → Four-gram

        I love learning NLP
        1. Unigram (N = 1)

        Each individual word is one token.

        I
        love
        learning
        NLP

        2. Bigram (N = 2)

        Take two consecutive words.

        I love
        love learning
        learning NLP

        Output:

        [I love]
        [love learning]
        [learning NLP]
        */

        public List<string> N_Gram(List<string> words)
        {
            string sentence = "i love football very much";
            //words = sentence.Split(' ').ToList();

            List<string> bigrams = new();

            for (int i = 0; i < words.Count - 1; i++)
            {
                bigrams.Add($"{words[i]} {words[i + 1]}");
            }

            foreach (var gram in bigrams)
            {
                Console.WriteLine(gram);
            }
            return bigrams;
        }


        /*
         Term Frequency(TF) measures how many times a word appears in a document.
         TF measures how important a word is within a single document
         Instead of only knowing whether a word exists(BoW), 
         TF tells us how important the word is inside that document based on how often it appears.
        */
        public Dictionary<string, double> CalculateTF(List<string> words)
        {
            Dictionary<string, double> tf = new();

            int totalWords = words.Count;

            var frequencies = words
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var item in frequencies)
            {
                tf[item.Key] = (double)item.Value / totalWords;
            }

            return tf;
        }

        /*
         The important difference is:

        TF → Calculated for one document.
        IDF → Calculated using all documents.

        So you should not pass List<string> words to the IDF method. Instead, pass all processed FAQ documents.

        Formula
        IDF(t)=log(N/DF(t)​)

        Where:

        N = Total number of documents
        DF = Number of documents containing the word

        For example:

            Doc1: reset password
            Doc2: change password
            Doc3: update email

            To calculate the IDF of password, you must know:

            Total documents = 3
            Documents containing "password" = 2
      */
        public Dictionary<string, double> CalculateIDF(List<List<string>> documents)
        {
            Dictionary<string, double> idf = new();

            int totalDocuments = documents.Count;

            // Vocabulary
            var vocabulary = documents
                .SelectMany(x => x)
                .Distinct();

            foreach (var word in vocabulary)
            {
                int documentFrequency = documents.Count(doc => doc.Contains(word));

                idf[word] = Math.Log((double)totalDocuments / documentFrequency);
            }

            return idf;
        }

        //This preprocessing happens once when the application starts.
        public Dictionary<string, double> CalculateTFIDF( Dictionary<string, double> tf, Dictionary<string, double> idf)
        {
            Dictionary<string, double> tfidf = new();

            foreach (var item in tf)
            {
                if (idf.TryGetValue(item.Key, out double idfValue))
                {
                    tfidf[item.Key] = item.Value * idfValue;
                }
            }

            return tfidf;
        }

        public double[] CreateVector( Dictionary<string, double> tfidf,List<string> vocabulary)
        {
            double[] vector = new double[vocabulary.Count];

            for (int i = 0; i < vocabulary.Count; i++)
            {
                if (tfidf.TryGetValue(vocabulary[i], out double value))
                    vector[i] = value;
                else
                    vector[i] = 0;
            }

            return vector;
        }


        public double CalculateCosineSimilarity(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same length.");

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
        }

        public List<string> Preprocess(string question)
        {
            // Step 1
            string normalized = NormalizeText(question);

            // Step 2 
            List<string> words = Tokenization(normalized);

            // Step 3
            words = RemoveStopWord(words);

            // Step 4
            words = Lemmatization(words);

            return words;
        }

    }
}
