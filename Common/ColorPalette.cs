using System;
using SkiaSharp;

namespace NostromoGraphs.Common
{
    public class ColorPalette
    {
        public bool IsMonochrome { get; private set; } = true;

        public SKColor Primary { get; private set; }
        public SKColor PrimaryDim { get; private set; }
        public SKColor PrimaryFaint { get; private set; }
        public SKColor PrimaryGlow { get; private set; }
        public SKColor Background { get; private set; }
        public SKColor DarkPanel { get; private set; }
        public SKColor AlertRed { get; private set; }
        public SKColor WarningAmber { get; private set; }
        public SKColor CyanAccent { get; private set; }
        public SKColor WhiteBright { get; private set; }

        public ColorPalette(string primaryHex = "#00FF66", string backgroundHex = "#08140B", double brightness = 1.0, bool isMonochrome = true)
        {
            Update(primaryHex, backgroundHex, brightness, isMonochrome);
        }

        public void Update(string primaryHex, string backgroundHex, double brightness = 1.0, bool isMonochrome = true)
        {
            IsMonochrome = isMonochrome;

            if (!SKColor.TryParse(primaryHex, out SKColor baseColor))
            {
                baseColor = new SKColor(0x00, 0xFF, 0x66);
            }

            if (!SKColor.TryParse(backgroundHex, out SKColor bg))
            {
                bg = new SKColor(0x08, 0x14, 0x0B);
            }

            // Apply HUD Brightness
            byte r = (byte)Math.Clamp((int)(baseColor.Red * brightness), 0, 255);
            byte g = (byte)Math.Clamp((int)(baseColor.Green * brightness), 0, 255);
            byte b = (byte)Math.Clamp((int)(baseColor.Blue * brightness), 0, 255);

            Primary = new SKColor(r, g, b, 255);
            PrimaryDim = new SKColor(r, g, b, (byte)(160 * brightness));
            PrimaryFaint = new SKColor(r, g, b, (byte)(50 * brightness));
            PrimaryGlow = new SKColor(r, g, b, (byte)(100 * brightness));
            Background = bg;
            DarkPanel = new SKColor((byte)(bg.Red + 10), (byte)(bg.Green + 10), (byte)(bg.Blue + 10), 220);

            if (isMonochrome)
            {
                // Vintage Single-Phosphor Monochrome Aesthetic
                AlertRed = Primary;
                WarningAmber = PrimaryDim;
                CyanAccent = PrimaryDim;
                WhiteBright = Blend(Primary, SKColors.White, 0.45f);
            }
            else
            {
                // Modern Colorized HUD Aesthetic
                AlertRed = new SKColor(0xFF, 0x22, 0x44);
                WarningAmber = new SKColor(0xFF, 0xB0, 0x00);
                CyanAccent = new SKColor(0x00, 0xF0, 0xFF);
                WhiteBright = new SKColor(0xF0, 0xF8, 0xFF);
            }
        }

        public SKColor GetAccentColor(SKColor colorizedColor, byte alpha = 255)
        {
            if (IsMonochrome)
            {
                return WithAlpha(Primary, alpha);
            }
            return WithAlpha(colorizedColor, alpha);
        }

        public SKColor WithAlpha(SKColor color, byte alpha)
        {
            return new SKColor(color.Red, color.Green, color.Blue, alpha);
        }

        public SKColor Blend(SKColor from, SKColor to, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            byte r = (byte)(from.Red + (to.Red - from.Red) * amount);
            byte g = (byte)(from.Green + (to.Green - from.Green) * amount);
            byte b = (byte)(from.Blue + (to.Blue - from.Blue) * amount);
            byte a = (byte)(from.Alpha + (to.Alpha - from.Alpha) * amount);
            return new SKColor(r, g, b, a);
        }
    }
}

