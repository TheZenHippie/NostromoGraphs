using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Transitions
{
    public interface ITransitionEffect
    {
        bool IsActive { get; }
        float Progress { get; } // 0.0 to 1.0
        void StartTransition(Action onMidpointSwitch, float durationSeconds = 0.8f);
        void Update(float deltaTime);
        void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette, Action renderCurrentDisplay);
    }
}

