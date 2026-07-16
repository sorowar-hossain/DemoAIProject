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
            var idf = commonService.CalculateIDF(allDocuments);

            var tf_idf = commonService.CalculateTFIDF(tf,idf); 

            var queryVectors=commonService.CreateVector(tf_idf,vocabulary);

            List<double[]> faqVectors = new();

            foreach (var faq in faqs)
            {
                var lemmas = commonService.Preprocess(faq.Question);

                var tf_Faq = commonService.CalculateTF(lemmas);   

                var tfidf = commonService.CalculateTFIDF(tf, tf_Faq);

                var vector = commonService.CreateVector(tfidf, vocabulary);

                faqVectors.Add(vector);
            }

            double bestScore = 0;
            FAQModel? bestFaq = null;

            for (int i = 0; i < faqs.Count; i++)
            {
                double score = commonService.CalculateCosineSimilarity(
                    queryVectors,          // query vector
                    faqVectors[i]);    // FAQ vector

                if (score > bestScore)
                { 
                    bestScore = score;
                    bestFaq = faqs[i];
                }
            }

            double threshold = 0.20;

            if (bestScore < threshold)
            {
                bestFaq = new FAQModel();
                bestFaq.Answer = "Sorry, I couldn't find a matching FAQ.";
            }


            return bestFaq.Answer;
        }
    }
}
