using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class AlienSignalAnalysisDisplay : BaseTelemetryDisplay
    {
        public override string Title => "DEEP SPACE ACOUSTIC BEACON & SIGNAL DEMODULATION";
        public override string Subtitle => "EXTRATERRESTRIAL CARRIER WAVE // HARMONIC SPECTRUM ANALYSIS";
        public override string SystemCode => "SIG-XENO-0937";

        private readonly float[] _spectrumPeaks = new float[32];
        private readonly string[] _hexPackets = new string[]
        {
            "0xAA 0x93 0x70 0x11 0x00 0xFF 0x42 0x60 // BEACON CARRIER SYNC",
            "0x88 0x21 0xFE 0x19 0x79 0xDE 0xAD 0x01 // ACOUSTIC MODULATION",
            "0x00 0x93 0x07 0x00 0x53 0x4F 0x53 0x21 // WARNING: NOT SOS",
            "0xC0 0xDE 0x77 0x1A 0x8F 0x33 0x12 0x99 // DIRECTIVE INTERCEPT",
            "0x57 0x45 0x59 0x2D 0x59 0x55 0x54 0x21 // TRANSLATION CONFIRMED"
        };

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;
            float contentWidth = contentRight - contentLeft;

            float leftColWidth = contentWidth * 0.58f;

            // TOP-LEFT: Dual Channel Oscilloscope
            float oscHeight = (contentBottom - contentTop) * 0.46f;
            var oscBox = new SKRect(contentLeft, contentTop, contentLeft + leftColWidth, contentTop + oscHeight);
            canvas.DrawTechBox(oscBox, palette.PrimaryDim, palette.DarkPanel, title: "ACOUSTIC CARRIER WAVE OSCILLOSCOPE");

            float oscMidY = oscBox.MidY;
            float oscHalfH = oscBox.Height * 0.35f;

            // Draw center baseline
            using var basePaint = new SKPaint { IsAntialias = true, Color = palette.WithAlpha(palette.PrimaryFaint, 40), StrokeWidth = 0.8f };
            canvas.DrawLine(oscBox.Left + 10f, oscMidY, oscBox.Right - 10f, oscMidY, basePaint);

            // Channel 1: High frequency carrier
            using var wavePaint1 = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = palette.Primary, StrokeWidth = 1.4f };
            using var path1 = new SKPath();
            bool first1 = true;
            for (float x = oscBox.Left + 10f; x <= oscBox.Right - 10f; x += 2f)
            {
                float t = (x - oscBox.Left) * 0.04f + ElapsedTime * 8f;
                // Harmonic acoustic resonance modulated by low frequency beacon envelope
                float envelope = MathF.Sin(t * 0.3f);
                float y = oscMidY - (MathF.Sin(t * 3f) * 0.6f + MathF.Sin(t * 7.5f) * 0.3f * envelope) * (oscHalfH * 0.8f);

                if (first1) { path1.MoveTo(x, y); first1 = false; }
                else { path1.LineTo(x, y); }
            }
            canvas.DrawPath(path1, wavePaint1);

            canvas.DrawTechText("CH 1: 14.225 MHz CARRIER [ACTIVE BEACON]", oscBox.Left + 12f, oscBox.Bottom - 8f, 8.5f, palette.PrimaryDim);

            // BOTTOM-LEFT: 32-Band FFT Frequency Spectrum
            float fftTop = oscBox.Bottom + 10f;
            var fftBox = new SKRect(contentLeft, fftTop, contentLeft + leftColWidth, contentBottom);
            canvas.DrawTechBox(fftBox, palette.PrimaryDim, palette.DarkPanel, title: "32-BAND FFT HARMONIC SPECTRUM");

            int bands = 32;
            float barAreaW = fftBox.Width - 24f;
            float barW = barAreaW / bands - 2f;
            float barBottom = fftBox.Bottom - 22f;
            float maxBarH = fftBox.Height - 48f;

            for (int i = 0; i < bands; i++)
            {
                float bx = fftBox.Left + 12f + i * (barW + 2f);
                // Frequency distribution peaking around acoustic harmonic bands (Alien beacon profile)
                float targetH = (MathF.Sin(ElapsedTime * 4f + i * 0.4f) * 0.35f + MathF.Cos(ElapsedTime * 2f + i * 0.8f) * 0.35f + 0.4f);
                if (i == 12 || i == 13 || i == 24) targetH += 0.4f; // Resonant peaks
                targetH = Math.Clamp(targetH, 0.05f, 1.0f) * maxBarH;

                _spectrumPeaks[i] = Math.Max(targetH, _spectrumPeaks[i] - 1.2f);

                var barRect = new SKRect(bx, barBottom - targetH, bx + barW, barBottom);
                using var barPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = palette.Primary };
                canvas.DrawRect(barRect, barPaint);

                // Peak hold dot
                using var peakPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = palette.WhiteBright };
                canvas.DrawRect(new SKRect(bx, barBottom - _spectrumPeaks[i] - 2f, bx + barW, barBottom - _spectrumPeaks[i]), peakPaint);
            }

            canvas.DrawTechText("0 Hz ------------------ 22.05 kHz ------------------ 96 kHz", fftBox.MidX, fftBox.Bottom - 8f, 8f, palette.PrimaryDim, SKTextAlign.Center);

            // RIGHT PANEL: Decryption Matrix & Telemetry
            var rightBox = new SKRect(oscBox.Right + 12f, contentTop, contentRight, contentBottom);
            canvas.DrawTechBox(rightBox, palette.PrimaryDim, palette.DarkPanel, title: "SIGNAL PARSING & CIPHER");

            float ty = rightBox.Top + 28f;
            string[] sigTelemetry =
            {
                "SOURCE: NON-HUMAN TRANSMITTER",
                "BEACON EMISSION: PULSED HARMONIC",
                "FREQUENCY: 14.225 MHz RF / ACOUSTIC",
                "SIGNAL REPETITION: EVERY 12.00 SEC",
                "EST. TRANSMISSION AGE: > 2000 YRS",
                "MUTHUR TRANSLATION: IN PROGRESS",
                "STATUS: WARNING NOT S.O.S."
            };

            foreach (var item in sigTelemetry)
            {
                canvas.DrawTechText(item, rightBox.Left + 12f, ty, 9f, palette.Primary);
                ty += 18f;
            }

            // Live Hex Packets
            ty += 6f;
            canvas.DrawTechText("RAW TELEMETRY STREAM:", rightBox.Left + 12f, ty, 8.5f, palette.PrimaryDim);
            ty += 14f;

            for (int i = 0; i < _hexPackets.Length; i++)
            {
                canvas.DrawTechText(_hexPackets[i], rightBox.Left + 12f, ty, 8f, palette.PrimaryFaint);
                ty += 15f;
            }

            // Warning Box at Bottom
            var warnBox = new SKRect(rightBox.Left + 10f, rightBox.Bottom - 44f, rightBox.Right - 10f, rightBox.Bottom - 10f);
            canvas.DrawTechBox(warnBox, palette.AlertRed, palette.DarkPanel);
            canvas.DrawTechText("[ DANGER: UNKNOWN BIO-SIGNAL ]", warnBox.MidX, warnBox.MidY + 4f, 9.5f, palette.AlertRed, SKTextAlign.Center, bold: true);
        }
    }
}

