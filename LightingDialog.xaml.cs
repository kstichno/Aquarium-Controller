using System;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Aquarium
{
    public partial class LightingDialog
    {
        public LightingDialog()
        {
            this.InitializeComponent();

            OnTimePicker.Time = TimeSpan.FromSeconds(Values.LightsOnTime);
            OffTimePicker.Time = TimeSpan.FromSeconds(Values.LightsOffTime);
            CHOneSlider.Value = Values.LightOutOneMax * 100;
            CHTwoSlider.Value = Values.LightOutTwoMax * 100;
            CHThreeSlider.Value = Values.LightOutThreeMax * 100;
            CHFourSlider.Value = Values.LightOutFourMax * 100;
            CHFiveSlider.Value = Values.LightOutFiveMax * 100;
            CHNightLightSlider.Value = Values.LightOutNightLight * 100;

            CHOneSlider.ValueChanged += Slider_ValueChanged;
            CHTwoSlider.ValueChanged += Slider_ValueChanged;
            CHThreeSlider.ValueChanged += Slider_ValueChanged;
            CHFourSlider.ValueChanged += Slider_ValueChanged;
            CHFiveSlider.ValueChanged += Slider_ValueChanged;
        }

        private void ContentDialog_PrimaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Values.ProgramSettings.Values["LightsOnTime"] = (OnTimePicker.Time.TotalSeconds).ToString();
            Values.ProgramSettings.Values["LightsOffTime"] = (OffTimePicker.Time.TotalSeconds).ToString();
            Values.ProgramSettings.Values["LightOutOneMax"] = (CHOneSlider.Value / 100).ToString();
            Values.ProgramSettings.Values["LightOutTwoMax"] = (CHTwoSlider.Value / 100).ToString();
            Values.ProgramSettings.Values["LightOutThreeMax"] = (CHThreeSlider.Value / 100).ToString();
            Values.ProgramSettings.Values["LightOutFourMax"] = (CHFourSlider.Value / 100).ToString();
            Values.ProgramSettings.Values["LightOutFiveMax"] = (CHFiveSlider.Value / 100).ToString();
            Values.ProgramSettings.Values["LightOutNightLight"] = (CHNightLightSlider.Value / 100).ToString();
        }

        private void ContentDialog_SecondaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Values.LightingModeAuto = false;
            Values.LightOutOneMax = CHOneSlider.Value / 100;
            Values.LightOutTwoMax = CHTwoSlider.Value / 100;
            Values.LightOutThreeMax = CHThreeSlider.Value / 100;
            Values.LightOutFourMax = CHFourSlider.Value / 100;
            Values.LightOutFiveMax = CHFiveSlider.Value / 100;
            Values.LightOutNightLight = CHNightLightSlider.Value / 100;
        }
    }
}
