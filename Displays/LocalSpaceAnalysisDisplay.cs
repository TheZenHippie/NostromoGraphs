using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class LocalSpaceAnalysisDisplay : BaseTelemetryDisplay
    {
        public override string Title => "LOCAL SPACE ORBITAL TRACKER & GRAVITY WELL";
        public override string Subtitle => "CALPAMOS SYSTEM // MOONS LV-426 & LV-223 // RING DYNAMICS";
        public override string SystemCode => "ASTRO-LOC-426";

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;
            float contentWidth = contentRight - contentLeft;

            float mapWidth = contentWidth * 0.58f;

            // LEFT PANEL: Local Orbital Map
            var orbitBox = new SKRect(contentLeft, contentTop, contentLeft + mapWidth, contentBottom);
            canvas.DrawTechBox(orbitBox, palette.PrimaryDim, palette.DarkPanel, title: "CALPAMOS SUB-SYSTEM ORBITAL SCHEMATIC");

            float cx = orbitBox.MidX;
            float cy = orbitBox.MidY;

            // Gas Giant Calpamos at Center
            using var giantPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = palette.PrimaryDim };
            canvas.DrawCircle(cx, cy, 26f, giantPaint);

            // Planetary Ring System
            using var ringPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 80),
                StrokeWidth = 1.5f
            };
            var ringRect = new SKRect(cx - 55f, cy - 14f, cx + 55f, cy + 14f);
            canvas.DrawOval(ringRect, ringPaint);

            canvas.DrawTechText("CALPAMOS (GAS GIANT)", cx, cy + 34f, 8.5f, palette.Primary, SKTextAlign.Center, bold: true);

            // Moon Orbits
            // 1. LV-223 Orbit
            float rad223 = 90f;
            float angle223 = (ElapsedTime * 15f + 120f) * MathF.PI / 180f;
            using var orbPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = palette.WithAlpha(palette.PrimaryFaint, 50), StrokeWidth = 1f };
            canvas.DrawCircle(cx, cy, rad223, orbPaint);

            float x223 = cx + MathF.Cos(angle223) * rad223;
            float y223 = cy + MathF.Sin(angle223) * rad223 * 0.6f; // Orbital tilt
            canvas.DrawCircle(x223, y223, 4f, new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = palette.CyanAccent });
            canvas.DrawTechText("LV-223", x223 + 6f, y223 + 3f, 8f, palette.CyanAccent);

            // 2. LV-426 Orbit (Acheron)
            float rad426 = 140f;
            float angle426 = (ElapsedTime * 10f + 30f) * MathF.PI / 180f;
            canvas.DrawCircle(cx, cy, rad426, orbPaint);

            float x426 = cx + MathF.Cos(angle426) * rad426;
            float y426 = cy + MathF.Sin(angle426) * rad426 * 0.6f;
            canvas.DrawCircle(x426, y426, 5f, new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = palette.AlertRed });
            canvas.DrawCrosshair(x426, y426, 10f, palette.AlertRed, 1f, circle: true);
            canvas.DrawTechText("LV-426 (ACHERON)", x426 + 8f, y426 + 3f, 8.5f, palette.AlertRed, bold: true);

            // Nostromo Orbital Insertion Vector
            using var nostromoPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = palette.WhiteBright, StrokeWidth = 1.2f, PathEffect = SKPathEffect.CreateDash(new float[] { 4f, 4f }, 0) };
            canvas.DrawLine(x426 + 25f, y426 - 35f, x426, y426, nostromoPaint);
            canvas.DrawTechText("NOSTROMO VECTOR", x426 + 28f, y426 - 35f, 8f, palette.WhiteBright);

            // RIGHT PANEL: Astrometric & Orbital Telemetry
            var sideBox = new SKRect(orbitBox.Right + 12f, contentTop, contentRight, contentBottom);
            canvas.DrawTechBox(sideBox, palette.PrimaryDim, palette.DarkPanel, title: "GRAVITATIONAL DATA");

            float ty = sideBox.Top + 28f;
            string[] orbTelemetry =
            {
                "PRIMARY BODY: CALPAMOS",
                "CLASSIFICATION: CLASS IV GAS GIANT",
                "RADIUS: 68,400 KM",
                "SURFACE GRAVITY: 2.35 G",
                "SATELLITE 1: LV-426 (ACHERON)",
                "  - ORBITAL RADIUS: 1.22M KM",
                "  - PERIOD: 14.8 DAYS",
                "  - MASS: 0.082 EARTH MASS",
                "SATELLITE 2: LV-223",
                "  - ORBITAL RADIUS: 780K KM",
                "RING SYSTEM: SILICATE ICE / DUST",
                "TIDAL STRESS: HIGH ACCELERATION"
            };

            foreach (var line in orbTelemetry)
            {
                canvas.DrawTechText(line, sideBox.Left + 12f, ty, 8.5f, palette.Primary);
                ty += 17f;
            }

            // Proximity Warning Alert
            var proxBox = new SKRect(sideBox.Left + 10f, sideBox.Bottom - 45f, sideBox.Right - 10f, sideBox.Bottom - 10f);
            canvas.DrawTechBox(proxBox, palette.Primary, palette.DarkPanel);
            canvas.DrawTechText("[ ORBITAL INSERTION CONFIRMED ]", proxBox.MidX, proxBox.MidY + 4f, 9f, palette.Primary, SKTextAlign.Center, bold: true);
        }
    }
}

