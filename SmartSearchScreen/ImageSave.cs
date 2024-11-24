using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace SmartSearchScreen
{
    public static class ImageSave
    {
        public static void SaveImage(Bitmap image, string folderPath, string baseFileName)
        {
            try
            {

                if (image == null)
                {
                    throw new ArgumentNullException(nameof(image), "이미지 객체가 null입니다.");
                }

                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    throw new ArgumentException("폴더 경로가 유효하지 않습니다.", nameof(folderPath));
                }

                if (string.IsNullOrWhiteSpace(baseFileName))
                {
                    throw new ArgumentException("파일 이름이 유효하지 않습니다.", nameof(baseFileName));
                }

                string fileName = $"{baseFileName}.png";
                string filePath = Path.Combine(folderPath, fileName);

                // Ensure the directory exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

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
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("파일에 접근할 수 없습니다: " + ex.Message);
            }
            catch (IOException ex)
            {
                MessageBox.Show("파일이 이미 사용 중입니다: " + ex.Message);
            }
            catch (ExternalException ex)
            {
                MessageBox.Show("이미지를 저장하는 중 GDI+ 오류가 발생했습니다: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("이미지를 저장하는 중 오류가 발생했습니다: " + ex.Message);
            }
            finally
            {
                image.Dispose();
            }
        }
    }
}