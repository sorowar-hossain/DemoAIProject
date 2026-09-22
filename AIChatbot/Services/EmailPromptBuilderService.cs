using AIChatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class EmailPromptBuilderService
    {
        public string Build(EmailRequest request)
        {
            return $"""
            Write an email using the information below.

            Recipient:
            {request.Recipient}

            Purpose:
            {request.Purpose}

            Reason:
            {request.Reason}

            Duration:
            {request.Duration}

            Tone:
            {request.Tone}

            Requirements:
            - Include a suitable subject.
            - Use the requested tone.
            - Keep the email concise.
            - Do not invent information.
            """;
        }
    }
}
