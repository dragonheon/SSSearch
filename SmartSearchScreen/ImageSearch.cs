using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;

namespace SmartSearchScreen
{

    class ImageSearch
    {
        public ImageSearch()
        {
            string credential_path = @"C:\searchapp.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
        }

        public async Task<string> AnalyzeImageAsync(string imagePath)
        {
            var client = ImageAnnotatorClient.Create();
            var image = Image.FromFile(imagePath);
            var response = await client.DetectLabelsAsync(image);

            List<string> labels = new List<string>();
            foreach (var label in response)
            {
                labels.Add(label.Description);
            }
            return string.Join(", ", labels);
        }
    }
}
