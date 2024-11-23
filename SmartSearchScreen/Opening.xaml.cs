using SmartSearchScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmartSearchScreen
{
    /// <summary>
    /// Opening.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Opening : Window
    {
        public Opening()
        {
            InitializeComponent();
            StartOpeningAnimation();
        }

        private void StartOpeningAnimation()
        {
            // 글자 페이드 인 및 위로 올라가는 애니메이션
            var fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1));
            var moveUpAnimation = new DoubleAnimation(50, 0, TimeSpan.FromSeconds(1));

            // 애니메이션 동기화
            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeInAnimation);
            storyboard.Children.Add(moveUpAnimation);

            Storyboard.SetTarget(fadeInAnimation, OpeningText);
            Storyboard.SetTarget(moveUpAnimation, OpeningText);

            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(TextBlock.OpacityProperty));
            Storyboard.SetTargetProperty(moveUpAnimation, new PropertyPath(Canvas.TopProperty));

            storyboard.Begin();

            // SSS 텍스트 사이 거리 확장 애니메이션
            var distanceChangeAnimation = new DoubleAnimation(0, 40, TimeSpan.FromSeconds(1));
            distanceChangeAnimation.AutoReverse = false;
            distanceChangeAnimation.Completed += (s, e) =>
            {
                // SSS에서 Smart Screen Search로 텍스트 변경
                SSSRun.Text = "Smart Screen Search";
            };
            SSSRun.BeginAnimation(Run.FontSizeProperty, distanceChangeAnimation);

            // 2초 후에 MainWindow로 전환
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2); // 애니메이션이 끝난 후 2초
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                ShowMainWindow();
            };
            timer.Start();
        }

        private void ShowMainWindow()
        {
            // OpeningPage가 끝난 후 MainWindow로 전환
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close(); // OpeningPage 닫기
        }
    }
}
