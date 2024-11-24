using System;
using System.Windows;
using System.Windows.Media;

namespace SmartSearchScreen
{
    public static class OnFixUI
    {
        private static bool fixUIEnabled = false;
        private static Window overlayWindow;
        private static Rect? currentRegion;

        public static void EnableFixUI(MainWindow mainWindow)
        {
            fixUIEnabled = true;
            mainWindow.Topmost = true;
        }

        public static void DisableFixUI(MainWindow mainWindow)
        {
            fixUIEnabled = false;
            mainWindow.Topmost = false;
            if (overlayWindow != null)
            {
                overlayWindow.Close();
                overlayWindow = null;
            }
        }

        public static bool IsFixUIEnabled()
        {
            return fixUIEnabled;
        }

        public static Rect? GetRegion()
        {
            return currentRegion;
        }

        public static void HandleFixUI(MainWindow mainWindow, Rect region)
        {
            currentRegion = region;
            if (fixUIEnabled)
            {
                ShowOverlayWindow(region);
            }
        }

        private static void ShowOverlayWindow(Rect region)
        {
            if (overlayWindow != null)
            {
                overlayWindow.Close();
            }

            overlayWindow = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.SkyBlue,
                BorderThickness = new Thickness(2),
                Topmost = true,
                ShowInTaskbar = false,
                Width = region.Width,
                Height = region.Height,
                Left = region.X,
                Top = region.Y
            };

            // 마우스 이벤트를 통과시키기 위해 WS_EX_TRANSPARENT 스타일을 추가
            var hwnd = new System.Windows.Interop.WindowInteropHelper(overlayWindow).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);

            overlayWindow.Show();
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}