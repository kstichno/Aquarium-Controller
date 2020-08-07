using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.Devices;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Devices.Pwm;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using LiveCharts;
using LiveCharts.Uwp;
using Microsoft.IoT.Lightning.Providers;

namespace Aquarium
{
    public sealed partial class MainPage : Page
    {
        CancellationTokenSource cancellationTokenSource;
        CancellationTokenSource cancellationTokenSourceCharts;
        CancellationTokenSource cancellationTokenSourceInputs;
        CancellationTokenSource cancellationTokenSourceFeed;
        CancellationTokenSource cancellationTokenSourceAuto;

        private const int OUTPUT_ONE = 23;
        private const int OUTPUT_TWO = 24;
        private const int OUTPUT_THREE = 25;
        private const int OUTPUT_FOUR = 16;
        private const int OUTPUT_FIVE = 26;
        private const int OUTPUT_SIX = 6;
        private const int OUTPUT_SEVEN = 5;
        private const int OUTPUT_EIGHT = 22;
        private const int INPUT_ONE = 17;
        private const int INPUT_TWO = 27;
        private const int PWM_ONE = 12;
        private const int PWM_TWO = 13;
        private const int PWM_THREE = 18;
        private const int PWM_FOUR = 19;
        private const int PWM_FIVE = 20;
        private const int TEMPSENSORADDRESS = 0x66;
        private const int SALINITYSENSORADDRESS = 0x64;
        private const int PHSENSORADDRESS = 0x63;
        private const int DOSENSORADDRESS = 0x61;
        private const int ORPSENSORADDRESS = 0x62;

        private GpioPin pinOne;
        private GpioPin pinTwo;
        private GpioPin pinThree;
        private GpioPin pinFour;
        private GpioPin pinFive;
        private GpioPin pinSix;
        private GpioPin pinSeven;
        private GpioPin pinEight;
        private GpioPin pinInOne;
        private GpioPin pinInTwo;
        private PwmPin pwmOne;
        private PwmPin pwmTwo;
        private PwmPin pwmThree;
        private PwmPin pwmFour;
        private PwmPin pwmFive;

        private I2cDevice i2cTempSensor;
        private I2cDevice i2cSalinitySensor;
        private I2cDevice i2cPHSensor;
        private I2cDevice i2cDOSensor;
        private I2cDevice i2cORPSensor;

        private bool I2CUpdated = false;

        public static SeriesCollection TempSeries { get; set; }
        public static SeriesCollection PHSeries { get; set; }
        public static SeriesCollection DOSeries { get; set; }
        public static SeriesCollection SalinitySeries { get; set; }
        public static SeriesCollection ORPSeries { get; set; }
        public static SeriesCollection LightingSeries { get; set; }
        public static SeriesCollection ChartOneSeries { get; set; }
        public static SeriesCollection ChartTwoSeries { get; set; }

        private SolidColorBrush defaultBackColor;
        private SolidColorBrush chartOneColor;
        private SolidColorBrush chartTwoColor;

        private readonly Output outputOne = new Output();
        private readonly Output outputTwo = new Output();
        private readonly Output outputThree = new Output();
        private readonly Output outputFour = new Output();
        private readonly Output outputFive = new Output();
        private readonly Output outputSix = new Output();
        private readonly Output outputSeven = new Output();
        private readonly Output outputEight = new Output();

        public MainPage()
        {
            InitializeComponent();
            
            defaultBackColor = (SolidColorBrush)SalinityTextButton.Background;

            cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSourceCharts = new CancellationTokenSource();
            cancellationTokenSourceInputs = new CancellationTokenSource();
            cancellationTokenSourceFeed = new CancellationTokenSource();
            cancellationTokenSourceAuto = new CancellationTokenSource();

            Values.ProgramSettings = ApplicationData.Current.LocalSettings;
            Values.CalibrationMode = (int)CalibrationModes.None;
            Values.ChartIndex = 1;
            Values.LightingModeAuto = true;
            Values.Feeding = false;

            LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            SetupPorts();
            
            InitializeI2CSensors();

            OutputOneCombo.SelectionChanged += OutputCombo_SelectionChanged;
            OutputTwoCombo.SelectionChanged += OutputCombo_SelectionChanged;
            OutputThreeCombo.SelectionChanged += OutputCombo_SelectionChanged;
            OutputFourCombo.SelectionChanged += OutputCombo_SelectionChanged;
            OutputFiveCombo.SelectionChanged += OutputCombo_SelectionChanged;
            OutputSixCombo.SelectionChanged += OutputCombo_SelectionChanged;
            OutputSevenCombo.SelectionChanged += OutputCombo_SelectionChanged;
            OutputEightCombo.SelectionChanged += OutputCombo_SelectionChanged;

            LoadProgramSettings();
            
            InitializeCharts();

            Task.Run(() => UpdateInputsAsync());
        }

        private async void InitializeI2CSensors()
        {

            // initialize I2C communications
            try
            {
                I2cController controller = (await I2cController.GetControllersAsync(LightningI2cProvider.GetI2cProvider()))[0];
                i2cTempSensor = controller.GetDevice(new I2cConnectionSettings(TEMPSENSORADDRESS));
                i2cSalinitySensor = controller.GetDevice(new I2cConnectionSettings(SALINITYSENSORADDRESS));
                i2cPHSensor = controller.GetDevice(new I2cConnectionSettings(PHSENSORADDRESS));
                i2cDOSensor = controller.GetDevice(new I2cConnectionSettings(DOSENSORADDRESS));
                i2cORPSensor = controller.GetDevice(new I2cConnectionSettings(ORPSENSORADDRESS));

                byte[] sendByteArr = Encoding.ASCII.GetBytes("S,f");
                i2cTempSensor.Write(sendByteArr);
                await Task.Delay(300);

                sendByteArr = Encoding.ASCII.GetBytes("K,1.0");
                i2cSalinitySensor.Write(sendByteArr);
                await Task.Delay(300);

                sendByteArr = Encoding.ASCII.GetBytes("O,EC,0");
                i2cSalinitySensor.Write(sendByteArr);
                await Task.Delay(300);

                sendByteArr = Encoding.ASCII.GetBytes("O,TDS,0");
                i2cSalinitySensor.Write(sendByteArr);
                await Task.Delay(300);

                sendByteArr = Encoding.ASCII.GetBytes("O,S,1");
                i2cSalinitySensor.Write(sendByteArr);
                await Task.Delay(300);

                sendByteArr = Encoding.ASCII.GetBytes("O,SG,0");
                i2cSalinitySensor.Write(sendByteArr);
                await Task.Delay(300);

                sendByteArr = Encoding.ASCII.GetBytes("O,mg,1");
                i2cDOSensor.Write(sendByteArr);
                await Task.Delay(300);

                sendByteArr = Encoding.ASCII.GetBytes("O,%,0");
                i2cDOSensor.Write(sendByteArr);
                await Task.Delay(300);

                await Task.WhenAll(UpdateI2CValuesAsync(), UpdateAutoOutputsAsync());
            }
            catch (Exception ex)
            {
                var messageDialog = new MessageDialog("Setup Error Occured", ex.Message);
                await messageDialog.ShowAsync();
            }
        }

        private async void InitializeCharts()
        {
            try
            {
                TempSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Temp Series",
                        Values = new ChartValues<double> {},
                    }
                };
                
                PHSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "pH Series",
                        Values = new ChartValues<double> {},
                    }
                };
                
                DOSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "DO Series",
                        Values = new ChartValues<double> {},
                    }
                };
                
                SalinitySeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Salinity Series",
                        Values = new ChartValues<double> {},
                    }
                };
                
                ORPSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "ORP Series",
                        Values = new ChartValues<double> {},
                    }
                };

                LightingSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Lighting Series",
                        Values = new ChartValues<double> {},
                    }
                };

                ChartOneSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Chart One Series",
                        Values = new ChartValues<double> {},
                        Stroke = new SolidColorBrush(Colors.Blue),
                        Fill = new SolidColorBrush(Colors.LightBlue),
                        PointGeometry = null
                    }
                };
                chartOneColor = new SolidColorBrush(Colors.Blue);

                ChartTwoSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Chart Two Series",
                        Values = new ChartValues<double> {},
                        Stroke = new SolidColorBrush(Colors.Magenta),
                        Fill = new SolidColorBrush(Colors.LightPink),
                        PointGeometry = null
                    }
                };
                chartTwoColor = new SolidColorBrush(Colors.Magenta);
                
                await UpdateUIControlsAsync();
            }
            catch (Exception ex)
            {
                var messageDialog = new MessageDialog("Setup Error Occured", ex.Message);
                await messageDialog.ShowAsync();
            }
        }

        private async void ChartButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            cancellationTokenSourceCharts.Cancel(); 
            
            Button b = (Button)sender;
            Values.ChartTwo = Values.ChartOne;
            Values.ProgramSettings.Values["ChartIndexTwo"] = Values.ChartTwo.ToString();

            Values.ChartOne = GetChart(b.Content.ToString());
            Values.ProgramSettings.Values["ChartIndexOne"] = Values.ChartOne.ToString();

            ResetColors();

            await Task.Delay(500);
            cancellationTokenSourceCharts = new CancellationTokenSource();

            await UpdateUIControlsAsync();
        }

        private void ResetColors()
        {
            PHTextButton.Background = defaultBackColor;
            ORPTextButton.Background = defaultBackColor;
            TempTextButton.Background = defaultBackColor;
            SalinityTextButton.Background = defaultBackColor;
            DOTextButton.Background = defaultBackColor;
            TempTextButton.Background = defaultBackColor;
            LightingTextButton.Background = defaultBackColor;
        }

        private int GetChart(string str)
        {
            int i = 0;
            switch (str)
            {
                case "pH":
                    i = (int)CalibrationModes.PH;
                    break;
                case "ORP":
                    i = (int)CalibrationModes.ORP;
                    break;
                case "Temp":
                    i = (int)CalibrationModes.Temperature;
                    break;
                case "Salinity":
                    i = (int)CalibrationModes.Salinity;
                    break;
                case "DO":
                    i = (int)CalibrationModes.DO;
                    break;
                case "Lighting":
                    i = (int)CalibrationModes.Lighting;
                    break;
            }
            return i;
        }

        private async void FeedButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Values.Feeding)
            {
                cancellationTokenSourceFeed.Cancel();
            }
            else
            {
                Values.LoadingSettings = true;
                cancellationTokenSourceFeed = new CancellationTokenSource();
                cancellationTokenSourceFeed.CancelAfter(Values.FeedDuration * 1000 * 60);
                outputOne.ReturnMode = outputOne.CurrentMode;
                outputTwo.ReturnMode = outputTwo.CurrentMode;
                outputThree.ReturnMode = outputThree.CurrentMode;
                outputFour.ReturnMode = outputFour.CurrentMode;
                outputFive.ReturnMode = outputFive.CurrentMode;
                outputSix.ReturnMode = outputSix.CurrentMode;
                outputSeven.ReturnMode = outputSeven.CurrentMode;
                outputEight.ReturnMode = outputEight.CurrentMode; 
                OutputOneCombo.SelectedIndex = outputOne.FeedMode == 1 ? 1 : outputOne.CurrentMode;
                OutputTwoCombo.SelectedIndex = outputTwo.FeedMode == 1 ? 1 : outputTwo.CurrentMode;
                OutputThreeCombo.SelectedIndex = outputThree.FeedMode == 1 ? 1 : outputThree.CurrentMode;
                OutputFourCombo.SelectedIndex = outputFour.FeedMode == 1 ? 1 : outputFour.CurrentMode;
                OutputFiveCombo.SelectedIndex = outputFive.FeedMode == 0 ? 0 : outputFive.CurrentMode;
                OutputSixCombo.SelectedIndex = outputSix.FeedMode == 0 ? 0 : outputSix.CurrentMode;
                OutputSevenCombo.SelectedIndex = outputSeven.FeedMode == 0 ? 0 : outputSeven.CurrentMode;
                OutputEightCombo.SelectedIndex = outputEight.FeedMode == 0 ? 0 : outputEight.CurrentMode;
                Values.LoadingSettings = false;
                await FeedAsync();
            }
            Values.LoadingSettings = true;
            OutputOneCombo.SelectedIndex = outputOne.ReturnMode;
            OutputTwoCombo.SelectedIndex = outputTwo.ReturnMode;
            OutputThreeCombo.SelectedIndex = outputThree.ReturnMode;
            OutputFourCombo.SelectedIndex = outputFour.ReturnMode;
            OutputFiveCombo.SelectedIndex = outputFive.ReturnMode;
            OutputSixCombo.SelectedIndex = outputSix.ReturnMode;
            OutputSevenCombo.SelectedIndex = outputSeven.ReturnMode;
            OutputEightCombo.SelectedIndex = outputEight.ReturnMode;
            Values.LoadingSettings = false;
            FeedButton.Background = defaultBackColor;
            FeedButton.Content = "Feed" + Environment.NewLine + Values.FeedDuration.ToString() + " min(s)";
        }

        public async Task FeedAsync()
        {
            int countDown = Values.FeedDuration, i = 0;
            FeedButton.Content = "Feed" + Environment.NewLine + countDown.ToString() + " min(s)" + Environment.NewLine + "Remaining";

            try
            {
                Values.Feeding = true;
                while (!cancellationTokenSourceCharts.IsCancellationRequested)
                {
                    FeedButton.Background = new SolidColorBrush(Colors.DarkGoldenrod);
                    await Task.Delay(1750, cancellationTokenSourceFeed.Token);
                    FeedButton.Background = defaultBackColor;
                    await Task.Delay(250, cancellationTokenSourceFeed.Token);
                    i++;
                    if (i == 30)
                    {
                        countDown--;
                        FeedButton.Content = "Feed" + Environment.NewLine + countDown.ToString() + " min(s)" + Environment.NewLine + "Remaining";
                        i = 0;
                    }
                }
                Values.Feeding = false;
            }
            catch
            {

            }
            finally
            {
                Values.Feeding = false;
            }
        }

        private async void OutButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Button b = (Button)sender;
            ContentDialog labelUpdate = new ContentDialog(b.Content.ToString());

            ContentDialogResult result = await labelUpdate.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                b.Content = Values.ContentDialog;
                Values.ProgramSettings.Values[b.Name.ToString()] = b.Content;
            }
        }

        private async void SetupButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SetupDialog setupDialog = new SetupDialog();

            ContentDialogResult result = await setupDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Values.FeedDuration = int.Parse(Values.LoadStringSettings("FeedDuration") ?? "0");
                outputTwo.FeedMode = int.Parse(Values.LoadStringSettings("OutputTwoFeed") ?? "0");
                outputThree.FeedMode = int.Parse(Values.LoadStringSettings("OutputThreeFeed") ?? "0");
                outputFour.FeedMode = int.Parse(Values.LoadStringSettings("OutputFourFeed") ?? "0");
                outputFive.FeedMode = int.Parse(Values.LoadStringSettings("OutputFiveFeed") ?? "0");
                outputSix.FeedMode = int.Parse(Values.LoadStringSettings("OutputSixFeed") ?? "0");
                outputSeven.FeedMode = int.Parse(Values.LoadStringSettings("OutputSevenFeed") ?? "0");
                outputEight.FeedMode = int.Parse(Values.LoadStringSettings("OutputEightFeed") ?? "0");
                outputOne.FeedMode = int.Parse(Values.LoadStringSettings("OutputOneFeed") ?? "0");
                FeedButton.Content = "Feed" + Environment.NewLine + Values.FeedDuration.ToString() + " min(s)";
            }
        }

        private async void CalibrationLightingButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            LightingDialog lightingDialog = new LightingDialog();

            ContentDialogResult result = await lightingDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Values.LightsOnTime = double.Parse(Values.LoadStringSettings("LightsOnTime") ?? "0");
                Values.LightsOffTime = double.Parse(Values.LoadStringSettings("LightsOffTime") ?? "0");
                Values.LightOutOneMax = double.Parse(Values.LoadStringSettings("LightOutOneMax") ?? "0");
                Values.LightOutTwoMax = double.Parse(Values.LoadStringSettings("LightOutTwoMax") ?? "0");
                Values.LightOutThreeMax = double.Parse(Values.LoadStringSettings("LightOutThreeMax") ?? "0");
                Values.LightOutFourMax = double.Parse(Values.LoadStringSettings("LightOutFourMax") ?? "0");
                Values.LightOutFiveMax = double.Parse(Values.LoadStringSettings("LightOutFiveMax") ?? "0");
                Values.LightOutNightLight = double.Parse(Values.LoadStringSettings("LightOutNightLight") ?? "0");
            }
            Values.LightingModeAuto = true;
        }

        private async void OutputCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox c = (ComboBox)sender;
                Values.ProgramSettings.Values[c.Name.ToString()] = c.SelectedIndex.ToString();
                switch (c.Name.ToString())
                {
                    case "OutputOneCombo":
                        WriteOutput(pinOne, c.SelectedIndex, 1);
                        outputOne.CurrentMode = c.SelectedIndex;
                        OutOneButton.Background = c.SelectedIndex == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        break;
                    case "OutputTwoCombo":
                        WriteOutput(pinTwo, c.SelectedIndex, 2);
                        outputTwo.CurrentMode = c.SelectedIndex;
                        OutTwoButton.Background = c.SelectedIndex == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        break;
                    case "OutputThreeCombo":
                        WriteOutput(pinThree, c.SelectedIndex, 3);
                        outputThree.CurrentMode = c.SelectedIndex;
                        OutThreeButton.Background = c.SelectedIndex == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor; 
                        break;
                    case "OutputFourCombo":
                        WriteOutput(pinFour, c.SelectedIndex, 4);
                        outputFour.CurrentMode = c.SelectedIndex;
                        OutFourButton.Background = c.SelectedIndex == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor; 
                        break;
                    case "OutputFiveCombo":
                        WriteOutput(pinFive, c.SelectedIndex, 5);
                        outputFive.CurrentMode = c.SelectedIndex;
                        OutFiveButton.Background = c.SelectedIndex == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        break;
                    case "OutputSixCombo":
                        WriteOutput(pinSix, c.SelectedIndex, 6);
                        outputSix.CurrentMode = c.SelectedIndex;
                        OutSixButton.Background = c.SelectedIndex == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        break;
                    case "OutputSevenCombo":
                        WriteOutput(pinSeven, c.SelectedIndex, 7);
                        outputSeven.CurrentMode = c.SelectedIndex;
                        OutSevenButton.Background = c.SelectedIndex == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        break;
                    case "OutputEightCombo":
                        WriteOutput(pinEight, c.SelectedIndex, 8);
                        outputEight.CurrentMode = c.SelectedIndex;
                        OutEightButton.Background = c.SelectedIndex == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        break;
                }
            }
            catch (Exception ex)
            {
                var messageDialog = new MessageDialog("Setup Error Occured", ex.Message);
                await messageDialog.ShowAsync();
            }
        }

        private async void SetupPorts()
        {
            try
            {
                var gpio =  (await GpioController.GetControllersAsync(LightningGpioProvider.GetGpioProvider()))[0];

                // Configure pins.
                pinOne = gpio.OpenPin(OUTPUT_ONE);
                pinOne.SetDriveMode(GpioPinDriveMode.Output);
                pinOne.Write(GpioPinValue.High);

                pinTwo = gpio.OpenPin(OUTPUT_TWO);
                pinTwo.SetDriveMode(GpioPinDriveMode.Output);
                pinTwo.Write(GpioPinValue.High);

                pinThree = gpio.OpenPin(OUTPUT_THREE);
                pinThree.SetDriveMode(GpioPinDriveMode.Output);
                pinThree.Write(GpioPinValue.High);

                pinFour = gpio.OpenPin(OUTPUT_FOUR);
                pinFour.SetDriveMode(GpioPinDriveMode.Output);
                pinFour.Write(GpioPinValue.High);

                pinFive = gpio.OpenPin(OUTPUT_FIVE);
                pinFive.SetDriveMode(GpioPinDriveMode.Output);
                pinFive.Write(GpioPinValue.High);

                pinSix = gpio.OpenPin(OUTPUT_SIX);
                pinSix.SetDriveMode(GpioPinDriveMode.Output);
                pinSix.Write(GpioPinValue.High);

                pinSeven = gpio.OpenPin(OUTPUT_SEVEN);
                pinSeven.SetDriveMode(GpioPinDriveMode.Output);
                pinSeven.Write(GpioPinValue.High);

                pinEight = gpio.OpenPin(OUTPUT_EIGHT);
                pinEight.SetDriveMode(GpioPinDriveMode.Output);
                pinEight.Write(GpioPinValue.High);

                pinInOne = gpio.OpenPin(INPUT_ONE);
                pinInOne.SetDriveMode(GpioPinDriveMode.InputPullUp);

                pinInTwo = gpio.OpenPin(INPUT_TWO);
                pinInTwo.SetDriveMode(GpioPinDriveMode.InputPullUp);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async void WriteOutput(GpioPin pin, int state, int outputNumber)
        {
            try
            {
                if (state == 0)
                {
                    pin.Write(GpioPinValue.High);
                }
                else if (state == 1)
                {
                    pin.Write(GpioPinValue.Low);
                }
                else if (state == 2)
                {
                    if (!Values.LoadingSettings)
                    {
                        AutoDialog autoDialog = new AutoDialog(outputNumber);
                        ContentDialogResult result = await autoDialog.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            switch (outputNumber)
                            {
                                case 1:
                                    outputOne.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode " + outputNumber.ToString()) ?? "0"); 
                                    outputOne.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode " + outputNumber.ToString()) ?? "0");
                                    outputOne.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn " + outputNumber.ToString()) ?? "0");
                                    outputOne.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut " + outputNumber.ToString()) ?? "0");
                                    outputOne.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn " + outputNumber.ToString()) ?? "0");
                                    outputOne.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff " + outputNumber.ToString()) ?? "0");
                                    break;
                                case 2:
                                    outputTwo.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode " + outputNumber.ToString()) ?? "0"); 
                                    outputTwo.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode " + outputNumber.ToString()) ?? "0");
                                    outputTwo.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn " + outputNumber.ToString()) ?? "0");
                                    outputTwo.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut " + outputNumber.ToString()) ?? "0");
                                    outputTwo.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn " + outputNumber.ToString()) ?? "0");
                                    outputTwo.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff " + outputNumber.ToString()) ?? "0");
                                    break;
                                case 3:
                                    outputThree.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode " + outputNumber.ToString()) ?? "0"); 
                                    outputThree.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode " + outputNumber.ToString()) ?? "0");
                                    outputThree.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn " + outputNumber.ToString()) ?? "0");
                                    outputThree.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut " + outputNumber.ToString()) ?? "0");
                                    outputThree.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn " + outputNumber.ToString()) ?? "0");
                                    outputThree.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff " + outputNumber.ToString()) ?? "0");
                                    break;
                                case 4:
                                    outputFour.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode " + outputNumber.ToString()) ?? "0"); 
                                    outputFour.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode " + outputNumber.ToString()) ?? "0");
                                    outputFour.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn " + outputNumber.ToString()) ?? "0");
                                    outputFour.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut " + outputNumber.ToString()) ?? "0");
                                    outputFour.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn " + outputNumber.ToString()) ?? "0");
                                    outputFour.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff " + outputNumber.ToString()) ?? "0");
                                    break;
                                case 5:
                                    outputFive.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode " + outputNumber.ToString()) ?? "0"); 
                                    outputFive.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode " + outputNumber.ToString()) ?? "0");
                                    outputFive.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn " + outputNumber.ToString()) ?? "0");
                                    outputFive.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut " + outputNumber.ToString()) ?? "0");
                                    outputFive.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn " + outputNumber.ToString()) ?? "0");
                                    outputFive.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff " + outputNumber.ToString()) ?? "0");
                                    break;
                                case 6:
                                    outputSix.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode " + outputNumber.ToString()) ?? "0"); 
                                    outputSix.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode " + outputNumber.ToString()) ?? "0");
                                    outputSix.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn " + outputNumber.ToString()) ?? "0");
                                    outputSix.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut " + outputNumber.ToString()) ?? "0");
                                    outputSix.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn " + outputNumber.ToString()) ?? "0");
                                    outputSix.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff " + outputNumber.ToString()) ?? "0");
                                    break;
                                case 7:
                                    outputSeven.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode " + outputNumber.ToString()) ?? "0"); 
                                    outputSeven.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode " + outputNumber.ToString()) ?? "0");
                                    outputSeven.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn " + outputNumber.ToString()) ?? "0");
                                    outputSeven.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut " + outputNumber.ToString()) ?? "0");
                                    outputSeven.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn " + outputNumber.ToString()) ?? "0");
                                    outputSeven.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff " + outputNumber.ToString()) ?? "0");
                                    break;
                                case 8:
                                    outputEight.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode " + outputNumber.ToString()) ?? "0"); 
                                    outputEight.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode " + outputNumber.ToString()) ?? "0");
                                    outputEight.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn " + outputNumber.ToString()) ?? "0");
                                    outputEight.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut " + outputNumber.ToString()) ?? "0");
                                    outputEight.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn " + outputNumber.ToString()) ?? "0");
                                    outputEight.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff " + outputNumber.ToString()) ?? "0");
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var messageDialog = new MessageDialog("Setup Error Occured", ex.Message);
                await messageDialog.ShowAsync();
            }
        }

        public async Task UpdateAutoOutputsAsync()
        {
            try
            {
                GpioPinValue pinValue;
                while (!cancellationTokenSourceAuto.IsCancellationRequested)
                {
                    int status = 1;
                    LightingButton.Content = (Values.Power * 100).ToString("0.00") + "%";
                    pinValue = pinInOne.Read();
                    if (pinValue == GpioPinValue.High)
                    {
                        DigitalInOneButton.Background = defaultBackColor;
                        Values.InputOne = (int)StatusModes.Off;
                    }
                    else
                    {
                        DigitalInOneButton.Background = new SolidColorBrush(Colors.DarkGreen);
                        Values.InputOne = (int)StatusModes.On;
                    }

                    pinValue = pinInTwo.Read();
                    if (pinValue == GpioPinValue.High)
                    {
                        DigitalInTwoButton.Background = defaultBackColor;
                        Values.InputTwo = (int)StatusModes.Off;
                    }
                    else
                    {
                        DigitalInTwoButton.Background = new SolidColorBrush(Colors.DarkGreen);
                        Values.InputTwo = (int)StatusModes.On;
                    }
                    if (outputOne.CurrentMode == 2)
                    {
                        if (outputOne.ControlMode == 0)
                        {
                            //sensors
                            switch (outputOne.AutoMode)
                            {
                                case (int)Sensors.InOne:
                                    WriteOutput(pinOne, Values.InputOne, 1);
                                    OutOneButton.Background = Values.InputOne == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.InTwo:
                                    WriteOutput(pinOne, Values.InputTwo, 1);
                                    OutOneButton.Background = Values.InputTwo == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.Temp:
                                    if (Values.TemperatueValue < (outputOne.AutoOnValue - 0.5))
                                    {
                                        status = 1;
                                        WriteOutput(pinOne, status, 1);
                                        OutOneButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    else if (Values.TemperatueValue > (outputOne.AutoOnValue + 0.5))
                                    {
                                        status = 0;
                                        WriteOutput(pinOne, status, 1);
                                        OutOneButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    break;
                                case (int)Sensors.Salinity:
                                    break;
                                case (int)Sensors.pH:
                                    break;
                                case (int)Sensors.ORP:
                                    break;
                                case (int)Sensors.DO:
                                    break;
                            }
                        }
                        else
                        {
                            //timer
                            Values.RelaySeconds = DateTime.Now.TimeOfDay.TotalSeconds;
                            if (Values.RelaySeconds > outputOne.AutoTimeOn && Values.RelaySeconds < outputOne.AutoTimeOff)
                            {
                                status = 1;
                            }
                            else
                            {
                                status = 0;
                            }
                            WriteOutput(pinOne, status, 1);
                            OutOneButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        }
                    }

                    if (outputTwo.CurrentMode == 2)
                    {
                        if (outputTwo.ControlMode == 0)
                        {
                            //sensors
                            switch (outputTwo.AutoMode)
                            {
                                case (int)Sensors.InOne:
                                    WriteOutput(pinTwo, Values.InputOne, 2);
                                    OutTwoButton.Background = Values.InputOne == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.InTwo:
                                    WriteOutput(pinTwo, Values.InputTwo, 2);
                                    OutTwoButton.Background = Values.InputTwo == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.Temp:
                                    if (Values.TemperatueValue < (outputTwo.AutoOnValue - 0.5))
                                    {
                                        status = 1;
                                        WriteOutput(pinTwo, status, 2);
                                        OutTwoButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    else if (Values.TemperatueValue > (outputTwo.AutoOnValue + 0.5))
                                    {
                                        status = 0;
                                        WriteOutput(pinTwo, status, 2);
                                        OutTwoButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    break;
                                case (int)Sensors.Salinity:
                                    break;
                                case (int)Sensors.pH:
                                    break;
                                case (int)Sensors.ORP:
                                    break;
                                case (int)Sensors.DO:
                                    break;
                            }
                        }
                        else
                        {
                            //timer
                            Values.RelaySeconds = DateTime.Now.TimeOfDay.TotalSeconds;
                            if (Values.RelaySeconds > outputTwo.AutoTimeOn && Values.RelaySeconds < outputTwo.AutoTimeOff)
                            {
                                status = 1;
                            }
                            else
                            {
                                status = 0;
                            }
                            WriteOutput(pinTwo, status, 2);
                            OutTwoButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        }
                    }

                    if (outputThree.CurrentMode == 2)
                    {
                        if (outputThree.ControlMode == 0)
                        {
                            //sensors
                            switch (outputThree.AutoMode)
                            {
                                case (int)Sensors.InOne:
                                    WriteOutput(pinThree, Values.InputOne, 3);
                                    OutThreeButton.Background = Values.InputOne == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.InTwo:
                                    WriteOutput(pinThree, Values.InputTwo, 3);
                                    OutThreeButton.Background = Values.InputTwo == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.Temp:
                                    if (Values.TemperatueValue < (outputThree.AutoOnValue - 0.5))
                                    {
                                        status = 1;
                                        WriteOutput(pinThree, status, 3);
                                        OutThreeButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    else if (Values.TemperatueValue > (outputThree.AutoOnValue + 0.5))
                                    {
                                        status = 0;
                                        WriteOutput(pinThree, status, 3);
                                        OutThreeButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    break;
                                case (int)Sensors.Salinity:
                                    break;
                                case (int)Sensors.pH:
                                    break;
                                case (int)Sensors.ORP:
                                    break;
                                case (int)Sensors.DO:
                                    break;
                            }
                        }
                        else
                        {
                            //timer
                            Values.RelaySeconds = DateTime.Now.TimeOfDay.TotalSeconds;
                            if (Values.RelaySeconds > outputThree.AutoTimeOn && Values.RelaySeconds < outputThree.AutoTimeOff)
                            {
                                status = 1;
                            }
                            else
                            {
                                status = 0;
                            }
                            WriteOutput(pinThree, status, 3);
                            OutThreeButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        }
                    }

                    if (outputFour.CurrentMode == 2)
                    {
                        if (outputFour.ControlMode == 0)
                        {
                            //sensors
                            switch (outputFour.AutoMode)
                            {
                                case (int)Sensors.InOne:
                                    WriteOutput(pinFour, Values.InputOne, 4);
                                    OutFourButton.Background = Values.InputOne == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.InTwo:
                                    WriteOutput(pinFour, Values.InputTwo, 4);
                                    OutFourButton.Background = Values.InputTwo == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.Temp:
                                    if (Values.TemperatueValue < (outputFour.AutoOnValue - 0.5))
                                    {
                                        status = 1;
                                        WriteOutput(pinFour, status, 4);
                                        OutFourButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    else if (Values.TemperatueValue > (outputFour.AutoOnValue + 0.5))
                                    {
                                        status = 0;
                                        WriteOutput(pinFour, status, 4);
                                        OutFourButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    break;
                                case (int)Sensors.Salinity:
                                    break;
                                case (int)Sensors.pH:
                                    break;
                                case (int)Sensors.ORP:
                                    break;
                                case (int)Sensors.DO:
                                    break;
                            }
                        }
                        else
                        {
                            //timer
                            Values.RelaySeconds = DateTime.Now.TimeOfDay.TotalSeconds;
                            if (Values.RelaySeconds > outputFour.AutoTimeOn && Values.RelaySeconds < outputFour.AutoTimeOff)
                            {
                                status = 1;
                            }
                            else
                            {
                                status = 0;
                            }
                            WriteOutput(pinFour, status, 4);
                            OutFourButton.Background = status == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        }
                    }

                    if (outputFive.CurrentMode == 2)
                    {
                        if (outputFive.ControlMode == 0)
                        {
                            //sensors
                            switch (outputFive.AutoMode)
                            {
                                case (int)Sensors.InOne:
                                    WriteOutput(pinFive, Values.InputOne, 5);
                                    OutFiveButton.Background = Values.InputOne == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.InTwo:
                                    WriteOutput(pinFive, Values.InputTwo, 5);
                                    OutFiveButton.Background = Values.InputTwo == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.Temp:
                                    if (Values.TemperatueValue < (outputFive.AutoOnValue - 0.5))
                                    {
                                        status = 1;
                                        WriteOutput(pinFive, status, 5);
                                        OutFiveButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    else if (Values.TemperatueValue > (outputFive.AutoOnValue + 0.5))
                                    {
                                        status = 0;
                                        WriteOutput(pinFive, status, 5);
                                        OutFiveButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    break;
                                case (int)Sensors.Salinity:
                                    break;
                                case (int)Sensors.pH:
                                    break;
                                case (int)Sensors.ORP:
                                    break;
                                case (int)Sensors.DO:
                                    break;
                            }
                        }
                        else
                        {
                            //timer
                            Values.RelaySeconds = DateTime.Now.TimeOfDay.TotalSeconds;
                            if (Values.RelaySeconds > outputFive.AutoTimeOn && Values.RelaySeconds < outputFive.AutoTimeOff)
                            {
                                status = 1;
                            }
                            else
                            {
                                status = 0;
                            }
                            WriteOutput(pinFive, status, 5);
                            OutFiveButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        }
                    }

                    if (outputSix.CurrentMode == 2)
                    {
                        if (outputSix.ControlMode == 0)
                        {
                            //sensors
                            switch (outputSix.AutoMode)
                            {
                                case (int)Sensors.InOne:
                                    WriteOutput(pinSix, Values.InputOne, 6);
                                    OutSixButton.Background = Values.InputOne == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.InTwo:
                                    WriteOutput(pinSix, Values.InputTwo, 6);
                                    OutSixButton.Background = Values.InputTwo == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.Temp:
                                    if (Values.TemperatueValue < (outputSix.AutoOnValue - 0.5))
                                    {
                                        status = 1;
                                        WriteOutput(pinSix, status, 6);
                                        OutSixButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    else if (Values.TemperatueValue > (outputSix.AutoOnValue + 0.5))
                                    {
                                        status = 0;
                                        WriteOutput(pinSix, status, 6);
                                        OutSixButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    break;
                                case (int)Sensors.Salinity:
                                    break;
                                case (int)Sensors.pH:
                                    break;
                                case (int)Sensors.ORP:
                                    break;
                                case (int)Sensors.DO:
                                    break;
                            }
                        }
                        else
                        {
                            //timer
                            Values.RelaySeconds = DateTime.Now.TimeOfDay.TotalSeconds;
                            if (Values.RelaySeconds > outputSix.AutoTimeOn && Values.RelaySeconds < outputSix.AutoTimeOff)
                            {
                                status = 1;
                            }
                            else
                            {
                                status = 0;
                            }
                            WriteOutput(pinSix, status, 6);
                            OutSixButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        }
                    }

                    if (outputSeven.CurrentMode == 2)
                    {
                        if (outputSeven.ControlMode == 0)
                        {
                            //sensors
                            switch (outputSeven.AutoMode)
                            {
                                case (int)Sensors.InOne:
                                    WriteOutput(pinSeven, Values.InputOne, 7);
                                    OutSevenButton.Background = Values.InputOne == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.InTwo:
                                    WriteOutput(pinSeven, Values.InputTwo, 7);
                                    OutSevenButton.Background = Values.InputTwo == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.Temp:
                                    if (Values.TemperatueValue < (outputSeven.AutoOnValue - 0.5))
                                    {
                                        status = 1;
                                        WriteOutput(pinSeven, status, 7);
                                        OutSevenButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    else if (Values.TemperatueValue > (outputSeven.AutoOnValue + 0.5))
                                    {
                                        status = 0;
                                        WriteOutput(pinSeven, status, 7);
                                        OutSevenButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    break;
                                case (int)Sensors.Salinity:
                                    break;
                                case (int)Sensors.pH:
                                    break;
                                case (int)Sensors.ORP:
                                    break;
                                case (int)Sensors.DO:
                                    break;
                            }
                        }
                        else
                        {
                            //timer
                            Values.RelaySeconds = DateTime.Now.TimeOfDay.TotalSeconds;
                            if (Values.RelaySeconds > outputSeven.AutoTimeOn && Values.RelaySeconds < outputSeven.AutoTimeOff)
                            {
                                status = 1;
                            }
                            else
                            {
                                status = 0;
                            }
                            WriteOutput(pinSeven, status, 7);
                            OutSevenButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        }
                    }

                    if (outputEight.CurrentMode == 2)
                    {
                        if (outputEight.ControlMode == 0)
                        {
                            //sensors
                            switch (outputEight.AutoMode)
                            {
                                case (int)Sensors.InOne:
                                    WriteOutput(pinEight, Values.InputOne, 8);
                                    OutEightButton.Background = Values.InputOne == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.InTwo:
                                    WriteOutput(pinEight, Values.InputTwo, 8);
                                    OutEightButton.Background = Values.InputTwo == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    break;
                                case (int)Sensors.Temp:
                                    if (Values.TemperatueValue < (outputEight.AutoOnValue - 0.5))
                                    {
                                        status = 1;
                                        WriteOutput(pinEight, status, 8);
                                        OutEightButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    else if (Values.TemperatueValue > (outputEight.AutoOnValue + 0.5))
                                    {
                                        status = 0;
                                        WriteOutput(pinEight, status, 8);
                                        OutEightButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                                    }
                                    break;
                                case (int)Sensors.Salinity:
                                    break;
                                case (int)Sensors.pH:
                                    break;
                                case (int)Sensors.ORP:
                                    break;
                                case (int)Sensors.DO:
                                    break;
                            }
                        }
                        else
                        {
                            //timer
                            Values.RelaySeconds = DateTime.Now.TimeOfDay.TotalSeconds;
                            if (Values.RelaySeconds > outputEight.AutoTimeOn && Values.RelaySeconds < outputEight.AutoTimeOff)
                            {
                                status = 1;
                            }
                            else
                            {
                                status = 0;
                            }
                            WriteOutput(pinEight, status, 8);
                            OutEightButton.Background = status == 1 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;
                        }
                    }

                    await Task.Delay(10000);
                }
            }
            catch (Exception ex)
            {
                var messageDialog = new MessageDialog("Setup Error Occured", ex.Message);
                await messageDialog.ShowAsync();
            }
        }

        public async Task UpdateUIControlsAsync()
        {
            try
            {   
                while (!cancellationTokenSourceCharts.IsCancellationRequested)
                {
                    if (TempSeries[0].Values.Count >= 144)
                    {
                        TempSeries[0].Values.RemoveAt(0);
                    }
                    if (PHSeries[0].Values.Count >= 144)
                    {
                        PHSeries[0].Values.RemoveAt(0);
                    }
                    if (DOSeries[0].Values.Count >= 144)
                    {
                        DOSeries[0].Values.RemoveAt(0);
                    }
                    if (SalinitySeries[0].Values.Count >= 144)
                    {
                        SalinitySeries[0].Values.RemoveAt(0);
                    }
                    if (ORPSeries[0].Values.Count >= 144)
                    {
                        ORPSeries[0].Values.RemoveAt(0);
                    }
                    if (LightingSeries[0].Values.Count >= 144)
                    {
                        LightingSeries[0].Values.RemoveAt(0);
                    }

                    if (I2CUpdated)
                    {
                        if (i2cTempSensor != null)
                        {
                            TempSeries[0].Values.Add(Values.TemperatueValue);
                        }

                        if (i2cPHSensor != null)
                        {
                            PHSeries[0].Values.Add(Values.PHValue);
                        }


                        if (i2cDOSensor != null)
                        {
                            DOSeries[0].Values.Add(Values.DOValue);
                        }

                        if (i2cSalinitySensor != null)
                        {
                            SalinitySeries[0].Values.Add(Values.SalinityValue);
                        }

                        if (i2cORPSensor != null)
                        {
                            ORPSeries[0].Values.Add(Values.ORPValue);
                        }

                        LightingSeries[0].Values.Add(Values.LightingValue);

                        switch (Values.ChartOne)
                        {
                            case (int)CalibrationModes.DO:
                                DOTextButton.Background = chartOneColor;
                                ChartOneSeries[0].Values = DOSeries[0].Values;
                                ChartOne.AxisY[0].Title = "DO";
                                break;
                            case (int)CalibrationModes.ORP:
                                ORPTextButton.Background = chartOneColor;
                                ChartOneSeries[0].Values = ORPSeries[0].Values;
                                ChartOne.AxisY[0].Title = "ORP"; 
                                break;
                            case (int)CalibrationModes.PH:
                                PHTextButton.Background = chartOneColor;
                                ChartOneSeries[0].Values = PHSeries[0].Values;
                                ChartOne.AxisY[0].Title = "pH"; 
                                break;
                            case (int)CalibrationModes.Salinity:
                                SalinityTextButton.Background = chartOneColor;
                                ChartOneSeries[0].Values = SalinitySeries[0].Values;
                                ChartOne.AxisY[0].Title = "Salinity"; 
                                break;
                            case (int)CalibrationModes.Temperature:
                                TempTextButton.Background = chartOneColor;
                                ChartOneSeries[0].Values = TempSeries[0].Values;
                                ChartOne.AxisY[0].Title = "Temp"; 
                                break;
                            case (int)CalibrationModes.Lighting:
                                LightingTextButton.Background = chartOneColor;
                                ChartOneSeries[0].Values = LightingSeries[0].Values;
                                ChartOne.AxisY[0].Title = "Light";
                                break;
                        }

                        switch (Values.ChartTwo)
                        {
                            case (int)CalibrationModes.DO:
                                DOTextButton.Background = chartTwoColor;
                                ChartTwoSeries[0].Values = DOSeries[0].Values;
                                ChartTwo.AxisY[0].Title = "DO";
                                break;
                            case (int)CalibrationModes.ORP:
                                ORPTextButton.Background = chartTwoColor;
                                ChartTwoSeries[0].Values = ORPSeries[0].Values;
                                ChartTwo.AxisY[0].Title = "ORP";
                                break;
                            case (int)CalibrationModes.PH:
                                PHTextButton.Background = chartTwoColor;
                                ChartTwoSeries[0].Values = PHSeries[0].Values;
                                ChartTwo.AxisY[0].Title = "pH";
                                break;
                            case (int)CalibrationModes.Salinity:
                                SalinityTextButton.Background = chartTwoColor;
                                ChartTwoSeries[0].Values = SalinitySeries[0].Values;
                                ChartTwo.AxisY[0].Title = "Salinity";
                                break;
                            case (int)CalibrationModes.Temperature:
                                TempTextButton.Background = chartTwoColor;
                                ChartTwoSeries[0].Values = TempSeries[0].Values;
                                ChartTwo.AxisY[0].Title = "Temp";
                                break;
                            case (int)CalibrationModes.Lighting:
                                LightingTextButton.Background = chartTwoColor;
                                ChartTwoSeries[0].Values = LightingSeries[0].Values;
                                ChartTwo.AxisY[0].Title = "Light";
                                break;
                        }
                        DataContext = this;
                    }

                    if (!I2CUpdated)
                    {
                        await Task.Delay(1000 * 10, cancellationTokenSourceCharts.Token); // 10 sec delay
                    }
                    else 
                    {
                        await Task.Delay(1000 * 10 * 60, cancellationTokenSourceCharts.Token); // 10 min delay
                    }
                }
            }
            catch
            {
                
            }
        }

        public async Task UpdateI2CValuesAsync()
        {
            try 
            {
                DateTime localDate = new DateTime();
                double fToC = 25;
                byte[] readBuffer = new byte[32];
                byte[] writeBuffer;

                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    localDate = DateTime.Now;
                    DateTimeButton.Content = String.Format("{0:f}", localDate);

                    if (i2cTempSensor != null && (Values.CalibrationMode == (int)CalibrationModes.Temperature || Values.CalibrationMode == (int)CalibrationModes.None))
                    {
                        writeBuffer = Encoding.ASCII.GetBytes("R");
                        i2cTempSensor.Write(writeBuffer);
                        await Task.Delay(600, cancellationTokenSource.Token);

                        i2cTempSensor.Read(readBuffer);
                        string tempString = Encoding.ASCII.GetString(readBuffer, 1, readBuffer.Length - 1);
                        Values.TemperatueValue = double.Parse(tempString);
                        TempButton.Content = Values.TemperatueValue.ToString("0.00");
                        fToC = (Values.TemperatueValue - 32.0) * 5.0 / 9.0;
                    }

                    if (i2cSalinitySensor != null && (Values.CalibrationMode == (int)CalibrationModes.Salinity || Values.CalibrationMode == (int)CalibrationModes.None))
                    {
                        writeBuffer = Encoding.ASCII.GetBytes("RT," + fToC.ToString("0.0"));
                        i2cSalinitySensor.Write(writeBuffer);
                        await Task.Delay(900, cancellationTokenSource.Token);

                        i2cSalinitySensor.Read(readBuffer);
                        string tempString = Encoding.ASCII.GetString(readBuffer, 1, readBuffer.Length - 1);
                        Values.SalinityValue = double.Parse(tempString);
                        SalinityButton.Content = Values.SalinityValue.ToString("0.00");
                    }

                    if (i2cPHSensor != null && (Values.CalibrationMode == (int)CalibrationModes.PH || Values.CalibrationMode == (int)CalibrationModes.None))
                    {
                        writeBuffer = Encoding.ASCII.GetBytes("RT," + fToC.ToString("0.0"));
                        i2cPHSensor.Write(writeBuffer);
                        await Task.Delay(900, cancellationTokenSource.Token);

                        i2cPHSensor.Read(readBuffer);
                        string tempString = Encoding.ASCII.GetString(readBuffer, 1, readBuffer.Length - 1);
                        Values.PHValue = double.Parse(tempString);
                        PHButton.Content = Values.PHValue.ToString("0.00");
                    }

                    if (i2cDOSensor != null && (Values.CalibrationMode == (int)CalibrationModes.DO || Values.CalibrationMode == (int)CalibrationModes.None))
                    {
                        writeBuffer = Encoding.ASCII.GetBytes("S," + Values.SalinityValue.ToString("0.0") + ",ppt");
                        i2cDOSensor.Write(writeBuffer);
                        await Task.Delay(300, cancellationTokenSource.Token); 
                        
                        writeBuffer = Encoding.ASCII.GetBytes("RT," + fToC.ToString("0.0"));
                        i2cDOSensor.Write(writeBuffer);
                        await Task.Delay(900, cancellationTokenSource.Token);

                        i2cDOSensor.Read(readBuffer);
                        string tempString = Encoding.ASCII.GetString(readBuffer, 1, readBuffer.Length - 1);
                        Values.DOValue = double.Parse(tempString);
                        DOButton.Content = Values.DOValue.ToString("0.00");
                    }

                    if (i2cORPSensor != null && (Values.CalibrationMode == (int)CalibrationModes.ORP || Values.CalibrationMode == (int)CalibrationModes.None))
                    {
                        writeBuffer = Encoding.ASCII.GetBytes("R");
                        i2cORPSensor.Write(writeBuffer);
                        await Task.Delay(900, cancellationTokenSource.Token);

                        i2cORPSensor.Read(readBuffer);
                        string tempString = Encoding.ASCII.GetString(readBuffer, 1, readBuffer.Length - 1);
                        Values.ORPValue = double.Parse(tempString);
                        ORPButton.Content = Values.ORPValue.ToString("0.00");
                    }

                    Window.Current.CoreWindow.PointerCursor = null; //new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                    I2CUpdated = true;
                    await Task.Delay(1000 * 60);
                }
            }
            catch
            {
                
            }
            finally
            {
                I2CUpdated = false;
            }
        }

        public async Task UpdateInputsAsync()
        {
            try
            {
                var pwmControllers = await PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider());
                var pwmController = pwmControllers[1]; // use the on-device controller
                pwmController.SetDesiredFrequency(120); // try to match 120Hz

                pwmOne = pwmController.OpenPin(PWM_ONE);
                pwmOne.SetActiveDutyCyclePercentage(0.0);
                pwmOne.Start();

                pwmTwo = pwmController.OpenPin(PWM_TWO);
                pwmTwo.SetActiveDutyCyclePercentage(0.0);
                pwmTwo.Start();

                pwmThree = pwmController.OpenPin(PWM_THREE);
                pwmThree.SetActiveDutyCyclePercentage(0.0);
                pwmThree.Start();

                pwmFour = pwmController.OpenPin(PWM_FOUR);
                pwmFour.SetActiveDutyCyclePercentage(0.0);
                pwmFour.Start();

                pwmFive = pwmController.OpenPin(PWM_FIVE);
                pwmFive.SetActiveDutyCyclePercentage(0.0);
                pwmFive.Start();


                while (!cancellationTokenSourceInputs.IsCancellationRequested)
                {
                    Values.LightSeconds = DateTime.Now.TimeOfDay.TotalSeconds;
                    Values.PeakTime = Values.LightsOnTime + (Values.LightsOffTime - Values.LightsOnTime) / 2.0;
                    if (!Values.LightingModeAuto)
                    {
                        Values.Power = 1;
                        pwmOne.SetActiveDutyCyclePercentage(Values.LightOutOneMax);
                        pwmTwo.SetActiveDutyCyclePercentage(Values.LightOutTwoMax);
                        pwmThree.SetActiveDutyCyclePercentage(Values.LightOutThreeMax);
                        pwmFour.SetActiveDutyCyclePercentage(Values.LightOutFourMax);
                        pwmFive.SetActiveDutyCyclePercentage(Values.LightOutFiveMax);

                    }
                    else if (Values.LightSeconds < Values.LightsOnTime || Values.LightSeconds > Values.LightsOffTime)
                    {
                        Values.Power = 0;
                        pwmOne.SetActiveDutyCyclePercentage(0);
                        pwmTwo.SetActiveDutyCyclePercentage(Values.LightOutNightLight);
                        pwmThree.SetActiveDutyCyclePercentage(0);
                        pwmFour.SetActiveDutyCyclePercentage(0);
                        pwmFive.SetActiveDutyCyclePercentage(0);
                    }
                    else 
                    {
                        if (Values.LightSeconds < Values.PeakTime)
                        {
                            Values.Power = Math.Round((Values.LightSeconds - Values.LightsOnTime) / (Values.PeakTime - Values.LightsOnTime),3);
                        }
                        if (Values.LightSeconds > Values.PeakTime)
                        {
                            Values.Power = Math.Round((Values.LightSeconds - Values.LightsOffTime) / (Values.PeakTime - Values.LightsOffTime),3);
                        }

                        if (Values.LightingValue != Values.Power)
                        {
                            pwmOne.SetActiveDutyCyclePercentage(Values.Power * Values.LightOutOneMax);
                            pwmTwo.SetActiveDutyCyclePercentage(Values.Power * Values.LightOutTwoMax);
                            pwmThree.SetActiveDutyCyclePercentage(Values.Power * Values.LightOutThreeMax);
                            pwmFour.SetActiveDutyCyclePercentage(Values.Power * Values.LightOutFourMax);
                            pwmFive.SetActiveDutyCyclePercentage(Values.Power * Values.LightOutFiveMax);
                        }
                    }

                    Values.LightingValue = Values.Power;
       
                    await Task.Delay(1000*60*10, cancellationTokenSourceInputs.Token);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async void CalibrationButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            byte[] writeBuffer;
            byte[] readBuffer = new byte[32];

            cancellationTokenSourceCharts.Cancel();

            Button b = (Button)sender;
            Values.CalibrationDialog = b.Name.ToString();
            int numOfSteps = 0;

            switch (Values.CalibrationDialog)
            {
                case ("SalinityButton"):
                    Values.CalibrationMode = (int)CalibrationModes.Salinity;
                    numOfSteps = 3;
                    Values.CalibrationMessageOne = "Step 1 of 3: Calibrate Dry";
                    Values.CalibrationMessageTwo = "Step 2 of 3: Calibrate in 12,880 uS solution";
                    Values.CalibrationMessageThree = "Step 3 of 3: Calibrate in 80,000 uS solution";
                    Values.CalibrationDelay = 600;
                    break;
                case ("ORPButton"):
                    Values.CalibrationMode = (int)CalibrationModes.ORP;
                    numOfSteps = 1;
                    Values.CalibrationMessageOne = "Step 1 of 1: Calibrate in 225 mV solution";
                    Values.CalibrationDelay = 900; 
                    break;
                case ("PHButton"):
                    Values.CalibrationMode = (int)CalibrationModes.PH;
                    numOfSteps = 3;
                    Values.CalibrationMessageOne = "Step 1 of 3: Calibrate in 7.00 solution";
                    Values.CalibrationMessageTwo = "Step 2 of 3: Calibrate in 4.00 solution";
                    Values.CalibrationMessageThree = "Step 3 of 3: Calibrate in 10.00 solution";
                    Values.CalibrationDelay = 900; 
                    break;
                case ("DOButton"):
                    Values.CalibrationMode = (int)CalibrationModes.DO;
                    numOfSteps = 2;
                    Values.CalibrationMessageOne = "Step 1 of 2: Calibrate at atmosphere";
                    Values.CalibrationMessageTwo = "Step 2 of 2: Calibrate in 0 DO solution";
                    Values.CalibrationDelay = 1300; 
                    break;
                case ("TempButton"):
                    Values.CalibrationMode = (int)CalibrationModes.Temperature;
                    numOfSteps = 1;
                    Values.CalibrationMessageOne = "Step 1 of 1: Calibrate in boiling water";
                    Values.CalibrationDelay = 600; 
                    break;
            }

            for (int i = 1; i <= numOfSteps; i++)
            {
                switch (i)
                {
                    case 1:
                        Values.CalibrationDialog = Values.CalibrationMessageOne;
                        break;
                    case 2:
                        Values.CalibrationDialog = Values.CalibrationMessageTwo;
                        break;
                    case 3:
                        Values.CalibrationDialog = Values.CalibrationMessageThree;
                        break;
                }

                CalibrationgDialog calibrationUpdate = new CalibrationgDialog();
                ContentDialogResult result = await calibrationUpdate.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    cancellationTokenSource.Cancel();
                    await Task.Delay(1000);
                    
                    switch (Values.CalibrationMode)
                    {
                        case (int)CalibrationModes.DO:
                            writeBuffer = Encoding.ASCII.GetBytes(Values.CalibrationDialog);
                            i2cDOSensor.Write(writeBuffer);
                            await Task.Delay(Values.CalibrationDelay);
                            break;
                        case (int)CalibrationModes.ORP:
                            writeBuffer = Encoding.ASCII.GetBytes(Values.CalibrationDialog);
                            i2cORPSensor.Write(writeBuffer);
                            await Task.Delay(Values.CalibrationDelay);
                            break;
                        case (int)CalibrationModes.PH:
                            writeBuffer = Encoding.ASCII.GetBytes(Values.CalibrationDialog);
                            i2cPHSensor.Write(writeBuffer);
                            await Task.Delay(Values.CalibrationDelay);
                            break;
                        case (int)CalibrationModes.Salinity:
                            writeBuffer = Encoding.ASCII.GetBytes(Values.CalibrationDialog);
                            i2cSalinitySensor.Write(writeBuffer);
                            await Task.Delay(Values.CalibrationDelay); 
                            break;
                        case (int)CalibrationModes.Temperature:
                            writeBuffer = Encoding.ASCII.GetBytes(Values.CalibrationDialog);
                            i2cTempSensor.Write(writeBuffer);
                            await Task.Delay(Values.CalibrationDelay); 
                            break;
                    }
                    cancellationTokenSource = new CancellationTokenSource();

                    await UpdateI2CValuesAsync();
                }
                else
                {
                    i = numOfSteps + 1;
                }
            }

            cancellationTokenSource.Cancel();
            await Task.Delay(1000);
            Values.CalibrationDialog = "Cal,?";
            switch (Values.CalibrationMode)
            {
                case (int)CalibrationModes.DO:
                    writeBuffer = Encoding.ASCII.GetBytes(Values.CalibrationDialog);
                    i2cDOSensor.Write(writeBuffer);
                    await Task.Delay(Values.CalibrationDelay);
                    i2cDOSensor.Read(readBuffer);
                    Values.CalibrationDialog = Encoding.ASCII.GetString(readBuffer, 1, readBuffer.Length - 1);
                    break;
                case (int)CalibrationModes.ORP:
                    writeBuffer = Encoding.ASCII.GetBytes(Values.CalibrationDialog);
                    i2cORPSensor.Write(writeBuffer);
                    await Task.Delay(Values.CalibrationDelay);
                    i2cORPSensor.Read(readBuffer);
                    Values.CalibrationDialog = Encoding.ASCII.GetString(readBuffer, 1, readBuffer.Length - 1);
                    break;
                case (int)CalibrationModes.PH:
                    writeBuffer = Encoding.ASCII.GetBytes(Values.CalibrationDialog);
                    i2cPHSensor.Write(writeBuffer);
                    await Task.Delay(Values.CalibrationDelay);
                    i2cPHSensor.Read(readBuffer);
                    Values.CalibrationDialog = Encoding.ASCII.GetString(readBuffer, 1, readBuffer.Length - 1);
                    break;
                case (int)CalibrationModes.Salinity:
                    writeBuffer = Encoding.ASCII.GetBytes(Values.CalibrationDialog);
                    i2cSalinitySensor.Write(writeBuffer);
                    await Task.Delay(Values.CalibrationDelay);
                    i2cSalinitySensor.Read(readBuffer);
                    Values.CalibrationDialog = Encoding.ASCII.GetString(readBuffer, 1, readBuffer.Length - 1);
                    break;
                case (int)CalibrationModes.Temperature:
                    writeBuffer = Encoding.ASCII.GetBytes(Values.CalibrationDialog);
                    i2cTempSensor.Write(writeBuffer);
                    await Task.Delay(Values.CalibrationDelay);
                    i2cTempSensor.Read(readBuffer);
                    Values.CalibrationDialog = Encoding.ASCII.GetString(readBuffer, 1, readBuffer.Length - 1);
                    break;
            }
            var messageDialog = new MessageDialog(Values.CalibrationDialog.Substring(5,1) + " Point Calibration Saved", "Calibration Status");
            await messageDialog.ShowAsync();

            Values.CalibrationMode = (int)CalibrationModes.None; 
            cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSourceCharts = new CancellationTokenSource();

            await Task.WhenAll(UpdateI2CValuesAsync(), UpdateUIControlsAsync());
        }

        private void LoadProgramSettings()
        {
            Values.LoadingSettings = true;

            DigitalInOneButton.Content = Values.LoadStringSettings("DigitalInOneButton") ?? DigitalInOneButton.Content;
            DigitalInTwoButton.Content = Values.LoadStringSettings("DigitalInTwoButton") ?? DigitalInTwoButton.Content;

            OutOneButton.Content = Values.LoadStringSettings("OutOneButton") ?? OutOneButton.Content;
            outputOne.Name = Values.CalibrationDialog;

            OutTwoButton.Content = Values.LoadStringSettings("OutTwoButton") ?? OutTwoButton.Content;
            outputTwo.Name = Values.CalibrationDialog;

            OutThreeButton.Content = Values.LoadStringSettings("OutThreeButton") ?? OutThreeButton.Content;
            outputThree.Name = Values.CalibrationDialog;

            OutFourButton.Content = Values.LoadStringSettings("OutFourButton") ?? OutFourButton.Content;
            outputFour.Name = Values.CalibrationDialog;

            OutFiveButton.Content = Values.LoadStringSettings("OutFiveButton") ?? OutFiveButton.Content;
            outputFive.Name = Values.CalibrationDialog;

            OutSixButton.Content = Values.LoadStringSettings("OutSixButton") ?? OutSixButton.Content;
            outputSix.Name = Values.CalibrationDialog;

            OutSevenButton.Content = Values.LoadStringSettings("OutSevenButton") ?? OutSevenButton.Content;
            outputSeven.Name = Values.CalibrationDialog;

            OutEightButton.Content = Values.LoadStringSettings("OutEightButton") ?? OutEightButton.Content;
            outputEight.Name = Values.CalibrationDialog;

            OutputOneCombo.SelectedIndex = int.Parse(Values.LoadStringSettings("OutputOneCombo") ?? "0");
            outputOne.CurrentMode = OutputOneCombo.SelectedIndex;
            OutOneButton.Background = OutputOneCombo.SelectedIndex == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;

            OutputTwoCombo.SelectedIndex = int.Parse(Values.LoadStringSettings("OutputTwoCombo") ?? "0");
            outputTwo.CurrentMode = OutputTwoCombo.SelectedIndex;
            OutTwoButton.Background = OutputTwoCombo.SelectedIndex == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;

            OutputThreeCombo.SelectedIndex = int.Parse(Values.LoadStringSettings("OutputThreeCombo") ?? "0");
            outputThree.CurrentMode = OutputThreeCombo.SelectedIndex;
            OutThreeButton.Background = OutputThreeCombo.SelectedIndex == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;

            OutputFourCombo.SelectedIndex = int.Parse(Values.LoadStringSettings("OutputFourCombo") ?? "0");
            outputFour.CurrentMode = OutputFourCombo.SelectedIndex;
            OutFourButton.Background = OutputFourCombo.SelectedIndex == 0 ? new SolidColorBrush(Colors.DarkGreen) : defaultBackColor;

            OutputFiveCombo.SelectedIndex = int.Parse(Values.LoadStringSettings("OutputFiveCombo") ?? "0");
            outputFive.CurrentMode = OutputFiveCombo.SelectedIndex;
            
            OutputSixCombo.SelectedIndex = int.Parse(Values.LoadStringSettings("OutputSixCombo") ?? "0");
            outputSix.CurrentMode = OutputSixCombo.SelectedIndex;
            
            OutputSevenCombo.SelectedIndex = int.Parse(Values.LoadStringSettings("OutputSevenCombo") ?? "0");
            outputSeven.CurrentMode = OutputSevenCombo.SelectedIndex;
            
            OutputEightCombo.SelectedIndex = int.Parse(Values.LoadStringSettings("OutputEightCombo") ?? "0");
            outputEight.CurrentMode = OutputEightCombo.SelectedIndex;
            
            outputOne.FeedMode = int.Parse(Values.LoadStringSettings("OutputOneFeed") ?? "0");
            outputTwo.FeedMode = int.Parse(Values.LoadStringSettings("OutputTwoFeed") ?? "0");
            outputThree.FeedMode = int.Parse(Values.LoadStringSettings("OutputThreeFeed") ?? "0");
            outputFour.FeedMode = int.Parse(Values.LoadStringSettings("OutputFourFeed") ?? "0");
            outputFive.FeedMode = int.Parse(Values.LoadStringSettings("OutputFiveFeed") ?? "0");
            outputSix.FeedMode = int.Parse(Values.LoadStringSettings("OutputSixFeed") ?? "0");
            outputSeven.FeedMode = int.Parse(Values.LoadStringSettings("OutputSevenFeed") ?? "0");
            outputEight.FeedMode = int.Parse(Values.LoadStringSettings("OutputEightFeed") ?? "0");
            
            Values.ChartOne = int.Parse(Values.LoadStringSettings("ChartIndexOne") ?? "0");
            Values.ChartTwo = int.Parse(Values.LoadStringSettings("ChartIndexTwo") ?? "0");
            
            Values.LightsOnTime = double.Parse(Values.LoadStringSettings("LightsOnTime") ?? "0");
            Values.LightsOffTime = double.Parse(Values.LoadStringSettings("LightsOffTime") ?? "0");
            Values.LightOutOneMax = double.Parse(Values.LoadStringSettings("LightOutOneMax") ?? "0");
            Values.LightOutTwoMax = double.Parse(Values.LoadStringSettings("LightOutTwoMax") ?? "0");
            Values.LightOutThreeMax = double.Parse(Values.LoadStringSettings("LightOutThreeMax") ?? "0");
            Values.LightOutFourMax = double.Parse(Values.LoadStringSettings("LightOutFourMax") ?? "0");
            Values.LightOutFiveMax = double.Parse(Values.LoadStringSettings("LightOutFiveMax") ?? "0");
            Values.LightOutNightLight = double.Parse(Values.LoadStringSettings("LightOutNightLight") ?? "0");

            Values.FeedDuration = (int)double.Parse(Values.LoadStringSettings("FeedDuration") ?? "0");
            FeedButton.Content = "Feed" + Environment.NewLine + Values.FeedDuration.ToString() + " min(s)";

            outputOne.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode 1") ?? "0");
            outputOne.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn 1") ?? "0");
            outputOne.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut 1") ?? "0");
            outputOne.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn 1") ?? "0");
            outputOne.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff 1") ?? "0");
            outputTwo.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode 2") ?? "0");
            outputTwo.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn 2") ?? "0");
            outputTwo.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut 2") ?? "0");
            outputTwo.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn 2") ?? "0");
            outputTwo.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff 2") ?? "0");
            outputThree.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode 3") ?? "0");
            outputThree.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn 3") ?? "0");
            outputThree.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut 3") ?? "0");
            outputThree.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn 3") ?? "0");
            outputThree.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff 3") ?? "0");
            outputFour.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode 4") ?? "0");
            outputFour.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn 4") ?? "0");
            outputFour.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut 4") ?? "0");
            outputFour.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn 4") ?? "0");
            outputFour.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff 4") ?? "0");
            outputFive.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode 5") ?? "0");
            outputFive.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn 5") ?? "0");
            outputFive.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut 5") ?? "0");
            outputFive.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn 5") ?? "0");
            outputFive.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff 5") ?? "0");
            outputSix.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode 6") ?? "0");
            outputSix.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn 6") ?? "0");
            outputSix.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut 6") ?? "0");
            outputSix.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn 6") ?? "0");
            outputSix.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff 6") ?? "0");
            outputSeven.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode 7") ?? "0");
            outputSeven.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn 7") ?? "0");
            outputSeven.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut 7") ?? "0");
            outputSeven.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn 7") ?? "0");
            outputSeven.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff 7") ?? "0");
            outputEight.AutoMode = int.Parse(Values.LoadStringSettings("AutoMode 8") ?? "0");
            outputEight.AutoOnValue = double.Parse(Values.LoadStringSettings("AutoOn 8") ?? "0");
            outputEight.AutoTimeOut = int.Parse(Values.LoadStringSettings("AutoTimeOut 8") ?? "0");
            outputEight.AutoTimeOn = double.Parse(Values.LoadStringSettings("AutoTimeOn 8") ?? "0");
            outputEight.AutoTimeOff = double.Parse(Values.LoadStringSettings("AutoTimeOff 8") ?? "0");
            outputOne.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode 1") ?? "0");
            outputTwo.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode 2") ?? "0");
            outputThree.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode 3") ?? "0");
            outputFour.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode 4") ?? "0");
            outputFive.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode 5") ?? "0");
            outputSix.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode 6") ?? "0");
            outputSeven.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode 7") ?? "0");
            outputEight.ControlMode = int.Parse(Values.LoadStringSettings("ControlMode 8") ?? "0");

            Values.LoadingSettings = false;
        }
    }
}
