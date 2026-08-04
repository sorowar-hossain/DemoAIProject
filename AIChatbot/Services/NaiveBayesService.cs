using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class NaiveBayesService
    {
        CommonService commonService;
        public NaiveBayesService() 
        { 
            this.commonService = new CommonService();
        }
        private double spamPrior;
        private double hamPrior;

        private Dictionary<string, int> spamWordCounts = new();
        private Dictionary<string, int> hamWordCounts = new();

        private int totalSpamWords;
        private int totalHamWords;

        private Dictionary<string, double> spamProbabilities = new();
        private Dictionary<string, double> hamProbabilities = new();

        private int vocabularySize;


        public void CalculatePrior(List<SpamData> data)
        {
            int total = data.Count;

            int spamCount = data.Count(x => x.Label.ToLower() == "spam");

            int hamCount = data.Count(x => x.Label.ToLower() == "ham");

            spamPrior = (double)spamCount / total;

            hamPrior = (double)hamCount / total;
        }

        public void CountWordFrequencies(List<SpamData> data)
        {
            spamWordCounts.Clear();
            hamWordCounts.Clear();

            totalSpamWords = 0;
            totalHamWords = 0;

            foreach (var document in data)
            {
                var tokens = commonService.Preprocess(document.Text);
                foreach (var word in tokens)
                {
                    if (document.Label.ToLower() == "spam")
                    {
                        if (!spamWordCounts.ContainsKey(word))
                            spamWordCounts[word] = 0;

                        spamWordCounts[word]++;

                        totalSpamWords++;
                    }
                    else
                    {
                        if (!hamWordCounts.ContainsKey(word))
                            hamWordCounts[word] = 0;

                        hamWordCounts[word]++;

                        totalHamWords++;
                    }
                }
            }
        }

        /*
            Laplace Smoothing
            Why do we need it?

            Suppose your training data is:

            Spam

            free prize
            free lottery

            Ham

            meeting project
            meeting today

            Now suppose a new message is:

            free meeting bonus

            The word bonus never appeared in the training data.

            Without smoothing:

            P(bonus|Spam) = 0

            Since Naive Bayes multiplies probabilities:

            Score = 0.8 × 0.5 × 0 × 0.3 = 0

            A single unseen word makes the entire score zero, which is undesirable.

            Laplace Smoothing Formula

            For every word:

            P(word|Spam)
            =
            (Count(word in Spam) + 1)
            /
            (Total Spam Words + Vocabulary Size)

            Similarly:

            P(word|Ham)
            =
            (Count(word in Ham) + 1)
            /
            (Total Ham Words + Vocabulary Size)

            Adding 1 ensures every word has a non-zero probability.
         
         */
        //Calculate Conditional Probabilities(with Laplace Smoothing) , add 1
        public void CalculateWordProbabilities(List<string> vocabulary)
        {
            spamProbabilities.Clear();
            hamProbabilities.Clear();

            vocabularySize = vocabulary.Count;

            foreach (var word in vocabulary)
            {
                int spamCount = spamWordCounts.ContainsKey(word)
                    ? spamWordCounts[word]
                    : 0;

                int hamCount = hamWordCounts.ContainsKey(word)
                    ? hamWordCounts[word]
                    : 0;

                spamProbabilities[word] =
                    (double)(spamCount + 1) /
                    (totalSpamWords + vocabularySize);

                hamProbabilities[word] =
                    (double)(hamCount + 1) /
                    (totalHamWords + vocabularySize);
            }
        }

        public void Train(List<SpamData> data, List<string> vocabulary)
        {
            CalculatePrior(data);

            CountWordFrequencies(data);

            CalculateWordProbabilities(vocabulary);
        }

        public string Predict(string text)
        {
            // Preprocess the message
            List<string> words = commonService.Preprocess(text);

            // Start with prior probabilities
            double spamScore = Math.Log(spamPrior);
            double hamScore = Math.Log(hamPrior);

            foreach (string word in words)
            {
                // Spam
                if (spamProbabilities.ContainsKey(word))
                {
                    spamScore += Math.Log(spamProbabilities[word]);
                }

                // Ham
                if (hamProbabilities.ContainsKey(word))
                {
                    hamScore += Math.Log(hamProbabilities[word]);
                }
            }

            return spamScore > hamScore ? "Spam" : "Ham";
        }
    }
}
