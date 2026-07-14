// See https://aka.ms/new-console-template for more information

using AIChatbot;
using AIChatbot.Models;
using AIChatbot.Services;
using System.Numerics;
// Rule based chatbot
RuleBasedChatbot ruleBasedChatbot = new RuleBasedChatbot();
//ruleBasedChatbot.StartChat();

// Intent base chatbot
IntentBasedChatbot intentBasedChatbot = new IntentBasedChatbot();
//var res=intentBasedChatbot.GetResponse("Goodbye");
//Console.WriteLine(res);

KnowledgeService knowledgeService = new KnowledgeService();
//KnowledgeItem? item = knowledgeService.Search("Leave?");

//if (item != null)
//{
//    Console.WriteLine($"Question : {item.Question}");
//    Console.WriteLine($"Answer   : {item.Answer}");
//}
//else
//{
//    Console.WriteLine("No matching answer found.");
//}


//NLP_NaturalLanProcessService obj= new NLP_NaturalLanProcessService();
//Console.WriteLine(obj.NormalizeText("Hello,,,    world!!   How are   you? you are my woRld "));


//List<string> lemmas = new()
//{
//    "learn",
//    "c#",
//    "c#"
//};
//BoW_BagOfWordService obj = new BoW_BagOfWordService();
// var vector = obj.BagOfWords_FeatureExtraction(lemmas);
//Console.WriteLine(string.Join(", ", vector));

//TF_TermFrequencyService tfObj=new TF_TermFrequencyService();
//var tf = tfObj.CalculateTF();

//IDF_InverseDocumentFrequencyService itfObj = new IDF_InverseDocumentFrequencyService();
//var tf = itfObj.CalculateIDF();

CosineSimilarityService cosine=new CosineSimilarityService();
Console.WriteLine(cosine.GetCosineSimilarity());

Console.ReadKey();
