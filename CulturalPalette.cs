using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Int19h.Bannerlord.PettyKingdoms {
    internal class CulturalPalette {
        public CultureObject Culture { get; }

        public HashSet<uint> Colors { get; } = new();

        public int Size => Colors.Count;

        public CulturalPalette(CultureObject culture) {
            Culture = culture;
        }

        public uint ClaimColor() {
            var newColor = Colors.OrderBy(c => ColorDistance(c, Culture.Color)).FirstOrDefault();
            if (newColor == default) {
                Debug.FailedAssert($"No color to claim for {Culture}");
                return Culture.Color;
            }
            Colors.Remove(newColor);
            return newColor;
        }

        internal static double ColorDistance(uint c1, uint c2) =>
            ColorDistance(new HsvColor(c1), new HsvColor(c2));

        internal static double ColorDistance(HsvColor hsv1, HsvColor hsv2) {
            // To allow for recognizable cultural clusters, use hue as the main determinant
            // of similarity while treating saturation and brightness as less important.
            var d = Math.Abs(hsv1.Hue - hsv2.Hue);
            if (d > 180) {
                d = 360 - d;
            }
            d /= 180;
            d += Math.Abs(hsv1.Saturation - hsv2.Saturation) * 0.2;
            d += Math.Abs(hsv1.Value - hsv2.Value) * 0.1;
            return d;
        }
    }

    internal class CulturalPalettes {
        private readonly Dictionary<CultureObject, CulturalPalette> palettes;

        public uint NeutralColor { get; private set; }

        public CulturalPalette this[CultureObject culture] => palettes[culture];

        public CulturalPalettes() {
            // The darkest gray available.
            NeutralColor = (
                from bc in BannerManager.ColorPalette.Values
                let hsv = new HsvColor(bc.Color)
                where hsv.Saturation == 0
                orderby hsv.Value
                select bc.Color
            ).First();

            // We only care about cultures of clans that own fiefs.
            HashSet<CultureObject> cultures = new(
                from c in Clan.All
                where c.Fiefs.Count > 0
                select c.Culture
            );
            palettes = cultures.ToDictionary(c => c, c => new CulturalPalette(c));

            // Start with all the valid banner colors.
            HashSet<uint> unassignedColors = new(
                from bc in BannerManager.ColorPalette.Values
                let hsv = new HsvColor(bc.Color)
                // Exclude black/gray/white and similar.
                where hsv.Saturation > 0.3
                // Exclude too dark and too bright.
                where hsv.Value > 0.2 && hsv.Value < 0.9
                select bc.Color
            );

            // Iterate until either all cultures have enough colors, or there are
            // no more colors left to assign.
            while (cultures.Any() && unassignedColors.Any()) {
                // Pick the culture that currently has the fewest colors.
                var (culture, palette) = (
                    from c in cultures
                    let p = this[c]
                    orderby p.Size
                    select (c, p)
                ).First();

                // Claim a color that is the closest match to the culture's base color.
                var color = unassignedColors.OrderBy(
                    c => CulturalPalette.ColorDistance(c, culture.Color)
                ).First();
                palette.Colors.Add(color);
                unassignedColors.Remove(color);

                // If the culture now has enough colors to assign a distinct one to each
                // of its towns, exclude it from further consideration.
                if (palette.Size >= Town.AllTowns.Count(t => t.Culture == culture)) {
                    cultures.Remove(culture);
                }
            }
        }
    }
}
