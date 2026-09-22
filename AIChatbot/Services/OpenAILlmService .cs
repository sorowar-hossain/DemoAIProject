using AIChatbot.IServices;
using OpenAI.Chat;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class OpenAILlmService : ILlmService
    {
        private readonly ChatClient client;
        public OpenAILlmService()
        {
            string? apiKey =
                Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            

            Console.WriteLine($"API Key Exists: {!string.IsNullOrWhiteSpace(apiKey)}");
            Console.WriteLine($"API Key Length: {apiKey?.Length}");
            Console.WriteLine($"API Key Start: {apiKey?[..Math.Min(7, apiKey.Length)]}");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "OPENAI_API_KEY is not set.");
            }

            Console.WriteLine("API key found.");

            client = new ChatClient(
                model: "gpt-5.1",
                apiKey: apiKey);
        }

        // here i have real OpenAI key they it is ok
        public async Task<string> GenerateAsync(string prompt)
        {
            ChatCompletion completion =
                await client.CompleteChatAsync(prompt);

            return completion.Content[0].Text;
        }
    }
}
