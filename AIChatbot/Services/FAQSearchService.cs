using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class FAQSearchService
    {
        List<FAQModel> faqs;
        List<string> vocabulary = new();
        CommonService commonService;
        public FAQSearchService() 
        {
            commonService=new CommonService();
            string path = Path.Combine(AppContext.BaseDirectory, "Data", "faqs.json");
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            faqs = JsonSerializer.Deserialize<List<FAQModel>>(json, options);
        }

        public List<string> BuildVocabulary()
        {
            return faqs
                    .SelectMany(faq =>
                        commonService.Lemmatization(
                            commonService.RemoveStopWord(
                                commonService.Tokenization(
                                    commonService.NormalizeText(faq.Question)))))
        .Distinct()
        .OrderBy(word => word)
        .ToList();
        }
      

        public string PreprocessFAQ(string question)
        {
            string cleaned=commonService.NormalizeText(question);
            List<string> tokens =commonService.Tokenization(cleaned);
            List<string> list = commonService.RemoveStopWord(tokens);
            List<string> lemms = commonService.Lemmatization(list);
            vocabulary = BuildVocabulary();
            var tf = commonService.CalculateTF(lemms);

            List<List<string>> allDocuments = new();

            foreach (var faq in faqs)
            {
                List<string> lemmas = commonService.Preprocess(faq.Question);

                allDocuments.Add(lemmas);
            }


            return "Processing now";
        }
    }
}
