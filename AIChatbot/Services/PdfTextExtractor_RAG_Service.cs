using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace AIChatbot.Services
{
    public class PdfTextExtractor_RAG_Service
    {
        EmbeddingService embeddingService;
        private readonly List<DocumentChunk> chunks = new();

        public PdfTextExtractor_RAG_Service()
        {
            embeddingService = new EmbeddingService();
            embeddingService.InitializeAsync();
        }
        public async Task<List<PdfPage>> ExtractText()
        {

            string pdfPath = Path.Combine(
               AppContext.BaseDirectory,
               "Data",
               "RAG_Data.pdf");

            using var document = PdfDocument.Open(pdfPath);

            var pages = new List<PdfPage>(); 

            foreach (var page in document.GetPages())
            {
                pages.Add(new PdfPage
                {
                    PageNumber = page.Number,
                    Text = page.Text
                });
            }

            Console.WriteLine("===== PDF TEXT =====");
            foreach (var page in pages)
            {
                Console.WriteLine("Page No: " + page.PageNumber);
                Console.WriteLine(page.Text);
                Console.WriteLine();

            }


            return pages;
        }

        /*
         
            What does chunkSize = 500 mean?

            It means approximately:

            500 characters

            per chunk.

            For example:

            Page 1:

            ABCDEFGHIJKLMNOPQRSTUVWXYZ...

            We divide it:

            Chunk 1
            0 → 500

            Chunk 2
            400 → 900

            Chunk 3
            800 → 1300

            Why does Chunk 2 start at 400?

            Because:

            chunkSize = 500
            overlap = 100

            Therefore:

            500 - 100 = 400

            So every new chunk starts 400 characters after the previous start.

            3.6 Why overlap?

            Suppose we have:

            Chunk 1
            "...employees must submit their leave
            request at least three days before..."

            If we split exactly at 500 characters, an important sentence might be divided:

            Chunk 1:
            "Employees must submit their leave..."

            Chunk 2:
            "...request at least three days before..."

            That can hurt retrieval.

            With overlap:

            Chunk 1
                    ┌─────────────────┐
                    │ important text  │
                    └────────┬────────┘
                             │
            Chunk 2          │
                    ┌────────┴────────┐
                    │ same text + new │
                    └─────────────────┘

            Some content appears in both chunks.
         */
        public async Task<List<DocumentChunk>> CreateChunks(List<PdfPage> pages,int chunkSize = 500, int overlap = 100)
        {
          chunks.Clear();
            int chunkId = 1;

            foreach (var page in pages)
            {
                string text = page.Text.Trim();

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                int start = 0;

                while (start < text.Length)
                {
                    int length = Math.Min(
                        chunkSize,
                        text.Length - start);

                    string chunkText = text
                        .Substring(start, length)
                        .Trim();

                    if (!string.IsNullOrWhiteSpace(chunkText))
                    {
                        chunks.Add(new DocumentChunk
                        {
                            Id = chunkId++,
                            PageNumber = page.PageNumber,
                            Text = chunkText
                        });
                    }

                    start += chunkSize - overlap;
                }
            }

            Console.WriteLine($"Total Chunks: {chunks.Count}");

            foreach (var chunk in chunks)
            {
                Console.WriteLine();
                Console.WriteLine($"===== Chunk {chunk.Id} =====");
                Console.WriteLine($"Page: {chunk.PageNumber}");
                Console.WriteLine(chunk.Text);
            }

            return chunks;
        }

        public async Task GenerateEmbeddings(List<DocumentChunk> chunks)
        {
            foreach (var chunk in chunks)
            {
                chunk.Embedding =embeddingService.GenerateEmbedding(chunk.Text);

            }


            /*
                All Chunks
                    ↓
                Generate embedding for each chunk
                    ↓
                Chunk 1 → 384 dimensions
                Chunk 2 → 384 dimensions
                Chunk 3 → 384 dimensions
                ...
                    ↓
                AddRange(chunks)
                    ↓
                Vector Store
             */

            //  Create Vector Store
            // The VectorStore simply keeps these chunks in memory, That means saved in RAM:
           
        }

       
        public void AddRange(IEnumerable<DocumentChunk> documents)
        {
            chunks.AddRange(documents);
        }

        public IReadOnlyList<DocumentChunk> GetAll()
        {
            return chunks;
        }
        public async Task RAGChat( string userquestion) 
        {
         
            var pages = await ExtractText();
            var pdfChunks = await CreateChunks(pages);
            await GenerateEmbeddings(pdfChunks);



            var searchService =new PdfVectorSearchService(GetAll());

            Console.WriteLine("User Question: " + userquestion);
            var queryVector =embeddingService.GenerateEmbedding(userquestion);

            var results =
                searchService.Search(
                    queryVector,
                    topK: 3);

            Console.WriteLine();
            Console.WriteLine("===== SEARCH RESULTS =====");

            foreach (var result in results)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"Similarity: {result.Score:F4}");

                Console.WriteLine(
                    $"Page: {result.Chunk.PageNumber}");

                Console.WriteLine(
                    $"Chunk: {result.Chunk.Id}");

                Console.WriteLine(
                    result.Chunk.Text);
            }
        }
    }
}
