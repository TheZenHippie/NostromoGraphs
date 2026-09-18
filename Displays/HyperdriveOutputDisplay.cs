using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class HyperdriveOutputDisplay : BaseTelemetryDisplay
    {
        public override string Title => "PROPULSION, FTL HYPERDRIVE & 2.8 TW REACTOR";
        public override string Subtitle => "YUTANI T-72 BIMODAL FUSION CORE // TACHYON FLUX HARMONICS";
        public override string SystemCode => "ENG-PROP-2800";

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;
            float contentWidth = contentRight - contentLeft;

            float leftColWidth = contentWidth * 0.55f;

            // LEFT PANEL: Dual Circular Gauges & Thruster Vector Balance
            var leftBox = new SKRect(contentLeft, contentTop, contentLeft + leftColWidth, contentBottom);
            canvas.DrawTechBox(leftBox, palette.PrimaryDim, palette.DarkPanel, title: "REACTOR GAUGES & THRUST BALANCE");

            // Gauge 1: Fusion Core Output (2.8 TW)
            float g1CenterX = leftBox.Left + leftBox.Width * 0.28f;
            float gCenterY = leftBox.Top + 75f;
            float gRadius = 45f;
            float coreVal = 0.92f + MathF.Sin(ElapsedTime * 1.8f) * 0.03f;
            canvas.DrawCircularGauge(new SKPoint(g1CenterX, gCenterY), gRadius, coreVal, 135f, 270f, palette.Primary, palette.WithAlpha(palette.PrimaryDim, 40), strokeWidth: 7f);
            canvas.DrawTechText($"{(coreVal * 2.8f):F2} TW", g1CenterX, gCenterY - 2f, 11f, palette.Primary, SKTextAlign.Center, bold: true);
            canvas.DrawTechText("CORE OUTPUT", g1CenterX, gCenterY + 12f, 8f, palette.PrimaryDim, SKTextAlign.Center);

            // Gauge 2: Tachyon Drive Flux (FTL Harmonics)
            float g2CenterX = leftBox.Left + leftBox.Width * 0.72f;
            float fluxVal = 0.88f + MathF.Cos(ElapsedTime * 2.2f) * 0.04f;
            canvas.DrawCircularGauge(new SKPoint(g2CenterX, gCenterY), gRadius, fluxVal, 135f, 270f, palette.CyanAccent, palette.WithAlpha(palette.PrimaryDim, 40), strokeWidth: 7f);
            canvas.DrawTechText($"{(fluxVal * 100f):F1}%", g2CenterX, gCenterY - 2f, 11f, palette.CyanAccent, SKTextAlign.Center, bold: true);
            canvas.DrawTechText("FTL FLUX HARMONIC", g2CenterX, gCenterY + 12f, 8f, palette.PrimaryDim, SKTextAlign.Center);

            // Sub-light Thruster Balance Bars
            float thrusterY = gCenterY + gRadius + 30f;
            canvas.DrawTechText("SUB-LIGHT RCS & VECTOR MANIFOLDS:", leftBox.Left + 14f, thrusterY, 9f, palette.PrimaryDim);
            thrusterY += 12f;

            (string Manifold, float Val)[] thrusters =
            {
                ("PORT CHEMICAL THRUSTER", 0.78f),
                ("STARBOARD CHEMICAL THRUSTER", 0.79f),
                ("DORSAL ATTITUDE NOZZLE", 0.64f),
                ("VENTRAL DECCEL ROCKETS", 0.91f)
            };

            for (int i = 0; i < thrusters.Length; i++)
            {
                var t = thrusters[i];
                float dVal = t.Val + MathF.Sin(ElapsedTime * 3f + i) * 0.03f;
                canvas.DrawTechText(t.Manifold, leftBox.Left + 14f, thrusterY + 10f, 8.5f, palette.Primary);
                canvas.DrawTechText($"{(dVal * 100f):F1}%", leftBox.Right - 14f, thrusterY + 10f, 8.5f, palette.PrimaryDim, SKTextAlign.Right);

                var barRect = new SKRect(leftBox.Left + 14f, thrusterY + 14f, leftBox.Right - 14f, thrusterY + 22f);
                canvas.DrawSegmentedBar(barRect, dVal, 22, palette.Primary, palette.WithAlpha(palette.PrimaryDim, 40));
                thrusterY += 26f;
            }

            // RIGHT PANEL: Engine Health & Deuterium Injection
            var rightBox = new SKRect(leftBox.Right + 12f, contentTop, contentRight, contentBottom);
            canvas.DrawTechBox(rightBox, palette.PrimaryDim, palette.DarkPanel, title: "ENGINEERING SUBSYSTEM STATUS");

            float ty = rightBox.Top + 28f;
            string[] engMetrics =
            {
                "DEUTERIUM/TRITIUM FEED: 99.4%",
                "MAGNETIC BOTTLE: 14.8 TESLA",
                "PLASMA TEMP: 1.42 x 10^8 K",
                "COOLANT LOOP A: 48.2°C [STABLE]",
                "COOLANT LOOP B: 49.0°C [STABLE]",
                "GRAV-DRIVE COMPRESSION: 1.002",
                "EXHAUST VELOCITY: 0.12 C",
                "TACHYON FIELD STABILITY: NOMINAL",
                "EMERGENCY SCRAM: ARMED [READY]"
            };

            foreach (var metric in engMetrics)
            {
                canvas.DrawTechText(metric, rightBox.Left + 12f, ty, 9f, palette.Primary);
                ty += 19f;
            }

            // Containment Box
            var alertBox = new SKRect(rightBox.Left + 10f, rightBox.Bottom - 45f, rightBox.Right - 10f, rightBox.Bottom - 10f);
            canvas.DrawTechBox(alertBox, palette.Primary, palette.DarkPanel);
            canvas.DrawTechText("[ REACTOR CONTAINMENT: 100% NOMINAL ]", alertBox.MidX, alertBox.MidY + 4f, 9f, palette.Primary, SKTextAlign.Center, bold: true);
        }
    }
}

