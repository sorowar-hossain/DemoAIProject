using AIChatbot.Models;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tokenizers.HuggingFace.Tokenizer;

namespace AIChatbot.Services
{
    public class SimilarDocumentFinderEbeddingService 
    {
        CommonService commonService;
        EmbeddingService embeddingService;
        List<EmbedingDataModel> dataList;
        private InferenceSession session;
        private Tokenizer tokenizer;
        string modelPath = @"Models\all-MiniLM-L6-v2\model.onnx";
        string tokenizerPath = @"Models\all-MiniLM-L6-v2\tokenizer.json";

        public async Task InitializeAsync()
        {
            commonService = new CommonService();
            embeddingService = new EmbeddingService();
            string path = Path.Combine(
                AppContext.BaseDirectory,
                "Data",
                "similar_document_embedding_dataset.json");

            string json = await File.ReadAllTextAsync(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            dataList = JsonSerializer.Deserialize<List<EmbedingDataModel>>(json, options);

            session = new InferenceSession(modelPath);
            tokenizer = Tokenizer.FromFile(tokenizerPath);
            await embeddingService.InitializeAsync();
            await CalculateEmbedding();
        }
        public async Task CalculateEmbedding()
        {
            foreach (var document in dataList!)
            {
                document.Embedding = embeddingService.GenerateEmbedding(document.Content);
                Console.WriteLine($"Id: {document.Id}");

                Console.WriteLine($"Title: {document.Title}");
                Console.WriteLine($"Title: {document.Content}");

                Console.WriteLine($"Vector Length: {document.Embedding?.Length}");

                Console.WriteLine();
            }
        }
        public List<SemanticSearchResult> SearchSimilarDocumentEbedding(int doucumentId) 
        {
            if (dataList == null || dataList.Count == 0)
                return new List<SemanticSearchResult>();
            var queryDocument = dataList.Where(x => x.Id == doucumentId).FirstOrDefault();
            if (queryDocument == null)
            {
                Console.WriteLine("Document not found.");
                return new List<SemanticSearchResult>();
            }

            Console.WriteLine("--------------- ");
            Console.WriteLine("Query: "+queryDocument.Content);

            // already calculated
            //var queryVector = embeddingService.GenerateEmbedding(queryDocument.Content);

            var queryVector = queryDocument.Embedding;
            var results = dataList
                .Where(x => x.Embedding != null && x.Id != doucumentId)
                .Select(x => new SemanticSearchResult
                {
                    Document = x,
                    Score = commonService.CalculateCosineSimilarity(
                            queryVector,
                            x.Embedding!)
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            foreach (var item in results.Take(2))
            {
                Console.WriteLine("Answer\n" + item.Document.Content);
            }

            return results;
        }
    }
}

