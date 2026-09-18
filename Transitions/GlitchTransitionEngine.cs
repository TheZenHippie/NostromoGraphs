using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Transitions
{
    public class GlitchTransitionEngine : ITransitionEffect
    {
        private bool _isActive;
        private float _elapsedTime;
        private float _totalDuration = 0.8f;
        private Action? _onMidpointAction;
        private bool _midpointFired;
        private readonly Random _rand = new Random();

        public bool IsActive => _isActive;
        public float Progress => _isActive ? Math.Clamp(_elapsedTime / _totalDuration, 0f, 1f) : 0f;

        public void StartTransition(Action onMidpointSwitch, float durationSeconds = 0.8f)
        {
            _isActive = true;
            _elapsedTime = 0f;
            _totalDuration = Math.Max(0.2f, durationSeconds);
            _onMidpointAction = onMidpointSwitch;
            _midpointFired = false;
        }

        public void Update(float deltaTime)
        {
            if (!_isActive) return;

            _elapsedTime += deltaTime;
            float p = Progress;

            if (p >= 0.5f && !_midpointFired)
            {
                _midpointFired = true;
                _onMidpointAction?.Invoke();
            }

            if (_elapsedTime >= _totalDuration)
            {
                _isActive = false;
                _elapsedTime = 0f;
                _midpointFired = false;
            }
        }

        public void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette, Action renderCurrentDisplay)
        {
            if (!_isActive)
            {
                renderCurrentDisplay();
                return;
            }

            float p = Progress;

            // Save canvas state
            canvas.Save();

            // Calculate glitch intensity envelope (peaks at midpoint 0.5)
            float intensity = 1.0f - MathF.Abs(p - 0.5f) * 2.0f; // 0 -> 1 -> 0

            // Apply horizontal screen jitter
            if (intensity > 0.1f)
            {
                float jitterX = (_rand.NextSingle() - 0.5f) * 16f * intensity;
                float jitterY = (_rand.NextSingle() - 0.5f) * 6f * intensity;
                canvas.Translate(jitterX, jitterY);
            }

            // Render underlying display
            renderCurrentDisplay();

            // Render Glitch & Screen Clear Overlays
            // 1. Horizontal Glitch Slice Displacement Bars
            int sliceCount = (int)(12 * intensity);
            using (var slicePaint = new SKPaint
            {
                IsAntialias = false,
                Style = SKPaintStyle.Fill,
                Color = palette.WithAlpha(palette.Primary, (byte)(180 * intensity))
            })
            {
                for (int i = 0; i < sliceCount; i++)
                {
                    float y = bounds.Top + _rand.NextSingle() * bounds.Height;
                    float h = 2f + _rand.NextSingle() * 14f;
                    float xOffset = (_rand.NextSingle() - 0.5f) * bounds.Width * 0.4f * intensity;
                    float w = bounds.Width * (0.2f + _rand.NextSingle() * 0.7f);
                    float x = bounds.Left + _rand.NextSingle() * (bounds.Width - w) + xOffset;

                    canvas.DrawRect(new SKRect(x, y, x + w, y + h), slicePaint);
                }
            }

            // 2. High-energy CRT Screen Clear Phosphor Beam Sweep
            if (intensity > 0.4f)
            {
                float beamY = bounds.Top + (p * bounds.Height * 2f) % bounds.Height;
                using var beamPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    Color = palette.WithAlpha(SKColors.White, (byte)(200 * intensity))
                };
                canvas.DrawRect(new SKRect(bounds.Left, beamY - 4f, bounds.Right, beamY + 4f), beamPaint);

                // Phosphor flash bloom
                using var flashPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    Color = palette.WithAlpha(palette.Primary, (byte)(90 * intensity))
                };
                canvas.DrawRect(bounds, flashPaint);
            }

            // 3. Hexadecimal / Binary Telemetry Noise Burst at Midpoint
            if (intensity > 0.6f)
            {
                using var fontPaint = new SKPaint
                {
                    IsAntialias = true,
                    Color = palette.WithAlpha(palette.Primary, (byte)(220 * intensity)),
                    TextSize = 10f,
                    Typeface = SkiaUtils.MonospaceTypeface,
                    FakeBoldText = true
                };

                string[] hexChunks = { "CLEAR_SCREEN", "0x937_PURGE", "RELOAD_VECTORS", "MUTHUR//OVERRIDE", "BUFFER_SYNC", "0xFF00AA" };
                for (int i = 0; i < 6; i++)
                {
                    float tx = bounds.Left + 20f + _rand.NextSingle() * (bounds.Width - 160f);
                    float ty = bounds.Top + 30f + _rand.NextSingle() * (bounds.Height - 60f);
                    canvas.DrawText(hexChunks[i % hexChunks.Length], tx, ty, fontPaint);
                }
            }

            // 4. Analog Scanline Tear
            using (var tearPaint = new SKPaint
            {
                IsAntialias = false,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.Background, (byte)(230 * intensity)),
                StrokeWidth = 3f
            })
            {
                for (int i = 0; i < 4; i++)
                {
                    float ty = bounds.Top + _rand.NextSingle() * bounds.Height;
                    canvas.DrawLine(bounds.Left, ty, bounds.Right, ty, tearPaint);
                }
            }

            canvas.Restore();
        }
    }
}

