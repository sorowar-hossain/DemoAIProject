using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIChatbot.Services
{
    public class IDF_InverseDocumentFrequencyService
    {
        /*What is IDF?
            IDF measures how rare a word is across all documents.
                A word appearing in many documents gets a low IDF.
                A word appearing in few documents gets a high IDF.

            Document 1:
            AI is changing the world
            Document 2:
            AI is helping healthcare
            Document 3:
            AI is transforming education

        ai appears in every document.
        It appears everywhere, so it doesn't help distinguish one document from another.

        Real-World Example
            Suppose you're building a search engine for technical articles.

        Article 1:
        C# dependency injection service lifetime
        Article 2:
        SQL Server indexing performance tuning
        Article 3:
        C# async await performance

        The word performance appears in multiple articles, so it has a lower IDF. 
        The word indexing appears only in one article, so it has a higher IDF. 
        This means a search for "indexing" can more easily identify the SQL Server article 
        as the most relevant.
        */

        public List<List<string>> documents = new()
                                {
                                    new() { "ai","changing","world" },
                                    new() { "ai","helping","healthcare" },
                                    new() { "ai","transforming","education" }
                                };




        public Dictionary<string, int> CalculateDocumentFrequency()
        {
            Dictionary<string, int> df = new();

            foreach (var document in documents)
            {
                //Distinct,Because Document Frequency counts whether a word appears in a document, not how many times it appears.
                // And store it into Database for next use.
                // If a new document comes then just process it
                foreach (var word in document.Distinct())
                {
                    if (df.ContainsKey(word))
                        df[word]++;
                    else
                        df[word] = 1;
                }
            }

            return df;
        }

        public Dictionary<string, double> CalculateIDF()
        {
            int totalDocuments = documents.Count;

            var documentFrequency = CalculateDocumentFrequency();

            Dictionary<string, double> idf = new();

            foreach (var item in documentFrequency)
            {
                double score = Math.Log((double)totalDocuments / item.Value);

                idf[item.Key] = score;
            }
            foreach (var item in idf)
            {
                Console.WriteLine($"{item.Key} : {item.Value:F3}");
            }
            return idf;
        }
    }
}
