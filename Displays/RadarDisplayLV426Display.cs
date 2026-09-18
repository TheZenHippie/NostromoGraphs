using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class RadarDisplayLV426Display : BaseTelemetryDisplay
    {
        public override string Title => "ORBITAL POLAR RADAR & SURFACE TOPOGRAPHY // LV-426";
        public override string Subtitle => "NOSTROMO DOWN-LOOKING RADAR SCANNER // ACOUSTIC BEACON LOCK";
        public override string SystemCode => "RAD-ACH-4260";

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;

            float radarSize = Math.Min(contentRight - contentLeft - 220f, contentBottom - contentTop);
            var radarBox = new SKRect(contentLeft, contentTop, contentLeft + radarSize, contentTop + radarSize);
            canvas.DrawTechBox(radarBox, palette.PrimaryDim, palette.DarkPanel, title: "POLAR SCANNER: LV-426 ACHERON");

            float cx = radarBox.MidX;
            float cy = radarBox.MidY;
            float radius = (radarSize - 30f) / 2f;

            // Concentric Range Rings
            using var ringPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 60),
                StrokeWidth = 1f
            };

            for (int r = 1; r <= 4; r++)
            {
                float ringRad = radius * (r / 4f);
                canvas.DrawCircle(cx, cy, ringRad, ringPaint);
                canvas.DrawTechText($"{r * 50} KM", cx + 4f, cy - ringRad + 11f, 8.5f, palette.PrimaryFaint);
            }

            // Crosshair Axes & Degree Ticks
            canvas.DrawLine(cx - radius, cy, cx + radius, cy, ringPaint);
            canvas.DrawLine(cx, cy - radius, cx, cy + radius, ringPaint);

            // Polar Degree Labels
            canvas.DrawTechText("000° [N]", cx, cy - radius - 4f, 9f, palette.Primary, SKTextAlign.Center);
            canvas.DrawTechText("090° [E]", cx + radius + 6f, cy + 3f, 9f, palette.Primary, SKTextAlign.Left);
            canvas.DrawTechText("180° [S]", cx, cy + radius + 12f, 9f, palette.Primary, SKTextAlign.Center);
            canvas.DrawTechText("270° [W]", cx - radius - 6f, cy + 3f, 9f, palette.Primary, SKTextAlign.Right);

            // Wireframe Surface Terrain Contour Ribs
            using var terrainPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 50),
                StrokeWidth = 0.8f
            };

            for (int i = -4; i <= 4; i++)
            {
                float ribY = cy + i * (radius * 0.2f);
                using var ribPath = new SKPath();
                bool first = true;
                for (float rx = cx - radius * 0.85f; rx <= cx + radius * 0.85f; rx += 10f)
                {
                    float dx = rx - cx;
                    float dist = MathF.Sqrt(dx * dx + (ribY - cy) * (ribY - cy));
                    if (dist > radius * 0.9f) continue;

                    float elevation = MathF.Sin(rx * 0.05f + i) * MathF.Cos(ribY * 0.04f + ElapsedTime * 0.5f) * 8f;
                    float ry = ribY + elevation;

                    if (first) { ribPath.MoveTo(rx, ry); first = false; }
                    else { ribPath.LineTo(rx, ry); }
                }
                canvas.DrawPath(ribPath, terrainPaint);
            }

            // Sweep Line with phosphor gradient trail
            float sweepAngle = (ElapsedTime * 60f) % 360f; // 60 deg/sec
            float radAngle = sweepAngle * MathF.PI / 180f;

            using var sweepPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WhiteBright,
                StrokeWidth = 1.6f
            };
            float sweepX = cx + MathF.Cos(radAngle) * radius;
            float sweepY = cy + MathF.Sin(radAngle) * radius;
            canvas.DrawLine(cx, cy, sweepX, sweepY, sweepPaint);

            // Sweep phosphor arc trail
            using var sweepFillPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = palette.WithAlpha(palette.Primary, 35)
            };
            var arcRect = new SKRect(cx - radius, cy - radius, cx + radius, cy + radius);
            canvas.DrawArc(arcRect, sweepAngle - 35f, 35f, true, sweepFillPaint);

            // DERELICT ACOUSTIC BEACON BLIP
            // Azimuth ~037 degrees, Range 78.4 km (radius * (78.4/200))
            float beaconDist = radius * (78.4f / 200f);
            float beaconRadAngle = (37f - 90f) * MathF.PI / 180f; // Convert 0 deg = North
            float bx = cx + MathF.Cos(beaconRadAngle) * beaconDist;
            float by = cy + MathF.Sin(beaconRadAngle) * beaconDist;

            // Pulse intensity based on sweep passing
            float angleDiff = (sweepAngle - 307f + 360f) % 360f;
            float blipAlpha = Math.Clamp(1.0f - angleDiff / 180f, 0.2f, 1.0f);

            using var blipPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = palette.WithAlpha(palette.AlertRed, (byte)(255 * blipAlpha))
            };
            canvas.DrawCircle(bx, by, 4f, blipPaint);

            // Target lock box around beacon
            var blipBox = new SKRect(bx - 10f, by - 10f, bx + 10f, by + 10f);
            canvas.DrawCornerBrackets(blipBox, palette.AlertRed, length: 5f, strokeWidth: 1.2f);
            canvas.DrawTechText("UNKNOWN BEACON", bx + 12f, by + 3f, 8.5f, palette.AlertRed, bold: true);

            // RIGHT SIDEBAR: Radar Sensor Telemetry
            var sideRect = new SKRect(radarBox.Right + 14f, contentTop, contentRight, contentBottom);
            canvas.DrawTechBox(sideRect, palette.PrimaryDim, palette.DarkPanel, title: "RADAR CONTACT LOCK");

            float ty = sideRect.Top + 28f;
            string[] contactTelemetry =
            {
                "TARGET: NON-TERRESTRIAL BEACON",
                "BEARING: 037.4° AZIMUTH",
                "SURFACE RANGE: 78.42 KM",
                "ELEVATION: -140M (CRATER VALLEY)",
                "ATMOS DENSITY: 0.89 BAR",
                "SURFACE TEMP: -42.0°C",
                "WIND SHEAR: 110 KM/H GUSTS",
                "TRANSMISSION: ACOUSTIC / EM",
                "INTERVAL: 12.0s INTERVAL",
                "CLASSIFICATION: DISTRESS / DIRECTIVE"
            };

            foreach (var line in contactTelemetry)
            {
                canvas.DrawTechText(line, sideRect.Left + 12f, ty, 9.5f, palette.Primary);
                ty += 22f;
            }

            // Radar Modulation Bar
            float barY = ty + 10f;
            canvas.DrawTechText("ECHOLOCATION GAIN:", sideRect.Left + 12f, barY, 9f, palette.PrimaryDim);
            var gainBar = new SKRect(sideRect.Left + 12f, barY + 6f, sideRect.Right - 12f, barY + 16f);
            float gainVal = 0.84f + MathF.Sin(ElapsedTime * 3f) * 0.08f;
            canvas.DrawSegmentedBar(gainBar, gainVal, 16, palette.Primary, palette.WithAlpha(palette.PrimaryDim, 40));

            var alertBox = new SKRect(sideRect.Left + 10f, sideRect.Bottom - 45f, sideRect.Right - 10f, sideRect.Bottom - 12f);
            canvas.DrawTechBox(alertBox, palette.AlertRed, palette.DarkPanel);
            canvas.DrawTechText("[ HOMING SIGNAL DETECTED ]", alertBox.MidX, alertBox.MidY + 4f, 9f, palette.AlertRed, SKTextAlign.Center, bold: true);
        }
    }
}

