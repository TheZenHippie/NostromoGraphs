using System;
using System.Collections.Generic;
using SkiaSharp;
using NostromoGraphs.Common;
using NostromoGraphs.Transitions;
using NostromoGraphs.Settings;

namespace NostromoGraphs.Displays
{
    public class DisplayManager
    {
        private readonly List<ITelemetryDisplay> _displays = new List<ITelemetryDisplay>();
        private readonly ITransitionEffect _transitionEngine;
        private int _currentIndex = 0;
        private float _displayTimer = 0f;
        private bool _isPaused = false;
        private readonly WidgetSettings _settings;

        public event Action? OnDisplayChanged;

        public int Count => _displays.Count;
        public int CurrentIndex => _currentIndex;
        public ITelemetryDisplay CurrentDisplay => _displays[_currentIndex];
        public bool IsPaused => _isPaused;
        public float TimeRemaining => Math.Max(0f, (float)_settings.DisplayDurationSeconds - _displayTimer);
        public float TotalDuration => (float)_settings.DisplayDurationSeconds;
        public bool IsTransitioning => _transitionEngine.IsActive;

        public IReadOnlyList<ITelemetryDisplay> Displays => _displays;

        public DisplayManager(WidgetSettings settings, ITransitionEffect transitionEngine)
        {
            _settings = settings;
            _transitionEngine = transitionEngine;

            // Register all 16 Nostromo telemetry displays
            _displays.Add(new EnvironmentalGassesDisplay());
            _displays.Add(new StellarPlotToEarthDisplay());
            _displays.Add(new CrewmemberVitalsDisplay());
            _displays.Add(new RadarDisplayLV426Display());
            _displays.Add(new StellarCartographyDisplay());
            _displays.Add(new AlienSignalAnalysisDisplay());
            _displays.Add(new AtmosphericAnalysisLV426Display());
            _displays.Add(new AshUplinkSignalDisplay());
            _displays.Add(new LocalSpaceAnalysisDisplay());
            _displays.Add(new HyperdriveOutputDisplay());
            _displays.Add(new RefineryProcessingDisplay());
            _displays.Add(new CryosleepStatusDisplay());
            _displays.Add(new TimeToEarthDisplay());
            _displays.Add(new CompositionLV426Display());
            _displays.Add(new OrbitalInsertion());
            _displays.Add(new InitialSurvey());

            // Validate and restore last index
            _currentIndex = Math.Clamp(_settings.LastSelectedDisplayIndex, 0, _displays.Count - 1);

            // Initialize all displays
            foreach (var display in _displays)
            {
                display.Initialize();
            }
        }

        public void NextDisplay()
        {
            int nextIndex = (_currentIndex + 1) % _displays.Count;
            SwitchToDisplay(nextIndex);
        }

        public void PreviousDisplay()
        {
            int prevIndex = (_currentIndex - 1 + _displays.Count) % _displays.Count;
            SwitchToDisplay(prevIndex);
        }

        public void SwitchToDisplay(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= _displays.Count) return;
            if (targetIndex == _currentIndex && !_transitionEngine.IsActive) return;

            if (_settings.ScreenClearTransitionsEnabled)
            {
                _transitionEngine.StartTransition(() =>
                {
                    _currentIndex = targetIndex;
                    _settings.LastSelectedDisplayIndex = targetIndex;
                    _displayTimer = 0f;
                    _displays[_currentIndex].Initialize();
                    OnDisplayChanged?.Invoke();
                }, (float)_settings.TransitionDurationSeconds);
            }
            else
            {
                _currentIndex = targetIndex;
                _settings.LastSelectedDisplayIndex = targetIndex;
                _displayTimer = 0f;
                _displays[_currentIndex].Initialize();
                OnDisplayChanged?.Invoke();
            }
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            OnDisplayChanged?.Invoke();
        }

        public void SetPaused(bool paused)
        {
            _isPaused = paused;
            OnDisplayChanged?.Invoke();
        }

        public void Update(float deltaTime)
        {
            _transitionEngine.Update(deltaTime);

            if (_displays.Count > 0)
            {
                _displays[_currentIndex].Update(deltaTime);
            }

            if (!_isPaused && _settings.AutoCycleEnabled && !_transitionEngine.IsActive)
            {
                _displayTimer += deltaTime;
                if (_displayTimer >= _settings.DisplayDurationSeconds)
                {
                    NextDisplay();
                }
            }
        }

        public void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            if (_displays.Count == 0) return;

            if (_settings.ScreenClearTransitionsEnabled && _transitionEngine.IsActive)
            {
                _transitionEngine.Render(canvas, bounds, palette, () =>
                {
                    _displays[_currentIndex].Render(canvas, bounds, palette);
                });
            }
            else
            {
                _displays[_currentIndex].Render(canvas, bounds, palette);
            }
        }
    }
}

