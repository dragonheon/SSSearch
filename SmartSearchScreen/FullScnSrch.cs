using SmartSearchScreen;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SmartSearchScreen
{
    internal class FullScnSrch
    {
        public static void TakeScreenshot()
        {
            MainWindow mw = new MainWindow();
            try
            {
                // 전체 화면 캡처
                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                using (Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(screenshot))
                    {
                        g.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size);
                    }
                    // 이미지 저장
                    ImageSave.SaveImage(screenshot, mw.imagesFolderPath, "screenshot");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("스크린샷을 저장하는 중 오류가 발생했습니다: " + ex.Message);
            }
        }
    }
}