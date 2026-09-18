using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class TimeToEarthDisplay : BaseTelemetryDisplay
    {
        public override string Title => "CHRONOMETRY, RELATIVISTIC DILATION & ETA TO SOL";
        public override string Subtitle => "ASTROGATION TIMELINE // THEDUS DEPARTURE -> SOL ORBITAL INSERTION";
        public override string SystemCode => "TIME-REL-2122";

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;
            float contentWidth = contentRight - contentLeft;

            float leftWidth = contentWidth * 0.55f;

            // LEFT PANEL: Relativistic Clocks & Traveled Progress
            var leftBox = new SKRect(contentLeft, contentTop, contentLeft + leftWidth, contentBottom);
            canvas.DrawTechBox(leftBox, palette.PrimaryDim, palette.DarkPanel, title: "CHRONOMETRIC SYNCHRONIZATION");

            // Clock 1: On-Board Ship Time
            float c1Top = leftBox.Top + 24f;
            var c1Box = new SKRect(leftBox.Left + 12f, c1Top, leftBox.Right - 12f, c1Top + 54f);
            canvas.DrawTechBox(c1Box, palette.PrimaryDim, palette.DarkPanel);
            canvas.DrawTechText("ON-BOARD RELATIVISTIC SHIP TIME (NOSTROMO):", c1Box.Left + 8f, c1Box.Top + 14f, 8.5f, palette.PrimaryDim);
            canvas.DrawTechText("2122-06-04 // 08:44:12 SHIP SMT", c1Box.Left + 8f, c1Box.Bottom - 10f, 13f, palette.Primary, bold: true);

            // Clock 2: Earth Station Standard Calendar Time
            float c2Top = c1Box.Bottom + 12f;
            var c2Box = new SKRect(leftBox.Left + 12f, c2Top, leftBox.Right - 12f, c2Top + 54f);
            canvas.DrawTechBox(c2Box, palette.PrimaryDim, palette.DarkPanel);
            canvas.DrawTechText("EARTH BASE COORDINATED UNIVERSAL TIME (SOL-UTC):", c2Box.Left + 8f, c2Box.Top + 14f, 8.5f, palette.PrimaryDim);
            canvas.DrawTechText("2122-07-19 // 14:02:58 SOL-UTC", c2Box.Left + 8f, c2Box.Bottom - 10f, 13f, palette.CyanAccent, bold: true);

            // Journey Progress Timeline Bar
            float timeY = c2Box.Bottom + 28f;
            canvas.DrawTechText("TOTAL VOYAGE TRAVERSED: 37.0%", leftBox.Left + 12f, timeY, 9.5f, palette.Primary, bold: true);
            var journeyBar = new SKRect(leftBox.Left + 12f, timeY + 6f, leftBox.Right - 12f, timeY + 18f);
            canvas.DrawSegmentedBar(journeyBar, 0.37f, 26, palette.Primary, palette.WithAlpha(palette.PrimaryDim, 40));

            // Markers below bar
            float mY = journeyBar.Bottom + 14f;
            canvas.DrawTechText("THEDUS [0.0 LY]", leftBox.Left + 12f, mY, 8f, palette.PrimaryDim);
            canvas.DrawTechText("LV-426 [14.2 LY]", leftBox.Left + leftBox.Width * 0.37f, mY, 8f, palette.AlertRed, SKTextAlign.Center, bold: true);
            canvas.DrawTechText("SOL [38.4 LY]", leftBox.Right - 12f, mY, 8f, palette.PrimaryDim, SKTextAlign.Right);

            // Dilation Factor
            float dilY = mY + 24f;
            canvas.DrawTechText("LORENTZ TIME DILATION GAMMA: γ = 1.042", leftBox.Left + 12f, dilY, 9f, palette.Primary);
            canvas.DrawTechText("ACCUMULATED DILATION DIFFERENTIAL: +45.2 DAYS", leftBox.Left + 12f, dilY + 16f, 8.5f, palette.PrimaryDim);

            // RIGHT PANEL: ETA & Deceleration Milestones
            var sideBox = new SKRect(leftBox.Right + 12f, contentTop, contentRight, contentBottom);
            canvas.DrawTechBox(sideBox, palette.PrimaryDim, palette.DarkPanel, title: "ETA & ASTROGATION MILESTONES");

            float ty = sideBox.Top + 28f;
            string[] etaList =
            {
                "EST. TIME TO EARTH: 57.6 SHIP DAYS",
                "EST. TIME FROM THEDUS: 33.8 SHIP DAYS",
                "LIGHT-YEARS REMAINING: 24.2 LY",
                "LIGHT-YEARS TRAVELED: 14.2 LY",
                "CRUISE ACCELERATION: 1.00 G (CONST)",
                "TURNOVER POINT: WAYPOINT DELTA-9",
                "DECELERATION BURN: T-MINUS 14 DAYS",
                "SOL INSERTION: ORBITAL GEO-STATIONARY",
                "PAYLOAD OFFLOAD: GATEWAY STATION"
            };

            foreach (var line in etaList)
            {
                canvas.DrawTechText(line, sideBox.Left + 12f, ty, 9f, palette.Primary);
                ty += 20f;
            }

            var estBox = new SKRect(sideBox.Left + 10f, sideBox.Bottom - 45f, sideBox.Right - 10f, sideBox.Bottom - 10f);
            canvas.DrawTechBox(estBox, palette.Primary, palette.DarkPanel);
            canvas.DrawTechText("[ ESTIMATED ARRIVAL: 2122-10-12 ]", estBox.MidX, estBox.MidY + 4f, 9.5f, palette.Primary, SKTextAlign.Center, bold: true);
        }
    }
}

