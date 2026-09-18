using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public abstract class BaseTelemetryDisplay : ITelemetryDisplay
    {
        public abstract string Title { get; }
        public abstract string Subtitle { get; }
        public abstract string SystemCode { get; }

        protected float ElapsedTime { get; private set; }
        protected readonly Random Rand = new Random();

        public virtual void Initialize()
        {
            ElapsedTime = 0f;
        }

        public virtual void Update(float deltaTime)
        {
            ElapsedTime += deltaTime;
        }

        public abstract void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette);

        protected void DrawStandardHeader(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            // Outer technical frame with corner notches
            canvas.DrawCornerBrackets(bounds, palette.PrimaryDim, length: 14f, strokeWidth: 1.5f);

            // Sub-header title
            float topY = bounds.Top + 18f;
            canvas.DrawTechText($"// MUTHUR 6000 TELEMETRY FEED: {Title}", bounds.Left + 16f, topY, 12f, palette.Primary, bold: true);
            canvas.DrawTechText($"SEC: {SystemCode} // {Subtitle}", bounds.Left + 16f, topY + 14f, 9.5f, palette.PrimaryDim);

            // Timecode & status indicator on top right
            string timeStr = $"T+{(int)ElapsedTime / 60:D2}:{(int)ElapsedTime % 60:D2}.{(int)((ElapsedTime * 10) % 10)}";
            canvas.DrawTechText(timeStr, bounds.Right - 16f, topY, 11f, palette.Primary, SKTextAlign.Right, bold: true);
            canvas.DrawTechText("LIVE STREAM [NOMINAL]", bounds.Right - 16f, topY + 14f, 9.5f, palette.PrimaryDim, SKTextAlign.Right);

            // Horizontal dividing rule
            using var rulePaint = new SKPaint
            {
                IsAntialias = true,
                Color = palette.PrimaryFaint,
                StrokeWidth = 1f
            };
            canvas.DrawLine(bounds.Left + 14f, topY + 22f, bounds.Right - 14f, topY + 22f, rulePaint);
        }
    }
}

