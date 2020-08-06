using System;
using Windows.Storage;

namespace Aquarium
{
    public static class Values
    {
        public static int InputOne { get; set; }
        public static int InputTwo { get; set; }
        public static double TemperatueValue { get; set; }
        public static double SalinityValue { get; set; }
        public static double PHValue { get; set; }
        public static double DOValue { get; set; }
        public static double ORPValue { get; set; }
        public static double LightingValue { get; set; }
        public static string ContentDialog { get; set; }
        public static string CalibrationDialog { get; set; }
        public static int CalibrationMode { get; set; }
        public static string CalibrationMessageOne { get; set; }
        public static string CalibrationMessageTwo { get; set; }
        public static string CalibrationMessageThree { get; set; }
        public static int CalibrationDelay { get; set; }
        public static int ChartOne { get; set; }
        public static int ChartTwo { get; set; }
        public static int ChartIndex { get; set; }
        public static double LightsOnTime {get; set;}
        public static double LightsOffTime { get; set; }
        public static double LightOutOneMax { get; set; }
        public static double LightOutTwoMax { get; set; }
        public static double LightOutNightLight { get; set; }
        public static double LightOutThreeMax { get; set; }
        public static double LightOutFourMax { get; set; }
        public static double LightOutFiveMax { get; set; }
        public static double Power { get; set; }
        public static double LightSeconds { get; set; }
        public static double RelaySeconds { get; set; }

        public static double PeakTime { get; set; }
        public static bool LightingModeAuto { get; set; }
        public static int FeedDuration { get; set; }
        public static bool Feeding { get; set; }
        public static bool LoadingSettings { get; set; }
        public static ApplicationDataContainer ProgramSettings { get; set; }

        public static string LoadStringSettings(string control)
        {
           return ProgramSettings.Values[control] as string;
        }
    }

    public enum CalibrationModes
    {
        None,
        Salinity,
        PH,
        DO,
        Temperature,
        ORP,
        Lighting,
    }

    public enum  Sensors
    {
        InOne,
        InTwo,
        Temp,
        Salinity,
        pH,
        ORP,
        DO,
        Time,
    }

    public enum StatusModes
    {
        Off,
        On,
    }

    public class Output
    {
        public int CurrentMode { get; set; }
        public int ReturnMode { get; set; }
        public int FeedMode { get; set; }
        public int AutoMode { get; set; }
        public double AutoOnValue { get; set; }
        public double AutoOffValue { get; set; }
        public double AutoTimeOn { get; set; }
        public double AutoTimeOff { get; set; }
        public int AutoTimeOut { get; set; }
        public int ControlMode { get; set; }
        public string Name { get; set; }
    }

    public class TemperatureData
    {
        public double XValue { get; set; }

        public double YValue { get; set; }
    }
}
