using AIChatbot.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tokenizers.HuggingFace.Tokenizer;

namespace AIChatbot.Services
{
    public class EmbeddingService : IDisposable
    {
        /*
         Let's build EmbeddingService step by step 
        using all-MiniLM-L6-v2 + ONNX Runtime + Hugging Face tokenizer.
        The current Tokenizers.HuggingFace package supports loading a tokenizer.json, 
        and its NuGet documentation specifically includes an all-MiniLM-L6-v2 sentence-similarity example. The official model repository contains the ONNX model and tokenizer files.
         
        Install the packages

        dotnet add package Microsoft.ML.OnnxRuntime
        dotnet add package Tokenizers.HuggingFace
         */
        CommonService commonService;
        List<EmbedingDataModel> dataList;
        private  InferenceSession session;
        private  Tokenizer tokenizer;
        string modelPath =@"Models\all-MiniLM-L6-v2\model.onnx";
        string tokenizerPath = @"Models\all-MiniLM-L6-v2\tokenizer.json";

        public async Task InitializeAsync()
        {
            commonService = new CommonService();
            string path = Path.Combine(
                AppContext.BaseDirectory,
                "Data",
                "semantic_embeding_dataset.json");

            string json = await File.ReadAllTextAsync(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            dataList = JsonSerializer.Deserialize<List<EmbedingDataModel>>(json, options);

            session = new InferenceSession(modelPath);
            tokenizer = Tokenizer.FromFile(tokenizerPath);

            await CalculateEmbedding();
        }
        public async Task CalculateEmbedding()
        {
            foreach (var document in dataList!)
            {
                document.Embedding = GenerateEmbedding(document.Content);
                //Console.WriteLine($"Id: {document.Id}");

                //Console.WriteLine($"Title: {document.Title}");
                //Console.WriteLine($"Title: {document.Content}");

                //Console.WriteLine($"Vector Length: {document.Embedding?.Length}");

                //Console.WriteLine();
            }
        }
        public List<SemanticSearchResult> SearchEbedding(string query, int topK = 5) 
        {
            if (dataList == null || dataList.Count == 0)
                return new List<SemanticSearchResult>();

            var queryVector = GenerateEmbedding(query);

            var results = dataList
                .Where(x => x.Embedding != null)
                .Select(x => new SemanticSearchResult
                {
                    Document = x,
                    Score  =commonService. CalculateCosineSimilarity(
                            queryVector,
                            x.Embedding!)
                })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToList();

            foreach(var item in results.Take(1))
            {
                Console.WriteLine("Answer\n"+item.Document.Content);
            }

            return results;
        }

       
        public double[] GenerateEmbedding(string text)
        {
            var encoding = tokenizer
                .Encode(text, true)
                .First();

            var inputIds = encoding.Ids
                .Select(x => (long)x)
                .ToArray();

            var attentionMask = encoding.AttentionMask.Count > 0
                            ? encoding.AttentionMask
                                .Select(x => (long)x)
                                .ToArray()
                            : Enumerable
                                .Repeat(1L, inputIds.Length)
                                .ToArray();

            var tokenTypeIds =
                        Enumerable
                            .Repeat(0L, inputIds.Length)
                            .ToArray();

            int sequenceLength = inputIds.Length;

            var inputIdsTensor = new DenseTensor<long>(
                inputIds,
                new[] { 1, sequenceLength });

            var attentionMaskTensor = new DenseTensor<long>(
                attentionMask,
                new[] { 1, sequenceLength });

            var tokenTypeIdsTensor = new DenseTensor<long>(
                tokenTypeIds,
                new[] { 1, sequenceLength });

            var inputs = new List<NamedOnnxValue>
                            {
                                NamedOnnxValue.CreateFromTensor(
                                    "input_ids",
                                    inputIdsTensor),

                                NamedOnnxValue.CreateFromTensor(
                                    "attention_mask",
                                    attentionMaskTensor),

                                NamedOnnxValue.CreateFromTensor(
                                    "token_type_ids",
                                    tokenTypeIdsTensor)
                            };

            using var outputs = session.Run(inputs);

            var output = outputs
                        .First(x => x.Name == "last_hidden_state")
                        .AsTensor<float>();

            return MeanPooling(
                output,
                attentionMask);
        }

        private double[] MeanPooling(Tensor<float> tokenEmbeddings,long[] attentionMask)
        {
            int sequenceLength = tokenEmbeddings.Dimensions[1];
            int embeddingSize = tokenEmbeddings.Dimensions[2];

            var sentenceEmbedding =
                new double[embeddingSize];

            float totalTokens = 0;

            for (int token = 0; token < sequenceLength; token++)
            {
                if (attentionMask[token] == 0)
                    continue;

                totalTokens++;

                for (int dimension = 0;
                     dimension < embeddingSize;
                     dimension++)
                {
                    sentenceEmbedding[dimension] +=
                        tokenEmbeddings[0, token, dimension];
                }
            }

            if (totalTokens == 0)
                return sentenceEmbedding;

            for (int dimension = 0;
                 dimension < embeddingSize;
                 dimension++)
            {
                sentenceEmbedding[dimension] /=
                    totalTokens;
            }

            return Normalize(sentenceEmbedding);
        }


        private double[] Normalize(double[] vector)
        {
            double magnitude = 0;

            foreach (float value in vector)
            {
                magnitude += value * value;
            }

            magnitude = Math.Sqrt(magnitude);

            if (magnitude == 0)
                return vector;

            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] =
                    (float)(vector[i] / magnitude);
            }

            return vector;
        }

        public void Dispose()
        {
            session.Dispose();
            tokenizer.Dispose();
        }


        // Test model
        public void PrintModelInfo()
        {
            Console.WriteLine("========== INPUTS ==========");

            foreach (var input in session.InputMetadata)
            {
                Console.WriteLine(
                    $"Name: {input.Key}");

                Console.WriteLine(
                    $"Type: {input.Value.ElementType}");

                Console.WriteLine(
                    $"Dimensions: {string.Join(", ", input.Value.Dimensions)}");

                Console.WriteLine();
            }

            Console.WriteLine("========== OUTPUTS ==========");

            foreach (var output in session.OutputMetadata)
            {
                Console.WriteLine(
                    $"Name: {output.Key}");

                Console.WriteLine(
                    $"Type: {output.Value.ElementType}");

                Console.WriteLine(
                    $"Dimensions: {string.Join(", ", output.Value.Dimensions)}");

                Console.WriteLine();
            }
        }
    }
}
