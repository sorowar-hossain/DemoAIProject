using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class SimilarDocumentFinderService
    {
        List<Document> documents = new();
        List<string> vocabulary = new();
        CommonService commonService;
        public SimilarDocumentFinderService()
        {
            commonService = new CommonService();
            documents = LoadDocuments();
        }

        public List<Document> LoadDocuments()
        {

            string folder = Path.Combine(AppContext.BaseDirectory, "Documents");

            string[] files = Directory.GetFiles(folder, "*.txt");

            int id = 1;

            foreach (string file in files)
            {
                documents.Add(new Document
                {
                    Id = id++,
                    Title = Path.GetFileName(file),
                    Content = File.ReadAllText(file),
                    //Tokens= Preprocess( File.ReadAllText(file))
                });
            }

            return documents;
        }

        public List<string> BuildVocabulary(List<Document> resumes)
        {
            return resumes
                .SelectMany(x => x.Tokens)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
        public List<string> GetSimilarDocuments(string document)
        {
            // Query
            // if input type is a document name
            //var queryDocument = documents.Where(x => x.Title.ToLower() == document.ToLower()).ToArray().FirstOrDefault();
            //var normalizedContent= commonService.ReplaceTechnologiesWord(queryDocument.Content);

            var normalizedContent = commonService.ReplaceTechnologiesWord(document);
            var lemmas = Preprocess(normalizedContent);

            Dictionary<string, double> tf = commonService.CalculateTF(lemmas);
            // Resumes list

            foreach (var resume in documents)
            {
                normalizedContent = commonService.ReplaceTechnologiesWord(resume.Content);
                resume.Tokens = Preprocess(normalizedContent);
                resume.TF = commonService.CalculateTF(resume.Tokens);
            }


            List<List<string>> allDocuments = documents
                                              .Select(r => r.Tokens)
                                              .ToList();
            Dictionary<string, double> idf = commonService.CalculateIDF(allDocuments);

            foreach (var resume in documents)
            {
                resume.TFIDF = commonService.CalculateTFIDF(resume.TF, idf);
            }

            Dictionary<string, double> queryTFIDF = commonService.CalculateTFIDF(tf, idf);

            vocabulary = BuildVocabulary(documents);

            double[] queryVector = commonService.CreateVector(queryTFIDF, vocabulary);

            foreach (var resume in documents)
            {
                double[] resumeVector = commonService.CreateVector(resume.TFIDF, vocabulary);

                resume.Score = commonService.CalculateCosineSimilarity(queryVector, resumeVector);
            }

            var result = documents
                        .OrderByDescending(x => x.Score)
                        .ToList();

            foreach (var resume in result)
            {
                Console.WriteLine($"{resume.Title} : {resume.Score:F4}");
            }
            return null;
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
