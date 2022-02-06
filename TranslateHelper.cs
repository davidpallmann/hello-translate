using Amazon;
using Amazon.S3;
using Amazon.Translate;
using Amazon.Translate.Model;

namespace hello_translate
{
    public class TranslateHelper
    {
        const string DataAccessRoleArn = "arn:aws:iam::[aws-account]:role/translate-s3";

        private RegionEndpoint _region { get; set; }
        private AmazonTranslateClient _translateClient { get; set; }
        private AmazonS3Client _s3Client { get; set; }

        public TranslateHelper(RegionEndpoint region)
        {
            _region = region;
            _translateClient = new AmazonTranslateClient(_region);
            _s3Client = new AmazonS3Client(_region);
        }

        /// <summary>
        /// Translate text.
        /// </summary>
        /// <param name="sourceLang">source language code</param>
        /// <param name="targetLang">target language code</param>
        /// <param name="text">text to translate</param>
        /// <returns>translated text</returns>
        public async Task<string> TranslateText(string sourceLang, string targetLang, string text)
        {
            if (text.Contains("\\"))
            {
                text = File.ReadAllText(text);
            }
            Console.WriteLine($"--- {sourceLang} ---");
            Console.WriteLine(text);

            var request = new TranslateTextRequest
            {
                Text = text,
                SourceLanguageCode = sourceLang,
                TargetLanguageCode = targetLang
            };

            var response = await _translateClient.TranslateTextAsync(request);

            Console.WriteLine($"--- {targetLang} ---");
            Console.WriteLine(response.TranslatedText);

            return response.TranslatedText;
        }

        /// <summary>
        /// Translate documents using a batch job. Input and output folders are named with the source/target language codes. 
        /// </summary>
        /// <param name="sourceLang">source language code</param>
        /// <param name="targetLang">target language code</param>
        /// <param name="bucketName">S3 URI, minus the folder, for source/target documents</param>
        /// <param name="docType">document type - docx | xlsx | pptx | html | txt</param>
        public async Task TranslateBatch(string sourceLang, string targetLang, string s3Uri, string docType)
        {
            if (s3Uri != null && !s3Uri.EndsWith("/")) s3Uri += "/";
            var inputConfig = new InputDataConfig()
            {
                S3Uri = $"{s3Uri}{sourceLang}", 
                ContentType = docType switch
                {
                    "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                    "htm" => "text/html",
                    "html" => "text/html",
                    "txt" => "text/plain",
                    _ => "text/plain"
                },
            };

            var outputConfig = new OutputDataConfig()
            {
                S3Uri = $"{s3Uri}{targetLang}",
            };

            var request = new StartTextTranslationJobRequest()
            {
                InputDataConfig = inputConfig,
                OutputDataConfig = outputConfig,
                SourceLanguageCode = sourceLang,
                TargetLanguageCodes = new List<string> { targetLang },
                DataAccessRoleArn = DataAccessRoleArn
            };

            Console.WriteLine($"Start batch translation - S3 input folder: {inputConfig.S3Uri}, S3 output folder: {outputConfig.S3Uri}");

            var response = await _translateClient.StartTextTranslationJobAsync(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Job {response.JobId} is running - status: {response.JobStatus}");
            }
            else
            {
                Console.WriteLine($"Error: {response.HttpStatusCode} {response.JobStatus}");
            }

        }
    }
}
