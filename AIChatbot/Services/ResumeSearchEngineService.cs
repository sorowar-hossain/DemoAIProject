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
    public class ResumeSearchEngineService
    {
        List<ResumeModel> resumes = new();
        List<string> vocabulary = new();
        CommonService commonService;
        public ResumeSearchEngineService()
        {
            commonService = new CommonService();
            resumes = LoadResumes();
        }

        /*
            1.Read all.txt files.
            2.Preprocess the text (normalize, tokenize, remove stop words, lemmatize).
            3.Calculate TF-IDF for each resume.
            4.Convert the user's search query into a TF-IDF vector.
            5.Compute cosine similarity.
            6.Rank resumes from highest to lowest similarity.

        */

        public List<ResumeModel> LoadResumes()
        {

            string folder = Path.Combine(AppContext.BaseDirectory, "Resumes");

            string[] files = Directory.GetFiles(folder, "*.txt");

            int id = 1;

            foreach (string file in files)
            {
                resumes.Add(new ResumeModel
                {
                    Id = id++,
                    FileName = Path.GetFileName(file),
                    Content = File.ReadAllText(file)
                });
            }

            return resumes;
        }

        public List<string> BuildVocabulary(List<ResumeModel> resumes)
        {
            return resumes
                .SelectMany(x => x.Tokens)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
        public List<string> GetResumes(string question)
        {
            List<ResumeModel> normalizedresumes = new();

            // Query
           
            question = commonService.ReplaceTechnologiesWord(question);
            var lemmas = Preprocess(question);
            Dictionary<string, double> tf = commonService.CalculateTF(lemmas);
            // Resumes list

            foreach (var resume in resumes)
            {
                string normalizedContent = commonService.ReplaceTechnologiesWord(resume.Content);
                resume.Tokens = Preprocess(normalizedContent);
                resume.TF = commonService.CalculateTF(resume.Tokens);

                normalizedresumes.Add(resume);
            }


            List<List<string>> allDocuments = normalizedresumes
                                              .Select(r => r.Tokens)
                                              .ToList();
            Dictionary<string, double> idf = commonService.CalculateIDF(allDocuments);

            foreach (var resume in normalizedresumes)
            {
                resume.TFIDF = commonService.CalculateTFIDF(resume.TF, idf);
            }

            Dictionary<string, double> queryTFIDF = commonService.CalculateTFIDF(tf, idf);

            vocabulary = BuildVocabulary(normalizedresumes);

            double[] queryVector = commonService.CreateVector(queryTFIDF, vocabulary);

            foreach (var resume in normalizedresumes)
            {
                double[] resumeVector = commonService.CreateVector(resume.TFIDF, vocabulary);

                resume.Score = commonService.CalculateCosineSimilarity(queryVector, resumeVector);
            }

            var result = normalizedresumes
                        .OrderByDescending(x => x.Score)
                        .ToList();

            foreach (var resume in result)
            {
                Console.WriteLine($"{resume.FileName} : {resume.Score:F4}");
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
