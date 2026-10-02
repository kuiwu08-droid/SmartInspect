using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmartInspect.Vision
{
    public class VisionService
    {
        public VisionResult InspectImage(
            string imagePath,
            double pixelsPerMillimeter)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return new VisionResult
                {
                    Success = false,
                    Message = "图片路径不能为空。"
                };
            }

            if (!File.Exists(imagePath))
            {
                return new VisionResult
                {
                    Success = false,
                    Message = "图片文件不存在。"
                };
            }

            if (!double.IsFinite(pixelsPerMillimeter) || pixelsPerMillimeter <= 0)
            {
                return new VisionResult
                {
                    Success = false,
                    Message = "标定比例必须大于 0。"
                };
            }

            try
            {
                using Mat image = Cv2.ImRead(imagePath, ImreadModes.Color);

                if (image.Empty())
                {
                    return new VisionResult
                    {
                        Success = false,
                        Message = "OpenCV 无法读取该图片。"
                    };
                }

                using Mat gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                using Mat filtered = new Mat();
                Cv2.MedianBlur(gray, filtered, 5);

                using Mat binary = new Mat();
                Cv2.Threshold(
                    filtered,
                    binary,
                    0,
                    255,
                    ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);

                Cv2.FindContours(
                    binary,
                    out Point[][] contours,
                    out HierarchyIndex[] hierarchy,
                    RetrievalModes.External,
                    ContourApproximationModes.ApproxSimple);

                List<VisionObjectResult> objects =
                    new List<VisionObjectResult>();

                foreach (Point[] contour in contours)
                {
                    double area = Cv2.ContourArea(contour);

                    if (area < 1000)
                    {
                        continue;
                    }

                    RotatedRect rotatedRect =
                        Cv2.MinAreaRect(contour);

                    double side1 = rotatedRect.Size.Width;
                    double side2 = rotatedRect.Size.Height;

                    double widthPixels = Math.Max(side1, side2);
                    double heightPixels = Math.Min(side1, side2);

                    double widthMillimeters =
                        widthPixels / pixelsPerMillimeter;

                    double heightMillimeters =
                        heightPixels / pixelsPerMillimeter;

                    double angle = rotatedRect.Angle;

                    if (angle < -45)
                    {
                        angle += 90;
                    }

                    if (angle > 45)
                    {
                        angle -= 90;
                    }

                    VisionObjectResult visionObject =
                        new VisionObjectResult
                        {
                            Corners = rotatedRect.Points(),
                            CenterX = rotatedRect.Center.X,
                            CenterY = rotatedRect.Center.Y,
                            WidthPixels = widthPixels,
                            HeightPixels = heightPixels,
                            AreaPixels = area,
                            WidthMillimeters = widthMillimeters,
                            HeightMillimeters = heightMillimeters,
                            Angle = angle
                        };

                    objects.Add(visionObject);
                }

                if (objects.Count == 0)
                {
                    return new VisionResult
                    {
                        Success = false,
                        Message = "没有找到有效产品轮廓。",
                        ImageWidth = image.Width,
                        ImageHeight = image.Height,
                        Channels = image.Channels()
                    };
                }

                objects.Sort(
                    (a, b) => a.CenterX.CompareTo(b.CenterX));

                for (int i = 0; i < objects.Count; i++)
                {
                    objects[i].Index = i + 1;
                }

                return new VisionResult
                {
                    Success = true,
                    Message = "多目标视觉检测成功。",
                    ImageWidth = image.Width,
                    ImageHeight = image.Height,
                    Channels = image.Channels(),
                    ContourCount = objects.Count,
                    Objects = objects
                };
            }
            catch (Exception ex)
            {
                return new VisionResult
                {
                    Success = false,
                    Message = "视觉检测失败：" + ex.Message
                };
            }
        }

        public TemplateMatchResult MatchTemplate(
            string scenePath,
            string templatePath,
            double minimumScore = 0.8)
        {
            if (!File.Exists(scenePath))
            {
                return new TemplateMatchResult
                {
                    Success = false,
                    Message = "场景图片不存在。"
                };
            }

            if (!File.Exists(templatePath))
            {
                return new TemplateMatchResult
                {
                    Success = false,
                    Message = "模板图片不存在。"
                };
            }

            if (!double.IsFinite(minimumScore) || minimumScore < 0 || minimumScore > 1)
            {
                return new TemplateMatchResult
                {
                    Success = false,
                    Message = "最低匹配分数必须在 0～1 之间。"
                };
            }

            try
            {
                using Mat scene =
                    Cv2.ImRead(scenePath, ImreadModes.Grayscale);

                using Mat template =
                    Cv2.ImRead(templatePath, ImreadModes.Grayscale);

                if (scene.Empty() || template.Empty())
                {
                    return new TemplateMatchResult
                    {
                        Success = false,
                        Message = "场景图片或模板图片读取失败。"
                    };
                }

                if (template.Width > scene.Width ||
                    template.Height > scene.Height)
                {
                    return new TemplateMatchResult
                    {
                        Success = false,
                        Message = "模板尺寸不能大于场景图片。"
                    };
                }

                using Mat matchResult = new Mat();

                Cv2.MatchTemplate(
                    scene,
                    template,
                    matchResult,
                    TemplateMatchModes.CCoeffNormed);

                Cv2.MinMaxLoc(
                    matchResult,
                    out double minValue,
                    out double maxValue,
                    out Point minLocation,
                    out Point maxLocation);

                if (maxValue < minimumScore)
                {
                    return new TemplateMatchResult
                    {
                        Success = false,
                        Message = "未找到达到匹配阈值的目标。",
                        Score = maxValue,
                        MatchX = maxLocation.X,
                        MatchY = maxLocation.Y,
                        TemplateWidth = template.Width,
                        TemplateHeight = template.Height
                    };
                }

                return new TemplateMatchResult
                {
                    Success = true,
                    Message = "模板匹配成功。",
                    Score = maxValue,
                    MatchX = maxLocation.X,
                    MatchY = maxLocation.Y,
                    TemplateWidth = template.Width,
                    TemplateHeight = template.Height
                };
            }
            catch (Exception ex)
            {
                return new TemplateMatchResult
                {
                    Success = false,
                    Message = "模板匹配失败：" + ex.Message
                };
            }
        }
    }
}
