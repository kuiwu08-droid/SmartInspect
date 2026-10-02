namespace SmartInspect.Vision
{
    public class VisionObjectResult
    {
        public OpenCvSharp.Point2f[] Corners { get; set; } = Array.Empty<OpenCvSharp.Point2f>();

        public int Index { get; set; }

        public double CenterX { get; set; }

        public double CenterY { get; set; }

        public double WidthPixels { get; set; }

        public double HeightPixels { get; set; }

        public double AreaPixels { get; set; }

        public double WidthMillimeters { get; set; }

        public double HeightMillimeters { get; set; }

        public double Angle { get; set; }
    }
}
