using System;
using System.Collections.Generic;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class OrbitalInsertion : BaseTelemetryDisplay
    {
        public override string Title => "ORBIT INSERTION & SURFACE DESCENT // LV-426";
        public override string Subtitle => "APPROACH CORRIDOR // NOSTROMO AUTOMATED DESCENT SEQUENCE";
        public override string SystemCode => "ORB-INS-4352";

        // Topographical Terrain Contour Path definition on the unit sphere
        private class TerrainContour
        {
            public List<Vector3D> Vertices = new List<Vector3D>();
            public bool IsClosed = true;
            public int Level = 1; // 1 = lowlands/coast, 2 = plateau, 3 = mountain peaks/caldera
        }

        private readonly List<TerrainContour> _contours = new List<TerrainContour>();

        // Planetoid Landing Target Coordinates (Latitude, Longitude in radians)
        private const float TargetLat = 0.28f; // ~16 deg North
        private const float TargetLon = 1.15f; // ~66 deg East

        public override void Initialize()
        {
            base.Initialize();
            GenerateTerrainContours();
        }

        private void GenerateTerrainContours()
        {
            _contours.Clear();

            // Seeded procedural & hand-crafted contours for LV-426 volcanic continent & crater ridge system
            // 1. Major Continental Landmass with multi-tier elevation contours
            GenerateContinentContours(centerLat: 0.25f, centerLon: 1.0f, baseRadius: 0.65f, levels: 3, seed: 101);

            // 2. Secondary Archipelago / Highlands
            GenerateContinentContours(centerLat: -0.35f, centerLon: 2.2f, baseRadius: 0.50f, levels: 2, seed: 202);

            // 3. Polar Rift Mountain System
            GenerateContinentContours(centerLat: 0.70f, centerLon: 0.4f, baseRadius: 0.42f, levels: 2, seed: 303);

            // 4. Equatorial Impact Basin (Concentric crater rings)
            GenerateCraterContours(centerLat: -0.15f, centerLon: -1.2f, radius: 0.38f, rings: 3);

            // 5. Eastern Fractured Ridge
            GenerateContinentContours(centerLat: -0.40f, centerLon: -2.4f, baseRadius: 0.48f, levels: 2, seed: 404);

            // 6. Primary Landing Site Caldera Rim around TargetLat, TargetLon
            GenerateCraterContours(centerLat: TargetLat, centerLon: TargetLon, radius: 0.18f, rings: 2);
        }

        private void GenerateContinentContours(float centerLat, float centerLon, float baseRadius, int levels, int seed)
        {
            var rng = new Random(seed);
            int points = 36;

            for (int lvl = 1; lvl <= levels; lvl++)
            {
                float lvlRadius = baseRadius * (1.0f - (lvl - 1) * 0.28f);
                var contour = new TerrainContour { IsClosed = true, Level = lvl };

                for (int i = 0; i < points; i++)
                {
                    float angle = i * (MathF.PI * 2f / points);
                    // Organic perimeter harmonic noise
                    float noise = MathF.Sin(angle * 3f + lvl * 1.5f + seed) * 0.18f
                                + MathF.Cos(angle * 5f - seed * 0.5f) * 0.12f
                                + MathF.Sin(angle * 8f) * 0.06f;

                    float r = Math.Max(0.05f, lvlRadius * (1.0f + noise));
                    float lat = centerLat + r * MathF.Sin(angle);
                    float lon = centerLon + r * MathF.Cos(angle) / MathF.Max(0.2f, MathF.Cos(lat));

                    // Clamp latitude
                    lat = Math.Clamp(lat, -MathF.PI * 0.48f, MathF.PI * 0.48f);

                    // Convert to 3D unit sphere point
                    var v = SphericalToCartesian(lat, lon);
                    contour.Vertices.Add(v);
                }

                _contours.Add(contour);
            }
        }

        private void GenerateCraterContours(float centerLat, float centerLon, float radius, int rings)
        {
            int points = 28;
            for (int r = 1; r <= rings; r++)
            {
                float ringRad = radius * (r / (float)rings);
                var contour = new TerrainContour { IsClosed = true, Level = r };

                for (int i = 0; i < points; i++)
                {
                    float angle = i * (MathF.PI * 2f / points);
                    float noise = MathF.Sin(angle * 6f + r) * 0.05f;
                    float rad = ringRad * (1.0f + noise);

                    float lat = centerLat + rad * MathF.Sin(angle);
                    float lon = centerLon + rad * MathF.Cos(angle) / MathF.Max(0.2f, MathF.Cos(lat));
                    lat = Math.Clamp(lat, -MathF.PI * 0.48f, MathF.PI * 0.48f);

                    var v = SphericalToCartesian(lat, lon);
                    contour.Vertices.Add(v);
                }

                _contours.Add(contour);
            }
        }

        private static Vector3D SphericalToCartesian(float lat, float lon)
        {
            float cosLat = MathF.Cos(lat);
            return new Vector3D(
                cosLat * MathF.Sin(lon),
                -MathF.Sin(lat),
                cosLat * MathF.Cos(lon)
            );
        }

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            // Outer double-line border box matching the vintage screen
            DrawScreenOuterFrame(canvas, bounds, palette);

            // Compute layout rects
            float contentTop = bounds.Top + 38f;
            float contentBottom = bounds.Bottom - 12f;
            float contentLeft = bounds.Left + 12f;
            float contentRight = bounds.Right - 12f;

            // Telemetry Column on Right (width ~200px)
            float sideWidth = 205f;
            var telemetryBox = new SKRect(contentRight - sideWidth, contentTop, contentRight, contentBottom);
            var viewportBox = new SKRect(contentLeft, contentTop, telemetryBox.Left - 6f, contentBottom);

            // 1. Top Header Subsystems & Timecode
            DrawTopHeaders(canvas, bounds, telemetryBox, palette);

            // 2. Viewport Reticles & Crosshairs (Left side)
            DrawViewportReticles(canvas, viewportBox, palette);

            // 3. 3D Rotating Planetoid & Topography
            DrawRotatingPlanetoid(canvas, viewportBox, palette, out SKPoint landingSite2D, out bool isLandingSiteVisible, out SKPoint corridorCenter);

            // 4. Flight Path Approach Corridor Reticles (Left overlay)
            DrawApproachCorridor(canvas, viewportBox, corridorCenter, landingSite2D, isLandingSiteVisible, palette);

            // 5. Right-Hand Telemetry & Live Altitude Descent Readouts
            DrawRightTelemetryPanel(canvas, telemetryBox, palette);
        }

        private void DrawScreenOuterFrame(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            using var borderPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.PrimaryDim,
                StrokeWidth = 1.2f
            };

            // Outer rectangular border with slight inner padding
            var outerRect = new SKRect(bounds.Left + 6f, bounds.Top + 6f, bounds.Right - 6f, bounds.Bottom - 6f);
            canvas.DrawRect(outerRect, borderPaint);

            // Secondary inner rule frame
            var innerRect = new SKRect(outerRect.Left + 3f, outerRect.Top + 3f, outerRect.Right - 3f, outerRect.Bottom - 3f);
            borderPaint.Color = palette.WithAlpha(palette.PrimaryDim, 90);
            borderPaint.StrokeWidth = 0.8f;
            canvas.DrawRect(innerRect, borderPaint);

            // Top technical header horizontal divider
            float headerDividerY = bounds.Top + 34f;
            borderPaint.Color = palette.PrimaryDim;
            borderPaint.StrokeWidth = 1.2f;
            canvas.DrawLine(outerRect.Left, headerDividerY, outerRect.Right, headerDividerY, borderPaint);
            canvas.DrawLine(outerRect.Left, headerDividerY + 2f, outerRect.Right, headerDividerY + 2f, borderPaint);
        }

        private void DrawTopHeaders(SKCanvas canvas, SKRect bounds, SKRect telemetryBox, ColorPalette palette)
        {
            float headerY = bounds.Top + 24f;

            // Left Title: "ORBIT INSERTION"
            canvas.DrawTechText("ORBIT INSERTION", bounds.Left + 22f, headerY, 14f, palette.Primary, bold: true);

            // Center Badges: "[APPROACH]" and "SYSTEM :SL: 43.52 :GA:"
            float midX = bounds.Left + (bounds.Width - telemetryBox.Width) * 0.58f;

            // [APPROACH] small box
            float badgeW = 74f;
            float badgeH = 14f;
            var approachBox = new SKRect(midX - badgeW / 2f, bounds.Top + 9f, midX + badgeW / 2f, bounds.Top + 9f + badgeH);
            using var boxPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.PrimaryDim,
                StrokeWidth = 1.0f
            };
            canvas.DrawRect(approachBox, boxPaint);
            canvas.DrawTechText("APPROACH", approachBox.MidX, approachBox.MidY + 3.5f, 8.5f, palette.Primary, SKTextAlign.Center, bold: true);

            // "SYSTEM :SL: 43.52 :GA:" sub-box
            float sysBoxW = 160f;
            float sysBoxH = 11f;
            var sysBox = new SKRect(midX - sysBoxW / 2f, bounds.Top + 24f, midX + sysBoxW / 2f, bounds.Top + 24f + sysBoxH);
            canvas.DrawRect(sysBox, boxPaint);

            // Dynamic sub-frequency glitch
            float freqSub = 43.52f + MathF.Sin(ElapsedTime * 0.8f) * 0.04f;
            string sysCodeStr = $"SYSTEM :SL: {freqSub:F2} :GA:";
            canvas.DrawTechText(sysCodeStr, sysBox.MidX, sysBox.MidY + 3.2f, 7.5f, palette.PrimaryDim, SKTextAlign.Center, bold: true);
        }

        private void DrawViewportReticles(SKCanvas canvas, SKRect viewport, ColorPalette palette)
        {
            // Large technical crosshairs on the left margin (as in reference image)
            float leftX = viewport.Left + 35f;
            float topCrossY = viewport.Top + viewport.Height * 0.22f;
            float bottomCrossY = viewport.Bottom - viewport.Height * 0.12f;

            DrawDoubleTickCrosshair(canvas, leftX, topCrossY, 26f, palette.PrimaryDim);
            DrawDoubleTickCrosshair(canvas, leftX, bottomCrossY, 26f, palette.PrimaryDim);

            // Vertical separating border for right telemetry column
            using var sepPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.PrimaryDim,
                StrokeWidth = 1.2f
            };
            canvas.DrawLine(viewport.Right + 3f, viewport.Top, viewport.Right + 3f, viewport.Bottom, sepPaint);
            canvas.DrawLine(viewport.Right + 5f, viewport.Top, viewport.Right + 5f, viewport.Bottom, sepPaint);
        }

        private void DrawDoubleTickCrosshair(SKCanvas canvas, float x, float y, float size, SKColor color)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = color,
                StrokeWidth = 1.2f
            };

            // Main crosshair arms
            canvas.DrawLine(x - size, y, x + size, y, paint);
            canvas.DrawLine(x, y - size, x, y + size, paint);

            // Dual tick marks at the tips
            float tick = 5f;
            // Left & Right
            canvas.DrawLine(x - size, y - tick, x - size, y + tick, paint);
            canvas.DrawLine(x + size, y - tick, x + size, y + tick, paint);
            // Top & Bottom
            canvas.DrawLine(x - tick, y - size, x + tick, y - size, paint);
            canvas.DrawLine(x - tick, y + size, x + tick, y + size, paint);
        }

        private void DrawRotatingPlanetoid(
            SKCanvas canvas,
            SKRect viewport,
            ColorPalette palette,
            out SKPoint landingSite2D,
            out bool isLandingSiteVisible,
            out SKPoint corridorCenter)
        {
            // Position planetoid in right-center portion of viewport (as seen in photo)
            float planetRadius = Math.Min(viewport.Width * 0.36f, viewport.Height * 0.44f);
            float cx = viewport.Left + viewport.Width * 0.63f;
            float cy = viewport.Top + viewport.Height * 0.52f;

            // Approach corridor center is located to the left of the planetoid
            corridorCenter = new SKPoint(viewport.Left + viewport.Width * 0.30f, cy + 20f);

            // Rotation & Orientation parameters (Axial tilt ~24 degrees)
            float rotSpeed = 0.16f; // Smooth rotation
            float planetSpin = ElapsedTime * rotSpeed;
            float tiltX = 0.22f;  // ~12.6 deg pitch forward
            float tiltZ = -0.38f; // ~21.7 deg roll/axial tilt

            // 1. Draw Background faint grid/parallels on back hemisphere
            DrawPlanetLatitudeGrid(canvas, cx, cy, planetRadius, planetSpin, tiltX, tiltZ, palette, isFront: false);

            // 2. Draw 3D Topographic Terrain Contours (The reddish/amber wireframe continent loops)
            DrawTerrainContours(canvas, cx, cy, planetRadius, planetSpin, tiltX, tiltZ, palette);

            // 3. Draw Foreground Latitude & Longitude grid (Parallels & Meridians with dashed style)
            DrawPlanetLatitudeGrid(canvas, cx, cy, planetRadius, planetSpin, tiltX, tiltZ, palette, isFront: true);
            DrawPlanetLongitudeMeridians(canvas, cx, cy, planetRadius, planetSpin, tiltX, tiltZ, palette);

            // 4. Draw Outer Silhouette / Limb Circle with glowing atmosphere rim
            DrawPlanetLimb(canvas, cx, cy, planetRadius, palette);

            // 5. Compute Landing Site coordinates
            var targetCartesian = SphericalToCartesian(TargetLat, TargetLon);
            var rotTarget = Rotate3D(targetCartesian, planetSpin, tiltX, tiltZ);

            isLandingSiteVisible = rotTarget.Z > -0.05f;
            landingSite2D = new SKPoint(cx + rotTarget.X * planetRadius, cy + rotTarget.Y * planetRadius);

            if (isLandingSiteVisible)
            {
                DrawLandingTarget(canvas, landingSite2D, palette);
            }
        }

        private Vector3D Rotate3D(Vector3D v, float spinY, float tiltX, float tiltZ)
        {
            // Spin around Y axis
            var sp = v.RotateY(spinY);
            // Tilt around X axis
            var tx = sp.RotateX(tiltX);
            // Tilt around Z axis
            var tz = tx.RotateZ(tiltZ);
            return tz;
        }

        private void DrawPlanetLatitudeGrid(
            SKCanvas canvas,
            float cx,
            float cy,
            float radius,
            float spinY,
            float tiltX,
            float tiltZ,
            ColorPalette palette,
            bool isFront)
        {
            // Latitudes (Parallels): horizontal dashed rings
            float[] lats = { -75f, -60f, -45f, -30f, -15f, 0f, 15f, 30f, 45f, 60f, 75f };

            using var frontPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.Primary,
                StrokeWidth = 1.3f,
                PathEffect = SKPathEffect.CreateDash(new float[] { 5f, 4f }, 0)
            };

            using var backPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 35),
                StrokeWidth = 0.8f
            };

            int segments = 60;
            foreach (float latDeg in lats)
            {
                float latRad = latDeg * (MathF.PI / 180f);
                using var currentPath = new SKPath();
                bool inPath = false;

                for (int i = 0; i <= segments; i++)
                {
                    float lonRad = i * (MathF.PI * 2f / segments);
                    var v = SphericalToCartesian(latRad, lonRad);
                    var r = Rotate3D(v, spinY, tiltX, tiltZ);

                    bool visible = isFront ? (r.Z >= 0f) : (r.Z < 0f);
                    float px = cx + r.X * radius;
                    float py = cy + r.Y * radius;

                    if (visible)
                    {
                        if (!inPath)
                        {
                            currentPath.MoveTo(px, py);
                            inPath = true;
                        }
                        else
                        {
                            currentPath.LineTo(px, py);
                        }
                    }
                    else
                    {
                        if (inPath)
                        {
                            canvas.DrawPath(currentPath, isFront ? frontPaint : backPaint);
                            currentPath.Reset();
                            inPath = false;
                        }
                    }
                }

                if (inPath)
                {
                    canvas.DrawPath(currentPath, isFront ? frontPaint : backPaint);
                }
            }
        }

        private void DrawPlanetLongitudeMeridians(
            SKCanvas canvas,
            float cx,
            float cy,
            float radius,
            float spinY,
            float tiltX,
            float tiltZ,
            ColorPalette palette)
        {
            // Meridians (Longitudes): curved lines from North to South pole
            int meridianCount = 12;

            using var meridianPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.Primary, 210),
                StrokeWidth = 1.1f
            };

            int steps = 40;
            for (int m = 0; m < meridianCount; m++)
            {
                float lonRad = m * (MathF.PI * 2f / meridianCount);
                using var path = new SKPath();
                bool inPath = false;

                for (int i = 0; i <= steps; i++)
                {
                    float latRad = -MathF.PI * 0.48f + i * (MathF.PI * 0.96f / steps);
                    var v = SphericalToCartesian(latRad, lonRad);
                    var r = Rotate3D(v, spinY, tiltX, tiltZ);

                    // Front hemisphere culling with smooth horizon boundary
                    if (r.Z >= -0.02f)
                    {
                        float px = cx + r.X * radius;
                        float py = cy + r.Y * radius;

                        if (!inPath)
                        {
                            path.MoveTo(px, py);
                            inPath = true;
                        }
                        else
                        {
                            path.LineTo(px, py);
                        }
                    }
                    else
                    {
                        if (inPath)
                        {
                            canvas.DrawPath(path, meridianPaint);
                            path.Reset();
                            inPath = false;
                        }
                    }
                }

                if (inPath)
                {
                    canvas.DrawPath(path, meridianPaint);
                }
            }
        }

        private void DrawTerrainContours(
            SKCanvas canvas,
            float cx,
            float cy,
            float radius,
            float spinY,
            float tiltX,
            float tiltZ,
            ColorPalette palette)
        {
            // In vintage 1979 film: Terrain contours are vivid reddish-orange / amber phosphor lines
            SKColor contourColor1 = palette.IsMonochrome ? palette.Primary : palette.AlertRed;
            SKColor contourColor2 = palette.IsMonochrome ? palette.PrimaryDim : palette.WarningAmber;

            using var paintLevel1 = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(contourColor1, 230),
                StrokeWidth = 1.3f
            };

            using var paintLevel2 = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(contourColor2, 210),
                StrokeWidth = 1.1f
            };

            using var paintLevel3 = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.Primary, 180),
                StrokeWidth = 0.9f
            };

            foreach (var contour in _contours)
            {
                var paint = contour.Level switch
                {
                    1 => paintLevel1,
                    2 => paintLevel2,
                    _ => paintLevel3
                };

                using var path = new SKPath();
                bool inPath = false;
                int count = contour.Vertices.Count;
                int maxIter = contour.IsClosed ? count + 1 : count;

                for (int i = 0; i < maxIter; i++)
                {
                    var v = contour.Vertices[i % count];
                    var r = Rotate3D(v, spinY, tiltX, tiltZ);

                    // Only draw points on visible front hemisphere
                    if (r.Z >= 0.02f)
                    {
                        float px = cx + r.X * radius;
                        float py = cy + r.Y * radius;

                        if (!inPath)
                        {
                            path.MoveTo(px, py);
                            inPath = true;
                        }
                        else
                        {
                            path.LineTo(px, py);
                        }
                    }
                    else
                    {
                        if (inPath)
                        {
                            canvas.DrawPath(path, paint);
                            path.Reset();
                            inPath = false;
                        }
                    }
                }

                if (inPath)
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawPlanetLimb(SKCanvas canvas, float cx, float cy, float radius, ColorPalette palette)
        {
            // Bright circular limb
            using var limbPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WhiteBright,
                StrokeWidth = 1.8f
            };
            canvas.DrawCircle(cx, cy, radius, limbPaint);

            // Subtle outer atmosphere glow ring
            using var glowPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 70),
                StrokeWidth = 2.5f
            };
            canvas.DrawCircle(cx, cy, radius + 2f, glowPaint);
        }

        private void DrawLandingTarget(SKCanvas canvas, SKPoint pos, ColorPalette palette)
        {
            // Blinking target crosshair at landing site
            float pulse = (MathF.Sin(ElapsedTime * 8f) + 1f) * 0.5f;
            SKColor targetColor = palette.IsMonochrome ? palette.Primary : palette.AlertRed;

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(targetColor, (byte)(180 + pulse * 75)),
                StrokeWidth = 1.4f
            };

            // Crosshair
            float size = 8f;
            canvas.DrawLine(pos.X - size, pos.Y, pos.X + size, pos.Y, paint);
            canvas.DrawLine(pos.X, pos.Y - size, pos.X, pos.Y + size, paint);

            // Small square target lock
            var rect = new SKRect(pos.X - 5f, pos.Y - 5f, pos.X + 5f, pos.Y + 5f);
            canvas.DrawCornerBrackets(rect, targetColor, length: 3f, strokeWidth: 1.2f);
        }

        private void DrawApproachCorridor(
            SKCanvas canvas,
            SKRect viewport,
            SKPoint corridorCenter,
            SKPoint landingSite2D,
            bool isLandingSiteVisible,
            ColorPalette palette)
        {
            // 4 Nested Wireframe Perspective Rectangles (HUD Flight Approach Corridor)
            float[] scales = { 1.0f, 0.76f, 0.54f, 0.36f };
            float baseW = 160f;
            float baseH = 125f;

            // Small dynamic attitude drift oscillation
            float driftX = MathF.Sin(ElapsedTime * 1.8f) * 2.5f;
            float driftY = MathF.Cos(ElapsedTime * 1.4f) * 2.0f;

            using var gatePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.Primary,
                StrokeWidth = 1.4f
            };

            var rects = new List<SKRect>();
            for (int i = 0; i < scales.Length; i++)
            {
                float s = scales[i];
                float w = baseW * s;
                float h = baseH * s;
                // Perspective convergence towards center
                float offsetX = (1.0f - s) * 12f + driftX * (1.0f - s);
                float offsetY = (1.0f - s) * 6f + driftY * (1.0f - s);

                var r = new SKRect(
                    corridorCenter.X - w / 2f + offsetX,
                    corridorCenter.Y - h / 2f + offsetY,
                    corridorCenter.X + w / 2f + offsetX,
                    corridorCenter.Y + h / 2f + offsetY
                );
                rects.Add(r);
                canvas.DrawRect(r, gatePaint);
            }

            // Diagonal corner connecting guide lines from outer gate to inner gate
            using var rayPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 90),
                StrokeWidth = 0.8f,
                PathEffect = SKPathEffect.CreateDash(new float[] { 4f, 4f }, 0)
            };

            var outer = rects[0];
            var inner = rects[rects.Count - 1];
            canvas.DrawLine(outer.Left, outer.Top, inner.Left, inner.Top, rayPaint);
            canvas.DrawLine(outer.Right, outer.Top, inner.Right, inner.Top, rayPaint);
            canvas.DrawLine(outer.Left, outer.Bottom, inner.Left, inner.Bottom, rayPaint);
            canvas.DrawLine(outer.Right, outer.Bottom, inner.Right, inner.Bottom, rayPaint);

            // Diagonal cross / rotated alignment indicator inside the innermost gate
            using var innerGuidePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.Primary, 160),
                StrokeWidth = 1.0f
            };
            canvas.DrawLine(inner.Left + 4f, inner.Bottom - 4f, inner.Left + 14f, inner.Top + 4f, innerGuidePaint);
            canvas.DrawLine(inner.Right - 4f, inner.Top + 4f, inner.Right - 14f, inner.Bottom - 4f, innerGuidePaint);

            // Trajectory Arc leading from Corridor Gate to Landing Site on Planetoid
            if (isLandingSiteVisible)
            {
                using var trajPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    Color = palette.IsMonochrome ? palette.Primary : palette.WarningAmber,
                    StrokeWidth = 1.5f,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 6f, 5f }, 0)
                };

                using var trajPath = new SKPath();
                trajPath.MoveTo(inner.Right, inner.MidY);
                // Quadratic bezier arc down to landing site
                float ctrlX = (inner.Right + landingSite2D.X) * 0.5f;
                float ctrlY = Math.Min(inner.MidY, landingSite2D.Y) - 30f;
                trajPath.QuadTo(ctrlX, ctrlY, landingSite2D.X, landingSite2D.Y);
                canvas.DrawPath(trajPath, trajPaint);

                // Animated descending energy packet on trajectory
                float packetT = (ElapsedTime * 0.45f) % 1.0f;
                float px = (1 - packetT) * (1 - packetT) * inner.Right + 2 * (1 - packetT) * packetT * ctrlX + packetT * packetT * landingSite2D.X;
                float py = (1 - packetT) * (1 - packetT) * inner.MidY + 2 * (1 - packetT) * packetT * ctrlY + packetT * packetT * landingSite2D.Y;

                using var dotPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    Color = palette.WhiteBright
                };
                canvas.DrawCircle(px, py, 3.5f, dotPaint);
            }
        }

        private void DrawRightTelemetryPanel(SKCanvas canvas, SKRect sideRect, ColorPalette palette)
        {
            using var boxPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.PrimaryDim,
                StrokeWidth = 1.0f
            };

            // Descent simulation calculation over a 60-second operational loop
            float cycleDuration = 60f;
            float t = ElapsedTime % cycleDuration;
            float progress = t / cycleDuration; // 0.0 (high orbit) -> 1.0 (touchdown)

            // Altitude descent curve: Starts at 142,500m -> drops to 0m (touchdown)
            float altitudeMeters;
            float descentVelMs;
            string stageName;
            float markSec;
            float burnSec;

            if (progress < 0.25f)
            {
                // Phase 1: De-orbit Insertion Burn (142,500m -> 85,000m)
                float p1 = progress / 0.25f;
                altitudeMeters = SkiaUtils.Lerp(142500f, 85000f, p1);
                descentVelMs = -SkiaUtils.Lerp(180f, 340f, p1);
                stageName = "ORBIT INSERTION";
                burnSec = Math.Max(0f, 4.60f - p1 * 4.60f);
                markSec = 0.95f - p1 * 0.40f;
            }
            else if (progress < 0.60f)
            {
                // Phase 2: Atmospheric Entry & Aerobraking (85,000m -> 14,000m)
                float p2 = (progress - 0.25f) / 0.35f;
                altitudeMeters = SkiaUtils.Lerp(85000f, 14000f, p2);
                descentVelMs = -SkiaUtils.Lerp(340f, 120f, p2);
                stageName = "ATMOS BRAKING";
                burnSec = 0.00f;
                markSec = 0.55f - p2 * 0.30f;
            }
            else if (progress < 0.88f)
            {
                // Phase 3: Terminal Powered Descent (14,000m -> 300m)
                float p3 = (progress - 0.60f) / 0.28f;
                altitudeMeters = SkiaUtils.Lerp(14000f, 300f, p3);
                descentVelMs = -SkiaUtils.Lerp(120f, 8.5f, p3);
                stageName = "TERMINAL DESCENT";
                burnSec = (p3 * 8.2f) % 3.0f;
                markSec = 0.25f - p3 * 0.20f;
            }
            else
            {
                // Phase 4: Final Touchdown & Surface Lock (300m -> 0m)
                float p4 = (progress - 0.88f) / 0.12f;
                altitudeMeters = Math.Max(0f, SkiaUtils.Lerp(300f, 0f, p4));
                descentVelMs = Math.Max(-1.2f, -SkiaUtils.Lerp(8.5f, 0.0f, p4));
                stageName = altitudeMeters <= 5f ? "TOUCHDOWN LOCK" : "FINAL FLARE";
                burnSec = 0.00f;
                markSec = 0.05f;
            }

            float currentY = sideRect.Top + 6f;

            // 1. "TIME FROM #7" and mission clock box
            canvas.DrawTechText("TIME FROM #7", sideRect.Left + 8f, currentY + 9f, 9.5f, palette.Primary, bold: true);
            var timeBox = new SKRect(sideRect.Left + 8f, currentY + 14f, sideRect.Right - 8f, currentY + 30f);
            canvas.DrawRect(timeBox, boxPaint);

            // Mission time format "M+01:45:03"
            int totalSeconds = (int)ElapsedTime;
            int mMin = 1 + totalSeconds / 60;
            int mSec = 45 + (totalSeconds % 60);
            int mFrame = (int)((ElapsedTime * 10) % 60);
            string timeStr = $"M+{mMin:D2}:{mSec % 60:D2}:{mFrame:D2}";
            canvas.DrawTechText(timeStr, timeBox.MidX, timeBox.MidY + 4f, 10.5f, palette.Primary, SKTextAlign.Center, bold: true);

            currentY += 40f;

            // 2. "PRESENT / P.O.R. / NOSTROMO / [ S 1 ]"
            canvas.DrawTechText("PRESENT", sideRect.Left + 8f, currentY + 8f, 9f, palette.Primary);
            canvas.DrawTechText("P.O.R.", sideRect.Left + 8f, currentY + 20f, 9f, palette.Primary);
            canvas.DrawTechText("NOSTROMO", sideRect.Left + 8f, currentY + 32f, 9f, palette.Primary, bold: true);

            var s1Box = new SKRect(sideRect.Left + 8f, currentY + 38f, sideRect.Left + 72f, currentY + 54f);
            canvas.DrawRect(s1Box, boxPaint);
            canvas.DrawTechText("S 1", s1Box.MidX, s1Box.MidY + 4f, 9.5f, palette.Primary, SKTextAlign.Center, bold: true);

            currentY += 66f;

            // 3. "ATTITUDE / CORRECTION / LAT. ROT. / [ .89 DEG. ]"
            canvas.DrawTechText("ATTITUDE", sideRect.Left + 8f, currentY + 8f, 9f, palette.Primary);
            canvas.DrawTechText("CORRECTION", sideRect.Left + 8f, currentY + 20f, 9f, palette.Primary);
            canvas.DrawTechText("LAT. ROT.", sideRect.Left + 8f, currentY + 32f, 9f, palette.Primary);

            // Live small attitude correction oscillation
            float latRotDeg = 0.89f + MathF.Sin(ElapsedTime * 2.2f) * 0.14f;
            var rotBox = new SKRect(sideRect.Left + 8f, currentY + 38f, sideRect.Right - 8f, currentY + 54f);
            canvas.DrawRect(rotBox, boxPaint);
            canvas.DrawTechText($".{Math.Abs((int)(latRotDeg * 100)):D2} DEG.", rotBox.MidX, rotBox.MidY + 4f, 9.5f, palette.Primary, SKTextAlign.Center, bold: true);

            currentY += 66f;

            // 4. "MARK / [ T - .95 ]"
            canvas.DrawTechText("MARK", sideRect.Left + 8f, currentY + 8f, 9f, palette.Primary);
            var markBox = new SKRect(sideRect.Left + 8f, currentY + 14f, sideRect.Right - 8f, currentY + 30f);
            canvas.DrawRect(markBox, boxPaint);
            canvas.DrawTechText($"T - .{Math.Abs((int)(markSec * 100)):D2}", markBox.MidX, markBox.MidY + 4f, 9.5f, palette.Primary, SKTextAlign.Center, bold: true);

            currentY += 42f;

            // 5. "BURN 1 / [ 4.60 SECS ]"
            canvas.DrawTechText("BURN 1", sideRect.Left + 8f, currentY + 8f, 9f, palette.Primary);
            var burnBox = new SKRect(sideRect.Left + 8f, currentY + 14f, sideRect.Right - 8f, currentY + 30f);
            canvas.DrawRect(burnBox, boxPaint);
            canvas.DrawTechText($"{burnSec:F2} SECS", burnBox.MidX, burnBox.MidY + 4f, 9.5f, palette.Primary, SKTextAlign.Center, bold: true);

            currentY += 42f;

            // 6. "SYSTEM [ 4 0 ]"
            canvas.DrawTechText("SYSTEM", sideRect.Left + 8f, currentY + 12f, 9f, palette.Primary);
            var sysNumBox = new SKRect(sideRect.Left + 68f, currentY, sideRect.Right - 8f, currentY + 16f);
            canvas.DrawRect(sysNumBox, boxPaint);
            canvas.DrawTechText("4 0", sysNumBox.MidX, sysNumBox.MidY + 4f, 9.5f, palette.Primary, SKTextAlign.Center, bold: true);

            currentY += 26f;

            // 7. "AUTODECOUNT"
            canvas.DrawTechText("AUTODECOUNT", sideRect.Left + 8f, currentY + 10f, 8.5f, palette.PrimaryDim, bold: true);

            currentY += 20f;

            // 8. LIVE DESCENT ALTITUDE & TELEMETRY BLOCK
            var altCard = new SKRect(sideRect.Left + 6f, currentY, sideRect.Right - 6f, sideRect.Bottom - 8f);
            canvas.DrawTechBox(altCard, palette.PrimaryDim, palette.DarkPanel, title: "DESCENT RADAR");

            float altY = altCard.Top + 20f;
            canvas.DrawTechText("ALTITUDE:", altCard.Left + 8f, altY, 8.5f, palette.PrimaryDim);
            string altStr = altitudeMeters >= 1000f ? $"{altitudeMeters / 1000f:F1} KM" : $"{altitudeMeters:F0} M";
            canvas.DrawTechText(altStr, altCard.Right - 8f, altY, 9.5f, palette.Primary, SKTextAlign.Right, bold: true);

            // Descent segmented bar
            altY += 6f;
            var altBar = new SKRect(altCard.Left + 8f, altY, altCard.Right - 8f, altY + 7f);
            float altProgress = Math.Clamp(1.0f - (altitudeMeters / 142500f), 0f, 1f);
            canvas.DrawSegmentedBar(altBar, altProgress, 12, palette.Primary, palette.WithAlpha(palette.PrimaryDim, 40));

            altY += 16f;
            canvas.DrawTechText("DESCENT RATE:", altCard.Left + 8f, altY, 8.5f, palette.PrimaryDim);
            canvas.DrawTechText($"{descentVelMs:F1} M/S", altCard.Right - 8f, altY, 8.5f, palette.Primary, SKTextAlign.Right);

            altY += 14f;
            canvas.DrawTechText("STAGE:", altCard.Left + 8f, altY, 8.5f, palette.PrimaryDim);
            canvas.DrawTechText(stageName, altCard.Right - 8f, altY, 8.5f, palette.Primary, SKTextAlign.Right, bold: true);

            altY += 14f;
            float distKm = Math.Max(0f, (1.0f - progress) * 342.0f);
            canvas.DrawTechText("RANGE TO LZ:", altCard.Left + 8f, altY, 8.5f, palette.PrimaryDim);
            canvas.DrawTechText($"{distKm:F1} KM", altCard.Right - 8f, altY, 8.5f, palette.Primary, SKTextAlign.Right);
        }
    }
}

