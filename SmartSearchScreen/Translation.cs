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
         //텍스트 추출과 번역을 분리한 메서드
         public async Task<string> ExtractTextFromImageAsync(string imagePath)
        {
            SetVisionApiCredentials(); // Cloud Vision 인증 설정

            var visionClient = ImageAnnotatorClient.Create();
            var image = Image.FromFile(imagePath); 
            var response = await visionClient.DetectTextAsync(image);

            string extractedText = string.Empty;
            foreach (var annotation in response)
            {
                extractedText += annotation.Description + "\n";
            }

            return extractedText;
        }

        // 번역을 수행하는 메서드
        public async Task<string> TranslateTextAsync(string text, string targetLanguage)
        {
            SetVisionApiCredentials(); // Cloud Translation 인증 설정

            var translationClient = TranslationClient.Create();
            var translatedText = await translationClient.TranslateTextAsync(text, targetLanguage);

            return translatedText.TranslatedText;
        }

        // 이미지에서 텍스트를 추출하고 번역하는 메서드
        public async Task<string> ExtractTextAndTranslateAsync(string imagePath, string targetLanguage)
        {
            // 텍스트 추출
            string extractedText = await ExtractTextFromImageAsync(imagePath);

            // 텍스트가 추출되지 않으면 종료
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                return "No text detected in the image.";
            }

            // 텍스트 번역
            string translatedText = await TranslateTextAsync(extractedText, targetLanguage);
            return translatedText;
        }
    }
}
