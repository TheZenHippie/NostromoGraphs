using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public interface ITelemetryDisplay
    {
        string Title { get; }
        string Subtitle { get; }
        string SystemCode { get; }

        void Initialize();
        void Update(float deltaTime);
        void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette);
    }
}

