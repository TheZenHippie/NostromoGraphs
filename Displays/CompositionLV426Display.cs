using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class CompositionLV426Display : BaseTelemetryDisplay
    {
        public override string Title => "GEOLOGICAL CROSS-SECTION & SEISMICITY // LV-426";
        public override string Subtitle => "PLANETARY CORE STRATIGRAPHY & SUBSURFACE ACOUSTIC SURVEY";
        public override string SystemCode => "GEO-ACH-426";

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;
            float contentWidth = contentRight - contentLeft;

            float leftColWidth = contentWidth * 0.54f;

            // LEFT PANEL: Planetary Cross-Section Cutaway
            var crossBox = new SKRect(contentLeft, contentTop, contentLeft + leftColWidth, contentBottom);
            canvas.DrawTechBox(crossBox, palette.PrimaryDim, palette.DarkPanel, title: "PLANETARY STRATA CUTAWAY");

            float cx = crossBox.MidX;
            float cy = crossBox.Top + crossBox.Height * 0.40f;
            float maxR = Math.Min(crossBox.Width, crossBox.Height) * 0.32f;

            // Concentric Geological Layers
            // 1. Crust
            using var crustPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = palette.Primary, StrokeWidth = 2f };
            canvas.DrawCircle(cx, cy, maxR, crustPaint);

            // 2. Mantle (Silicates & Basalt)
            float mantleR = maxR * 0.70f;
            using var mantlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = palette.WithAlpha(palette.PrimaryDim, 100), StrokeWidth = 1.5f };
            canvas.DrawCircle(cx, cy, mantleR, mantlePaint);

            // 3. Metallic Iron-Nickel Core
            float coreR = maxR * 0.35f;
            using var corePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = palette.WithAlpha(palette.WarningAmber, 160) };
            canvas.DrawCircle(cx, cy, coreR, corePaint);

            canvas.DrawTechText("METALLIC CORE", cx, cy + 3f, 8f, palette.Background, SKTextAlign.Center, bold: true);

            // Layer callout lines
            canvas.DrawLine(cx + mantleR * 0.7f, cy - mantleR * 0.7f, crossBox.Right - 20f, cy - maxR * 0.8f, crustPaint);
            canvas.DrawTechText("CRUST: 35 KM (SILICATE/BASALT)", crossBox.Right - 15f, cy - maxR * 0.8f, 8f, palette.Primary, SKTextAlign.Right);

            canvas.DrawLine(cx + mantleR * 0.5f, cy + mantleR * 0.5f, crossBox.Right - 20f, cy + maxR * 0.4f, mantlePaint);
            canvas.DrawTechText("MANTLE: 820 KM (HIGH-DENSITY OLIVINE)", crossBox.Right - 15f, cy + maxR * 0.4f, 8f, palette.PrimaryDim, SKTextAlign.Right);

            // Seismograph Trace at Bottom of Left Panel
            float seisTop = crossBox.Bottom - 65f;
            var seisBox = new SKRect(crossBox.Left + 10f, seisTop, crossBox.Right - 10f, crossBox.Bottom - 10f);
            canvas.DrawTechBox(seisBox, palette.PrimaryDim, palette.DarkPanel, title: "SUBSURFACE SEISMOGRAPH (RICHTER 0-8)");

            using var seisPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = palette.Primary, StrokeWidth = 1.2f };
            using var seisPath = new SKPath();
            bool first = true;
            for (float sx = seisBox.Left + 8f; sx <= seisBox.Right - 8f; sx += 2f)
            {
                float t = (sx - seisBox.Left) * 0.1f + ElapsedTime * 6f;
                // Periodic seismic tremor
                float spike = MathF.Sin(t * 0.5f) > 0.8f ? MathF.Sin(t * 8f) * 16f : MathF.Sin(t * 2f) * 3f;
                float sy = seisBox.MidY + 4f - spike;

                if (first) { seisPath.MoveTo(sx, sy); first = false; }
                else { seisPath.LineTo(sx, sy); }
            }
            canvas.DrawPath(seisPath, seisPaint);

            // RIGHT PANEL: Physical & Geological Parameters
            var sideBox = new SKRect(crossBox.Right + 12f, contentTop, contentRight, contentBottom);
            canvas.DrawTechBox(sideBox, palette.PrimaryDim, palette.DarkPanel, title: "PLANETARY CHARACTERISTICS");

            float ty = sideBox.Top + 28f;
            string[] geoStats =
            {
                "PLANETARY RADIUS: 1,200 KM",
                "SURFACE GRAVITY: 0.86 G",
                "MEAN DENSITY: 5.42 G/CM³",
                "ROTATIONAL PERIOD: 2.1 EARTH DAYS",
                "AXIAL TILT: 24.2 DEGREES",
                "VOLCANISM: MINIMAL / TECTONIC LAVA",
                "MAGNETIC FIELD: 0.18 GAUSS [WEAK]",
                "SURFACE ESCAPE VEL: 4.8 KM/S",
                "AGE OF CRUST: 4.2 BILLION YRS",
                "TECTONIC FAULTS: 14 ACTIVE RIFTS"
            };

            foreach (var stat in geoStats)
            {
                canvas.DrawTechText(stat, sideBox.Left + 12f, ty, 9f, palette.Primary);
                ty += 19f;
            }

            var lockBox = new SKRect(sideBox.Left + 10f, sideBox.Bottom - 45f, sideBox.Right - 10f, sideBox.Bottom - 10f);
            canvas.DrawTechBox(lockBox, palette.Primary, palette.DarkPanel);
            canvas.DrawTechText("[ GEOLOGICAL SURVEY COMPLETE ]", lockBox.MidX, lockBox.MidY + 4f, 9f, palette.Primary, SKTextAlign.Center, bold: true);
        }
    }
}

