using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Diagnostics;
using System.Management;
using System.Windows.Controls;
using SmartSearchScreen;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Net.Http;

namespace SmartSearchScreen
{
    public partial class MainWindow : Window
    {
        private bool goLens = false;

        //로드할 이미지 페이지 수
        private const int ImagesPerPage = 9;
        private int currentPage = 1;
        private int totalPages;

        // 사용자의 문서 폴더 내에 이미지 저장 경로
        public readonly string imagesFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"Images");

        public static string search_image = "";
        // 일단 무시 static string bucketName = "sssearch";
        // 일단 무시 GoogleCloudStorageUploader uploader = new GoogleCloudStorageUploader(bucketName);
        ImageSearch ImageSearch { get; set; } = new ImageSearch();
        Translation translation { get; set; } = new Translation();
        private FullScnSrch_TopMost fullscnsrch;
        private ImageLoader imageLoader;

        private DispatcherTimer imageUpdateTimer;

        private CaptureSrch captureSrch;

        public MainWindow()
        {
            // XAML 파일을 초기화하는 함수
            InitializeComponent();
            CreateImagesFolder();   // Images 폴더를 생성하는 메서드 호출

            // ImageLoader 인스턴스 생성
            imageLoader = new ImageLoader(imagesFolderPath);
            totalPages = imageLoader.GetTotalPages(ImagesPerPage);
            UpdatePageNumber();
            LoadCurrentPageImages();

            // Window Loaded 이벤트에서 WindowManager 초기화
            this.Loaded += OnWindowLoaded;
            this.Closing += OnWindowClosing;

            // 타이머 설정: 3초마다 이미지 갱신
            imageUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)  // 3초마다 갱신
            };
            imageUpdateTimer.Tick += OnImageUpdateTimerTick;
            imageUpdateTimer.Start();  // 타이머 시작

            captureSrch = new CaptureSrch(this); // CaptureSrch 초기화
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                fullscnsrch = new FullScnSrch_TopMost(this);
                fullscnsrch.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize WindowManager: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Window Closed 이벤트 핸들러
        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // 창 닫힘을 취소
            this.Hide(); // 창 숨기기
        }

        public void realclose()
        {
            // 핫키 등록 해제
            fullscnsrch?.Dispose();
            captureSrch?.Dispose();
            Application.Current.Shutdown();
        }

        // 검색 버튼 클릭 이벤트 핸들러
        private void BtnHistory(object sender, RoutedEventArgs e)
        {
            myTabControl.SelectedIndex = 0;
            page.Visibility = Visibility.Visible;
        }

        private void BtnSearch(object sender, RoutedEventArgs e)
        {
            
            var region = OnFixUI.GetRegion();
            if (region.HasValue && OnFixUI.IsFixUIEnabled())
            {
                CaptureRegion(region.Value);
            }
            else
            {
                SearchImage();
            }
        }

        public void BtnTranslate(object sender, RoutedEventArgs e)
        {
            SearchImage();
            TranslateImage();
        }
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;

            // 이미지의 Opacity 값을 변경하여 클릭 여부를 표시합니다.
            if (image.Opacity == 1)
            {
                // 이미지가 아직 클릭되지 않은 상태라면 Opacity 값을 0.5로 변경합니다.
                image.Opacity = 0.5;
            }
            else
            {
                // 이미지가 이미 클릭된 상태라면 Opacity 값을 1로 변경합니다.
                image.Opacity = 1;
            }
        }
        // 이전 페이지, 다음 페이지 버튼 클릭 이벤트 핸들러
        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadCurrentPageImages();
                UpdatePageNumber();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadCurrentPageImages();
                UpdatePageNumber();
            }
        }

        // 현재 페이지의 이미지를 로드하는 메서드
        public void LoadCurrentPageImages()
        {
            imageLoader.LoadImagesByPage(ImageGrid, currentPage, ImagesPerPage);
        }

        private void UpdatePageNumber()
        {
            pageNum.Text = $" {currentPage}/{totalPages} ";
        }

        // 이미지 검색 메서드
        public async void SearchImage()
        {
            if (goLens)
            {
                BtnLens(null, null);
                return;
            }
            else
            {
                search_image = imageLoader.GetLastImage();
                myTabControl.SelectedIndex = 1;
                page.Visibility = Visibility.Hidden;

                // 파일 스트림을 사용하여 BitmapImage 생성
                using (var stream = new FileStream(search_image, FileMode.Open, FileAccess.Read))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze(); // BitmapImage를 고정하여 UI 스레드에서 안전하게 사용 가능

                    // 검색 이미지 설정
                    SearchedImage.Source = bitmap;
                }

                string resultText = await ImageSearch.AnalyzeImageAsync(search_image);
                SearchResults.Text = resultText;
            }
        }

        // 번역 메서드
        public async void TranslateImage()
        {
            try
            {
                // 마지막으로 선택된 이미지 경로 가져오기
                search_image = imageLoader.GetLastImage();

                // UI 업데이트 - 번역 진행 중 상태를 표시
                myTabControl.SelectedIndex = 1; // 번역 화면으로 전환
                page.Visibility = Visibility.Hidden;

                // 파일 스트림을 사용하여 BitmapImage 생성
                using (var stream = new FileStream(search_image, FileMode.Open, FileAccess.Read))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze(); // BitmapImage를 고정하여 UI 스레드에서 안전하게 사용 가능

                    // 이미지 표시
                    SearchedImage.Source = bitmap;
                }

                SearchResults.Text = "번역 진행 중";

                // 번역 대상 언어 설정 (예: 영어로 번역)
                string targetLanguage = "ko";

                // Translation 클래스의 텍스트 추출 및 번역 메서드 호출
                string translatedText = await translation.ExtractTextAndTranslateAsync(search_image, targetLanguage);

                // 번역 결과를 UI에 업데이트
                SearchResults.Text = translatedText;
            }
            catch (Exception ex)
            {
                // 예외 처리 - 오류 메시지 표시
                MessageBox.Show($"Error: {ex.Message}", "Translation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // 문서 디렉토리에 Images 폴더가 없으면 생성하는 메서드
        private void CreateImagesFolder()
        {
            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }
        }


        // 구글 렌즈 버튼 클릭 이벤트 핸들러
        private async void BtnLens(object sender, RoutedEventArgs e)
        {
            try
            {
                // 마지막으로 선택된 이미지 경로 가져오기
                search_image = imageLoader.GetLastImage();

                if (string.IsNullOrEmpty(search_image) || !File.Exists(search_image))
                {
                    MessageBox.Show("No image found to upload.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 이미지를 Imgur에 업로드하고 URL 가져오기
                string imageUrl = await UploadImageToImgur(search_image);

                // 구글 렌즈 URL 생성
                string googleLensUrl = "https://lens.google.com/uploadbyurl?url=" + Uri.EscapeDataString(imageUrl);

                // 웹 브라우저로 구글 렌즈 URL 열기
                Process.Start(new ProcessStartInfo
                {
                    FileName = googleLensUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Google Lens: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // 이미지를 Imgur에 업로드하는 메서드
        private async Task<string> UploadImageToImgur(string imagePath)
        {
            string clientId = "e83eb7aa4b3a6a1"; 
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Client-ID", clientId);
                using (var content = new MultipartFormDataContent())
                {
                    byte[] imageData = File.ReadAllBytes(imagePath);
                    content.Add(new ByteArrayContent(imageData), "image", Path.GetFileName(imagePath));
                    var response = await httpClient.PostAsync("https://api.imgur.com/3/image", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);
                        return result.data.link;
                    }
                    else
                    {
                        throw new Exception("Failed to upload image to Imgur.");
                    }
                }
            }
        }

        // 타이머가 틱할 때마다 호출되어 이미지를 갱신
        private void OnImageUpdateTimerTick(object sender, EventArgs e)
        {
            imageLoader.LoadAllImages();  // 모든 이미지를 다시 로드
            totalPages = imageLoader.GetTotalPages(ImagesPerPage);  // 총 페이지 수 갱신
            LoadCurrentPageImages();  // 현재 페이지의 이미지를 다시 로드
            UpdatePageNumber();  // 페이지 번호 갱신
        }

        public void Capture()
        {
            var overlay = new CaptureOverlayWindow();
            if (overlay.ShowDialog() == true)
            {
                var region = overlay.SelectedRegion;
                CaptureRegion(region);
                OnFixUI.HandleFixUI(this, region);
            }
        }

        private void CaptureRegion(Rect region)
        {
            // Get the DPI scale
            var dpiScale = GetDpiScale();

            // Adjust the coordinates and size based on the DPI scale
            int screenLeft = (int)(region.X * dpiScale);
            int screenTop = (int)(region.Y * dpiScale);
            int screenWidth = (int)(region.Width * dpiScale);
            int screenHeight = (int)(region.Height * dpiScale);

            using (var bitmap = new Bitmap(screenWidth, screenHeight))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(screenLeft, screenTop, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));
                }
                // 캡처 이미지 저장 후 검색
                ImageSave.SaveImage(bitmap, imagesFolderPath, "capture");
                fullscnsrch.loadAndSearch();
            }
        }

        private double GetDpiScale()
        {
            var source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                var dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                return dpiX / 96.0;
            }
            return 1.0;
        }

        private void GoLens(object sender, RoutedEventArgs e)
        {
            goLens = true;
        }
        private void nGoLens(object sender, RoutedEventArgs e)
        {
            goLens = false;
        }

        private void FixUI(object sender, RoutedEventArgs e)
        {
            OnFixUI.EnableFixUI(this);
        }

        private void nFixUI(object sender, RoutedEventArgs e)
        {
            OnFixUI.DisableFixUI(this);
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {
            // ImageLoader 인스턴스를 생성하고 이미지 그리드를 언로드합니다.
            ImageLoader imageLoader = new ImageLoader(imagesFolderPath);
            imageLoader.UnloadAllImages(ImageGrid);

            // 창을 닫습니다.
            this.Close();
        }

        private async void DeleteImages(object sender, RoutedEventArgs e)
        {
            try
            {
                // 타이머 일시 중지
                imageUpdateTimer.Stop();
                

                // 모든 파일이 갱신될 때까지 대기
                await Task.Run(() => imageLoader.LoadAllImages());

                global::SmartSearchScreen.ImageSearch imageSearch = new global::SmartSearchScreen.ImageSearch();
                var resultText = await imageSearch.AnalyzeImageAsync(imageLoader.GetLastImage());
                if (resultText.Contains("No image found"))
                {
                    MessageBox.Show("No image found. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                //모든 Image폴더 내 파일 해제
                imageLoader.UnloadAllImages(ImageGrid);


                
                var pngFiles = Directory.GetFiles(imagesFolderPath, "*.png");
                foreach (var file in pngFiles)
                {
                    try
                    {
                        // 파일 속성을 읽기 전용에서 일반으로 변경
                        File.SetAttributes(file, FileAttributes.Normal);
                        // 파일 삭제
                        File.Delete(file);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // 권한이 부족한 경우 권한 부여 후 삭제 시도
                        GrantFileAccess(file);
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                        // 파일이 사용 중인 경우
                        var processes = GetProcessesUsingFile(file);
                        if (processes.Any())
                        {
                            string processNames = string.Join(", ", processes.Select(p => p.ProcessName));
                            MessageBox.Show($"File is in use by the following processes: {processNames}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            // 사용 중인 프로세스가 없는 경우 파일 삭제 시도
                            try
                            {
                                File.Delete(file);
                            }
                            catch (IOException ex)
                            {
                                MessageBox.Show($"File is still in use and cannot be deleted: {file}. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                // 권한이 부족한 경우 권한 부여 후 삭제 시도
                                GrantFileAccess(file);
                                File.Delete(file);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Failed to delete file: {file}. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete file: {file}. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                MessageBox.Show("All PNG files have been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete PNG files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 타이머 다시 시작
                imageUpdateTimer.Start();
            }
        }

        private void GrantFileAccess(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var fileSecurity = fileInfo.GetAccessControl();
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            fileSecurity.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl, AccessControlType.Allow));
            fileInfo.SetAccessControl(fileSecurity);
        }

        private List<Process> GetProcessesUsingFile(string filePath)
        {
            var processes = new List<Process>();
            var fileHandle = CreateFile(filePath, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (fileHandle == IntPtr.Zero)
            {
                return processes;
            }

            try
            {
                foreach (var process in Process.GetProcesses())
                {
                    foreach (ProcessModule module in process.Modules)
                    {
                        if (module.FileName.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                        {
                            processes.Add(process);
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 무시
            }
            finally
            {
                CloseHandle(fileHandle);
            }

            return processes;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}