using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class CrewmemberVitalsDisplay : BaseTelemetryDisplay
    {
        public override string Title => "CREW BIO-TELEMETRY & VITAL SIGNS MONITOR";
        public override string Subtitle => "USCSS NOSTROMO // 7 HUMAN COMPLEMENT + 1 FELINE // LIFE POD LINK";
        public override string SystemCode => "BIO-MED-7714";

        private struct CrewMember
        {
            public string Name;
            public string Rank;
            public float BaseBpm;
            public float Temp;
            public float SpO2;
            public string Status;
            public bool IsAlert;
            public bool IsFeline;
            public bool IsSynthetic;
        }

        private readonly CrewMember[] _crew = new CrewMember[]
        {
            new CrewMember { Name = "DALLAS, A.", Rank = "CAPTAIN", BaseBpm = 68f, Temp = 37.1f, SpO2 = 99f, Status = "NOMINAL", IsAlert = false },
            new CrewMember { Name = "RIPLEY, E.", Rank = "WARRANT OFFICER", BaseBpm = 72f, Temp = 36.9f, SpO2 = 99f, Status = "ACTIVE", IsAlert = false },
            new CrewMember { Name = "KANE, G.", Rank = "EXECUTIVE OFFICER", BaseBpm = 96f, Temp = 38.4f, SpO2 = 95f, Status = "ELEVATED BPM", IsAlert = true },
            new CrewMember { Name = "LAMBERT, J.", Rank = "NAVIGATOR", BaseBpm = 82f, Temp = 36.8f, SpO2 = 98f, Status = "RESTING", IsAlert = false },
            new CrewMember { Name = "PARKER, J.", Rank = "CHIEF ENGINEER", BaseBpm = 75f, Temp = 37.2f, SpO2 = 98f, Status = "ACTIVE", IsAlert = false },
            new CrewMember { Name = "BRETT, S.", Rank = "ENGINEERING TECH", BaseBpm = 64f, Temp = 36.7f, SpO2 = 97f, Status = "STEADY", IsAlert = false },
            new CrewMember { Name = "ASH", Rank = "SCIENCE OFFICER", BaseBpm = 60f, Temp = 36.6f, SpO2 = 100f, Status = "SYNTH_LOCK", IsAlert = false, IsSynthetic = true },
            new CrewMember { Name = "JONESY", Rank = "SHIP'S CAT / FELINE", BaseBpm = 135f, Temp = 38.6f, SpO2 = 99f, Status = "AWAKE", IsAlert = false, IsFeline = true }
        };

        private float GetEcgSample(float t, float bpm, bool isElevated)
        {
            float period = 60f / bpm;
            float phase = (t % period) / period; // 0.0 to 1.0

            if (phase < 0.12f)
            {
                // P wave
                return MathF.Sin(phase / 0.12f * MathF.PI) * 0.25f;
            }
            else if (phase < 0.18f)
            {
                // PR segment
                return 0f;
            }
            else if (phase < 0.22f)
            {
                // Q dip
                return -MathF.Sin((phase - 0.18f) / 0.04f * MathF.PI) * 0.35f;
            }
            else if (phase < 0.28f)
            {
                // R peak
                return MathF.Sin((phase - 0.22f) / 0.06f * MathF.PI) * (isElevated ? 1.4f : 1.1f);
            }
            else if (phase < 0.33f)
            {
                // S dip
                return -MathF.Sin((phase - 0.28f) / 0.05f * MathF.PI) * 0.45f;
            }
            else if (phase < 0.45f)
            {
                // ST segment
                return 0f;
            }
            else if (phase < 0.62f)
            {
                // T wave
                return MathF.Sin((phase - 0.45f) / 0.17f * MathF.PI) * 0.40f;
            }
            else
            {
                // Baseline with slight muscle noise
                return (SkiaUtils.PseudoNoise(phase * 40f, t) - 0.5f) * 0.05f;
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

            int cols = 2;
            int rows = 4;
            float cardGap = 8f;
            float cardWidth = (contentWidth - cardGap) / cols;
            float cardHeight = (contentHeight - (rows - 1) * cardGap) / rows;

            for (int i = 0; i < _crew.Length; i++)
            {
                int r = i / cols;
                int c = i % cols;

                float x = contentLeft + c * (cardWidth + cardGap);
                float y = contentTop + r * (cardHeight + cardGap);
                var cardRect = new SKRect(x, y, x + cardWidth, y + cardHeight);

                var member = _crew[i];
                float currentBpm = member.BaseBpm + MathF.Sin(ElapsedTime * 0.3f + i) * 1.5f;
                float currentTemp = member.Temp + MathF.Sin(ElapsedTime * 0.15f + i) * 0.08f;

                SKColor cardBorder = member.IsAlert ? palette.AlertRed : palette.PrimaryDim;
                SKColor nameColor = member.IsAlert ? palette.AlertRed : palette.Primary;

                canvas.DrawTechBox(cardRect, cardBorder, palette.DarkPanel);

                // Member Name & Rank
                canvas.DrawTechText($"[{member.Rank}] {member.Name}", cardRect.Left + 8f, cardRect.Top + 14f, 10f, nameColor, bold: true);
                
                // Right status badge
                canvas.DrawTechText(member.Status, cardRect.Right - 8f, cardRect.Top + 14f, 9f, cardBorder, SKTextAlign.Right, bold: member.IsAlert);

                // Telemetry metrics
                string vitalsStr = member.IsFeline 
                    ? $"HR: {currentBpm:F0} BPM | TEMP: {currentTemp:F1}°C | FELIS CATUS" 
                    : member.IsSynthetic 
                    ? $"SYNC: 100% | CORE: {currentTemp:F1}°C | CYBERNETIC LINK"
                    : $"HR: {currentBpm:F0} BPM | TEMP: {currentTemp:F1}°C | SPO2: {member.SpO2:F0}%";

                canvas.DrawTechText(vitalsStr, cardRect.Left + 8f, cardRect.Top + 27f, 8.5f, palette.PrimaryDim);

                // ECG Trace Area
                float ecgLeft = cardRect.Left + 8f;
                float ecgRight = cardRect.Right - 8f;
                float ecgMidY = cardRect.Bottom - 14f;
                float ecgHeight = cardHeight * 0.38f;

                // Draw faint ECG baseline grid
                using var ecgGridPaint = new SKPaint
                {
                    IsAntialias = true,
                    Color = palette.WithAlpha(palette.PrimaryFaint, 30),
                    StrokeWidth = 0.5f
                };
                canvas.DrawLine(ecgLeft, ecgMidY, ecgRight, ecgMidY, ecgGridPaint);

                // Draw Live ECG Waveform
                using var wavePaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    Color = member.IsAlert ? palette.AlertRed : (member.IsSynthetic ? palette.CyanAccent : palette.Primary),
                    StrokeWidth = 1.4f
                };

                using var wavePath = new SKPath();
                float sampleStep = 2f;
                bool first = true;

                // Slower authentic medical sweep speed (0.35x)
                float scrollSpeed = 0.35f;

                for (float sx = ecgLeft; sx <= ecgRight; sx += sampleStep)
                {
                    float timeOffset = (sx - ecgLeft) / (ecgRight - ecgLeft) * 2.8f;
                    float sampleTime = ElapsedTime * scrollSpeed + timeOffset;
                    float ecgVal = GetEcgSample(sampleTime, currentBpm, member.IsAlert);

                    float sy = ecgMidY - ecgVal * (ecgHeight * 0.5f);

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
            }
        }
    }
}

