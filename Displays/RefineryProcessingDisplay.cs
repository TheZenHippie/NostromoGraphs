using System;
using SkiaSharp;
using NostromoGraphs.Common;

namespace NostromoGraphs.Displays
{
    public class RefineryProcessingDisplay : BaseTelemetryDisplay
    {
        public override string Title => "AUTOMATED PETROCHEMICAL REFINERY & CARGO TELEMETRY";
        public override string Subtitle => "20,000,000 MT HEAVY HYDROCARBON & MINERAL CARGO // THE DUS TO EARTH";
        public override string SystemCode => "REF-PETRO-20MT";

        // Distillation fractions & temperature profile
        private struct DistillationCut
        {
            public string Name;
            public string Range;
            public float TempC;
            public float YieldPct;
            public string TargetHold;
        }

        private readonly DistillationCut[] _cuts = new DistillationCut[]
        {
            new DistillationCut { Name = "LIGHT NAPHTHA / C1-C4", Range = "35-70°C", TempC = 58f, YieldPct = 14.2f, TargetHold = "HOLD A-1 [POLYMER]" },
            new DistillationCut { Name = "KEROSENE / SYNTH-JET", Range = "175-230°C", TempC = 195f, YieldPct = 28.5f, TargetHold = "HOLD A-2 [FUEL]" },
            new DistillationCut { Name = "HEAVY GAS OIL / DIESEL", Range = "250-340°C", TempC = 295f, YieldPct = 32.1f, TargetHold = "HOLD B-1 [BUNKER]" },
            new DistillationCut { Name = "LUBE BASE STOCK / HC", Range = "340-490°C", TempC = 410f, YieldPct = 16.4f, TargetHold = "HOLD B-2 [LUBRICANT]" },
            new DistillationCut { Name = "TAR PITCH / BITUMEN", Range = ">510°C", TempC = 530f, YieldPct = 8.8f, TargetHold = "HOLD C-3 [SLAG]" }
        };

        // Storage Holds
        private struct StorageHold
        {
            public string HoldId;
            public string Product;
            public float CapacityMT;
            public float CurrentFillPct;
            public bool IsTransferActive;
            public float TransferRateTph;
        }

        private readonly StorageHold[] _holds = new StorageHold[]
        {
            new StorageHold { HoldId = "HOLD A-1", Product = "SYNTH-FUEL 98.4 RON", CapacityMT = 6200000f, CurrentFillPct = 84.6f, IsTransferActive = true, TransferRateTph = 380f },
            new StorageHold { HoldId = "HOLD A-2", Product = "POLYMER FEEDSTOCKS", CapacityMT = 4800000f, CurrentFillPct = 72.3f, IsTransferActive = true, TransferRateTph = 210f },
            new StorageHold { HoldId = "HOLD B-1", Product = "HEAVY BASE LUBRICANTS", CapacityMT = 5100000f, CurrentFillPct = 68.9f, IsTransferActive = false, TransferRateTph = 0f },
            new StorageHold { HoldId = "HOLD C-1", Product = "SULFUR & SLAG MATRIX", CapacityMT = 3900000f, CurrentFillPct = 43.1f, IsTransferActive = true, TransferRateTph = 145f }
        };

        public override void Render(SKCanvas canvas, SKRect bounds, ColorPalette palette)
        {
            DrawStandardHeader(canvas, bounds, palette);

            float contentTop = bounds.Top + 50f;
            float contentBottom = bounds.Bottom - 16f;
            float contentLeft = bounds.Left + 16f;
            float contentRight = bounds.Right - 16f;
            float contentWidth = contentRight - contentLeft;
            float contentHeight = contentBottom - contentTop;

            // Split into Left Section (Distillation & Waste Pyrolysis) and Right Section (Storage Holds & Telemetry)
            float leftWidth = contentWidth * 0.52f;

            var leftCol = new SKRect(contentLeft, contentTop, contentLeft + leftWidth, contentBottom);
            var rightCol = new SKRect(leftCol.Right + 12f, contentTop, contentRight, contentBottom);

            // LEFT TOP: Atmospheric Distillation Column & Fractional Cracking Tower
            float distilHeight = contentHeight * 0.58f;
            var distilBox = new SKRect(leftCol.Left, leftCol.Top, leftCol.Right, leftCol.Top + distilHeight);
            DrawDistillationColumn(canvas, distilBox, palette);

            // LEFT BOTTOM: Waste Slag Pyrolysis & Claus Sulfur Recovery
            var wasteBox = new SKRect(leftCol.Left, distilBox.Bottom + 8f, leftCol.Right, leftCol.Bottom);
            DrawWasteManagement(canvas, wasteBox, palette);

            // RIGHT TOP: Finished Product Storage Holds & Transfer Manifold
            float storageHeight = contentHeight * 0.44f;
            var storageBox = new SKRect(rightCol.Left, rightCol.Top, rightCol.Right, rightCol.Top + storageHeight);
            DrawStorageHolds(canvas, storageBox, palette);

            // RIGHT BOTTOM: Esoteric Petrochemical Parameters & Manifest
            var telemBox = new SKRect(rightCol.Left, storageBox.Bottom + 8f, rightCol.Right, rightCol.Bottom);
            DrawPetroTelemetry(canvas, telemBox, palette);
        }

        private void DrawDistillationColumn(SKCanvas canvas, SKRect box, ColorPalette palette)
        {
            canvas.DrawTechBox(box, palette.PrimaryDim, palette.DarkPanel, title: "FRACTIONAL DISTILLATION TOWER [ATM-COL 01]");

            // Distillation Column Body on the left half of the box
            float colW = 58f;
            float colLeft = box.Left + 22f;
            float colRight = colLeft + colW;
            float colTop = box.Top + 36f;
            float colBottom = box.Bottom - 38f;
            float colHeight = colBottom - colTop;

            var towerRect = new SKRect(colLeft, colTop, colRight, colBottom);

            // Tower Main Shell
            using var shellPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.Primary,
                StrokeWidth = 1.4f
            };
            canvas.DrawRoundRect(towerRect, 8f, 8f, shellPaint);

            // Slow industrial flow cycle (very slow pacing)
            float slowTime = ElapsedTime * 0.4f;

            // Internal Tray Levels & Vapor Bubble Caps
            int trayCount = _cuts.Length;
            float trayHeight = colHeight / trayCount;

            using var trayPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 120),
                StrokeWidth = 1.0f
            };

            using var pipePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 90),
                StrokeWidth = 1.2f,
                PathEffect = SKPathEffect.CreateDash(new float[] { 4f, 3f }, 0)
            };

            for (int i = 0; i < trayCount; i++)
            {
                float trayY = colTop + i * trayHeight;
                if (i > 0)
                {
                    // Internal horizontal tray plate with bubble-cap perforations
                    canvas.DrawLine(colLeft + 4f, trayY, colRight - 4f, trayY, trayPaint);

                    // Bubble cap vapor agitation dots (slow motion)
                    using var bubblePaint = new SKPaint
                    {
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill,
                        Color = palette.WithAlpha(palette.Primary, 140)
                    };
                    for (int b = 0; b < 3; b++)
                    {
                        float bx = colLeft + 14f + b * 15f;
                        float by = trayY - 4f - MathF.Sin(slowTime * 2f + i * 1.3f + b) * 3f;
                        canvas.DrawCircle(bx, by, 1.2f, bubblePaint);
                    }
                }

                // Fraction Cut Detail on Right side of Column
                var cut = _cuts[i];
                float midTrayY = trayY + trayHeight * 0.5f;

                // Take-off pipe from column to cut label
                float pipeEndX = colRight + 24f;
                canvas.DrawLine(colRight, midTrayY, pipeEndX, midTrayY, pipePaint);

                // Small valve indicator
                canvas.DrawCircle(colRight + 8f, midTrayY, 2.5f, shellPaint);

                // Fraction Name & Temperature Profile
                float textX = pipeEndX + 6f;
                canvas.DrawTechText(cut.Name, textX, midTrayY - 3f, 8.5f, palette.Primary, bold: true);

                float dynamicTemp = cut.TempC + MathF.Sin(slowTime * 0.5f + i) * 1.2f;
                string cutDetails = $"{cut.Range} [{dynamicTemp:F0}°C] -> {cut.YieldPct:F1}% YIELD";
                canvas.DrawTechText(cutDetails, textX, midTrayY + 9f, 7.5f, palette.PrimaryDim);
            }

            // Bottom Reboiler & Top Condenser annotations
            float reboilerY = colBottom + 12f;
            canvas.DrawTechText("REBOILER: 540°C [FLUID BED]", colLeft + colW * 0.5f, reboilerY, 7.5f, palette.PrimaryDim, SKTextAlign.Center);

            float condenserY = colTop - 12f;
            canvas.DrawTechText("OVERHEAD CONDENSER: 48°C", colLeft + colW * 0.5f, condenserY, 7.5f, palette.PrimaryDim, SKTextAlign.Center);

            // Reflux Loop Pipe around top left
            using var refluxPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = palette.WithAlpha(palette.PrimaryDim, 70),
                StrokeWidth = 1.0f
            };
            canvas.DrawLine(colLeft + 10f, colTop, colLeft - 10f, colTop, refluxPaint);
            canvas.DrawLine(colLeft - 10f, colTop, colLeft - 10f, colTop + 35f, refluxPaint);
            canvas.DrawLine(colLeft - 10f, colTop + 35f, colLeft, colTop + 35f, refluxPaint);
            canvas.DrawTechText("REFLUX: 3.42", colLeft - 12f, colTop + 20f, 6.5f, palette.PrimaryFaint, SKTextAlign.Right);

            // Column Differential Pressure & Zeolite Bed status
            float statY = box.Bottom - 18f;
            canvas.DrawTechText("ΔP: 1.84 BAR // ZEOLITE CATALYST FLUIDIZATION: 96.4% [STABLE]", box.Left + 14f, statY, 8f, palette.PrimaryDim);
        }

        private void DrawWasteManagement(SKCanvas canvas, SKRect box, ColorPalette palette)
        {
            canvas.DrawTechBox(box, palette.PrimaryDim, palette.DarkPanel, title: "WASTE SCRUBBING & SLAG PYROLYSIS [SRU-08]");

            float leftX = box.Left + 12f;
            float topY = box.Top + 24f;

            // 1. Claus Sulfur Recovery Unit Status
            canvas.DrawTechText("CLAUS UNIT SRU-08:", leftX, topY, 8.5f, palette.Primary, bold: true);
            float h2sConv = 99.42f + MathF.Sin(ElapsedTime * 0.2f) * 0.08f;
            canvas.DrawTechText($"H₂S CONVERSION: {h2sConv:F2}%", box.Right - 14f, topY, 8.5f, palette.CyanAccent, SKTextAlign.Right);

            topY += 14f;
            var sruBar = new SKRect(leftX, topY, box.Right - 14f, topY + 6f);
            canvas.DrawSegmentedBar(sruBar, h2sConv / 100f, 16, palette.Primary, palette.WithAlpha(palette.PrimaryDim, 30));

            topY += 18f;

            // 2. Toxic Acid Gas & Heavy Slag Pyrolysis
            string[] wasteTelemetry =
            {
                "SLAG PYROLYSIS RETORT: 1,120°C [INERT BLANKET]",
                "ACID GAS FLUX: 42.1 T/HR -> SOLID ELEMENTAL SULFUR CAKE",
                "PHENOLIC WATER NEUTRALIZATION: pH 7.18 [RECYCLED]",
                "PARTICULATE SLUDGE DISCHARGE: 18.4 T/HR -> BALLAST HOLD C-1"
            };

            foreach (var line in wasteTelemetry)
            {
                canvas.DrawTechText(line, leftX, topY, 8f, palette.PrimaryDim);
                topY += 14f;
            }
        }

        private void DrawStorageHolds(SKCanvas canvas, SKRect box, ColorPalette palette)
        {
            canvas.DrawTechBox(box, palette.PrimaryDim, palette.DarkPanel, title: "CARGO BALLAST HOLDS // PRODUCT ROUTING");

            float holdGap = 6f;
            float holdW = (box.Width - 28f - 3f * holdGap) / 4f;
            float holdH = box.Height * 0.65f;
            float holdTop = box.Top + 28f;

            for (int i = 0; i < _holds.Length; i++)
            {
                var hold = _holds[i];
                float hx = box.Left + 14f + i * (holdW + holdGap);
                var hRect = new SKRect(hx, holdTop, hx + holdW, holdTop + holdH);

                // Hold tank container box
                canvas.DrawTechBox(hRect, palette.PrimaryDim, palette.Background);

                // Fill level animation (slow viscous fill oscillation)
                float slowFill = hold.CurrentFillPct + MathF.Sin(ElapsedTime * 0.15f + i) * 0.3f;
                float fillHeight = (hRect.Height - 16f) * (slowFill / 100f);
                var fluidRect = new SKRect(hRect.Left + 2f, hRect.Bottom - fillHeight, hRect.Right - 2f, hRect.Bottom - 2f);

                SKColor fluidColor = i == 3 
                    ? (palette.IsMonochrome ? palette.PrimaryDim : palette.WarningAmber)
                    : (palette.IsMonochrome ? palette.Primary : palette.CyanAccent);

                using var fillPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    Color = palette.WithAlpha(fluidColor, 85)
                };
                canvas.DrawRect(fluidRect, fillPaint);

                // Hold Header
                canvas.DrawTechText(hold.HoldId, hRect.MidX, hRect.Top + 11f, 8f, palette.Primary, SKTextAlign.Center, bold: true);

                // Fill Percentage
                canvas.DrawTechText($"{slowFill:F1}%", hRect.MidX, hRect.MidY, 9f, palette.WhiteBright, SKTextAlign.Center, bold: true);

                // Transfer Status indicator
                float statY = hRect.Bottom - 6f;
                if (hold.IsTransferActive)
                {
                    // Active animated pulsing flow arrow
                    float pulse = (MathF.Sin(ElapsedTime * 2f + i) + 1f) * 0.5f;
                    using var pulsePaint = new SKPaint
                    {
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill,
                        Color = palette.WithAlpha(palette.Primary, (byte)(160 + pulse * 95))
                    };
                    canvas.DrawCircle(hRect.MidX, hRect.Top + 20f, 2.5f, pulsePaint);
                    canvas.DrawTechText("TRANSFER", hRect.MidX, statY, 6.5f, palette.Primary, SKTextAlign.Center);
                }
                else
                {
                    canvas.DrawTechText("SEALED", hRect.MidX, statY, 6.5f, palette.PrimaryDim, SKTextAlign.Center);
                }

                // Subtitle Product description below each hold
                canvas.DrawTechText(hold.Product, hRect.MidX, hRect.Bottom + 12f, 6.5f, palette.PrimaryDim, SKTextAlign.Center);
            }

            // Transfer Manifold Pump Status Bar at bottom
            float barY = box.Bottom - 18f;
            float pumpPress = 42.8f + MathF.Sin(ElapsedTime * 0.25f) * 0.6f;
            canvas.DrawTechText($"MANIFOLD PUMP #3: {pumpPress:F1} BAR // ROUTING: ACTIVE -> HOLDS A1/A2/C1", box.Left + 14f, barY, 7.5f, palette.Primary);
        }

        private void DrawPetroTelemetry(SKCanvas canvas, SKRect box, ColorPalette palette)
        {
            canvas.DrawTechBox(box, palette.PrimaryDim, palette.DarkPanel, title: "PETROCHEMICAL ASSAY & MASS BALANCE");

            float ty = box.Top + 24f;
            float leftX = box.Left + 14f;
            float rightX = box.Right - 14f;

            // Esoteric Petrochemical Parameters
            (string Param, string Value)[] assays =
            {
                ("FEEDSTOCK DENSITY:", "28.4° API [HEAVY SOUR CRUDE]"),
                ("REID VAPOR PRESSURE:", "6.85 PSI @ 37.8°C"),
                ("HYDROGEN RECYCLE PURITY:", "94.8% MOL [14.2 MPa]"),
                ("SULFUR FRACTION:", "2.84% WT -> <10 PPM TREATED"),
                ("RESEARCH OCTANE NUMBER:", "98.4 RON [SYNTH-CUT]"),
                ("KINEMATIC VISCOSITY:", "14.8 cSt @ 100°C // VI: 104"),
                ("TOTAL PROCESSED ORE:", "4,821,940 / 20,000,000 MT"),
                ("PROCESSING COMPLETION:", "24.11% [182.4 REL. DAYS ETA]")
            };

            for (int i = 0; i < assays.Length; i++)
            {
                var a = assays[i];
                canvas.DrawTechText(a.Param, leftX, ty, 8f, palette.PrimaryDim);
                canvas.DrawTechText(a.Value, rightX, ty, 8f, palette.Primary, SKTextAlign.Right, bold: (i >= 6));
                ty += 14.5f;
            }

            // Bottom Refinery Autonomous Lock status
            var lockBox = new SKRect(box.Left + 10f, box.Bottom - 30f, box.Right - 10f, box.Bottom - 8f);
            canvas.DrawTechBox(lockBox, palette.Primary, palette.DarkPanel);

            float totalThroughput = 4821940f + (ElapsedTime * 120f) % 50000f;
            canvas.DrawTechText($"[ 20MT PAYLOAD LOCK: NOMINAL // THROUGHPUT: 735 T/HR // PURITY: 99.1% ]", lockBox.MidX, lockBox.MidY + 3.5f, 8f, palette.Primary, SKTextAlign.Center, bold: true);
        }
    }
}

