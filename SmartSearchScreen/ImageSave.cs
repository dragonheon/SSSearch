using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SmartSearchScreen
{
    public static class ImageSave
    {
        public static void SaveImage(Bitmap image, string folderPath, string baseFileName)
        {
            try
            {
                string fileName = $"{baseFileName}.png";
                string filePath = Path.Combine(folderPath, fileName);

                // 동일한 이름의 파일이 이미 존재한다면 숫자를 증가시켜서 저장
                int fileIndex = 1;
                while (File.Exists(filePath))
                {
                    string newFileName = Path.GetFileNameWithoutExtension(fileName) + $"({fileIndex})" + Path.GetExtension(fileName);
                    filePath = Path.Combine(folderPath, newFileName);
                    fileIndex++;
                }

                // 이미지 저장
                image.Save(filePath, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("이미지를 저장하는 중 오류가 발생했습니다: " + ex.Message);
            }
        }
    }
}