using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Aquarium
{
    public partial class ContentDialog
    {
        public ContentDialog(string label)
        {
            this.InitializeComponent();
            LabelTextBox.Text = label;
        }

        private void ContentDialog_PrimaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Values.ContentDialog = LabelTextBox.Text.ToString();
        }

        private void ContentDialog_SecondaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
