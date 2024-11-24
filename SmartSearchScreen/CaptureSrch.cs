using SmartSearchScreen;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace SmartSearchScreen
{
    public class CaptureSrch : IDisposable
    {
        private const int HOTKEY_ID_CAPTURE = 9002;
        private const int MOD_CONTROL = 0x0002;
        private const int VK_D = 0x44;
        private const int MaxKeyPressInterval = 500; // 최대 키 입력 간격 (밀리초)
        private const int RequiredKeyPressCount = 2; // 필요한 키 입력 횟수

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly MainWindow _mainWindow;
        private IntPtr _windowHandle;
        private HwndSource _source;
        private int _keyPressCount;
        private DispatcherTimer _keyPressTimer;

        public CaptureSrch(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _mainWindow.Loaded += OnWindowLoaded;
            _keyPressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(MaxKeyPressInterval)
            };
            _keyPressTimer.Tick += OnKeyPressTimerTick;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var windowInteropHelper = new WindowInteropHelper(_mainWindow);
            _windowHandle = windowInteropHelper.Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(WndProc);
            RegisterHotKey();
        }

        private void RegisterHotKey()
        {
            if (!RegisterHotKey(_windowHandle, HOTKEY_ID_CAPTURE, MOD_CONTROL, VK_D))
            {
                MessageBox.Show("Failed to register hotkey for Capture", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UnregisterHotKey()
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID_CAPTURE);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID_CAPTURE)
            {
                _keyPressCount++;
                if (_keyPressCount == 1)
                {
                    _keyPressTimer.Start();
                }
                else if (_keyPressCount >= RequiredKeyPressCount)
                {
                    _mainWindow.Capture();
                    _keyPressCount = 0;
                    _keyPressTimer.Stop();
                }
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void OnKeyPressTimerTick(object sender, EventArgs e)
        {
            _keyPressCount = 0;
            _keyPressTimer.Stop();
        }

        public void Dispose()
        {
            UnregisterHotKey();
            _source.RemoveHook(WndProc);
            _keyPressTimer.Tick -= OnKeyPressTimerTick;
        }
    }
}