using Amazon;
using hello_translate;

RegionEndpoint region = RegionEndpoint.USWest2;

Console.OutputEncoding = System.Text.UnicodeEncoding.Unicode;

var translateHelper = new TranslateHelper(region);

if (args.Length == 3)
{
    await translateHelper.TranslateText(sourceLang: args[0], targetLang: args[1], text: args[2]);
}
else if (args.Length == 4)
{
    await translateHelper.TranslateBatch(sourceLang: args[0], targetLang: args[1], s3Uri: args[2], docType: args[3]);
}
else
{
    Console.WriteLine("For real-time translation of local text files:");
    Console.WriteLine("Usage: dotnet run -- [sourceLanguageCode] [destLanguageCode] [filename]");
    Console.WriteLine("Ex:    dotnet run -- en de \"Hello there. My name is David.\"");
    Console.WriteLine("Ex:    dotnet run -- fr ru data\\frenchText.txt");
    Console.WriteLine("For batch translation of S3 documents (using the bucket configured in the program):");
    Console.WriteLine("Usage: dotnet run -- [sourceLanguageCode] [destLanguageCode] [s3-uri] txt|html|docx|xlsx|pptx");
    Console.WriteLine("Ex:    dotnet run -- en fr s3://hello-translate docx");
}
