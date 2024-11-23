using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SmartSearchScreen
{
    public class CaptureOverlayWindow : Window
    {
        private System.Windows.Point startPoint;
        private RectangleGeometry selectionRectangle;
        private System.Windows.Shapes.Rectangle selectionVisual;
        private Canvas canvas;

        public Rect SelectedRegion { get; private set; }

        public CaptureOverlayWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = new SolidColorBrush(Colors.Black) { Opacity = 0.6 };
            Topmost = true;
            WindowState = WindowState.Maximized;
            Cursor = Cursors.Cross;

            selectionRectangle = new RectangleGeometry();
            selectionVisual = new System.Windows.Shapes.Rectangle
            {
                Stroke = System.Windows.Media.Brushes.SkyBlue,
                StrokeThickness = 3,
                Visibility = Visibility.Collapsed
            };

            canvas = new Canvas();
            Content = canvas;
            canvas.Children.Add(selectionVisual);

            SizeChanged += OnSizeChanged;

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseMove += OnMouseMove;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvas.Width = ActualWidth;
            canvas.Height = ActualHeight;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);
            selectionRectangle.Rect = new Rect(startPoint, startPoint);

            selectionVisual.Visibility = Visibility.Visible;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var endPoint = e.GetPosition(this);

                double x = Math.Min(startPoint.X, endPoint.X);
                double y = Math.Min(startPoint.Y, endPoint.Y);
                double width = Math.Abs(startPoint.X - endPoint.X);
                double height = Math.Abs(startPoint.Y - endPoint.Y);

                selectionRectangle.Rect = new Rect(x, y, width, height);
                selectionVisual.Width = width;
                selectionVisual.Height = height;

                Canvas.SetLeft(selectionVisual, x);
                Canvas.SetTop(selectionVisual, y);
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            double x = Canvas.GetLeft(selectionVisual);
            double y = Canvas.GetTop(selectionVisual);
            double width = selectionVisual.Width;
            double height = selectionVisual.Height;

            SelectedRegion = new Rect(x, y, width, height);

            DialogResult = true;
            Close();
        }
    }
}