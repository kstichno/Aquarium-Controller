using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Aquarium
{
    public partial class AutoDialog
    {
        public int OutNumber { get; set; }
        public AutoDialog(int outNumber)
        {
            OutNumber = outNumber;
            this.InitializeComponent();
            this.Title = "Auto Output Setup (CH " + outNumber.ToString() + ")";

            ControlModeCombo.SelectionChanged += ControlModeCombo_SelectionChanged;

            ControlModeCombo.SelectedIndex = int.Parse(Values.LoadStringSettings("ControlMode " + outNumber.ToString()) ?? "0");
            if (ControlModeCombo.SelectedIndex != 1)
            {
                SensorModeCombo.SelectedIndex = int.Parse(Values.LoadStringSettings("AutoMode " + outNumber.ToString()) ?? "0");
                SetPointSlider.Value = double.Parse(Values.LoadStringSettings("AutoOn " + outNumber.ToString()) ?? "0");
                TimeOutSlider.Value = double.Parse(Values.LoadStringSettings("AutoTimeOut " + outNumber.ToString()) ?? "0");
            }
            else
            {
                OnTimePicker.Time = TimeSpan.FromSeconds(double.Parse(Values.LoadStringSettings("AutoTimeOn " + outNumber.ToString()) ?? "0"));
                OffTimePicker.Time = TimeSpan.FromSeconds(double.Parse(Values.LoadStringSettings("AutoTimeOff " + outNumber.ToString()) ?? "0"));
            }
        }

        private void ContentDialog_PrimaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Values.ProgramSettings.Values["ControlMode " + OutNumber.ToString()] = ControlModeCombo.SelectedIndex.ToString(); 
            Values.ProgramSettings.Values["AutoMode " + OutNumber.ToString()] = SensorModeCombo.SelectedIndex.ToString();
            Values.ProgramSettings.Values["AutoOn " + OutNumber.ToString()] = SetPointSlider.Value.ToString();
            Values.ProgramSettings.Values["AutoTimeOut " + OutNumber.ToString()] = TimeOutSlider.Value.ToString();
            Values.ProgramSettings.Values["AutoTimeOn " + OutNumber.ToString()] = OnTimePicker.Time.TotalSeconds.ToString();
            Values.ProgramSettings.Values["AutoTimeOff " + OutNumber.ToString()] = OffTimePicker.Time.TotalSeconds.ToString();
        }

        private void ContentDialog_SecondaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void ControlModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ControlModeCombo.SelectedIndex == 0)
            {
                this.SensorStack.Visibility = Visibility.Visible;
                this.TimeStack.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.SensorStack.Visibility = Visibility.Collapsed;
                this.TimeStack.Visibility = Visibility.Visible;
            }
        }

        private void SensorModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (SensorModeCombo.SelectedIndex)
            {
                case (int)Sensors.InOne:
                    break;
                case (int)Sensors.InTwo:
                    break;
                case (int)Sensors.Temp:
                    SetPointSlider.Minimum = 60;
                    SetPointSlider.Maximum = 100;
                    break;
                case (int)Sensors.Salinity:
                    SetPointSlider.Minimum = 0;
                    SetPointSlider.Maximum = 42;
                    break;
                case (int)Sensors.pH:
                    SetPointSlider.Minimum = 0;
                    SetPointSlider.Maximum = 14;
                    break;
                case (int)Sensors.ORP:
                    SetPointSlider.Minimum = -1000;
                    SetPointSlider.Maximum = 1000;
                    break;
                case (int)Sensors.DO:
                    SetPointSlider.Minimum = 0;
                    SetPointSlider.Maximum = 100;
                    break;

            }
        }
    }
}
