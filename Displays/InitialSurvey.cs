using System;
using System.Collections.Generic;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class InitialSurvey : BaseTelemetryDisplay
    {
        public override string Title => "PRELIMINARY SURVEY & SURFACE EXPEDITION // LV-426";
        public override string Subtitle => "USCSS NOSTROMO // EVA SURVEY PARTY: DALLAS, KANE, LAMBERT";
        public override string SystemCode => "SURV-LV426-03";

        // Normalized Waypoints for the EVA Trek from Nostromo Landing Site to Position 2 (Derelict)
        private static readonly SKPoint[] PathWaypoints = new SKPoint[]
        {
            new SKPoint(0.035f, 0.965f), // Nostromo Landing Site
            new SKPoint(0.080f, 0.930f),
            new SKPoint(0.130f, 0.895f),
            new SKPoint(0.185f, 0.855f),
            new SKPoint(0.240f, 0.810f),
            new SKPoint(0.285f, 0.770f),
            new SKPoint(0.315f, 0.725f), // Weaving around western ridge
            new SKPoint(0.340f, 0.695f),
            new SKPoint(0.385f, 0.680f),
            new SKPoint(0.440f, 0.665f), // Southern pass along Position 1
            new SKPoint(0.505f, 0.655f),
            new SKPoint(0.570f, 0.650f),
            new SKPoint(0.630f, 0.645f),
            new SKPoint(0.655f, 0.670f), // Turning around eastern spur of Position 1
            new SKPoint(0.675f, 0.640f),
            new SKPoint(0.685f, 0.585f), // Canyon corridor heading north
            new SKPoint(0.680f, 0.520f),
            new SKPoint(0.690f, 0.460f),
            new SKPoint(0.725f, 0.435f), // Northeast bend toward Position 2
            new SKPoint(0.765f, 0.420f),
            new SKPoint(0.800f, 0.395f),
            new SKPoint(0.815f, 0.345f),
            new SKPoint(0.810f, 0.295f)  // Position 2: Derelict inside horseshoe outcrop
        };

        // Topographic contour polygon paths
        private readonly List<List<SKPoint>> _contours = new List<List<SKPoint>>();

        public override void Initialize()
        {
            base.Initialize();
            GenerateMapContours();
        }

        private void GenerateMapContours()
        {
            _contours.Clear();

            // 1. POSITION 1 (Central mountain/crater massif)
            // Outer base ring
            AddContour(new[]
            {
                new SKPoint(0.52f, 0.52f), new SKPoint(0.56f, 0.46f), new SKPoint(0.61f, 0.44f),
                new SKPoint(0.64f, 0.48f), new SKPoint(0.66f, 0.54f), new SKPoint(0.67f, 0.62f),
                new SKPoint(0.64f, 0.70f), new SKPoint(0.61f, 0.77f), new SKPoint(0.58f, 0.81f),
                new SKPoint(0.55f, 0.74f), new SKPoint(0.52f, 0.67f), new SKPoint(0.50f, 0.59f)
            }, isClosed: true);

            // Middle crater ring
            AddContour(new[]
            {
                new SKPoint(0.55f, 0.55f), new SKPoint(0.58f, 0.50f), new SKPoint(0.62f, 0.52f),
                new SKPoint(0.64f, 0.58f), new SKPoint(0.63f, 0.65f), new SKPoint(0.59f, 0.72f),
                new SKPoint(0.55f, 0.66f), new SKPoint(0.53f, 0.60f)
            }, isClosed: true);

            // Inner crater peak
            AddContour(new[]
            {
                new SKPoint(0.57f, 0.58f), new SKPoint(0.60f, 0.55f), new SKPoint(0.62f, 0.60f),
                new SKPoint(0.60f, 0.66f), new SKPoint(0.57f, 0.63f)
            }, isClosed: true);

            // Southern spur of Position 1
            AddContour(new[]
            {
                new SKPoint(0.56f, 0.75f), new SKPoint(0.58f, 0.84f), new SKPoint(0.62f, 0.88f),
                new SKPoint(0.64f, 0.83f), new SKPoint(0.65f, 0.73f)
            }, isClosed: false);

            // 2. POSITION 2 & HORSESHOE ROCKY OUTCROP (Upper right)
            // Outermost ridge
            AddContour(new[]
            {
                new SKPoint(0.68f, 0.08f), new SKPoint(0.72f, 0.12f), new SKPoint(0.75f, 0.18f),
                new SKPoint(0.77f, 0.24f), new SKPoint(0.79f, 0.32f), new SKPoint(0.83f, 0.35f),
                new SKPoint(0.85f, 0.28f), new SKPoint(0.83f, 0.21f), new SKPoint(0.81f, 0.14f),
                new SKPoint(0.78f, 0.08f), new SKPoint(0.74f, 0.04f)
            }, isClosed: false);

            // Intermediate ridge canyon
            AddContour(new[]
            {
                new SKPoint(0.72f, 0.08f), new SKPoint(0.75f, 0.15f), new SKPoint(0.77f, 0.20f),
                new SKPoint(0.78f, 0.26f), new SKPoint(0.81f, 0.26f), new SKPoint(0.82f, 0.19f),
                new SKPoint(0.79f, 0.12f), new SKPoint(0.76f, 0.06f)
            }, isClosed: false);

            // Horseshoe / U-shaped Rocky Outcrop surrounding the Derelict at Position 2
            AddContour(new[]
            {
                new SKPoint(0.795f, 0.260f), new SKPoint(0.785f, 0.285f), new SKPoint(0.790f, 0.325f),
                new SKPoint(0.820f, 0.335f), new SKPoint(0.840f, 0.315f), new SKPoint(0.845f, 0.275f),
                new SKPoint(0.830f, 0.255f), new SKPoint(0.810f, 0.255f)
            }, isClosed: false);

            // Inner cradle ring of outcrop
            AddContour(new[]
            {
                new SKPoint(0.800f, 0.275f), new SKPoint(0.795f, 0.295f), new SKPoint(0.810f, 0.315f),
                new SKPoint(0.825f, 0.310f), new SKPoint(0.830f, 0.280f)
            }, isClosed: false);

            // 3. WESTERN / NORTHWESTERN RIDGES
            AddContour(new[]
            {
                new SKPoint(0.04f, 0.12f), new SKPoint(0.12f, 0.10f), new SKPoint(0.18f, 0.14f),
                new SKPoint(0.25f, 0.11f), new SKPoint(0.32f, 0.16f), new SKPoint(0.38f, 0.13f),
                new SKPoint(0.44f, 0.18f), new SKPoint(0.48f, 0.14f)
            }, isClosed: false);

            AddContour(new[]
            {
                new SKPoint(0.06f, 0.22f), new SKPoint(0.14f, 0.20f), new SKPoint(0.20f, 0.26f),
                new SKPoint(0.28f, 0.22f), new SKPoint(0.35f, 0.28f), new SKPoint(0.42f, 0.24f),
                new SKPoint(0.46f, 0.30f)
            }, isClosed: false);

            AddContour(new[]
            {
                new SKPoint(0.08f, 0.34f), new SKPoint(0.16f, 0.32f), new SKPoint(0.22f, 0.38f),
                new SKPoint(0.30f, 0.35f), new SKPoint(0.36f, 0.42f), new SKPoint(0.45f, 0.38f)
            }, isClosed: false);

            // 4. SOUTHWESTERN RIDGES (Around Nostromo Approach Corridor)
            AddContour(new[]
            {
                new SKPoint(0.04f, 0.48f), new SKPoint(0.10f, 0.45f), new SKPoint(0.16f, 0.52f),
                new SKPoint(0.24f, 0.48f), new SKPoint(0.30f, 0.56f), new SKPoint(0.38f, 0.52f)
            }, isClosed: false);

            AddContour(new[]
            {
                new SKPoint(0.05f, 0.62f), new SKPoint(0.12f, 0.58f), new SKPoint(0.18f, 0.66f),
                new SKPoint(0.26f, 0.62f), new SKPoint(0.32f, 0.70f)
            }, isClosed: false);

            AddContour(new[]
            {
                new SKPoint(0.04f, 0.78f), new SKPoint(0.11f, 0.74f), new SKPoint(0.18f, 0.80f),
                new SKPoint(0.25f, 0.76f)
            }, isClosed: false);

            // Small isolated plateau hill at (0.20, 0.68)
            AddContour(new[]
            {
                new SKPoint(0.18f, 0.66f), new SKPoint(0.22f, 0.65f), new SKPoint(0.24f, 0.69f),
                new SKPoint(0.21f, 0.72f), new SKPoint(0.17f, 0.70f)
            }, isClosed: true);

            // 5. EASTERN & SOUTHEASTERN CRAGS
            AddContour(new[]
            {
                new SKPoint(0.72f, 0.72f), new SKPoint(0.75f, 0.68f), new SKPoint(0.78f, 0.74f),
                new SKPoint(0.82f, 0.70f), new SKPoint(0.86f, 0.78f), new SKPoint(0.92f, 0.74f),
                new SKPoint(0.96f, 0.82f)
            }, isClosed: false);

            AddContour(new[]
            {
                new SKPoint(0.76f, 0.84f), new SKPoint(0.80f, 0.80f), new SKPoint(0.84f, 0.88f),
                new SKPoint(0.88f, 0.82f), new SKPoint(0.94f, 0.90f)
            }, isClosed: false);

            AddContour(new[]
            {
                new SKPoint(0.84f, 0.44f), new SKPoint(0.88f, 0.40f), new SKPoint(0.92f, 0.48f),
                new SKPoint(0.96f, 0.42f)
            }, isClosed: false);

            AddContour(new[]
            {
                new SKPoint(0.86f, 0.56f), new SKPoint(0.90f, 0.52f), new SKPoint(0.94f, 0.60f),
                new SKPoint(0.97f, 0.54f)
            }, isClosed: false);

            // Bottom edge contour
            AddContour(new[]
            {
                new SKPoint(0.34f, 0.94f), new SKPoint(0.42f, 0.88f), new SKPoint(0.50f, 0.95f),
                new SKPoint(0.60f, 0.90f), new SKPoint(0.70f, 0.96f)
            }, isClosed: false);
        }

        private void AddContour(SKPoint[] points, bool isClosed)
        {
            var list = new List<SKPoint>(points);
            _contours.Add(list);
        }

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            // Compute screen framing matching the movie CRT layout
            float margin = 8f;
            var outerFrame = new SKRect(bounds.Left + margin, bounds.Top + margin, bounds.Right - margin, bounds.Bottom - margin);

            // Top Header Bar
            float headerH = 26f;
            var headerRect = new SKRect(outerFrame.Left, outerFrame.Top, outerFrame.Right, outerFrame.Top + headerH);
            var mapRect = new SKRect(outerFrame.Left, headerRect.Bottom, outerFrame.Right, outerFrame.Bottom);

            // Outer cyan border & header line
            SKColor cyanBorder = palette.IsMonochrome ? palette.Primary : palette.CyanAccent;
            using var borderPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = cyanBorder,
                StrokeWidth = 2.0f
            };
            canvas.DrawRect(outerFrame, borderPaint);
            canvas.DrawLine(headerRect.Left, headerRect.Bottom, headerRect.Right, headerRect.Bottom, borderPaint);

            // Header Text: "PRELIMINARY SURVEY - USCSS NOSTROMO - 3 JUN 2122"
            canvas.DrawTechText("PRELIMINARY SURVEY - USCSS NOSTROMO - 3 JUN 2122", headerRect.Left + 12f, headerRect.MidY + 4f, 10f, cyanBorder, bold: true);

            // 1. Draw Topographic Terrain Contours (Red Vector Lines)
            DrawTerrainContours(canvas, mapRect, palette);

            // 2. Draw Curved Perspective Coordinate Grid Overlay (Cyan)
            DrawCurvedPerspectiveGrid(canvas, mapRect, cyanBorder);

            // 3. Draw Position 1 & Position 2 Labels and Outcrop Detail
            DrawPositionMarkersAndDerelict(canvas, mapRect, cyanBorder, palette);

            // 4. Draw Animated EVA Tracking of Dallas, Kane, and Lambert
            DrawEvaTrackingPath(canvas, mapRect, cyanBorder, palette);
        }

        private void DrawTerrainContours(SKCanvas canvas, SKRect mapRect, ColorPalette palette)
        {
            // Authentic movie color: Vivid Crimson/Red phosphor contours
            SKColor contourColor = palette.IsMonochrome ? palette.Primary : palette.AlertRed;

            using var contourPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = contourColor,
                StrokeWidth = 1.4f,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeCap = SKStrokeCap.Round
            };

            foreach (var contour in _contours)
            {
                if (contour.Count < 2) continue;

                using var path = new SKPath();
                var p0 = ToScreen(contour[0], mapRect);
                path.MoveTo(p0);

                for (int i = 1; i < contour.Count; i++)
                {
                    var p = ToScreen(contour[i], mapRect);
                    path.LineTo(p);
                }

                if (contour.Count > 2)
                {
                    // If closed or near-closed
                    var first = ToScreen(contour[0], mapRect);
                    var last = ToScreen(contour[contour.Count - 1], mapRect);
                    if (SKPoint.Distance(first, last) < 20f)
                    {
                        path.Close();
                    }
                }

                canvas.DrawPath(path, contourPaint);
            }
        }

        private void DrawCurvedPerspectiveGrid(SKCanvas canvas, SKRect mapRect, SKColor gridColor)
        {
            using var gridPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = gridColor,
                StrokeWidth = 1.6f
            };

            // Transverse perspective curves (Upper-Left to Lower-Right)
            // Curve 1
            DrawBezier(canvas, mapRect, new SKPoint(-0.02f, 0.22f), new SKPoint(0.18f, 0.60f), new SKPoint(0.40f, 1.02f), gridPaint);
            // Curve 2
            DrawBezier(canvas, mapRect, new SKPoint(0.28f, -0.02f), new SKPoint(0.52f, 0.52f), new SKPoint(0.82f, 1.02f), gridPaint);
            // Curve 3
            DrawBezier(canvas, mapRect, new SKPoint(0.68f, -0.02f), new SKPoint(0.82f, 0.48f), new SKPoint(0.98f, 1.02f), gridPaint);

            // Longitudinal perspective curves (Lower-Left to Upper-Right)
            // Curve A (Lower)
            DrawBezier(canvas, mapRect, new SKPoint(-0.02f, 0.95f), new SKPoint(0.50f, 0.82f), new SKPoint(1.02f, 0.62f), gridPaint);
            // Curve B (Middle-Lower)
            DrawBezier(canvas, mapRect, new SKPoint(-0.02f, 0.74f), new SKPoint(0.48f, 0.56f), new SKPoint(1.02f, 0.32f), gridPaint);
            // Curve C (Middle-Upper)
            DrawBezier(canvas, mapRect, new SKPoint(-0.02f, 0.52f), new SKPoint(0.38f, 0.26f), new SKPoint(0.72f, -0.02f), gridPaint);
            // Curve D (Top-Left corner)
            DrawBezier(canvas, mapRect, new SKPoint(-0.02f, 0.20f), new SKPoint(0.16f, 0.08f), new SKPoint(0.34f, -0.02f), gridPaint);
        }

        private void DrawBezier(SKCanvas canvas, SKRect mapRect, SKPoint startNorm, SKPoint ctrlNorm, SKPoint endNorm, SKPaint paint)
        {
            var p0 = ToScreen(startNorm, mapRect);
            var p1 = ToScreen(ctrlNorm, mapRect);
            var p2 = ToScreen(endNorm, mapRect);

            using var path = new SKPath();
            path.MoveTo(p0);
            path.QuadTo(p1, p2);
            canvas.DrawPath(path, paint);
        }

        private void DrawPositionMarkersAndDerelict(SKCanvas canvas, SKRect mapRect, SKColor cyanColor, ColorPalette palette)
        {
            // 1. POSITION 1
            var pos1 = ToScreen(new SKPoint(0.55f, 0.55f), mapRect);
            canvas.DrawTechText("POSITION 1", pos1.X, pos1.Y, 9.5f, cyanColor, SKTextAlign.Left, bold: true);

            // 2. POSITION 2 & HORSESHOE OUTCROP
            var pos2Text = ToScreen(new SKPoint(0.68f, 0.30f), mapRect);
            canvas.DrawTechText("POSITION 2", pos2Text.X, pos2Text.Y, 9.5f, cyanColor, SKTextAlign.Left, bold: true);

            // DERELICT SPACECRAFT INSIDE OUTCROP at (0.815, 0.295)
            var derelictPos = ToScreen(new SKPoint(0.815f, 0.295f), mapRect);

            // Pulsing Acoustic Signal Waves from Derelict
            float pulseT = (ElapsedTime * 1.5f) % 1.0f;
            float pulseRad = 6f + pulseT * 18f;
            byte pulseAlpha = (byte)(255 * (1f - pulseT));

            SKColor pulseColor = palette.IsMonochrome ? palette.Primary : palette.AlertRed;
            using var beaconPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(pulseColor, pulseAlpha),
                StrokeWidth = 1.2f
            };
            canvas.DrawCircle(derelictPos.X, derelictPos.Y, pulseRad, beaconPaint);

            // Derelict Silhouette (Iconic horseshoe Juggernaut wireframe)
            using var derelictPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.IsMonochrome ? palette.Primary : palette.WarningAmber,
                StrokeWidth = 1.3f
            };

            using var jPath = new SKPath();
            float dx = derelictPos.X;
            float dy = derelictPos.Y;
            float scale = 7f;
            // Horseshoe crescent
            jPath.MoveTo(dx - 1.2f * scale, dy - 0.8f * scale);
            jPath.QuadTo(dx, dy - 1.5f * scale, dx + 1.2f * scale, dy - 0.8f * scale);
            jPath.LineTo(dx + 1.5f * scale, dy + 0.6f * scale);
            jPath.LineTo(dx + 0.8f * scale, dy + 1.0f * scale);
            jPath.LineTo(dx + 0.4f * scale, dy + 0.2f * scale);
            jPath.LineTo(dx - 0.4f * scale, dy + 0.2f * scale);
            jPath.LineTo(dx - 0.8f * scale, dy + 1.0f * scale);
            jPath.LineTo(dx - 1.5f * scale, dy + 0.6f * scale);
            jPath.Close();
            canvas.DrawPath(jPath, derelictPaint);

            // 3. NOSTROMO LANDING SITE in Bottom-Left Corner
            var nostromoPos = ToScreen(new SKPoint(0.045f, 0.955f), mapRect);
            using var shipPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = cyanColor,
                StrokeWidth = 1.2f
            };
            // Triangle landing footprint
            using var nPath = new SKPath();
            nPath.MoveTo(nostromoPos.X, nostromoPos.Y - 10f);
            nPath.LineTo(nostromoPos.X + 10f, nostromoPos.Y + 6f);
            nPath.LineTo(nostromoPos.X - 10f, nostromoPos.Y + 6f);
            nPath.Close();
            canvas.DrawPath(nPath, shipPaint);
            canvas.DrawTechText("NOSTROMO LZ", nostromoPos.X + 14f, nostromoPos.Y + 2f, 7.5f, cyanColor);
        }

        private void DrawEvaTrackingPath(SKCanvas canvas, SKRect mapRect, SKColor cyanColor, ColorPalette palette)
        {
            // Slower, atmospheric EVA Trek Animation over 60-second operational loop
            float cycleDuration = 60f;
            float t = ElapsedTime % cycleDuration;
            float trekDuration = 46f; // ~46s for full trek to Derelict, ~14s hold at Derelict
            float progress = Math.Clamp(t / trekDuration, 0f, 1.0f);

            int totalWaypoints = PathWaypoints.Length;
            float maxIndex = totalWaypoints - 1;

            // Trail draws along with the team
            float exactTrailIndex = progress * maxIndex;
            int maxDrawnIndex = (int)exactTrailIndex;

            using var dotPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = cyanColor
            };

            // Draw cyan dotted trail up to current explored point
            for (int i = 0; i <= maxDrawnIndex; i++)
            {
                var pt = ToScreen(PathWaypoints[i], mapRect);
                float dotRadius = (i == maxDrawnIndex && progress < 1.0f) ? 3.0f : 2.0f;
                canvas.DrawCircle(pt.X, pt.Y, dotRadius, dotPaint);
            }

            // Draw fractional step for smooth trail extension
            if (maxDrawnIndex < totalWaypoints - 1)
            {
                float frac = exactTrailIndex - maxDrawnIndex;
                var pA = ToScreen(PathWaypoints[maxDrawnIndex], mapRect);
                var pB = ToScreen(PathWaypoints[maxDrawnIndex + 1], mapRect);
                var pFrac = new SKPoint(SkiaUtils.Lerp(pA.X, pB.X, frac), SkiaUtils.Lerp(pA.Y, pB.Y, frac));
                canvas.DrawCircle(pFrac.X, pFrac.Y, 2.5f, dotPaint);
            }

            // 3 Tracking Beacons for Dallas, Kane, and Lambert
            // In lore (and as requested), the team stays in tight single-file formation within 3 meters (~0.12 waypoint index offset)
            (string Name, float PathOffset, SKPoint FinalHoldOffset, SKColor Color)[] crew =
            {
                ("DALLAS",   0.00f, new SKPoint( 0.000f,  0.000f), palette.WhiteBright),
                ("KANE",    -0.12f, new SKPoint( 0.004f, -0.006f), palette.IsMonochrome ? palette.Primary : palette.AlertRed),
                ("LAMBERT", -0.24f, new SKPoint(-0.004f,  0.006f), palette.IsMonochrome ? palette.PrimaryDim : palette.WarningAmber)
            };

            bool allArrived = progress >= 1.0f;

            // Base team position on the screen
            var basePoint = ToScreen(PathWaypoints[totalWaypoints - 1], mapRect);

            for (int c = 0; c < crew.Length; c++)
            {
                var member = crew[c];
                // Tight index offset along the path
                float memberIndex = Math.Clamp(exactTrailIndex + member.PathOffset, 0f, maxIndex);

                SKPoint memberPos;
                if (memberIndex >= maxIndex)
                {
                    // Arrived at Derelict inside horseshoe outcrop in tight cluster
                    var finalNorm = new SKPoint(
                        PathWaypoints[totalWaypoints - 1].X + member.FinalHoldOffset.X,
                        PathWaypoints[totalWaypoints - 1].Y + member.FinalHoldOffset.Y
                    );
                    memberPos = ToScreen(finalNorm, mapRect);
                }
                else
                {
                    int idxA = (int)memberIndex;
                    int idxB = Math.Min(totalWaypoints - 1, idxA + 1);
                    float frac = memberIndex - idxA;

                    var pA = ToScreen(PathWaypoints[idxA], mapRect);
                    var pB = ToScreen(PathWaypoints[idxB], mapRect);
                    memberPos = new SKPoint(SkiaUtils.Lerp(pA.X, pB.X, frac), SkiaUtils.Lerp(pA.Y, pB.Y, frac));
                }

                // Blip circle (distinct 3.2px blips within tight formation)
                using var memberPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    Color = member.Color
                };
                canvas.DrawCircle(memberPos.X, memberPos.Y, 3.2f, memberPaint);

                // Halo pulse
                float pulseSpeed = allArrived ? 2.5f : 3.5f;
                float pulse = (MathF.Sin(ElapsedTime * pulseSpeed + c * 1.2f) + 1f) * 0.5f;
                using var haloPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    Color = palette.WithAlpha(member.Color, (byte)(90 + pulse * 140)),
                    StrokeWidth = 0.9f
                };
                canvas.DrawCircle(memberPos.X, memberPos.Y, 4.8f + pulse * 2.5f, haloPaint);

                // Staggered neat label callouts to prevent overlap in tight 3m formation
                float tagX = memberPos.X + 10f;
                float tagY = memberPos.Y - 10f + c * 10f;
                canvas.DrawTechText(member.Name, tagX, tagY + 3f, 6.8f, member.Color, bold: true);
                canvas.DrawLine(memberPos.X + 2f, memberPos.Y, tagX - 1f, tagY + 1f, haloPaint);
            }

            // Bottom Right Telemetry Box Overlay (Live Distance & Status)
            float distMeters = Math.Max(0f, (1.0f - progress) * 2400f);
            float distKm = distMeters / 1000f;

            var infoBox = new SKRect(mapRect.Right - 180f, mapRect.Bottom - 50f, mapRect.Right - 8f, mapRect.Bottom - 8f);
            canvas.DrawTechBox(infoBox, cyanColor, palette.DarkPanel);

            if (allArrived)
            {
                canvas.DrawTechText("EVA TRACKING: AT TARGET", infoBox.Left + 6f, infoBox.Top + 11f, 7.5f, cyanColor, bold: true);
                canvas.DrawTechText("ALL CREW AT POSITION 2", infoBox.Left + 6f, infoBox.Top + 22f, 7.5f, palette.WhiteBright, bold: true);
                canvas.DrawTechText("DERELICT INGRESS: READY", infoBox.Left + 6f, infoBox.Top + 33f, 7f, palette.AlertRed, bold: true);
            }
            else
            {
                canvas.DrawTechText("EVA TRACKING: IN FORMATION", infoBox.Left + 6f, infoBox.Top + 11f, 7.5f, cyanColor, bold: true);
                canvas.DrawTechText($"RANGE TO POS 2: {distKm:F2} KM", infoBox.Left + 6f, infoBox.Top + 22f, 7.5f, palette.Primary);
                string statusStr = distMeters < 300f ? "BEACON SIGNAL: STRONG" : "BEARING: 037° NORTH-EAST";
                canvas.DrawTechText(statusStr, infoBox.Left + 6f, infoBox.Top + 33f, 7f, palette.PrimaryDim);
            }
        }

        private static SKPoint ToScreen(SKPoint norm, SKRect rect)
        {
            return new SKPoint(
                rect.Left + norm.X * rect.Width,
                rect.Top + norm.Y * rect.Height
            );
        }
    }
}

