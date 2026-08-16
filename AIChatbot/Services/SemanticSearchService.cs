using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    /*
        "I cannot remember my login password"
        → Forgot Password / Password Recovery / Password Reset

        "Where can I see my remaining holidays?"
        → Leave Balance / Annual Leave

        "When will my package arrive?"
        → Expected Delivery / Delivery Status / Order Tracking

        "I want my money returned"
        → Money Back / Refund Request / Refund Status

        "I need help from an agent"
        → Customer Support / Contact Support
     
     */
    public class SemanticSearchService
    {
        List<SemanticDocumentData> dataList;
        List<string> vocabulary = new();
        CommonService commonService;

        public SemanticSearchService() 
        { 
            commonService = new CommonService();
            string path = Path.Combine(AppContext.BaseDirectory, "Data", "semantic_search_dataset.json");
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            dataList = JsonSerializer.Deserialize<List<SemanticDocumentData>>(json, options);
        }

        public List<string> BuildVocabulary()
        {
            return dataList
                    .SelectMany(x =>
                        commonService.Lemmatization(
                            commonService.RemoveStopWord(
                                commonService.Tokenization(
                                    commonService.NormalizeText(x.Content)))))
        .Distinct()
        .OrderBy(word => word)
        .ToList();
        }


        public string SemanticSearch(string quary)
        {
            vocabulary = BuildVocabulary();
            var tokens = Preprocess(quary);
            Dictionary<string, double> tf = commonService.CalculateTF(tokens);

            foreach(var item in dataList)
            {
                item.Tokens = Preprocess(item.Content);
                item.TF = commonService.CalculateTF(item.Tokens);
            }

            List<List<string>> allData = dataList
                                        .Select(x => x.Tokens)
                                        .ToList();
            Dictionary<string,double> idf = commonService.CalculateIDF(allData); 

            foreach (var item in dataList)
            {
                item.TFIDF = commonService.CalculateTFIDF(item.TF,idf); //TF-IDF document vector.
            }

            Dictionary<string, double> queryVector = commonService.CalculateTFIDF(tf, idf); 
       


            var results = new List<SearchResult>();
            double score;

            foreach (var item in dataList)
            {
                 score = commonService.CosineSimilarity(
                    queryVector,
                    item.TFIDF
                );

                results.Add(new SearchResult
                {
                    Id = item.Id,
                    Score = score
                });
            }

            var rankedResults = results
                                .OrderByDescending(x => x.Score)
                                .Take(3).ToList();
            var output = rankedResults.FirstOrDefault();
            var answer = dataList.Where(x => x.Id == output.Id).Select(x => x.Content).FirstOrDefault();

            foreach (var result in rankedResults)
            {
                Console.WriteLine(
                    $"Id: {result.Id}, Score: {result.Score:F4}");
            }

            return answer;


        }


        public List<string> Preprocess(string text)
        {
            string normalized = commonService.NormalizeText(text);

            List<string> tokens = commonService.Tokenization(normalized);

            List<string> filtered = commonService.RemoveStopWord(tokens);

            List<string> lemmas = commonService.Lemmatization(filtered);

            return lemmas;
        }

        public class SearchResult
        {
            public int Id { get; set; }
            public double Score { get; set; }
        }
    }
}
