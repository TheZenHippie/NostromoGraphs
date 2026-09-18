using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class StellarCartographyDisplay : BaseTelemetryDisplay
    {
        public override string Title => "STELLAR CARTOGRAPHY & SECTOR DENSITY MAP";
        public override string Subtitle => "GALACTIC SECTOR 04 // RETICULI-THE DUS INTERSTELLAR CORRIDOR";
        public override string SystemCode => "CART-SEC-0491";

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;

            float mapWidth = contentRight - contentLeft - 220f;
            var mapBox = new SKRect(contentLeft, contentTop, contentLeft + mapWidth, contentBottom);
            canvas.DrawTechBox(mapBox, palette.PrimaryDim, palette.DarkPanel, title: "SECTOR 04 ASTROMETRIC GRID");

            // Draw Coordinate Grid with Declination and Right Ascension
            canvas.DrawGrid(mapBox, 40f, 40f, palette.WithAlpha(palette.PrimaryDim, 35));

            float cx = mapBox.MidX;
            float cy = mapBox.MidY;

            // Concentric Sector Lanes
            using var lanePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 60),
                StrokeWidth = 1f,
                PathEffect = SKPathEffect.CreateDash(new float[] { 6f, 4f }, 0)
            };

            for (int r = 1; r <= 3; r++)
            {
                float rad = r * 65f;
                canvas.DrawCircle(cx, cy, rad, lanePaint);
            }

            // Orbital vector paths & nodes
            (string Name, float Angle, float Dist, SKColor Color)[] systems =
            {
                ("SOL SYSTEM", 220f, 150f, palette.Primary),
                ("ZETA II RETICULI", 45f, 110f, palette.AlertRed),
                ("THEDUS STATION", 130f, 175f, palette.WarningAmber),
                ("GLIESE 667", 310f, 85f, palette.CyanAccent),
                ("EPSILON ERIDANI", 175f, 130f, palette.PrimaryDim)
            };

            using var linkPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 70),
                StrokeWidth = 1.2f
            };

            for (int i = 0; i < systems.Length; i++)
            {
                var sys = systems[i];
                float radAngle = (sys.Angle + ElapsedTime * 2f) * MathF.PI / 180f;
                float sx = cx + MathF.Cos(radAngle) * sys.Dist;
                float sy = cy + MathF.Sin(radAngle) * sys.Dist;

                if (!mapBox.Contains(sx, sy)) continue;

                // Line to center
                canvas.DrawLine(cx, cy, sx, sy, linkPaint);

                // Node circle
                using var nodePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = sys.Color };
                canvas.DrawCircle(sx, sy, 4.5f, nodePaint);
                canvas.DrawCrosshair(sx, sy, 10f, palette.PrimaryDim, 1f, circle: true);

                canvas.DrawTechText(sys.Name, sx + 8f, sy + 3f, 9f, sys.Color, bold: true);
            }

            // Center Astrometric Core
            canvas.DrawCrosshair(cx, cy, 16f, palette.Primary, 1.5f, circle: true);
            canvas.DrawTechText("CORRIDOR APEX", cx, cy - 14f, 8.5f, palette.Primary, SKTextAlign.Center);

            // RIGHT SIDEBAR: Density & Coordinates Telemetry
            var sideRect = new SKRect(mapBox.Right + 14f, contentTop, contentRight, contentBottom);
            canvas.DrawTechBox(sideRect, palette.PrimaryDim, palette.DarkPanel, title: "CARTOGRAPHY DATA");

            float ty = sideRect.Top + 28f;
            string[] cartData =
            {
                "SECTOR: ZETA-04 BRAVO",
                "GALACTIC LONG: 278.42°",
                "GALACTIC LAT: -32.18°",
                "STAR DENSITY: 0.042 / PC³",
                "INTERSTELLAR EXTINCTION: 0.01",
                "SOLAR MASS INDEX: 1.04 M☉",
                "GRAVITATIONAL SHEAR: NOMINAL",
                "NEBULAR DRIFT: NEGATIVE",
                "COMM ROUTE: WEY-YUT COMMERCIAL",
                "HAZARD LEVEL: UNCHARTED CLASS D"
            };

            foreach (var line in cartData)
            {
                canvas.DrawTechText(line, sideRect.Left + 12f, ty, 9.5f, palette.Primary);
                ty += 22f;
            }

            // Gyro Compass
            float gyroY = sideRect.Bottom - 60f;
            var gyroBox = new SKRect(sideRect.Left + 10f, gyroY, sideRect.Right - 10f, sideRect.Bottom - 12f);
            canvas.DrawTechBox(gyroBox, palette.Primary, palette.DarkPanel);

            float gcx = gyroBox.MidX;
            float gcy = gyroBox.MidY;
            float gRad = 16f;
            float compassAngle = ElapsedTime * 20f * MathF.PI / 180f;

            using var gPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = palette.Primary, StrokeWidth = 1.2f };
            canvas.DrawCircle(gcx, gcy, gRad, gPaint);
            canvas.DrawLine(gcx, gcy, gcx + MathF.Cos(compassAngle) * gRad, gcy + MathF.Sin(compassAngle) * gRad, gPaint);
            canvas.DrawTechText("GYRO STABILIZED", gcx, gyroBox.Top + 10f, 8f, palette.PrimaryDim, SKTextAlign.Center);
        }
    }
}

