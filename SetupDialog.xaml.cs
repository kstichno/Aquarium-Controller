using System.Threading.Tasks;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Aquarium
{
    public partial class SetupDialog
    {
        private readonly SolidColorBrush defaultBackColor;

        public SetupDialog()
        {
            this.InitializeComponent();

            defaultBackColor = (SolidColorBrush)OutOneButton.Background;
            
            FeedSlider.Value = double.Parse(Values.LoadStringSettings("FeedDuration") ?? "0");
            OutOneButton.Content = Values.LoadStringSettings("OutOneButton") ?? "Out One";
            OutTwoButton.Content = Values.LoadStringSettings("OutTwoButton") ?? "Out Two";
            OutThreeButton.Content = Values.LoadStringSettings("OutThreeButton") ?? "Out Three";
            OutFourButton.Content = Values.LoadStringSettings("OutFourButton") ?? "Out Four";
            OutFiveButton.Content = Values.LoadStringSettings("OutFiveButton") ?? "Out Five";
            OutSixButton.Content = Values.LoadStringSettings("OutSixButton") ?? "Out Six";
            OutSevenButton.Content = Values.LoadStringSettings("OutSevenButton") ?? "Out Seven";
            OutEightButton.Content = Values.LoadStringSettings("OutEightButton") ?? "Out Eight";
            
            if (int.Parse(Values.LoadStringSettings("OutputOneFeed") ?? "1") == 1)
            {
                OutOneButton.Background = new SolidColorBrush(Colors.DarkRed);
            }

            if (int.Parse(Values.LoadStringSettings("OutputTwoFeed") ?? "1") == 1)
            {
                OutTwoButton.Background = new SolidColorBrush(Colors.DarkRed);
            }

            if (int.Parse(Values.LoadStringSettings("OutputThreeFeed") ?? "1") == 1)
            {
                OutThreeButton.Background = new SolidColorBrush(Colors.DarkRed);
            }

            if (int.Parse(Values.LoadStringSettings("OutputFourFeed") ?? "1") == 1)
            {
                OutFourButton.Background = new SolidColorBrush(Colors.DarkRed);
            }

            if (int.Parse(Values.LoadStringSettings("OutputFiveFeed") ?? "0") == 0)
            {
                OutFiveButton.Background = new SolidColorBrush(Colors.DarkRed);
            }

            if (int.Parse(Values.LoadStringSettings("OutputSixFeed") ?? "0") == 0)
            {
                OutSixButton.Background = new SolidColorBrush(Colors.DarkRed);
            }

            if (int.Parse(Values.LoadStringSettings("OutputSevenFeed") ?? "0") == 0)
            {
                OutSevenButton.Background = new SolidColorBrush(Colors.DarkRed);
            }

            if (int.Parse(Values.LoadStringSettings("OutputEightFeed") ?? "0") == 0)
            {
                OutEightButton.Background = new SolidColorBrush(Colors.DarkRed);
            }
        }

        private void ContentDialog_PrimaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Values.FeedDuration = (int)FeedSlider.Value;
            Values.ProgramSettings.Values["FeedDuration"] = Values.FeedDuration.ToString();
            Values.ProgramSettings.Values["OutputOneFeed"] = OutOneButton.Background == defaultBackColor ? "0" : "1";
            Values.ProgramSettings.Values["OutputTwoFeed"] = OutTwoButton.Background == defaultBackColor ? "0" : "1";
            Values.ProgramSettings.Values["OutputThreeFeed"] = OutThreeButton.Background == defaultBackColor ? "0" : "1";
            Values.ProgramSettings.Values["OutputFourFeed"] = OutFourButton.Background == defaultBackColor ? "0" : "1";
            Values.ProgramSettings.Values["OutputFiveFeed"] = OutFiveButton.Background == defaultBackColor ? "1" : "0";
            Values.ProgramSettings.Values["OutputSixFeed"] = OutSixButton.Background == defaultBackColor ? "1" : "0";
            Values.ProgramSettings.Values["OutputSevenFeed"] = OutSevenButton.Background == defaultBackColor ? "1" : "0";
            Values.ProgramSettings.Values["OutputEightFeed"] = OutEightButton.Background == defaultBackColor ? "1" : "0";
        }

        private void ContentDialog_SecondaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //Window.Current.CoreWindow.IsInputEnabled = false;
            await Task.Delay(500);
            Button b = (Button)sender;
            b.Background = b.Background == defaultBackColor ? new SolidColorBrush(Colors.DarkRed) : defaultBackColor;   
        }
    }
}
