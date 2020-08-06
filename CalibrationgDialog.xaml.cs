using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Aquarium
{
    public partial class CalibrationgDialog
    {
        public CalibrationgDialog()
        {
            this.InitializeComponent();
            this.Title = Values.CalibrationDialog;
        }

        private void ContentDialog_PrimaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            switch (Values.CalibrationDialog)
            {
                case "Step 1 of 3: Calibrate Dry":
                    Values.CalibrationDelay = 600;
                    Values.CalibrationDialog = "Cal,dry";
                    break;
                case "Step 2 of 3: Calibrate in 12,880 uS solution":
                    Values.CalibrationDelay = 600;
                    Values.CalibrationDialog = "Cal,low,12880";
                    break;
                case "Step 3 of 3: Calibrate in 80,000 uS solution":
                    Values.CalibrationDelay = 600;
                    Values.CalibrationDialog = "Cal,high,80000"; 
                    break;
                case "Step 1 of 1: Calibrate in 225 mV solution":
                    Values.CalibrationDelay = 900;
                    Values.CalibrationDialog = "Cal,225";
                    break;
                case "Step 1 of 3: Calibrate in 7.00 solution":
                    Values.CalibrationDelay = 900;
                    Values.CalibrationDialog = "Cal,mid,7.00";
                    break;
                case "Step 2 of 3: Calibrate in 4.00 solution":
                    Values.CalibrationDelay = 900;
                    Values.CalibrationDialog = "Cal,low,4.00"; 
                    break;
                case "Step 3 of 3: Calibrate in 10.00 solution":
                    Values.CalibrationDelay = 900;
                    Values.CalibrationDialog = "Cal,high,10.00"; 
                    break;
                case "Step 1 of 2: Calibrate at atmosphere":
                    Values.CalibrationDelay = 1300;
                    Values.CalibrationDialog = "Cal";
                    break;
                case "Step 2 of 2: Calibrate in 0 DO solution":
                    Values.CalibrationDelay = 1300;
                    Values.CalibrationDialog = "Cal,0";
                    break;
                case "Step 1 of 1: Calibrate in boiling water":
                    Values.CalibrationDelay = 600;
                    Values.CalibrationDialog = "Cal,212.00";
                    break;
            }

        }

        private void ContentDialog_SecondaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
