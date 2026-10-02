namespace SmartInspect.Core
{
    public class Product
    {
        public string InspectTime { get; set; } = "";
        public string BatchNumber { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public double Measurement { get; set; }
        public string Result { get; set; } = "";
    }
}
