using AIChatbot.IServices;
using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class EmailGeneratorService
    {
        private readonly ILlmService llmService;
        private readonly EmailPromptBuilderService promptBuilder;

        public EmailGeneratorService(ILlmService llmService, EmailPromptBuilderService promptBuilder)
        {
            this.llmService = llmService;
            this.promptBuilder = promptBuilder;
        }

        public async Task<string> GenerateAsync( EmailRequest request)
        {
            string prompt =
                promptBuilder.Build(request);

            return await llmService.GenerateAsync(prompt);
        }
    }
}
