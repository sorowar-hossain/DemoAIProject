using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class IntentClassificationService
    {
        List<IntentData> dataList;
        private List<string> intents = new();
        Dictionary<string, double> intentPrior = new();
        private Dictionary<string, Dictionary<string, int>> wordFrequency = new();
        Dictionary<string, Dictionary<string, double>> wordProbability = new();
        private Dictionary<string, int> totalWordsByIntent = new();
        List<string> vocabulary = new();
        CommonService commonService;
        NaiveBayesService naiveBayesService;
        public IntentClassificationService()
        {
            commonService = new CommonService();
            naiveBayesService = new NaiveBayesService();

            string path = Path.Combine(AppContext.BaseDirectory, "Data", "intent_dataset.json");
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            dataList = JsonSerializer.Deserialize<List<IntentData>>(json, options);
            intents = dataList.Select(x => x.Intent).Distinct().ToList();
        }

        public string GetIntent(string userQuestion)
        {

            vocabulary = BuildVocabulary();
            var tokens = Preprocess(userQuestion);
            Dictionary<string, double> tf = commonService.CalculateTF(tokens);

            foreach (var intent in dataList)
            {
                intent.Tokens = Preprocess(intent.Text);
                intent.TF = commonService.CalculateTF(intent.Tokens);
            }

            List<List<string>> allDataList = dataList.Select(x => x.Tokens).ToList();
            Dictionary<string, double> idf = commonService.CalculateIDF(allDataList);

            foreach (var intent in dataList)
            {
                intent.TFIDF = commonService.CalculateTFIDF(tf, idf);
            }

            Train(dataList, vocabulary);
            var res = Predict(userQuestion);

            return res;
        }

        public void Train(List<IntentData> list,List<string> dictionary)
        {
            CalcualatePriorProbability(list);
            CalculateWordFrequency(list);
            CalculateWordProbabilities(wordFrequency,vocabulary);

        }

        public void CalcualatePriorProbability(List<IntentData> list)
        {
            int totalCount = list.Count;
            intentPrior = list
                .GroupBy(x => x.Intent)
                .ToDictionary(
                    x => x.Key,
                    x => (double)x.Count() / totalCount
                );

        }

        public void CalculateWordFrequency(List<IntentData> data)
        {
            wordFrequency.Clear();

            foreach (var item in data)
            {
                string intent = item.Intent;

                if (!wordFrequency.ContainsKey(intent))
                {
                    wordFrequency[intent] = new Dictionary<string, int>();
                }

                var words = item.Tokens;

                foreach (var word in words)
                {
                    if (wordFrequency[intent].ContainsKey(word))
                    {
                        wordFrequency[intent][word]++;
                    }
                    else
                    {
                        wordFrequency[intent][word] = 1;
                    }
                }
            }
        }

        public void CalculateWordProbabilities(Dictionary<string, Dictionary<string, int>> wrdFrequency,List<string> aVocabulary)   
        {
            int vocabularySize = aVocabulary.Count;
            foreach (var item in wrdFrequency)
            {
                string intentName = item.Key;
                int totalWord=item.Value.Values.Sum();
                wordProbability[intentName] = new Dictionary<string, double>();

                foreach(var word in item.Value)
                {
                    double probability = (double)(word.Value + 1) / (totalWord+ vocabularySize);
                    wordProbability[intentName][word.Key] = probability;
                }
            }
           
        }

        public string Predict(string text)
        {
            var words = Preprocess(text);

            Dictionary<string, double> intentScores = new();

            foreach (var intent in intentPrior.Keys)
            {
                double score = intentPrior[intent];

                var totalWords = wordFrequency.Where(x => x.Key == intent).FirstOrDefault().Value.Count; 

                foreach (var word in words)
                {
                    double probability;

                    if (wordProbability[intent].ContainsKey(word))
                    {
                        probability =
                            wordProbability[intent][word];
                    }
                    else
                    {
                        // Laplace smoothing for unseen word
                        probability = 
                            1.0 /
                            (totalWords + vocabulary.Count);
                    }

                    score *= probability;
                }

                intentScores[intent] = score;
            }

            return intentScores
                .OrderByDescending(x => x.Value)
                .First()
                .Key;
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
