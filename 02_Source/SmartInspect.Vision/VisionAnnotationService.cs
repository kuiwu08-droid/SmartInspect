using OpenCvSharp;

namespace SmartInspect.Vision;

public class VisionAnnotationService
{
    // PNG bytes keep Mat lifetime out of the UI and do not hold the source file open.
    public byte[] CreateAnnotatedImage(string path, VisionResult result, string productResult)
    {
        using var image = Cv2.ImRead(path, ImreadModes.Color);
        if (image.Empty()) throw new IOException("结果图原始图片读取失败。");
        double scale = Math.Clamp(image.Width / 1280.0, 0.4, 2.0);
        int thickness = Math.Max(1, (int)(2 * scale));
        for (int i = 0; i < result.Objects.Count; i++)
        {
            var item = result.Objects[i];
            var color = i == 0 ? (productResult == "OK" ? Scalar.LimeGreen : Scalar.Red) : Scalar.Yellow;
            var points = item.Corners.Select(p => new Point((int)Math.Round(p.X), (int)Math.Round(p.Y))).ToArray();
            if (points.Length == 4) Cv2.Polylines(image, new[] { points }, true, color, thickness);
            string text = $"#{item.Index} Size: {item.WidthMillimeters:F2} x {item.HeightMillimeters:F2} mm";
            var position = new Point(Math.Clamp((int)item.CenterX - 100, 0, Math.Max(0, image.Width - 250)), Math.Clamp((int)item.CenterY, 20, Math.Max(20, image.Height - 10)));
            Cv2.PutText(image, text, position, HersheyFonts.HersheySimplex, 0.8 * scale, Scalar.Black, thickness + 2);
            Cv2.PutText(image, text, position, HersheyFonts.HersheySimplex, 0.8 * scale, color, thickness);
        }
        string summary = $"Result: {productResult} (object #1)";
        Cv2.PutText(image, summary, new Point(10, Math.Max(20, (int)(40 * scale))), HersheyFonts.HersheySimplex, 1.3 * scale, Scalar.Black, thickness + 3);
        Cv2.PutText(image, summary, new Point(10, Math.Max(20, (int)(40 * scale))), HersheyFonts.HersheySimplex, 1.3 * scale, productResult == "OK" ? Scalar.LimeGreen : Scalar.Red, thickness);
        return image.ToBytes(".png");
    }
}
