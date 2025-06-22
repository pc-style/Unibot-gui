using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenCvSharp;
using Unibot.Models;

namespace Unibot.Core;

public class ScreenCapture : IDisposable
{
    private readonly ScreenSettings _settings;
    private bool _disposed = false;

    public ScreenCapture(ScreenSettings settings)
    {
        _settings = settings;
    }

    public Mat? CaptureRegion(Rectangle region)
    {
        try
        {
            using var bitmap = new Bitmap(region.Width, region.Height, PixelFormat.Format24bppRgb);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.CopyFromScreen(region.X, region.Y, 0, 0, region.Size, CopyPixelOperation.SourceCopy);
            
            return BitmapToMat(bitmap);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Screen capture failed: {ex.Message}");
            return null;
        }
    }

    public (System.Drawing.Point? target, bool trigger) DetectTarget(Rectangle fovRegion, int recoilOffset = 0)
    {
        var adjustedRegion = new Rectangle(
            fovRegion.X,
            fovRegion.Y - recoilOffset,
            fovRegion.Width,
            fovRegion.Height
        );

        using var image = CaptureRegion(adjustedRegion);
        if (image == null) return (null, false);

        return ProcessImageForTargets(image, fovRegion.Size);
    }

    private (System.Drawing.Point? target, bool trigger) ProcessImageForTargets(Mat image, System.Drawing.Size fovSize)
    {
        try
        {
            // Convert BGR to HSV for color detection
            using var hsvImage = new Mat();
            Cv2.CvtColor(image, hsvImage, ColorConversionCodes.BGR2HSV);

            // Create color mask
            var lowerBound = new Scalar(_settings.ColorRange.Lower.H, _settings.ColorRange.Lower.S, _settings.ColorRange.Lower.V);
            var upperBound = new Scalar(_settings.ColorRange.Upper.H, _settings.ColorRange.Upper.S, _settings.ColorRange.Upper.V);
            
            using var mask = new Mat();
            Cv2.InRange(hsvImage, lowerBound, upperBound, mask);

            // Apply morphological operations to clean up the mask
            var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, 
                new OpenCvSharp.Size(_settings.DetectionThresholdX, _settings.DetectionThresholdY));
            
            using var dilatedMask = new Mat();
            Cv2.Dilate(mask, dilatedMask, kernel, iterations: 5);

            // Apply threshold
            using var thresholdMask = new Mat();
            Cv2.Threshold(dilatedMask, thresholdMask, 60, 255, ThresholdTypes.Binary);

            // Find contours
            Cv2.FindContours(thresholdMask, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxNone);

            if (contours.Length == 0) return (null, false);

            // Find closest target
            var fovCenter = new OpenCvSharp.Point(fovSize.Width / 2, fovSize.Height / 2);
            var aimFovRect = new Rect(
                fovCenter.X - _settings.AimFovX / 2,
                fovCenter.Y - _settings.AimFovY / 2,
                _settings.AimFovX,
                _settings.AimFovY
            );

            OpenCvSharp.Point[]? closestContour = null;
            System.Drawing.Point? targetPoint = null;
            double minDistance = double.MaxValue;

            foreach (var contour in contours)
            {
                var boundingRect = Cv2.BoundingRect(contour);
                
                // Calculate target center with aim height adjustment
                var centerX = boundingRect.X + boundingRect.Width / 2;
                var centerY = boundingRect.Y + (int)(boundingRect.Height * (1 - _settings.AimHeight));
                
                var relativeX = centerX - fovCenter.X;
                var relativeY = centerY - fovCenter.Y;
                
                var distance = Math.Sqrt(relativeX * relativeX + relativeY * relativeY);
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestContour = contour;
                    
                    // Check if target is within aim FOV
                    if (aimFovRect.Contains(centerX, centerY))
                    {
                        targetPoint = new System.Drawing.Point(relativeX, relativeY);
                    }
                }
            }

            // Check for trigger condition
            bool trigger = false;
            if (closestContour != null)
            {
                var centerPoint = new OpenCvSharp.Point(fovCenter.X, fovCenter.Y);
                
                // Check multiple points around crosshair for better trigger detection
                var triggerPoints = new[]
                {
                    centerPoint,
                    new OpenCvSharp.Point(centerPoint.X + _settings.TriggerThreshold, centerPoint.Y),
                    new OpenCvSharp.Point(centerPoint.X - _settings.TriggerThreshold, centerPoint.Y),
                    new OpenCvSharp.Point(centerPoint.X, centerPoint.Y + _settings.TriggerThreshold),
                    new OpenCvSharp.Point(centerPoint.X, centerPoint.Y - _settings.TriggerThreshold)
                };

                // Trigger if center point is inside the contour, or if majority of points are inside
                var pointsInside = triggerPoints.Count(point => Cv2.PointPolygonTest(closestContour, point, false) >= 0);
                trigger = Cv2.PointPolygonTest(closestContour, centerPoint, false) >= 0 || pointsInside >= 3;
            }

            return (targetPoint, trigger);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Target detection failed: {ex.Message}");
            return (null, false);
        }
    }

    private static Mat BitmapToMat(Bitmap bitmap)
    {
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);

        var mat = new Mat(bitmap.Height, bitmap.Width, MatType.CV_8UC3, bitmapData.Scan0);
        var result = mat.Clone();
        
        bitmap.UnlockBits(bitmapData);
        
        return result;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _disposed = true;
        }
    }
}