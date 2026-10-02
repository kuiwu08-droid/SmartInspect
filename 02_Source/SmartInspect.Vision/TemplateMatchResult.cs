namespace SmartInspect.Vision
{
    public class TemplateMatchResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";

        public double Score { get; set; }

        public int MatchX { get; set; }

        public int MatchY { get; set; }

        public int TemplateWidth { get; set; }

        public int TemplateHeight { get; set; }
    }
}