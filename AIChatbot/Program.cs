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
BoW_BagOfWordService obj = new BoW_BagOfWordService();
// var vector = obj.BagOfWords_FeatureExtraction(lemmas);
//Console.WriteLine(string.Join(", ", vector));

TF_TermFrequencyService tfObj=new TF_TermFrequencyService();
//var tf = tfObj.CalculateTF();

IDF_InverseDocumentFrequencyService idfObj = new IDF_InverseDocumentFrequencyService();
//var tf = idfObj.CalculateIDF();

//CosineSimilarityService cosine=new CosineSimilarityService();

CommonService commonsrv = new CommonService();
/*
var normalized = commonsrv.NormalizeText("  Hello world World ! This is a Goat!@# and a boy is playing Football!, studies,studies");
Console.WriteLine(normalized);
Console.WriteLine("-------------------");
var tokens= commonsrv.Tokenization(normalized);
foreach(var token in tokens)
{
    Console.WriteLine(token);
}
Console.WriteLine("-------------------");
var rmvStopWord = commonsrv.RemoveStopWord(tokens);
foreach (var token in rmvStopWord)
{
    Console.WriteLine(token);
}

Console.WriteLine("-------------------");
var stems = commonsrv.Stemming(rmvStopWord);
foreach (var token in stems)
{
    Console.WriteLine(token);
}

Console.WriteLine("-------------------");
var lemms = commonsrv.Lemmatize(rmvStopWord);
foreach (var token in lemms)
{
    Console.WriteLine(token);
}

Console.WriteLine("-------------------");
var bow = obj.BagOfWords_FeatureExtraction(lemms);
foreach (var token in bow)
{
    Console.WriteLine($"{token}");
}

Console.WriteLine("-------------------");
var tf = tfObj.CalculateTF(lemms);

Console.WriteLine("-------------------");
var idf = idfObj.CalculateDocumentFrequency(lemms);
Console.WriteLine("-------------------");
idfObj.CalculateIDF();
Console.WriteLine("-------------------");
commonsrv.N_Gram(lemms);


*/

/*
    FAQSearchService faq=new FAQSearchService();
    string question = "subscription? cancel";
    Console.WriteLine(question);
    Console.WriteLine("-------------------");
    var res=faq.PreprocessFAQ(question);
    Console.WriteLine(res);
 */

/*
    ResumeSearchEngineService resumeSearchEngineService = new ResumeSearchEngineService();
    string question = "senior c# developer with sql and azure";
    Console.WriteLine(question);
    Console.WriteLine("-------------------");
    var response = resumeSearchEngineService.GetResumes(question);
 */
/*
    SimilarDocumentFinderService similarDocumentFinderService = new SimilarDocumentFinderService();
    string document = "PostgreSQL";
    Console.WriteLine(document);
    Console.WriteLine("-------------------");
    var response = similarDocumentFinderService.GetSimilarDocuments(document);
 */

/*
    SpamDetectionService spamDetectionService = new SpamDetectionService(); 
    string query = "You w1n A lotery? Click here..!";
    Console.WriteLine(query);
    Console.WriteLine("-------------------");
    var response = spamDetectionService.DtectionSpam(query);
 
 */

SentimentAnalysisService service = new SentimentAnalysisService();
string query = "I absolutely love this phone.";
Console.WriteLine(query);
Console.WriteLine("-------------------");
var response = service.SentimentAnalysis(query);
Console.WriteLine(response.Sentiment);


Console.ReadKey();


