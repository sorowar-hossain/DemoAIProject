using AIChatbot.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class SqlLlmService : ILlmService
    {
        public Task<string> GenerateAsync(string prompt)
        {
            return Task.FromResult("""
            SELECT Id, Name, Department
            FROM Employees
            WHERE Department = 'IT';
            """);
        }
    }
}
