using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class PdfVectorSearchService
    {

        private readonly IReadOnlyList<DocumentChunk> chunks;

        public PdfVectorSearchService( IReadOnlyList<DocumentChunk> chunks)
        {
            this.chunks = chunks;
        }

        public List<(DocumentChunk Chunk, double Score)> Search(double[] queryVector,int topK = 3)
        {
            return chunks
                .Where(x => x.Embedding.Length > 0)
                .Select(x => (
                    Chunk: x,
                    Score: CalculateCosineSimilarity(
                        queryVector,
                        x.Embedding)))
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToList();
        }

        private double CalculateCosineSimilarity(
            double[] vector1,
            double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException(
                    "Vectors must have the same length.");

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct +=
                    vector1[i] * vector2[i];

                magnitude1 +=
                    vector1[i] * vector1[i];

                magnitude2 +=
                    vector2[i] * vector2[i];
            }

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct /
                   (Math.Sqrt(magnitude1) *
                    Math.Sqrt(magnitude2));
        }
    }
}
