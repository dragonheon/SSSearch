using System.Runtime.InteropServices;
using System;
using SmartSearchScreen;

namespace SmartSearchScreen
{
    public class FullScnSrch_TopMost
    {
        private const int HOTKEY_ID = 9000; // 핫키 ID
        private const int HOTKEY_ID_CLOSE = 9001; // 종료 핫키 ID
        private const int MOD_CONTROL = 0x0002; // Ctrl 키
        private const int MOD_SHIFT = 0x0004; // Shift 키
        private const int VK_F = 0x46; // F 키

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9; // 창 복원

        private readonly MainWindow _mainWindow;
        private IntPtr _windowHandle;
        private bool _isHotKeyRegistered;
        private bool _isCloseHotKeyRegistered;

        public FullScnSrch_TopMost(MainWindow mainWindow)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        }

        public void Initialize()
        {
            // MainWindow의 핸들 가져오기
            var windowInteropHelper = new System.Windows.Interop.WindowInteropHelper(_mainWindow);
            _windowHandle = windowInteropHelper.Handle;

            // 핫키 등록
            _isHotKeyRegistered = RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL, VK_F);
            _isCloseHotKeyRegistered = RegisterHotKey(_windowHandle, HOTKEY_ID_CLOSE, MOD_CONTROL | MOD_SHIFT, VK_F);
            if (!_isHotKeyRegistered || !_isCloseHotKeyRegistered)
            {
                throw new InvalidOperationException("핫키 등록 실패");
            }

            // 메시지 처리 등록
            var source = System.Windows.Interop.HwndSource.FromHwnd(_windowHandle);
            if (source != null)
            {
                source.AddHook(WndProc);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                if (hotkeyId == HOTKEY_ID)
                {
                    FullScnSrch.TakeScreenshot();

                    loadAndSearch();
                    handled = true;
                }
                else if (hotkeyId == HOTKEY_ID_CLOSE)
                {
                    _mainWindow.realclose();
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public void loadAndSearch()
        {
            ShowMainWindow();
            BringWindowToTop();
            _mainWindow.LoadCurrentPageImages();
            _mainWindow.SearchImage();
        }

        private void ShowMainWindow()
        {
            if (_mainWindow != null)
            {
                if (_mainWindow.WindowState == System.Windows.WindowState.Minimized)
                {
                    ShowWindow(_windowHandle, SW_RESTORE); // 창 복원
                }
                _mainWindow.Show(); // 창 다시 표시
            }
        }

        private void BringWindowToTop()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Topmost = true;  // 창을 맨 위로 설정
                _mainWindow.Activate();     // 포커스 활성화
                _mainWindow.Topmost = false; // 원래 상태로 복구
            }
        }

        public void Dispose()
        {
            if (_isHotKeyRegistered)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
            }
            if (_isCloseHotKeyRegistered)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID_CLOSE);
            }
        }
    }
}