using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Actions;

namespace AIChatbot.Services
{
    public class PdfTextExtractor_RAG_Service
    {
        EmbeddingService embeddingService;
        private readonly List<DocumentChunk> chunks = new();

        public PdfTextExtractor_RAG_Service()
        {
            embeddingService = new EmbeddingService();
            
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
        // =========================================================
        // 4. CREATE CHUNKS
        // =========================================================

        public Task<List<DocumentChunk>> CreateChunks(
            List<PdfPage> pages,
            int maxWords = 200,
            int overlapWords = 50)
        {
          

            int chunkId = 1;

            foreach (var page in pages)
            {
                if (string.IsNullOrWhiteSpace(page.Text))
                    continue;

                // Normalize PDF text
                string normalizedText =
                    NormalizePdfText(page.Text);

                if (string.IsNullOrWhiteSpace(normalizedText))
                    continue;

                // Detect sections
                var sections =
                    SplitIntoSections(normalizedText);

                foreach (var section in sections)
                {
                    var words = section.Text
                        .Split(
                            ' ',
                            StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    if (words.Count == 0)
                        continue;

                    int start = 0;

                    while (start < words.Count)
                    {
                        int count =
                            Math.Min(
                                maxWords,
                                words.Count - start);

                        var chunkWords =
                            words
                                .Skip(start)
                                .Take(count)
                                .ToList();

                        AddChunk(
                            chunks,
                            ref chunkId,
                            page.PageNumber,
                            section.Title,
                            chunkWords);

                        /*
                         * Stop if this was the final chunk.
                         */
                        if (start + count >= words.Count)
                            break;

                        /*
                         * Move forward while keeping overlap.
                         *
                         * Example:
                         *
                         * maxWords = 300
                         * overlap = 50
                         *
                         * Chunk 1: words 1 - 300
                         * Chunk 2: words 251 - 550
                         * Chunk 3: words 501 - 800
                         */

                        start +=
                            maxWords - overlapWords;
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Total Chunks : {chunks.Count}");

            return Task.FromResult(chunks);
        }

      

        private string NormalizePdfText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Normalize whitespace
            text = Regex.Replace(text, @"\s+", " ");

            // Add space after punctuation when PDF extraction
            // joins two sentences together.
            text = Regex.Replace(
                text,
                @"([.!?])([A-Z])",
                "$1 $2");

            return text.Trim();
        }

        private List<PdfPage> SplitIntoSections(string text)
        {
            var sections = new List<PdfPage>();

            if (string.IsNullOrWhiteSpace(text))
                return sections;

            // Find:
            // 1.
            // 2.
            // 3.
            // ...
            // 10.
            var matches = Regex.Matches(
                text,
                @"(?<!\d)\d+\.\s+");

            for (int i = 0; i < matches.Count; i++)
            {
                int start = matches[i].Index;

                int end = i + 1 < matches.Count
                    ? matches[i + 1].Index
                    : text.Length;

                string sectionText = text
                    .Substring(start, end - start)
                    .Trim();

                // Remove "1.", "2.", etc.
                var titleMatch = Regex.Match(
                    sectionText,
                    @"^\d+\.\s+(.+?)(?=Employees|Employee|Managers|The|Salary|Medical|New|Training|HR|$)");

                if (!titleMatch.Success)
                    continue;

                string title =
                    titleMatch.Groups[1].Value.Trim();

                string content =
                    sectionText
                        .Substring(titleMatch.Length)
                        .Trim();

                sections.Add(new PdfPage
                {
                    Title = title,
                    Text = content
                });
            }

            return sections;
        }

        // =========================================================
        // 5. ADD CHUNK
        // =========================================================

        private void AddChunk(
            List<DocumentChunk> chunks,
            ref int chunkId,
            int pageNumber,
            string sectionTitle,
            List<string> words)
        {
            if (words.Count == 0)
                return;

            string content =
                string.Join(" ", words);

            chunks.Add(new DocumentChunk
            {
                Id = chunkId++,
                PageNumber = pageNumber,
                SectionTitle = sectionTitle,
                Text = content
            });
        }
        public async Task GenerateEmbeddings(List<DocumentChunk> chunks)
        {
            Console.WriteLine("========== CHUNKS ==========");
            foreach (var chunk in chunks)
            {
                chunk.Embedding =embeddingService.GenerateEmbedding(chunk.Text);
                    Console.WriteLine();
                    Console.WriteLine($"Chunk ID     : {chunk.Id}");
                    Console.WriteLine($"Page         : {chunk.PageNumber}");
                    Console.WriteLine($"Section      : {chunk.SectionTitle}");
                    Console.WriteLine($"Text         : {chunk.Text}");
                    Console.WriteLine($"Word Count   : {chunk.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length}");

                
                Console.WriteLine("First 10 values:");

                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine(
                        $"[{i}] = {chunk.Embedding[i]}");
                }
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
           await embeddingService.InitializeAsync();
            var pages = await ExtractText();
            await CreateChunks(pages);
            await GenerateEmbeddings(chunks);



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
