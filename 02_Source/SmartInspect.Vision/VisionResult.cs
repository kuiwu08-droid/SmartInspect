using System.Collections.Generic;

namespace SmartInspect.Vision
{
    public class VisionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public int Channels { get; set; }

        public int ContourCount { get; set; }

        public List<VisionObjectResult> Objects { get; set; } =
            new List<VisionObjectResult>();
    }
}