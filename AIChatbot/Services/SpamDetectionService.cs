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
    public class SpamDetectionService
    {
        List<SpamData> spamData;
        List<string> vocabulary = new();
        CommonService commonService;
        public SpamDetectionService()
        {
            commonService = new CommonService();
            LoadData();
        }


        public void LoadData()
        {

            string path = Path.Combine(AppContext.BaseDirectory, "Data", "spam_dataset_1000.json");
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            spamData = JsonSerializer.Deserialize<List<SpamData>>(json, options);

        }

        public void BuildVocabulary(List<SpamData> data)
        {
            List<string> voc = new();

            foreach (var item in data)
            {
                string normalized = commonService.NormalizeText(item.Text);

                List<string> tokens = commonService.Tokenization(normalized);

                List<string> filtered = commonService.RemoveStopWord(tokens);

                List<string> lemmas = commonService.Lemmatization(filtered);

                voc.AddRange(lemmas);
            }

          

           vocabulary= voc
                         .GroupBy(x => x)
                         .Where(g => g.Count() > 1)
                         .Select(g => g.Key)
                         .ToList();
        }

        public string DtectionSpam(string question)
        {
            BuildVocabulary(spamData);
            foreach (var word in vocabulary.Take(10))
            {
                Console.WriteLine(word);
            }

            List<string> lemmas = Preprocess(question);
            Dictionary<string, double> tf = commonService.CalculateTF(lemmas);

            // x => x.Text, is a lambda expression and call for each data of spamData
            Dictionary<string,double> idf = commonService.CalculateIDF_FromList(spamData,vocabulary, x => x.Text);
            Dictionary<string, double> tf_idf = commonService.CalculateTFIDF(tf,idf);
            return "On Processing";
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
