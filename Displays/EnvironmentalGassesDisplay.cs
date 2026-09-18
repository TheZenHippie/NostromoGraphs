using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class EnvironmentalGassesDisplay : BaseTelemetryDisplay
    {
        public override string Title => "ATMOSPHERIC & LIFE SUPPORT GAS MATRIX";
        public override string Subtitle => "ENVIRONMENTAL RECIRCULATION SYSTEM // PRIMARY DECK 1-3";
        public override string SystemCode => "SYS-ENV-0937";

        private struct GasComponent
        {
            public string Name;
            public string Formula;
            public float Percent;
            public float Fluctuation;
            public SKColor Color;
        }

        private readonly GasComponent[] _gasses = new GasComponent[]
        {
            new GasComponent { Name = "NITROGEN", Formula = "N2", Percent = 78.08f, Fluctuation = 0.12f, Color = new SKColor(0x00, 0xE5, 0xFF) },
            new GasComponent { Name = "OXYGEN", Formula = "O2", Percent = 20.95f, Fluctuation = 0.25f, Color = new SKColor(0x00, 0xFF, 0x66) },
            new GasComponent { Name = "ARGON", Formula = "Ar", Percent = 0.93f, Fluctuation = 0.05f, Color = new SKColor(0xFF, 0xB0, 0x00) },
            new GasComponent { Name = "CARBON DIOXIDE", Formula = "CO2", Percent = 0.04f, Fluctuation = 0.02f, Color = new SKColor(0xFF, 0x33, 0x66) }
        };

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;
            float contentWidth = contentRight - contentLeft;
            float contentHeight = contentBottom - contentTop;

            float colWidth = (contentWidth - 16f) / 2f;

            // LEFT PANEL: Circular Pie/Donut Chart of Gas Mix
            var leftRect = new SKRect(contentLeft, contentTop, contentLeft + colWidth, contentBottom);
            canvas.DrawTechBox(leftRect, palette.PrimaryDim, palette.DarkPanel, title: "GAS CHROMATOGRAPHY BREAKDOWN");

            float centerX = leftRect.MidX;
            float centerY = leftRect.MidY - 10f;
            float outerRadius = Math.Min(colWidth, contentHeight) * 0.30f;
            float innerRadius = outerRadius * 0.55f;

            // Draw Donut Segments
            float startAngle = ElapsedTime * 8f; // Slow rotation
            for (int i = 0; i < _gasses.Length; i++)
            {
                var gas = _gasses[i];
                float sweep = (gas.Percent / 100f) * 360f;
                float currentPercent = gas.Percent + MathF.Sin(ElapsedTime * 2f + i) * gas.Fluctuation;

                SKColor gasCol = palette.GetAccentColor(gas.Color);
                using var fillPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = outerRadius - innerRadius,
                    Color = palette.WithAlpha(gasCol, (byte)(palette.IsMonochrome ? (160 + i * 25) : 200))
                };

                float midRadius = (outerRadius + innerRadius) / 2f;
                var arcRect = new SKRect(centerX - midRadius, centerY - midRadius, centerX + midRadius, centerY + midRadius);
                canvas.DrawArc(arcRect, startAngle, sweep - 2f, false, fillPaint);

                startAngle += sweep;
            }

            // Inner circle text
            canvas.DrawTechText("101.3 kPa", centerX, centerY - 4f, 13f, palette.Primary, SKTextAlign.Center, bold: true);
            canvas.DrawTechText("CABIN PRESS", centerX, centerY + 12f, 8.5f, palette.PrimaryDim, SKTextAlign.Center);

            // Legend below chart
            float legendY = centerY + outerRadius + 22f;
            for (int i = 0; i < _gasses.Length; i++)
            {
                var gas = _gasses[i];
                float lx = leftRect.Left + 20f + (i % 2) * (colWidth * 0.48f);
                float ly = legendY + (i / 2) * 22f;

                SKColor gasCol = palette.GetAccentColor(gas.Color);
                using var swatchPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    Color = palette.WithAlpha(gasCol, (byte)(palette.IsMonochrome ? (160 + i * 25) : 255))
                };
                canvas.DrawRect(new SKRect(lx, ly - 8f, lx + 10f, ly + 2f), swatchPaint);

                float dynamicVal = gas.Percent + MathF.Sin(ElapsedTime * 2.5f + i) * gas.Fluctuation;
                canvas.DrawTechText($"{gas.Formula}: {dynamicVal:F2}%", lx + 16f, ly, 10f, palette.Primary);
            }

            // RIGHT PANEL: Subsystem Scrubber Telemetry & Flow Gauges
            var rightRect = new SKRect(leftRect.Right + 16f, contentTop, contentRight, contentBottom);
            canvas.DrawTechBox(rightRect, palette.PrimaryDim, palette.DarkPanel, title: "LIFE SUPPORT SUBSYSTEMS");

            float gaugeY = rightRect.Top + 34f;
            string[] metrics = { "O2 INJECTION VALVE", "CO2 SCRUBBER A", "CO2 SCRUBBER B", "ARGON REGULATOR", "TRACE VOC REMOVAL", "PARTICULATE FILTER" };
            float[] baselines = { 0.88f, 0.94f, 0.92f, 0.76f, 0.98f, 0.65f };

            for (int i = 0; i < metrics.Length; i++)
            {
                float val = baselines[i] + MathF.Sin(ElapsedTime * 1.5f + i * 1.2f) * 0.04f;
                val = Math.Clamp(val, 0f, 1f);

                canvas.DrawTechText(metrics[i], rightRect.Left + 16f, gaugeY, 10f, palette.Primary);
                canvas.DrawTechText($"{(val * 100f):F1}%", rightRect.Right - 16f, gaugeY, 10f, palette.PrimaryDim, SKTextAlign.Right);

                var barRect = new SKRect(rightRect.Left + 16f, gaugeY + 4f, rightRect.Right - 16f, gaugeY + 14f);
                SKColor activeCol = val > 0.70f ? palette.Primary : palette.WarningAmber;
                canvas.DrawSegmentedBar(barRect, val, 24, activeCol, palette.WithAlpha(palette.PrimaryDim, 40));

                gaugeY += 34f;
            }

            // Lower Status Alert Box
            var alertBox = new SKRect(rightRect.Left + 16f, gaugeY + 4f, rightRect.Right - 16f, rightRect.Bottom - 14f);
            canvas.DrawTechBox(alertBox, palette.PrimaryDim, palette.DarkPanel);
            canvas.DrawTechText("[ STATUS: NORMAL // O2/N2 MIX NOMINAL // 7 CREW CONSUMPTION ACTIVE ]", alertBox.MidX, alertBox.MidY + 4f, 9.5f, palette.Primary, SKTextAlign.Center, bold: true);
        }
    }
}

