using System;
using System.Collections.Generic;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class StellarPlotToEarthDisplay : BaseTelemetryDisplay
    {
        public override string Title => "STELLAR ASTROGATION PLOT // INTERSTELLAR TRAJECTORY";
        public override string Subtitle => "ZETA II RETICULI [LV-426] -> SOL [EARTH] // SECTOR 4 CORRIDOR";
        public override string SystemCode => "ASTRO-PLT-8821";

        private struct StarPoint
        {
            public Vector3D Pos;
            public string? Label;
            public float Size;
            public bool IsKeyWaypoint;
        }

        private readonly List<StarPoint> _stars = new List<StarPoint>();

        public override void Initialize()
        {
            base.Initialize();
            _stars.Clear();

            // Key Waypoints
            _stars.Add(new StarPoint { Pos = new Vector3D(-140f, 60f, -80f), Label = "LV-426 (ACHERON)", Size = 4f, IsKeyWaypoint = true });
            _stars.Add(new StarPoint { Pos = new Vector3D(-90f, 40f, -40f), Label = "ZETA II RETICULI", Size = 5f, IsKeyWaypoint = true });
            _stars.Add(new StarPoint { Pos = new Vector3D(-20f, 10f, 10f), Label = "WAYPOINT EPSILON", Size = 3f, IsKeyWaypoint = true });
            _stars.Add(new StarPoint { Pos = new Vector3D(50f, -20f, 60f), Label = "GLIESE 667 SECTOR", Size = 3.5f, IsKeyWaypoint = true });
            _stars.Add(new StarPoint { Pos = new Vector3D(130f, -50f, 110f), Label = "SOL (EARTH / REACH)", Size = 5.5f, IsKeyWaypoint = true });

            // Background Deep Space Starfield
            var rand = new Random(42);
            for (int i = 0; i < 48; i++)
            {
                float x = (rand.NextSingle() - 0.5f) * 360f;
                float y = (rand.NextSingle() - 0.5f) * 260f;
                float z = (rand.NextSingle() - 0.5f) * 260f;
                _stars.Add(new StarPoint { Pos = new Vector3D(x, y, z), Label = null, Size = 1.2f + rand.NextSingle() * 1.5f, IsKeyWaypoint = false });
            }
        }

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;

            // Main Starfield Plot Box
            float plotRight = contentRight - 220f;
            var plotRect = new SKRect(contentLeft, contentTop, plotRight, contentBottom);
            canvas.DrawTechBox(plotRect, palette.PrimaryDim, palette.DarkPanel, title: "3D ASTROGATION VECTOR GRID");

            // Rotation angle over time
            float rotY = ElapsedTime * 0.18f;
            float rotX = 0.25f + MathF.Sin(ElapsedTime * 0.1f) * 0.08f;

            // Draw 3D Grid Plane (Galactic Ecliptic Plane)
            using var gridPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 40),
                StrokeWidth = 0.8f
            };

            for (float gx = -160f; gx <= 160f; gx += 40f)
            {
                var p1 = new Vector3D(gx, 50f, -160f).RotateX(rotX).RotateY(rotY).Project(plotRect.Width, plotRect.Height, 380f, 280f);
                var p2 = new Vector3D(gx, 50f, 160f).RotateX(rotX).RotateY(rotY).Project(plotRect.Width, plotRect.Height, 380f, 280f);
                canvas.DrawLine(plotRect.Left + p1.X, plotRect.Top + p1.Y, plotRect.Left + p2.X, plotRect.Top + p2.Y, gridPaint);
            }

            for (float gz = -160f; gz <= 160f; gz += 40f)
            {
                var p1 = new Vector3D(-160f, 50f, gz).RotateX(rotX).RotateY(rotY).Project(plotRect.Width, plotRect.Height, 380f, 280f);
                var p2 = new Vector3D(160f, 50f, gz).RotateX(rotX).RotateY(rotY).Project(plotRect.Width, plotRect.Height, 380f, 280f);
                canvas.DrawLine(plotRect.Left + p1.X, plotRect.Top + p1.Y, plotRect.Left + p2.X, plotRect.Top + p2.Y, gridPaint);
            }

            // Draw Projected Trajectory Line between Key Waypoints
            var keyPoints = new List<SKPoint>();
            for (int i = 0; i < 5; i++)
            {
                var proj = _stars[i].Pos.RotateX(rotX).RotateY(rotY).Project(plotRect.Width, plotRect.Height, 380f, 280f);
                keyPoints.Add(new SKPoint(plotRect.Left + proj.X, plotRect.Top + proj.Y));
            }

            using var trajPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.Primary, 180),
                StrokeWidth = 1.8f
            };

            for (int i = 0; i < keyPoints.Count - 1; i++)
            {
                canvas.DrawLine(keyPoints[i], keyPoints[i + 1], trajPaint);
            }

            // Traveling Pulse on Trajectory
            float pulseT = (ElapsedTime * 0.4f) % (keyPoints.Count - 1);
            int segIndex = (int)pulseT;
            float segFraction = pulseT - segIndex;
            var ptA = keyPoints[segIndex];
            var ptB = keyPoints[segIndex + 1];
            float px = SkiaUtils.Lerp(ptA.X, ptB.X, segFraction);
            float py = SkiaUtils.Lerp(ptA.Y, ptB.Y, segFraction);

            using var pulsePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = palette.WhiteBright
            };
            canvas.DrawCircle(px, py, 4f, pulsePaint);
            canvas.DrawCircle(px, py, 8f, new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = palette.Primary, StrokeWidth = 1f });

            // Draw Stars & Waypoints
            for (int i = 0; i < _stars.Count; i++)
            {
                var star = _stars[i];
                var proj = star.Pos.RotateX(rotX).RotateY(rotY).Project(plotRect.Width, plotRect.Height, 380f, 280f);
                float sx = plotRect.Left + proj.X;
                float sy = plotRect.Top + proj.Y;

                if (!plotRect.Contains(sx, sy)) continue;

                if (star.IsKeyWaypoint)
                {
                    using var starPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = palette.Primary };
                    canvas.DrawCircle(sx, sy, star.Size, starPaint);
                    canvas.DrawCrosshair(sx, sy, star.Size * 2.2f, palette.PrimaryDim, 1f, circle: true);

                    if (!string.IsNullOrEmpty(star.Label))
                    {
                        canvas.DrawTechText(star.Label, sx + 8f, sy + 3f, 9.5f, palette.Primary, bold: true);
                    }
                }
                else
                {
                    using var bgStarPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = palette.WithAlpha(palette.PrimaryDim, 140) };
                    canvas.DrawCircle(sx, sy, star.Size, bgStarPaint);
                }
            }

            // RIGHT SIDEBAR: Telemetry Readouts
            var sideRect = new SKRect(plotRight + 12f, contentTop, contentRight, contentBottom);
            canvas.DrawTechBox(sideRect, palette.PrimaryDim, palette.DarkPanel, title: "ASTROGATION TELEMETRY");

            float ty = sideRect.Top + 28f;
            string[] telemetries =
            {
                "DESTINATION: SOL // TERRA",
                "CURRENT SECTOR: 04-RETICULI",
                "DISTANCE REMAINING: 38.4 LY",
                "DISTANCE TRAVELED: 14.2 LY",
                "CRUISING VELOCITY: 0.42 LY/D",
                "FTL DRIVE FLUX: 99.8% STABLE",
                "RELATIVISTIC ETA: 91.4 DAYS",
                "EARTH CALENDAR ETA: 2122-10-12",
                "DECEL WAYPOINT: EPSILON-4",
                "PARALLAX DRIFT: 0.0024 ARCSEC"
            };

            foreach (var item in telemetries)
            {
                canvas.DrawTechText(item, sideRect.Left + 12f, ty, 9.5f, palette.Primary);
                ty += 22f;
            }

            // Target lock box at bottom of sidebar
            var lockBox = new SKRect(sideRect.Left + 10f, ty + 10f, sideRect.Right - 10f, sideRect.Bottom - 12f);
            canvas.DrawTechBox(lockBox, palette.Primary, palette.DarkPanel);
            canvas.DrawTechText("[ TRAJECTORY LOCKED ]", lockBox.MidX, lockBox.MidY - 2f, 10f, palette.Primary, SKTextAlign.Center, bold: true);
            canvas.DrawTechText("COMMERCIAL TOWING ROUTE 9", lockBox.MidX, lockBox.MidY + 12f, 8.5f, palette.PrimaryDim, SKTextAlign.Center);
        }
    }
}

