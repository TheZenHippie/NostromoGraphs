using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using NostromoGraphs.Common;
using NostromoGraphs.Displays;
using NostromoGraphs.Settings;
using NostromoGraphs.Transitions;

namespace NostromoGraphs
{
    public partial class MainWindow : Window
    {
        private readonly WidgetSettings _settings;
        private readonly ColorPalette _palette;
        private readonly GlitchTransitionEngine _transitionEngine;
        private readonly DisplayManager _displayManager;
        private bool _isInitialized = false;

        private Stopwatch _renderStopwatch = new Stopwatch();
        private double _lastFrameTime = 0;
        private readonly Random _rand = new Random();

        private WriteableBitmap? _snowBitmap;
        private byte[]? _snowPixels;

        // Context Menu Hover Bridge & Popup Hook
        private readonly DispatcherTimer _effectsCloseTimer;
        private FrameworkElement? _subscribedPopupChild;

        public MainWindow()
        {
            _settings = WidgetSettings.Load();
            _palette = new ColorPalette(_settings.PhosphorColorHex, _settings.BackgroundColorHex, _settings.HudBrightness, _settings.IsMonochrome);
            _transitionEngine = new GlitchTransitionEngine();
            _displayManager = new DisplayManager(_settings, _transitionEngine);
            _displayManager.OnDisplayChanged += OnDisplayChanged;

            _effectsCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _effectsCloseTimer.Tick += EffectsCloseTimer_Tick;

            InitializeComponent();

            ApplyLoadedSettings();
            PopulateDisplaysMenu();
            UpdateUiHeaders();

            _isInitialized = true;

            _renderStopwatch.Start();
            CompositionTarget.Rendering += OnRenderFrame;
        }

        private void ApplyLoadedSettings()
        {
            // Restore window size and position
            if (_settings.WindowWidth.HasValue && _settings.WindowWidth.Value >= MinWidth)
                Width = _settings.WindowWidth.Value;
            if (_settings.WindowHeight.HasValue && _settings.WindowHeight.Value >= MinHeight)
                Height = _settings.WindowHeight.Value;

            if (_settings.WindowLeft.HasValue && _settings.WindowTop.HasValue)
            {
                Left = _settings.WindowLeft.Value;
                Top = _settings.WindowTop.Value;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            Topmost = _settings.AlwaysOnTop;
            AlwaysOnTopMenuItem.IsChecked = _settings.AlwaysOnTop;
            ShadowMenuItem.IsChecked = _settings.WindowShadow;
            WindowDropShadow.Opacity = _settings.WindowShadow ? 0.40 : 0.0;

            // Display Color Mode
            MonochromeModeMenuItem.IsChecked = _settings.IsMonochrome;
            ColorizedModeMenuItem.IsChecked = !_settings.IsMonochrome;

            // Display Duration & Auto-Cycle
            DurationSlider.Value = _settings.DisplayDurationSeconds;
            if (DurationValueText != null) DurationValueText.Text = $"{_settings.DisplayDurationSeconds:F0}s";
            if (DurationBadgeText != null) DurationBadgeText.Text = $"[ {_settings.DisplayDurationSeconds:F0}s CYCLE ]";
            AutoCycleMenuItem.IsChecked = _settings.AutoCycleEnabled;
            ScreenClearGlitchMenuItem.IsChecked = _settings.ScreenClearTransitionsEnabled;

            // Opacity & Brightness
            BackgroundOpacitySlider.Value = _settings.BackgroundOpacity;
            if (BackgroundOpacityValueText != null) BackgroundOpacityValueText.Text = $"{(int)(_settings.BackgroundOpacity * 100)}%";
            WindowBackgroundBorder.Opacity = _settings.BackgroundOpacity;

            HudBrightnessSlider.Value = _settings.HudBrightness;
            if (HudBrightnessValueText != null) HudBrightnessValueText.Text = $"{(int)(_settings.HudBrightness * 100)}%";

            // Effects
            BloomGlowMenuItem.IsChecked = _settings.BloomEnabled;
            PhosphorBloomEffect.Opacity = _settings.BloomEnabled ? 0.85 : 0.0;
            BloomIntensitySlider.Value = _settings.BloomIntensity;
            PhosphorBloomEffect.BlurRadius = _settings.BloomIntensity;
            if (BloomIntensityValueText != null) BloomIntensityValueText.Text = $"{_settings.BloomIntensity:F0}px";

            CrtScanlinesMenuItem.IsChecked = _settings.CrtScanlinesEnabled;
            CrtScanlinesOverlay.Visibility = _settings.CrtScanlinesEnabled ? Visibility.Visible : Visibility.Collapsed;
            ScanlineThicknessSlider.Value = _settings.ScanlineThickness;
            if (ScanlineThicknessValueText != null) ScanlineThicknessValueText.Text = $"{_settings.ScanlineThickness:F0}px";
            UpdateScanlineBrush();

            CrtGlitchMenuItem.IsChecked = _settings.CrtGlitchEnabled;
            GlitchIntensitySlider.Value = _settings.GlitchIntensity;
            if (GlitchIntensityValueText != null) GlitchIntensityValueText.Text = $"{_settings.GlitchIntensity:F0}%";

            CrtSnowMenuItem.IsChecked = _settings.CrtSnowEnabled;
            CrtSnowOverlay.Visibility = _settings.CrtSnowEnabled ? Visibility.Visible : Visibility.Collapsed;
            SnowAmountSlider.Value = _settings.SnowAmount;
            if (SnowAmountValueText != null) SnowAmountValueText.Text = $"{_settings.SnowAmount:F0}%";

            ApplyColors();
        }

        private void ApplyColors()
        {
            _palette.Update(_settings.PhosphorColorHex, _settings.BackgroundColorHex, _settings.HudBrightness, _settings.IsMonochrome);

            var phosphorWpfColor = (Color)ColorConverter.ConvertFromString(_settings.PhosphorColorHex);
            var phosphorBrush = new SolidColorBrush(phosphorWpfColor);
            var bgWpfColor = (Color)ColorConverter.ConvertFromString(_settings.BackgroundColorHex);

            // Window Glass
            WindowBackgroundBorder.Background = new SolidColorBrush(Color.FromArgb(
                (byte)(230 * _settings.BackgroundOpacity), bgWpfColor.R, bgWpfColor.G, bgWpfColor.B));
            WindowBackgroundBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, phosphorWpfColor.R, phosphorWpfColor.G, phosphorWpfColor.B));

            // Header & Footer
            SystemBadgeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x60, phosphorWpfColor.R, phosphorWpfColor.G, phosphorWpfColor.B));
            SystemBadgeText.Foreground = phosphorBrush;

            HeaderBadgeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x60, phosphorWpfColor.R, phosphorWpfColor.G, phosphorWpfColor.B));
            HeaderStatusText.Foreground = phosphorBrush;

            DurationBadgeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x50, phosphorWpfColor.R, phosphorWpfColor.G, phosphorWpfColor.B));
            DurationBadgeText.Foreground = phosphorBrush;

            ActiveFeedText.Foreground = phosphorBrush;
            HeaderBarBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x25, phosphorWpfColor.R, phosphorWpfColor.G, phosphorWpfColor.B));
            FooterBarBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x25, phosphorWpfColor.R, phosphorWpfColor.G, phosphorWpfColor.B));

            // Phosphor Bloom
            PhosphorBloomEffect.Color = phosphorWpfColor;

            UpdateMenuChecks();
        }

        private void UpdateMenuChecks()
        {
            if (!_isInitialized) return;

            // Display Color Mode Checks
            MonochromeModeMenuItem.IsChecked = _settings.IsMonochrome;
            ColorizedModeMenuItem.IsChecked = !_settings.IsMonochrome;

            // Phosphor Color Checks
            foreach (var item in PhosphorColorMenuItem.Items)
            {
                if (item is MenuItem mi && mi.Tag is string tagHex)
                {
                    mi.IsChecked = string.Equals(tagHex, _settings.PhosphorColorHex, StringComparison.OrdinalIgnoreCase);
                }
            }

            // Background Color Checks
            foreach (var item in BackgroundColorMenuItem.Items)
            {
                if (item is MenuItem mi && mi.Tag is string tagHex)
                {
                    mi.IsChecked = string.Equals(tagHex, _settings.BackgroundColorHex, StringComparison.OrdinalIgnoreCase);
                }
            }

            // Displays Menu Checks
            for (int i = 0; i < DisplaysMenu.Items.Count; i++)
            {
                if (DisplaysMenu.Items[i] is MenuItem mi)
                {
                    mi.IsChecked = (i == _displayManager.CurrentIndex);
                }
            }
        }

        private void PopulateDisplaysMenu()
        {
            DisplaysMenu.Items.Clear();
            for (int i = 0; i < _displayManager.Displays.Count; i++)
            {
                int index = i;
                var display = _displayManager.Displays[i];
                var mi = new MenuItem
                {
                    Header = $"{index + 1:D2}. {display.Title}",
                    Tag = index,
                    IsCheckable = true,
                    IsChecked = (index == _displayManager.CurrentIndex)
                };
                mi.Click += (s, e) =>
                {
                    _displayManager.SwitchToDisplay(index);
                    UpdateUiHeaders();
                };
                DisplaysMenu.Items.Add(mi);
            }
        }

        private void UpdateUiHeaders()
        {
            var cur = _displayManager.CurrentDisplay;
            int idx = _displayManager.CurrentIndex;
            int total = _displayManager.Count;

            HeaderStatusText.Text = $"[ {cur.SystemCode} // {cur.Title} ]";
            ActiveFeedText.Text = $"FEED: [{idx + 1:D2}/{total:D2}] - {cur.Title}";
            PauseButton.Content = _displayManager.IsPaused ? "RESUME" : "PAUSE";
            PauseResumeMenuItem.Header = _displayManager.IsPaused ? "Resume Auto-Cycle" : "Pause Auto-Cycle";

            UpdateMenuChecks();
        }

        private void OnDisplayChanged()
        {
            Dispatcher.Invoke(() =>
            {
                UpdateUiHeaders();
                _settings.Save();
            });
        }

        private void OnRenderFrame(object? sender, EventArgs e)
        {
            if (!_isInitialized) return;

            double currentTime = _renderStopwatch.Elapsed.TotalSeconds;
            float deltaTime = (float)(currentTime - _lastFrameTime);
            _lastFrameTime = currentTime;

            if (deltaTime > 0.1f) deltaTime = 0.1f; // Clamp hitch spikes

            _displayManager.Update(deltaTime);

            // Update live hold footer countdown
            if (_displayManager.IsPaused)
            {
                HoldStatusText.Text = "HOLD: PAUSED";
            }
            else
            {
                HoldStatusText.Text = $"HOLD: {_displayManager.TimeRemaining:F1}s / {_displayManager.TotalDuration:F0}s";
            }

            // CRT Snow Noise Update if enabled
            if (_settings.CrtSnowEnabled)
            {
                UpdateCrtSnow();
            }

            // Invalidate Skia canvas for 60 FPS redraw
            SkiaCanvasElement.InvalidateVisual();
        }

        private void SkiaCanvasElement_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (!_isInitialized) return;

            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear(SKColors.Transparent);

            var bounds = new SKRect(0, 0, info.Width, info.Height);
            _displayManager.Render(canvas, bounds, _palette);
        }

        private void UpdateCrtSnow()
        {
            int w = 160;
            int h = 100;
            if (_snowBitmap == null || _snowPixels == null)
            {
                _snowBitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
                _snowPixels = new byte[w * h * 4];
                CrtSnowOverlay.Source = _snowBitmap;
            }

            float snowAlpha = (float)_settings.SnowAmount / 100f * 0.35f;
            for (int i = 0; i < _snowPixels.Length; i += 4)
            {
                byte val = (byte)_rand.Next(256);
                _snowPixels[i] = val;     // B
                _snowPixels[i + 1] = val; // G
                _snowPixels[i + 2] = val; // R
                _snowPixels[i + 3] = (byte)(val * snowAlpha); // A
            }

            _snowBitmap.WritePixels(new Int32Rect(0, 0, w, h), _snowPixels, w * 4, 0);
            CrtSnowOverlay.Opacity = 0.8;
        }

        private void UpdateScanlineBrush()
        {
            double thickness = _settings.ScanlineThickness;
            CrtDrawingBrush.Viewport = new Rect(0, 0, 1, thickness * 2);
        }

        #region UI Interactions & Drag Move

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                // Prevent drag if clicking any button or child within a button
                DependencyObject? current = e.OriginalSource as DependencyObject;
                while (current != null && current != HeaderBarBorder)
                {
                    if (current is Button) return;
                    current = VisualTreeHelper.GetParent(current);
                }

                DragMove();
                _settings.WindowLeft = Left;
                _settings.WindowTop = Top;
                _settings.Save();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                // Prevent drag if clicking any interactive controls
                DependencyObject? current = e.OriginalSource as DependencyObject;
                while (current != null && current != this)
                {
                    if (current is Button || current is Slider || current is MenuItem || current is ContextMenu) return;
                    current = VisualTreeHelper.GetParent(current);
                }

                DragMove();
                _settings.WindowLeft = Left;
                _settings.WindowTop = Top;
                _settings.Save();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isInitialized) return;
            _settings.WindowWidth = Width;
            _settings.WindowHeight = Height;
            _settings.Save();
        }

        #endregion

        #region Context Menu & Hover Bridge

        private void Window_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            App.EnsureStandardMenuDropAlignment();
            _effectsCloseTimer.Stop();
            UpdateMenuChecks();
        }

        private void MainContextMenu_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _effectsCloseTimer.Stop();
        }

        private void MainContextMenu_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _effectsCloseTimer.Stop();
        }

        private void MainContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            _effectsCloseTimer.Stop();
            DetachPopupHook();
        }

        private void EffectsMenuItem_MouseEnter(object sender, MouseEventArgs e)
        {
            _effectsCloseTimer.Stop();
        }

        private void EffectsMenuItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (EffectsMenuItem != null && EffectsMenuItem.IsSubmenuOpen)
            {
                _effectsCloseTimer.Stop();
                _effectsCloseTimer.Start();
            }
        }

        private void EffectsMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            _effectsCloseTimer.Stop();
            AttachPopupHook();
        }

        private void EffectsMenuItem_SubmenuClosed(object sender, RoutedEventArgs e)
        {
            _effectsCloseTimer.Stop();
            DetachPopupHook();
        }

        private void EffectsCloseTimer_Tick(object? sender, EventArgs e)
        {
            _effectsCloseTimer.Stop();
            if (EffectsMenuItem != null && EffectsMenuItem.IsSubmenuOpen)
            {
                EffectsMenuItem.IsSubmenuOpen = false;
            }
        }

        private void AttachPopupHook()
        {
            if (_subscribedPopupChild != null) return;
            try
            {
                var popup = FindVisualChild<Popup>(EffectsMenuItem);
                if (popup?.Child is FrameworkElement popupChild)
                {
                    _subscribedPopupChild = popupChild;
                    _subscribedPopupChild.MouseEnter += Submenu_MouseEnter;
                    _subscribedPopupChild.MouseLeave += Submenu_MouseLeave;
                }
            }
            catch { }
        }

        private void DetachPopupHook()
        {
            if (_subscribedPopupChild != null)
            {
                try
                {
                    _subscribedPopupChild.MouseEnter -= Submenu_MouseEnter;
                    _subscribedPopupChild.MouseLeave -= Submenu_MouseLeave;
                }
                catch { }
                _subscribedPopupChild = null;
            }
        }

        private void Submenu_MouseEnter(object sender, MouseEventArgs e)
        {
            _effectsCloseTimer.Stop();
        }

        private void Submenu_MouseLeave(object sender, MouseEventArgs e)
        {
            if (EffectsMenuItem != null && EffectsMenuItem.IsSubmenuOpen)
            {
                _effectsCloseTimer.Stop();
                _effectsCloseTimer.Start();
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed) return typed;
                var found = FindVisualChild<T>(child);
                if (found != null) return found;
            }
            return null;
        }

        #endregion

        #region Menu Click & Slider Handlers

        private void DurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            double val = Math.Round(e.NewValue);
            _settings.DisplayDurationSeconds = val;
            if (DurationValueText != null) DurationValueText.Text = $"{val:F0}s";
            if (DurationBadgeText != null) DurationBadgeText.Text = $"[ {val:F0}s CYCLE ]";
            _settings.Save();
        }

        private void DurationPreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Tag is string tagStr && double.TryParse(tagStr, out double d))
            {
                DurationSlider.Value = d;
            }
        }

        private void DurationBadge_Click(object sender, RoutedEventArgs e)
        {
            // Cycle through standard duration presets: 10 -> 15 -> 20 -> 30 -> 45 -> 60 -> 10
            double cur = _settings.DisplayDurationSeconds;
            double next = cur switch
            {
                <= 10 => 15,
                <= 15 => 20,
                <= 20 => 30,
                <= 30 => 45,
                <= 45 => 60,
                _ => 10
            };
            DurationSlider.Value = next;
        }

        private void AutoCycle_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            _settings.AutoCycleEnabled = AutoCycleMenuItem.IsChecked;
            _settings.Save();
        }

        private void ScreenClearGlitch_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            _settings.ScreenClearTransitionsEnabled = ScreenClearGlitchMenuItem.IsChecked;
            _settings.Save();
        }

        private void PauseResume_Click(object sender, RoutedEventArgs e)
        {
            _displayManager.TogglePause();
            UpdateUiHeaders();
        }

        private void NextDisplay_Click(object sender, RoutedEventArgs e)
        {
            _displayManager.NextDisplay();
            UpdateUiHeaders();
        }

        private void PrevDisplay_Click(object sender, RoutedEventArgs e)
        {
            _displayManager.PreviousDisplay();
            UpdateUiHeaders();
        }

        private void TriggerGlitch_Click(object sender, RoutedEventArgs e)
        {
            _transitionEngine.StartTransition(() =>
            {
                _displayManager.CurrentDisplay.Initialize();
            }, (float)_settings.TransitionDurationSeconds);
        }

        private void MonochromeMode_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            _settings.IsMonochrome = true;
            ApplyColors();
            _settings.Save();
        }

        private void ColorizedMode_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            _settings.IsMonochrome = false;
            ApplyColors();
            _settings.Save();
        }

        private void PhosphorColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Tag is string hex)
            {
                _settings.PhosphorColorHex = hex;
                ApplyColors();
                _settings.Save();
            }
        }

        private void CustomPhosphorColor_Click(object sender, RoutedEventArgs e)
        {
            // Preset custom cycle
            string[] customHexes = { "#00FF66", "#FFB000", "#00F0FF", "#F0F0F0", "#FF2244", "#A040FF", "#FF007F" };
            int idx = Array.IndexOf(customHexes, _settings.PhosphorColorHex);
            string next = customHexes[(idx + 1) % customHexes.Length];
            _settings.PhosphorColorHex = next;
            ApplyColors();
            _settings.Save();
        }

        private void BackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Tag is string hex)
            {
                _settings.BackgroundColorHex = hex;
                ApplyColors();
                _settings.Save();
            }
        }

        private void CustomBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            string[] bgHexes = { "#08140B", "#140B04", "#000000", "#0E1116", "#040812" };
            int idx = Array.IndexOf(bgHexes, _settings.BackgroundColorHex);
            string next = bgHexes[(idx + 1) % bgHexes.Length];
            _settings.BackgroundColorHex = next;
            ApplyColors();
            _settings.Save();
        }

        private void BloomGlow_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            _settings.BloomEnabled = BloomGlowMenuItem.IsChecked;
            PhosphorBloomEffect.Opacity = _settings.BloomEnabled ? 0.85 : 0.0;
            _settings.Save();
        }

        private void BloomIntensitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            _settings.BloomIntensity = e.NewValue;
            PhosphorBloomEffect.BlurRadius = e.NewValue;
            if (BloomIntensityValueText != null) BloomIntensityValueText.Text = $"{e.NewValue:F0}px";
            _settings.Save();
        }

        private void CrtScanlines_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            _settings.CrtScanlinesEnabled = CrtScanlinesMenuItem.IsChecked;
            CrtScanlinesOverlay.Visibility = _settings.CrtScanlinesEnabled ? Visibility.Visible : Visibility.Collapsed;
            _settings.Save();
        }

        private void ScanlineThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            _settings.ScanlineThickness = e.NewValue;
            if (ScanlineThicknessValueText != null) ScanlineThicknessValueText.Text = $"{e.NewValue:F0}px";
            UpdateScanlineBrush();
            _settings.Save();
        }

        private void CrtGlitch_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            _settings.CrtGlitchEnabled = CrtGlitchMenuItem.IsChecked;
            _settings.Save();
        }

        private void GlitchIntensitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            _settings.GlitchIntensity = e.NewValue;
            if (GlitchIntensityValueText != null) GlitchIntensityValueText.Text = $"{e.NewValue:F0}%";
            _settings.Save();
        }

        private void CrtSnow_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            _settings.CrtSnowEnabled = CrtSnowMenuItem.IsChecked;
            CrtSnowOverlay.Visibility = _settings.CrtSnowEnabled ? Visibility.Visible : Visibility.Collapsed;
            _settings.Save();
        }

        private void SnowAmountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            _settings.SnowAmount = e.NewValue;
            if (SnowAmountValueText != null) SnowAmountValueText.Text = $"{e.NewValue:F0}%";
            _settings.Save();
        }

        private void BackgroundOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            _settings.BackgroundOpacity = e.NewValue;
            if (BackgroundOpacityValueText != null) BackgroundOpacityValueText.Text = $"{(int)(e.NewValue * 100)}%";
            WindowBackgroundBorder.Opacity = e.NewValue;
            _settings.Save();
        }

        private void HudBrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            _settings.HudBrightness = e.NewValue;
            if (HudBrightnessValueText != null) HudBrightnessValueText.Text = $"{(int)(e.NewValue * 100)}%";
            ApplyColors();
            _settings.Save();
        }

        private void AlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            _settings.AlwaysOnTop = AlwaysOnTopMenuItem.IsChecked;
            Topmost = _settings.AlwaysOnTop;
            _settings.Save();
        }

        private void Shadow_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            _settings.WindowShadow = ShadowMenuItem.IsChecked;
            WindowDropShadow.Opacity = _settings.WindowShadow ? 0.40 : 0.0;
            _settings.Save();
        }

        private void ResetSize_Click(object sender, RoutedEventArgs e)
        {
            Width = 860;
            Height = 580;
            _settings.WindowWidth = Width;
            _settings.WindowHeight = Height;
            _settings.Save();
        }

        private void Slider_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is Slider slider)
            {
                double step = slider.SmallChange > 0 ? slider.SmallChange : 1.0;
                slider.Value = Math.Clamp(slider.Value + (e.Delta > 0 ? step : -step), slider.Minimum, slider.Maximum);
                e.Handled = true;
            }
        }

        private void CloseWidget_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ExitAll_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion
    }
}

