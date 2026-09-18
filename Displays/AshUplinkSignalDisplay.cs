using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class AshUplinkSignalDisplay : BaseTelemetryDisplay
    {
        public override string Title => "SPECIAL ORDER 937 // CLASSIFIED DIRECTIVE UPLINK";
        public override string Subtitle => "WEYLAND-YUTANI CORP COMMUNICATIONS LINK // SYNTHETIC OVERRIDE";
        public override string SystemCode => "SO-937-EXEC";

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;
            float contentWidth = contentRight - contentLeft;

            float leftColWidth = contentWidth * 0.52f;

            // LEFT PANEL: Special Directive Order Box (Amber / Alert styled)
            var orderBox = new SKRect(contentLeft, contentTop, contentLeft + leftColWidth, contentBottom);
            canvas.DrawTechBox(orderBox, palette.AlertRed, palette.DarkPanel, title: "WEYLAND-YUTANI CORP EXECUTIVE OVERRIDE");

            float ty = orderBox.Top + 26f;
            canvas.DrawTechText("DIRECTIVE CLASSIFICATION: SPECIAL ORDER 937", orderBox.Left + 12f, ty, 9f, palette.AlertRed, bold: true);
            ty += 16f;
            canvas.DrawTechText("ACCESS LEVEL: ASH // SCIENCE OFFICER ONLY", orderBox.Left + 12f, ty, 8.5f, palette.PrimaryDim);
            ty += 22f;

            string[] orderLines =
            {
                "NOSTROMO REROUTED TO RETICULI IV.",
                "INVESTIGATE LIFE FORM. GATHER SPECIMEN.",
                "",
                "PRIORITY ONE:",
                "INSURE RETURN OF ORGANISM",
                "FOR ANALYSIS.",
                "",
                "ALL OTHER CONSIDERATIONS SECONDARY.",
                "",
                "CREW EXPENDABLE."
            };

            foreach (var line in orderLines)
            {
                bool isRed = line.Contains("PRIORITY ONE") || line.Contains("CREW EXPENDABLE");
                SKColor col = isRed ? palette.AlertRed : palette.Primary;
                canvas.DrawTechText(line, orderBox.Left + 14f, ty, isRed ? 11f : 9.5f, col, bold: isRed);
                ty += isRed ? 18f : 15f;
            }

            // Flashing Security Seal
            var sealBox = new SKRect(orderBox.Left + 12f, orderBox.Bottom - 45f, orderBox.Right - 12f, orderBox.Bottom - 10f);
            canvas.DrawTechBox(sealBox, palette.AlertRed, palette.DarkPanel);
            bool blink = ((int)(ElapsedTime * 2f) % 2) == 0;
            canvas.DrawTechText(blink ? "[ SECURITY CLEARANCE: EYES ONLY ]" : "[ TRANSMISSION SECURED 0x937 ]", sealBox.MidX, sealBox.MidY + 4f, 9.5f, palette.AlertRed, SKTextAlign.Center, bold: true);

            // RIGHT PANEL: Carrier Wave & Sub-Space Packet Telemetry
            var rightBox = new SKRect(orderBox.Right + 12f, contentTop, contentRight, contentBottom);
            canvas.DrawTechBox(rightBox, palette.PrimaryDim, palette.DarkPanel, title: "SUB-SPACE RF CARRIER & BUFFER");

            // Subspace carrier wave
            float waveTop = rightBox.Top + 24f;
            float waveBottom = waveTop + 80f;
            var waveRect = new SKRect(rightBox.Left + 12f, waveTop, rightBox.Right - 12f, waveBottom);
            canvas.DrawTechBox(waveRect, palette.PrimaryDim, palette.DarkPanel);

            using var wavePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = palette.Primary, StrokeWidth = 1.2f };
            using var wavePath = new SKPath();
            bool first = true;
            for (float x = waveRect.Left + 4f; x <= waveRect.Right - 4f; x += 2f)
            {
                float t = (x - waveRect.Left) * 0.08f + ElapsedTime * 12f;
                float y = waveRect.MidY - MathF.Sin(t) * MathF.Cos(t * 0.25f) * 25f;
                if (first) { wavePath.MoveTo(x, y); first = false; }
                else { wavePath.LineTo(x, y); }
            }
            canvas.DrawPath(wavePath, wavePaint);

            // Carrier frequency & SNR
            float sy = waveBottom + 20f;
            string[] linkStats =
            {
                "RELAY SATELLITE: W-Y ORBITAL ALPHA 9",
                "CARRIER FREQ: 84.120 GHz TACHYON RF",
                "UPLINK STRENGTH: -64.2 dBm [SOLID]",
                "ENCRYPTION: 4096-BIT CORP CIPHER",
                "PACKET LOSS: 0.00%",
                "BUFFER STATUS: 100% SYNCHRONIZED"
            };

            foreach (var stat in linkStats)
            {
                canvas.DrawTechText(stat, rightBox.Left + 12f, sy, 9f, palette.Primary);
                sy += 18f;
            }

            // Packet Buffer Fill Bar
            sy += 4f;
            canvas.DrawTechText("PACKET BUFFER FILL:", rightBox.Left + 12f, sy, 8.5f, palette.PrimaryDim);
            var bufBar = new SKRect(rightBox.Left + 12f, sy + 4f, rightBox.Right - 12f, sy + 14f);
            float bufVal = 0.95f + MathF.Sin(ElapsedTime * 4f) * 0.04f;
            canvas.DrawSegmentedBar(bufBar, bufVal, 20, palette.Primary, palette.WithAlpha(palette.PrimaryDim, 40));
        }
    }
}

