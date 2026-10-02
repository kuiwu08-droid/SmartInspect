namespace SmartInspect.Core
{
    public class AppConfig
    {
        public double PixelsPerMillimeter { get; set; } = 10;
        public double TemplateMinimumScore { get; set; } = 0.8;
        public string LastImagePath { get; set; } = "";
        public string ImageSourceMode { get; set; } = "Single";
        public string ImageFolder { get; set; } = "";

        public double MinMeasurement { get; set; } = 9.5;

        public double MaxMeasurement { get; set; } = 10.5;
    }
}
