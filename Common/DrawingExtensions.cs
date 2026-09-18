using System;
using SkiaSharp;

namespace NostromoGraphs.Common
{
    public static class DrawingExtensions
    {
        public static void DrawTechBox(
            this SKCanvas canvas,
            SKRect rect,
            SKColor strokeColor,
            SKColor? fillColor = null,
            float cornerLength = 8f,
            string? title = null,
            SKColor? titleColor = null,
            float fontSize = 11f)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = strokeColor,
                StrokeWidth = 1.2f
            };

            // Fill panel background if specified
            if (fillColor.HasValue)
            {
                using var fillPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    Color = fillColor.Value
                };
                canvas.DrawRect(rect, fillPaint);
            }

            // Draw full border or accented corner box
            canvas.DrawRect(rect, paint);

            // Draw technical corner notches
            paint.StrokeWidth = 2.4f;
            // Top-Left
            canvas.DrawLine(rect.Left, rect.Top, rect.Left + cornerLength, rect.Top, paint);
            canvas.DrawLine(rect.Left, rect.Top, rect.Left, rect.Top + cornerLength, paint);
            // Top-Right
            canvas.DrawLine(rect.Right - cornerLength, rect.Top, rect.Right, rect.Top, paint);
            canvas.DrawLine(rect.Right, rect.Top, rect.Right, rect.Top + cornerLength, paint);
            // Bottom-Left
            canvas.DrawLine(rect.Left, rect.Bottom - cornerLength, rect.Left, rect.Bottom, paint);
            canvas.DrawLine(rect.Left, rect.Bottom, rect.Left + cornerLength, rect.Bottom, paint);
            // Bottom-Right
            canvas.DrawLine(rect.Right - cornerLength, rect.Bottom, rect.Right, rect.Bottom, paint);
            canvas.DrawLine(rect.Right, rect.Bottom - cornerLength, rect.Right, rect.Bottom, paint);

            // Draw Title Header if present
            if (!string.IsNullOrEmpty(title))
            {
                using var textPaint = new SKPaint
                {
                    IsAntialias = true,
                    Color = titleColor ?? strokeColor,
                    TextSize = fontSize,
                    Typeface = SkiaUtils.MonospaceTypeface,
                    FakeBoldText = true
                };

                float textWidth = textPaint.MeasureText(title);
                var headerBgRect = new SKRect(rect.Left + 12f, rect.Top - 6f, rect.Left + 18f + textWidth, rect.Top + 8f);
                
                using var clearPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    Color = fillColor ?? new SKColor(0x08, 0x14, 0x0B)
                };
                canvas.DrawRect(headerBgRect, clearPaint);
                canvas.DrawText(title, rect.Left + 15f, rect.Top + 5f, textPaint);
            }
        }

        public static void DrawCornerBrackets(
            this SKCanvas canvas,
            SKRect rect,
            SKColor color,
            float length = 12f,
            float strokeWidth = 1.5f)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = color,
                StrokeWidth = strokeWidth
            };

            // Top-Left
            canvas.DrawLine(rect.Left, rect.Top, rect.Left + length, rect.Top, paint);
            canvas.DrawLine(rect.Left, rect.Top, rect.Left, rect.Top + length, paint);

            // Top-Right
            canvas.DrawLine(rect.Right - length, rect.Top, rect.Right, rect.Top, paint);
            canvas.DrawLine(rect.Right, rect.Top, rect.Right, rect.Top + length, paint);

            // Bottom-Left
            canvas.DrawLine(rect.Left, rect.Bottom - length, rect.Left, rect.Bottom, paint);
            canvas.DrawLine(rect.Left, rect.Bottom, rect.Left + length, rect.Bottom, paint);

            // Bottom-Right
            canvas.DrawLine(rect.Right - length, rect.Bottom, rect.Right, rect.Bottom, paint);
            canvas.DrawLine(rect.Right, rect.Bottom - length, rect.Right, rect.Bottom, paint);
        }

        public static void DrawCrosshair(
            this SKCanvas canvas,
            float x,
            float y,
            float size,
            SKColor color,
            float strokeWidth = 1f,
            bool circle = true)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = color,
                StrokeWidth = strokeWidth
            };

            canvas.DrawLine(x - size, y, x + size, y, paint);
            canvas.DrawLine(x, y - size, x, y + size, paint);

            if (circle)
            {
                canvas.DrawCircle(x, y, size * 0.5f, paint);
            }
        }

        public static void DrawGrid(
            this SKCanvas canvas,
            SKRect bounds,
            float stepX,
            float stepY,
            SKColor gridColor,
            bool drawSubTicks = true)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = gridColor,
                StrokeWidth = 0.8f
            };

            for (float x = bounds.Left; x <= bounds.Right; x += stepX)
            {
                canvas.DrawLine(x, bounds.Top, x, bounds.Bottom, paint);
            }

            for (float y = bounds.Top; y <= bounds.Bottom; y += stepY)
            {
                canvas.DrawLine(bounds.Left, y, bounds.Right, y, paint);
            }

            if (drawSubTicks)
            {
                using var dotPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    Color = new SKColor(gridColor.Red, gridColor.Green, gridColor.Blue, (byte)Math.Min(255, gridColor.Alpha * 2))
                };

                for (float x = bounds.Left; x <= bounds.Right; x += stepX)
                {
                    for (float y = bounds.Top; y <= bounds.Bottom; y += stepY)
                    {
                        canvas.DrawCircle(x, y, 1.2f, dotPaint);
                    }
                }
            }
        }

        public static void DrawSegmentedBar(
            this SKCanvas canvas,
            SKRect rect,
            float progress,
            int segments,
            SKColor activeColor,
            SKColor inactiveColor,
            float gap = 2f)
        {
            progress = Math.Clamp(progress, 0f, 1f);
            float totalGap = gap * (segments - 1);
            float segWidth = (rect.Width - totalGap) / segments;
            int activeCount = (int)MathF.Round(progress * segments);

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            for (int i = 0; i < segments; i++)
            {
                float x = rect.Left + i * (segWidth + gap);
                var segRect = new SKRect(x, rect.Top, x + segWidth, rect.Bottom);
                paint.Color = i < activeCount ? activeColor : inactiveColor;
                canvas.DrawRect(segRect, paint);
            }
        }

        public static void DrawCircularGauge(
            this SKCanvas canvas,
            SKPoint center,
            float radius,
            float progress,
            float startAngle,
            float sweepAngle,
            SKColor activeColor,
            SKColor inactiveColor,
            float strokeWidth = 6f)
        {
            progress = Math.Clamp(progress, 0f, 1f);

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                StrokeCap = SKStrokeCap.Round
            };

            var oval = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);

            // Inactive track
            paint.Color = inactiveColor;
            using (var path = new SKPath())
            {
                path.AddArc(oval, startAngle, sweepAngle);
                canvas.DrawPath(path, paint);
            }

            // Active track
            if (progress > 0.001f)
            {
                paint.Color = activeColor;
                using var activePath = new SKPath();
                activePath.AddArc(oval, startAngle, sweepAngle * progress);
                canvas.DrawPath(activePath, paint);
            }
        }

        public static void DrawTechText(
            this SKCanvas canvas,
            string text,
            float x,
            float y,
            float fontSize,
            SKColor color,
            SKTextAlign align = SKTextAlign.Left,
            bool bold = false)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Color = color,
                TextSize = fontSize,
                TextAlign = align,
                Typeface = SkiaUtils.MonospaceTypeface,
                FakeBoldText = bold
            };

            canvas.DrawText(text, x, y, paint);
        }
    }
}

