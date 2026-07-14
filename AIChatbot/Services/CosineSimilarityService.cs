using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class CosineSimilarityService
    {
        /*
         Definition

        Cosine Similarity measures how similar two documents (or sentences) are 
        by comparing the angle between their vectors, regardless of their length.

        Instead of comparing the number of words directly, it compares the direction of the vectors.

        Similarity ranges from:

        Value	                Meaning
        1	                    Exactly the same
        0.9	                    Very similar
        0.5	                    Somewhat similar
        0	                    Completely different
        -1	                    Opposite direction (rare in NLP)

        In NLP, the similarity is usually between 0 and 1 because TF-IDF values are non-negative.

        Why Do We Need Cosine Similarity?

        Imagine these two documents:

        Document A

        I love cats

        Document B

        I love cats very much

        They have different lengths.

        If you compare only word counts, they appear different.

        Cosine Similarity ignores document length and measures how similar their meanings are 
        based on word distribution.
         
         */


        //Suppose your TF-IDF service produces these vectors.

        public Dictionary<string, double> doc1 = new Dictionary<string, double>()
                                            {
                                                { "office", 0.58 },
                                                { "start", 0.58 },
                                                { "9am", 0.58 }
                                            };
        public Dictionary<string, double> doc2 = new Dictionary<string, double>()
                                            {
                                                { "office", 0.45 },
                                                { "start", 0.45 },
                                                { "10am", 0.77 }
                                            };
        public double GetCosineSimilarity()
        {
            // This double value means the two documents  similarity.

            return CalculateSimilarity(doc1, doc2);
        }
        public double CalculateSimilarity(Dictionary<string, double> vector1, Dictionary<string, double> vector2)
        {
            var allWords = vector1.Keys.Union(vector2.Keys);

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            foreach (var word in allWords)
            {
                double value1 = vector1.ContainsKey(word)
                    ? vector1[word]
                    : 0;

                double value2 = vector2.ContainsKey(word)
                    ? vector2[word]
                    : 0;

                dotProduct += value1 * value2;

                magnitude1 += value1 * value1;
                magnitude2 += value2 * value2;
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }


    }
}
