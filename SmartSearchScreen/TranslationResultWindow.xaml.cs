using System.Windows;

namespace SmartSearchScreen
{
    public partial class TranslationResultWindow : Window
    {
        public TranslationResultWindow(string translatedText)
        {
            InitializeComponent();
            TranslationResultTextBlock.Text = translatedText;
            this.Topmost = true; // 창을 항상 맨 위에 고정
        }
    }
}