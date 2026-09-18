using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class AtmosphericAnalysisLV426Display : BaseTelemetryDisplay
    {
        public override string Title => "ATMOSPHERIC COMPOSITION & TROPOSPHERE DYNAMICS // LV-426";
        public override string Subtitle => "SURFACE BAROMETRIC GRADIENT // CORROSIVE TOXIC AEROSOLS";
        public override string SystemCode => "ATM-ACH-426";

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;
            float contentWidth = contentRight - contentLeft;

            float leftColWidth = contentWidth * 0.55f;

            // LEFT PANEL: Barometric Pressure vs Altitude Curve
            var curveBox = new SKRect(contentLeft, contentTop, contentLeft + leftColWidth, contentBottom);
            canvas.DrawTechBox(curveBox, palette.PrimaryDim, palette.DarkPanel, title: "PRESSURE GRADIENT (ATM vs ALTITUDE KM)");

            // Draw graph axes
            float graphLeft = curveBox.Left + 40f;
            float graphRight = curveBox.Right - 20f;
            float graphBottom = curveBox.Bottom - 35f;
            float graphTop = curveBox.Top + 35f;

            using var axisPaint = new SKPaint { IsAntialias = true, Color = palette.PrimaryDim, StrokeWidth = 1.2f };
            canvas.DrawLine(graphLeft, graphTop, graphLeft, graphBottom, axisPaint);
            canvas.DrawLine(graphLeft, graphBottom, graphRight, graphBottom, axisPaint);

            // Axis labels
            canvas.DrawTechText("100 KM", graphLeft - 4f, graphTop + 4f, 8.5f, palette.PrimaryDim, SKTextAlign.Right);
            canvas.DrawTechText("50 KM", graphLeft - 4f, (graphTop + graphBottom) / 2f + 4f, 8.5f, palette.PrimaryDim, SKTextAlign.Right);
            canvas.DrawTechText("0 KM", graphLeft - 4f, graphBottom + 4f, 8.5f, palette.PrimaryDim, SKTextAlign.Right);

            canvas.DrawTechText("0.0 ATM", graphLeft, graphBottom + 16f, 8.5f, palette.PrimaryDim);
            canvas.DrawTechText("0.5", (graphLeft + graphRight) / 2f, graphBottom + 16f, 8.5f, palette.PrimaryDim, SKTextAlign.Center);
            canvas.DrawTechText("1.0 ATM", graphRight, graphBottom + 16f, 8.5f, palette.PrimaryDim, SKTextAlign.Right);

            // Draw grid lines
            canvas.DrawGrid(new SKRect(graphLeft, graphTop, graphRight, graphBottom), (graphRight - graphLeft) / 4f, (graphBottom - graphTop) / 4f, palette.WithAlpha(palette.PrimaryDim, 30));

            // Barometric decay curve: P = P0 * exp(-h / H)
            using var curvePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = palette.Primary, StrokeWidth = 2f };
            using var curvePath = new SKPath();
            bool first = true;

            for (float h = 0f; h <= 100f; h += 2f)
            {
                float pressure = 0.89f * MathF.Exp(-h / 18f); // Surface pressure 0.89 atm
                float gx = graphLeft + (pressure / 1.0f) * (graphRight - graphLeft);
                float gy = graphBottom - (h / 100f) * (graphBottom - graphTop);

                // Add slight atmospheric turbulent jitter
                gx += MathF.Sin(h * 0.2f + ElapsedTime * 2f) * 1.5f;

                if (first) { curvePath.MoveTo(gx, gy); first = false; }
                else { curvePath.LineTo(gx, gy); }
            }
            canvas.DrawPath(curvePath, curvePaint);

            // Surface Marker
            canvas.DrawCrosshair(graphLeft + 0.89f * (graphRight - graphLeft), graphBottom, 8f, palette.AlertRed, 1.2f, circle: true);
            canvas.DrawTechText("SURFACE: 0.89 ATM", graphLeft + 0.89f * (graphRight - graphLeft) - 8f, graphBottom - 12f, 8.5f, palette.AlertRed, SKTextAlign.Right, bold: true);

            // RIGHT PANEL: Chemical Composition & Environmental Hazard
            var rightBox = new SKRect(curveBox.Right + 12f, contentTop, contentRight, contentBottom);
            canvas.DrawTechBox(rightBox, palette.PrimaryDim, palette.DarkPanel, title: "GAS COMPOSITION & HAZARD PROFILE");

            float ty = rightBox.Top + 28f;
            (string Gas, float Pct, SKColor Col)[] gasses =
            {
                ("ARGON / CO2 MIX", 74.8f, palette.Primary),
                ("NITROGEN (N2)", 14.6f, palette.CyanAccent),
                ("METHANE (CH4)", 10.2f, palette.WarningAmber),
                ("CORROSIVE AEROSOLS", 0.4f, palette.AlertRed)
            };

            for (int i = 0; i < gasses.Length; i++)
            {
                var g = gasses[i];
                canvas.DrawTechText(g.Gas, rightBox.Left + 12f, ty, 9.5f, palette.Primary);
                canvas.DrawTechText($"{g.Pct:F1}%", rightBox.Right - 12f, ty, 9.5f, palette.PrimaryDim, SKTextAlign.Right);

                var barRect = new SKRect(rightBox.Left + 12f, ty + 4f, rightBox.Right - 12f, ty + 14f);
                canvas.DrawSegmentedBar(barRect, g.Pct / 100f, 20, g.Col, palette.WithAlpha(palette.PrimaryDim, 40));
                ty += 28f;
            }

            ty += 6f;
            string[] atmosMetrics =
            {
                "SURFACE TEMPERATURE: -42°C TO -18°C",
                "WIND VELOCITY: 120 KM/H (SQUALLS)",
                "HUMIDITY: 88% CONDENSED VAPOR",
                "TOXICITY RATING: LETHAL (CLASS 4)",
                "BREATHABILITY: 0.00% (FATAL IN < 10s)",
                "CORROSIVE INDEX: MILDLY ACIDIC"
            };

            foreach (var metric in atmosMetrics)
            {
                canvas.DrawTechText(metric, rightBox.Left + 12f, ty, 8.5f, palette.PrimaryDim);
                ty += 16f;
            }

            var alertRect = new SKRect(rightBox.Left + 10f, rightBox.Bottom - 44f, rightBox.Right - 10f, rightBox.Bottom - 10f);
            canvas.DrawTechBox(alertRect, palette.AlertRed, palette.DarkPanel);
            canvas.DrawTechText("[ SURFACE UNINHABITABLE // FULL SUIT REQUIRED ]", alertRect.MidX, alertRect.MidY + 4f, 8.5f, palette.AlertRed, SKTextAlign.Center, bold: true);
        }
    }
}

