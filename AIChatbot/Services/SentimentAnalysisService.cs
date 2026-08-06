using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AIChatbot.Services
{
    public class SentimentAnalysisService
    {
        List<SentimentData> dataList; 
        List<string> vocabulary = new();
        CommonService commonService;
        NaiveBayesService naiveBayesService;

        private double positivePrior; 
        private double negativePrior;
        private double neutralPrior;

        private Dictionary<string, int> positiveWordCounts = new();
        private Dictionary<string, int> negativeWordCounts = new();
        private Dictionary<string, int> neutralWordCounts = new(); 

        private int totalPosWords;
        private int totalNegWords;
        private int totalNeutralWords; 

        private Dictionary<string, double> positiveProbabilities = new();
        private Dictionary<string, double> negativeProbabilities = new();
        private Dictionary<string, double> neutralProbabilities = new(); 
        private int vocabularySize;

        public SentimentAnalysisService() 
        {
            commonService = new CommonService();
            naiveBayesService = new NaiveBayesService();
            string path = Path.Combine(AppContext.BaseDirectory, "Data", "sentiment_dataset.json");
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            dataList = JsonSerializer.Deserialize<List<SentimentData>>(json, options); 
        }

        public List<string> BuildVocabulary()
        {
            return dataList
                    .SelectMany(faq =>
                        commonService.Lemmatization(
                            commonService.RemoveStopWord(
                                commonService.Tokenization(
                                    commonService.NormalizeText(faq.Text)))))
        .Distinct()
        .OrderBy(word => word)
        .ToList();
        }


        public PredictionResult SentimentAnalysis(string sentence)
        {

            vocabulary= BuildVocabulary();
            var lemms = Preprocess(sentence); 
            Dictionary<string,double> tf=commonService.CalculateTF(lemms);

          
            foreach (var data in dataList) 
            {
                data.Tokens = Preprocess(data.Text);
                data.TF = commonService.CalculateTF(data.Tokens);
               
            }

            List<List<string>> allData = dataList
                                          .Select(r => r.Tokens)
                                          .ToList();

            Dictionary<string, double> idf = commonService.CalculateIDF(allData);

            // small dataset
            /*
            foreach (var data in dataList)
            {
                data.TFIDF = commonService.CalculateTFIDF(data.TF, idf);
            }
             */


            // Parallel (only for very large datasets)
            Parallel.ForEach(dataList, data =>
            {
                data.TFIDF = commonService.CalculateTFIDF(data.TF, idf);
            });

            Train(dataList, vocabulary);
            var prediction = Predict(sentence);

            PredictionResult predictionResult = new PredictionResult()
            {
                Sentiment = prediction
            };

            //Dictionary<string, double> queryTFIDF = commonService.CalculateTFIDF(tf,idf);
            //double[] queryVector = commonService.CreateVector(queryTFIDF, vocabulary);

            return predictionResult;
        }

        public void Train(List<SentimentData> data, List<string> vocabulary)
        {
            CalculatePrior(dataList);

            CountWordFrequencies(dataList);

            CalculateWordProbabilities(vocabulary);
        }

        public void CalculatePrior(List<SentimentData> data)
        {
            int total = data.Count;

            int posCount = data.Count(x => x.Sentiment.ToLower() == "positive");

            int negCount = data.Count(x => x.Sentiment.ToLower() == "negative");
            int neutralCount = data.Count(x => x.Sentiment.ToLower() == "neutral"); 

            positivePrior = (double)posCount / total;

            negativePrior = (double)negCount / total;

            neutralPrior = (double)neutralCount / total;
        }

        public void CountWordFrequencies(List<SentimentData> data)
        {
            positiveWordCounts.Clear();
            negativeWordCounts.Clear();
            neutralWordCounts.Clear();

            totalNegWords = 0;
            totalPosWords = 0;
            totalNeutralWords = 0;  

            foreach (var document in data)
            {
                var tokens = document.Tokens; //  commonService.Preprocess(document.Text);
                foreach (var word in tokens)
                {
                    if (document.Sentiment.ToLower() == "positive") 
                    {
                        if (!positiveWordCounts.ContainsKey(word))
                            positiveWordCounts[word] = 0;

                        positiveWordCounts[word]++;

                        totalPosWords++;
                    }
                  else  if (document.Sentiment.ToLower() == "negative") 
                    {
                        if (!negativeWordCounts.ContainsKey(word))
                            negativeWordCounts[word] = 0;

                        negativeWordCounts[word]++;

                        totalNegWords++;
                    }

                    else if (document.Sentiment.ToLower() == "neutral")
                    {
                        if (!neutralWordCounts.ContainsKey(word))
                            neutralWordCounts[word] = 0;

                        neutralWordCounts[word]++;

                        totalNeutralWords++;
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
            positiveProbabilities.Clear();
            negativeProbabilities.Clear();
            neutralProbabilities.Clear();

            vocabularySize = vocabulary.Count;

            foreach (var word in vocabulary)
            {
                int posCount = positiveWordCounts.ContainsKey(word)
                    ? positiveWordCounts[word]
                    : 0;

                int negCount = negativeWordCounts.ContainsKey(word)
                    ? negativeWordCounts[word]
                    : 0;

                int neutralCount = neutralWordCounts.ContainsKey(word) 
                  ? neutralWordCounts[word]
                  : 0;

                positiveProbabilities[word] =
                    (double)(posCount + 1) /
                    (totalPosWords + vocabularySize);

                negativeProbabilities[word] =
                    (double)(negCount + 1) /
                    (totalNegWords + vocabularySize);

                neutralProbabilities[word] =
                   (double)(neutralCount + 1) /
                   (totalNeutralWords + vocabularySize);
            }
        }

        public string Predict(string text)
        {
            // Preprocess the message
            List<string> words = commonService.Preprocess(text);

            // Start with prior probabilities
            double positiveScore = Math.Log(positivePrior);
            double negativeScore = Math.Log(negativePrior);
            double neutralScore = Math.Log(neutralPrior);

            foreach (string word in words)
            {
                // Positive
                if (positiveProbabilities.ContainsKey(word))
                {
                    positiveScore += Math.Log(positiveProbabilities[word]);
                }

                // Negative
                if (negativeProbabilities.ContainsKey(word))
                {
                    negativeScore += Math.Log(negativeProbabilities[word]);
                }

                // Neutral
                if (neutralProbabilities.ContainsKey(word))
                {
                    neutralScore += Math.Log(neutralProbabilities[word]);
                }
            }

            if (positiveScore > negativeScore &&
                        positiveScore > neutralScore)
            {
                return "Positive";
            }

            if (negativeScore > neutralScore)
            {
                return "Negative";
            }

            return "Neutral";
        }


        public List<string> Preprocess(string text)
        {
            string normalized = commonService.NormalizeText(text);

            List<string> tokens = commonService.Tokenization(normalized);

            List<string> filtered = commonService.RemoveStopWord(tokens);

            List<string> lemmas = commonService.Lemmatization(filtered);

            return lemmas;
        }

    }
}
