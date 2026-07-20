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
                    Content = File.ReadAllText(file)
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
            List<Document> normalizeddocuments = new();

            // Query

            
            var lemmas = Preprocess(document);
            Dictionary<string, double> tf = commonService.CalculateTF(lemmas);
            // Resumes list

            foreach (var resume in documents)
            {
                string normalizedContent = commonService.ReplaceTechnologiesWord(resume.Content);
                resume.Tokens = Preprocess(normalizedContent);
                resume.TF = commonService.CalculateTF(resume.Tokens);

                normalizeddocuments.Add(resume);
            }


            List<List<string>> allDocuments = normalizeddocuments
                                              .Select(r => r.Tokens)
                                              .ToList();
            Dictionary<string, double> idf = commonService.CalculateIDF(allDocuments);

            foreach (var resume in normalizeddocuments)
            {
                resume.TFIDF = commonService.CalculateTFIDF(resume.TF, idf);
            }

            Dictionary<string, double> queryTFIDF = commonService.CalculateTFIDF(tf, idf);

            vocabulary = BuildVocabulary(normalizeddocuments);

            double[] queryVector = commonService.CreateVector(queryTFIDF, vocabulary);

            foreach (var resume in normalizeddocuments)
            {
                double[] resumeVector = commonService.CreateVector(resume.TFIDF, vocabulary);

                resume.Score = commonService.CalculateCosineSimilarity(queryVector, resumeVector);
            }

            var result = normalizeddocuments
                        .OrderByDescending(x => x.Score)
                        .ToList();

            //foreach (var resume in result)
            //{
            //    Console.WriteLine($"{resume.FileName} : {resume.Score:F4}");
            //}
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
