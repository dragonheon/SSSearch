using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using SmartSearchScreen;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartSearchScreen
{
    public partial class MainWindow : Window
    {

        //로드할 이미지 페이지 수
        private const int ImagesPerPage = 9;
        private int currentPage = 1;
        private int totalPages;

        // 사용자의 문서 폴더 내에 이미지 저장 경로
        public readonly string imagesFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Images");


        public static string search_image = "";

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
            SearchImage();
        }

        private void BtnTranslate(object sender, RoutedEventArgs e)
        {
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
            search_image = imageLoader.GetLastImage();
            myTabControl.SelectedIndex = 1;
            page.Visibility = Visibility.Hidden;
            // 검색 이미지 설정
            SearchedImage.Source = new BitmapImage(new Uri(search_image));

            string resultText = await ImageSearch.AnalyzeImageAsync(search_image);

            SearchResults.Text = resultText;
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
                SearchedImage.Source = new BitmapImage(new Uri(search_image)); // 이미지 표시
                SearchResults.Text = "Translating image text...";

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
    }
}