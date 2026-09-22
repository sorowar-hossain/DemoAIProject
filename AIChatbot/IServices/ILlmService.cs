using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.IServices
{
    public interface ILlmService
    {
        Task<string> GenerateAsync(string prompt);
    }
}
