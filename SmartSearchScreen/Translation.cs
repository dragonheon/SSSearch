using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;
using Google.Cloud.Translation.V2;
using System.Linq;

namespace SmartSearchScreen
{
    class Translation
    {
        public Translation()
        {
            string credential_path = @"C:\searchapp.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
        }

        public async Task<string> TranslateTextFromImageAsync(string imagePath, string targetLanguage)
        {
            if (imagePath == null)
            {
                return "No image found";
            }

            var visionClient = ImageAnnotatorClient.Create();
            var image = Image.FromFile(imagePath);
            var textResponse = await visionClient.DetectTextAsync(image);

            var annotation = textResponse.FirstOrDefault();
            if (annotation == null || annotation.Description == null)
            {
                return "No text found in image";
            }

            var translationClient = TranslationClient.Create();
            var translationResponse = await translationClient.TranslateTextAsync(annotation.Description, targetLanguage);

            return translationResponse.TranslatedText;
        }

    }
}
