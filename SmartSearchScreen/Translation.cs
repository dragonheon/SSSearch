using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;
using Google.Cloud.Translation.V2;

namespace SmartSearchScreen
{
    public class Translation
    {
        //Cloud Vision 인증 파일 경로
        private string visionApiKeyFilePath = @"C:\searchapp.json";

        //Cloud Translation 인증 파일 경로
        private string translationApiKeyFilePath = @"C:\searchapp.json";

        //Cloud Vision API 인증을 설정하는 메서드
        private void SetVisionApiCredentials()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", visionApiKeyFilePath);
        }
        //Cloud Translation API 인증을 설정하는 메서드
        private void SetTranslationApiKey()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", translationApiKeyFilePath);
        }

        public Translation() 
        {
            SetTranslationApiKey();
            SetVisionApiCredentials();
        }
         //텍스트 추출과 번역을 분리한 메서드
         public async Task<string> ExtractTextFromImageAsync(string imagePath)
        {
            try
            {
                var visionClient = ImageAnnotatorClient.Create();
                var image = Image.FromFile(imagePath);
                var response = await visionClient.DetectTextAsync(image);
                int blockcount = response.Count();
                int i = 1;
                string extractedText = string.Empty;
                foreach (var annotation in response)
                {
                    if (i > ((blockcount) / 2 )- 1)
                    extractedText += annotation.Description + " ";
                    i++;
                }

                return extractedText;
            }
            catch (Exception ex)
            {
                return $"Error extracting text: {ex.Message}";
            }
        }

        // 번역을 수행하는 메서드
        public async Task<string> TranslateTextAsync(string text, string targetLanguage)
        {
            var translationClient = TranslationClient.Create();
            var translatedText1 = await translationClient.TranslateTextAsync(text, targetLanguage);

            return translatedText1.TranslatedText;
        }

        // 이미지에서 텍스트를 추출하고 번역하는 메서드
        public async Task<string> ExtractTextAndTranslateAsync(string imagePath, string targetLanguage)
        {
            try
            {
                string extractedText = await ExtractTextFromImageAsync(imagePath);

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    return "No text detected in the image.";
                }

                string translatedText = await TranslateTextAsync(extractedText, targetLanguage);
                return translatedText;
            }
            catch (Exception ex)
            {
                return $"Error processing image: {ex.Message}";
            }
        }
    }
}
