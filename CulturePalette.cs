using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Int19h.Bannerlord.PettyKingdoms {
    internal class CulturePalette {
        private static readonly Dictionary<CultureObject, CulturePalette> palettes = new();

        public static uint NeutralColor { get; private set; }

        public CultureObject Culture { get; }

        public HashSet<uint> Colors { get; } = new();

        public int Size => Colors.Count;

        private CulturePalette(CultureObject culture) {
            Culture = culture;
        }

        public static CulturePalette GetPalette(CultureObject culture) =>
            palettes[culture];

        public uint ClaimColor() {
            var newColor = Colors.OrderBy(c => ColorDistance(c, Culture.Color)).FirstOrDefault();
            if (newColor == default) {
                Debug.FailedAssert($"Couldn't claim color for {Culture}");
                return Culture.Color;
            }
            Colors.Remove(newColor);
            return newColor;
        }

        public static void Refresh() {
            NeutralColor = (
                from bc in BannerManager.ColorPalette.Values
                let hsv = new HSV(bc.Color)
                where hsv.S == 0
                orderby hsv.V
                select bc.Color
            ).First();

            HashSet<CultureObject> cultures = new(Kingdom.All.Select(k => k.Culture));

            palettes.Clear();
            foreach (var culture in cultures) {
                palettes.Add(culture, new CulturePalette(culture));
            }

            HashSet<uint> unassignedColors = new(
                from bc in BannerManager.ColorPalette.Values
                let hsv = new HSV(bc.Color)
                where hsv.S > 0.3
                where hsv.V > 0.2 && hsv.V < 0.9
                select bc.Color
            );

            while (cultures.Any() && unassignedColors.Any()) {
                var culture = cultures.OrderBy(c => GetPalette(c).Size).First();
                var palette = GetPalette(culture);
                var color = unassignedColors.OrderBy(c => ColorDistance(c, culture.Color)).First();
                palette.Colors.Add(color);
                unassignedColors.Remove(color);
                if (palette.Size >= Town.AllTowns.Count(t => t.Culture == culture)) {
                    cultures.Remove(culture);
                }
            }
        }

        private static double ColorDistance(Color c1, Color c2) =>
            ColorDistance(new HSV(c1), new HSV(c2));

        private static double ColorDistance(uint c1, uint c2) =>
            ColorDistance(new HSV(c1), new HSV(c2));

        private static double ColorDistance(HSV hsv1, HSV hsv2) {
            var d = Math.Abs(hsv1.H - hsv2.H);
            if (d > 180) {
                d = 360 - d;
            }
            d /= 180;
            d += Math.Abs(hsv1.S - hsv2.S) * 0.2;
            d += Math.Abs(hsv1.V - hsv2.V) * 0.1;
            return d;
        }

    }
}
