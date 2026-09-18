using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class CryosleepStatusDisplay : BaseTelemetryDisplay
    {
        public override string Title => "HYPERSLEEP CHAMBER & STASIS POD TELEMETRY";
        public override string Subtitle => "CRYOGENIC SUSPENSION ARRAY // DECK B MAIN CRYOBAY // ALL PODS SEALED";
        public override string SystemCode => "CRYO-BAY-0808";

        private struct CryoPod
        {
            public string Name;
            public string Role;
            public int PodNumber;
            public float CoreTemp;       // Hypothermic core temp (e.g. 2.1 - 3.4 °C)
            public float StasisDepth;     // 98.0 - 99.8%
            public float StasisBpm;       // Cryogenic metabolic rate (e.g. 4 - 8 BPM)
            public string Status;
            public bool IsSynthetic;
            public bool IsFeline;
            public bool HasBioAnomaly;   // Kane: subtle irregular stasis rhythm
        }

        private readonly CryoPod[] _pods = new CryoPod[]
        {
            new CryoPod { Name = "DALLAS, A.", Role = "CAPTAIN", PodNumber = 1, CoreTemp = 2.4f, StasisDepth = 99.4f, StasisBpm = 5.0f, Status = "IN STASIS" },
            new CryoPod { Name = "RIPLEY, E.", Role = "WARRANT OFF", PodNumber = 2, CoreTemp = 2.1f, StasisDepth = 99.7f, StasisBpm = 4.5f, Status = "IN STASIS" },
            new CryoPod { Name = "KANE, G.", Role = "EXEC OFFICER", PodNumber = 3, CoreTemp = 3.8f, StasisDepth = 97.8f, StasisBpm = 8.5f, Status = "METAB FLUCT", HasBioAnomaly = true },
            new CryoPod { Name = "LAMBERT, J.", Role = "NAVIGATOR", PodNumber = 4, CoreTemp = 2.6f, StasisDepth = 99.1f, StasisBpm = 5.5f, Status = "IN STASIS" },
            new CryoPod { Name = "PARKER, J.", Role = "CHIEF ENG", PodNumber = 5, CoreTemp = 2.8f, StasisDepth = 98.9f, StasisBpm = 6.0f, Status = "IN STASIS" },
            new CryoPod { Name = "BRETT, S.", Role = "ENG TECH", PodNumber = 6, CoreTemp = 2.5f, StasisDepth = 99.2f, StasisBpm = 5.0f, Status = "IN STASIS" },
            new CryoPod { Name = "ASH", Role = "SCIENCE OFF", PodNumber = 7, CoreTemp = 18.0f, StasisDepth = 100.0f, StasisBpm = 0.0f, Status = "SYNTH STBY", IsSynthetic = true },
            new CryoPod { Name = "JONESY", Role = "FELIS CATUS", PodNumber = 8, CoreTemp = 4.2f, StasisDepth = 98.6f, StasisBpm = 11.0f, Status = "FELINE STASIS", IsFeline = true }
        };

        private float GetStasisWaveSample(float t, float bpm, bool isSynthetic, bool isFeline, bool hasAnomaly, int podIndex)
        {
            if (isSynthetic)
            {
                // Synthetic logic clock sync: periodic stepped digital pulse
                float clk = (t * 1.5f) % 1.0f;
                if (clk < 0.15f) return 0.85f;
                if (clk < 0.30f) return -0.40f;
                return 0.05f * MathF.Sin(t * 8f);
            }

            // Cryogenic hypothermic stasis ECG / metabolic rhythm
            float period = 60f / MathF.Max(1f, bpm);
            float phase = (t % period) / period; // 0.0 -> 1.0

            if (phase < 0.08f)
            {
                // Slow P wave
                return MathF.Sin(phase / 0.08f * MathF.PI) * 0.18f;
            }
            else if (phase < 0.14f)
            {
                // Hypothermic baseline
                return 0f;
            }
            else if (phase < 0.18f)
            {
                // Q dip
                return -MathF.Sin((phase - 0.14f) / 0.04f * MathF.PI) * 0.22f;
            }
            else if (phase < 0.25f)
            {
                // R spike (compressed in cold stasis)
                float amp = hasAnomaly ? 1.35f : 0.95f;
                return MathF.Sin((phase - 0.18f) / 0.07f * MathF.PI) * amp;
            }
            else if (phase < 0.30f)
            {
                // S dip
                return -MathF.Sin((phase - 0.25f) / 0.05f * MathF.PI) * 0.30f;
            }
            else if (phase < 0.38f)
            {
                // Osborn J wave (pathognomonic of hypothermic cryostasis)
                return MathF.Sin((phase - 0.30f) / 0.08f * MathF.PI) * 0.32f;
            }
            else if (phase < 0.55f)
            {
                // Prolonged T wave
                return MathF.Sin((phase - 0.38f) / 0.17f * MathF.PI) * 0.28f;
            }
            else
            {
                // Slow metabolic delta wave & faint thermal noise
                float delta = MathF.Sin(phase * MathF.PI * 2f) * 0.08f;
                if (hasAnomaly)
                {
                    // Irregular secondary micro-flutter in Kane's pod
                    delta += MathF.Sin(t * 14f) * 0.18f;
                }
                return delta;
            }
        }

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;
            float contentWidth = contentRight - contentLeft;
            float contentHeight = contentBottom - contentTop;

            int cols = 4;
            int rows = 2;
            float podGap = 8f;
            float podW = (contentWidth - (cols - 1) * podGap) / cols;
            float podH = (contentHeight - (rows - 1) * podGap) / rows;

            for (int i = 0; i < _pods.Length; i++)
            {
                int r = i / cols;
                int c = i % cols;

                float px = contentLeft + c * (podW + podGap);
                float py = contentTop + r * (podH + podGap);
                var podBox = new SKRect(px, py, px + podW, py + podH);

                var pod = _pods[i];
                DrawPodCard(canvas, podBox, pod, i, palette);
            }
        }

        private void DrawPodCard(SKCanvas canvas, SKRect podBox, CryoPod pod, int podIndex, ColorPalette palette)
        {
            // Alert color for anomalies (Kane) or synthetic standby (Ash)
            SKColor borderColor = pod.HasBioAnomaly
                ? (palette.IsMonochrome ? palette.Primary : palette.AlertRed)
                : palette.PrimaryDim;

            SKColor statusColor = pod.HasBioAnomaly
                ? (palette.IsMonochrome ? palette.Primary : palette.AlertRed)
                : (pod.IsSynthetic ? palette.CyanAccent : palette.Primary);

            canvas.DrawTechBox(podBox, borderColor, palette.DarkPanel);

            // 1. Pod Header
            canvas.DrawTechText($"POD #{pod.PodNumber:D2}", podBox.Left + 7f, podBox.Top + 13f, 9.5f, palette.Primary, bold: true);

            // Status Badge with pulsing stasis indicator
            float pulse = (MathF.Sin(ElapsedTime * 3f + podIndex) + 1f) * 0.5f;
            string statusBadge = pod.Status;
            canvas.DrawTechText(statusBadge, podBox.Right - 7f, podBox.Top + 13f, 8.5f, statusColor, SKTextAlign.Right, bold: true);

            // 2. Middle Section: Capsule Silhouette + Cryogenic Vitals Readout
            float midTop = podBox.Top + 18f;
            float midHeight = podBox.Height * 0.44f;
            var midRect = new SKRect(podBox.Left + 5f, midTop, podBox.Right - 5f, midTop + midHeight);

            // Left: Cryo Capsule Wireframe
            float capW = 38f;
            float capH = midHeight - 6f;
            var capRect = new SKRect(midRect.Left + 2f, midRect.Top + 3f, midRect.Left + 2f + capW, midRect.Top + 3f + capH);

            using var capPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 180),
                StrokeWidth = 1.1f
            };
            canvas.DrawRoundRect(capRect, 6f, 6f, capPaint);

            // Observation Window with frosty cryogenic tint
            var winRect = new SKRect(capRect.Left + 4f, capRect.Top + 5f, capRect.Right - 4f, capRect.Top + capH * 0.48f);
            using var winPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.CyanAccent, (byte)(80 + pulse * 60)),
                StrokeWidth = 0.9f
            };
            canvas.DrawRoundRect(winRect, 3f, 3f, winPaint);

            // Frost Shimmer Vapor Hash lines inside window
            using var frostPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.CyanAccent, 45),
                StrokeWidth = 0.7f
            };
            for (float fy = winRect.Top + 3f; fy < winRect.Bottom - 2f; fy += 4f)
            {
                float xOff = MathF.Sin(fy * 2f + ElapsedTime * 1.5f + podIndex) * 2f;
                canvas.DrawLine(winRect.Left + 2f + xOff, fy, winRect.Right - 2f - xOff, fy, frostPaint);
            }

            // Coolant level vertical bar on right side of capsule
            float coolantProgress = pod.IsSynthetic ? 1.0f : 0.92f + MathF.Sin(ElapsedTime * 0.5f + podIndex) * 0.05f;
            var coolantBar = new SKRect(capRect.Left + 6f, capRect.Bottom - 8f, capRect.Right - 6f, capRect.Bottom - 3f);
            using var coolFill = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = palette.WithAlpha(palette.PrimaryDim, 140)
            };
            canvas.DrawRect(coolantBar, coolFill);

            // Right of Capsule: Occupant Name, Role, and Cryo Telemetry
            float textLeft = capRect.Right + 8f;
            float ty = midRect.Top + 11f;

            canvas.DrawTechText(pod.Name, textLeft, ty, 9f, palette.Primary, bold: true);
            ty += 11f;
            canvas.DrawTechText($"[{pod.Role}]", textLeft, ty, 7.5f, palette.PrimaryDim);
            ty += 11f;

            // Live Core Temperature & Stasis Depth
            float liveTemp = pod.CoreTemp + MathF.Sin(ElapsedTime * 0.2f + podIndex) * 0.05f;
            canvas.DrawTechText($"CORE: +{liveTemp:F1}°C", textLeft, ty, 8f, palette.Primary);
            ty += 10f;
            canvas.DrawTechText($"STASIS: {pod.StasisDepth:F1}%", textLeft, ty, 7.5f, palette.PrimaryDim);
            ty += 10f;

            string rateLabel = pod.IsSynthetic ? "SYS-CLK: 100%" : $"METAB: {pod.StasisBpm:F0} BPM";
            canvas.DrawTechText(rateLabel, textLeft, ty, 7.5f, pod.HasBioAnomaly ? statusColor : palette.PrimaryDim);

            // 3. Bottom Section: Live Animated Cryogenic Vital Signs Waveform Box
            float waveTop = midRect.Bottom + 4f;
            var waveBox = new SKRect(podBox.Left + 5f, waveTop, podBox.Right - 5f, podBox.Bottom - 5f);
            canvas.DrawTechBox(waveBox, palette.WithAlpha(borderColor, 100), palette.Background);

            // Waveform Title & Baseline
            canvas.DrawTechText(pod.IsSynthetic ? "CYBER-SYNC WAVE" : "STASIS METABOLIC TRACE", waveBox.Left + 4f, waveBox.Top + 8f, 6.5f, palette.PrimaryFaint);

            float waveMidY = waveBox.Top + waveBox.Height * 0.58f;
            float waveHeight = waveBox.Height * 0.65f;
            float waveLeft = waveBox.Left + 4f;
            float waveRight = waveBox.Right - 4f;

            // Draw faint central baseline
            using var basePaint = new SKPaint
            {
                IsAntialias = true,
                Color = palette.WithAlpha(palette.PrimaryFaint, 30),
                StrokeWidth = 0.6f
            };
            canvas.DrawLine(waveLeft, waveMidY, waveRight, waveMidY, basePaint);

            // Draw Live Hypothermic ECG / Metabolic Waveform
            using var wavePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = statusColor,
                StrokeWidth = 1.2f
            };

            using var wavePath = new SKPath();
            float sampleStep = 1.5f;
            bool first = true;

            // Slow cryogenic medical sweep speed
            float scrollSpeed = 0.28f;

            for (float sx = waveLeft; sx <= waveRight; sx += sampleStep)
            {
                float timeOffset = (sx - waveLeft) / (waveRight - waveLeft) * 3.5f;
                float sampleTime = ElapsedTime * scrollSpeed + timeOffset + podIndex * 1.7f;
                float sampleVal = GetStasisWaveSample(sampleTime, pod.StasisBpm, pod.IsSynthetic, pod.IsFeline, pod.HasBioAnomaly, podIndex);

                float sy = waveMidY - sampleVal * (waveHeight * 0.45f);

                if (first)
                {
                    wavePath.MoveTo(sx, sy);
                    first = false;
                }
                else
                {
                    wavePath.LineTo(sx, sy);
                }
            }

            canvas.DrawPath(wavePath, wavePaint);

            // Live pulse blip at leading edge
            float leadingX = waveRight - 2f;
            float leadingTime = ElapsedTime * scrollSpeed + 3.5f + podIndex * 1.7f;
            float leadingVal = GetStasisWaveSample(leadingTime, pod.StasisBpm, pod.IsSynthetic, pod.IsFeline, pod.HasBioAnomaly, podIndex);
            float leadingY = waveMidY - leadingVal * (waveHeight * 0.45f);

            using var blipPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = palette.WhiteBright
            };
            canvas.DrawCircle(leadingX, leadingY, 2.0f, blipPaint);
        }
    }
}

