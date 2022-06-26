using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Int19h.Bannerlord.PettyKingdoms {
    internal static class PettyKingdoms {
        private static string GetPolityName(CultureObject culture, Settlement capital) =>
            culture.StringId switch {
                "aserai" =>
                    $"Emirate of {capital.Name}",
                "battania" =>
                    $"Coiced of {capital.Name}",
                "empire" =>
                    $"Despotate of {capital.Name}",
                "khuzait" =>
                    $"Ulus of {capital.Name}",
                "sturgia" =>
                    $"Principality of {capital.Name}",
                "vlandia" =>
                    $"Duchy of {capital.Name}",
                _ =>
                    $"Kingdom of {capital.Name}",
            };

        [CommandLineFunctionality.CommandLineArgumentFunction("create", "petty_kingdoms")]
        public static void Create() {
            CulturalPalette.Refresh();
            var clans = Clan.All.Where(c => c.Kingdom != null).ToArray();

            foreach (var clan in clans) {
                if (clan == Clan.PlayerClan) {
                    continue;
                }
                while (true) {
                    var town = clan.Fiefs.Where(f => f.IsTown).Skip(1).FirstOrDefault()?.Settlement;
                    if (town == null) {
                        break;
                    }
                    var newClan = clans.FirstOrDefault(c => c.Culture == clan.Culture && !c.Fiefs.Any(f => f.IsTown));
                    if (newClan == null) {
                        break;
                    }
                    ChangeOwnerOfSettlementAction.ApplyByGift(town, newClan.Leader);
                }
            }

            foreach (var clan in clans) {
                if (clan == Clan.PlayerClan || !clan.Fiefs.Any(f => f.IsTown)) {
                    continue;
                }
                while (true) {
                    var castle = clan.Fiefs.FirstOrDefault(f => f.IsCastle)?.Settlement;
                    if (castle == null) {
                        break;
                    }
                    var newClan = (
                        from c in clans
                        where c.Culture == clan.Culture && !c.Fiefs.Any(f => f.IsTown)
                        orderby c.Fiefs.Count()
                        select c
                    ).FirstOrDefault();
                    if (newClan == null) {
                        break;
                    }
                    ChangeOwnerOfSettlementAction.ApplyByGift(castle, newClan.Leader);
                }
            }

            var palettes = new CulturalPalettes();
            foreach (var clan in clans) {
                foreach (var fief in clan.Fiefs) {
                    fief.GarrisonParty.MemberRoster.Reset();
                }

                var oldKingdom = clan.Kingdom;
                if (clan != oldKingdom.RulingClan) {
                    ChangeKingdomAction.ApplyByLeaveWithRebellionAgainstKingdom(clan, false);
                }

                var capital = clan.Fiefs.OrderByDescending(f => f.Prosperity)
                    .FirstOrDefault(f => f.IsTown)?.Settlement;
                if (capital == null) {
                    SetClanColor(clan, palettes.NeutralColor);
                    continue;
                }

                SetClanColor(clan, palettes[clan.Culture].ClaimColor());
                var nameObject = new TextObject(GetPolityName(clan.Culture, capital));
                if (clan == oldKingdom.RulingClan) {
                    oldKingdom.ChangeKingdomName(nameObject, nameObject);
                    oldKingdom.SetProperty("Color", clan.Color);
                    oldKingdom.SetProperty("PrimaryBannerColor", clan.Color);
                    oldKingdom.SetProperty("Color2", clan.Color2);
                    oldKingdom.SetProperty("SecondaryBannerColor", clan.Color2);
                    oldKingdom.Banner.ChangePrimaryColor(clan.Color);
                    oldKingdom.Banner.ChangeIconColors(clan.Color2);
                } else {
                    Campaign.Current.KingdomManager.CreateKingdom(nameObject, nameObject, clan.Culture, clan, encyclopediaTitle: nameObject);
                }
            }
        }

        private static void SetClanColor(Clan clan, uint color) {
            clan.Color = color;
            clan.Color2 = 0xFFFFFFFFu;
            clan.UpdateBannerColor(clan.Color, clan.Color2);
            if (clan.Banner != null) {
                clan.Banner.ChangePrimaryColor(clan.Color);
                clan.Banner.ChangeIconColors(clan.Color2);
            }
        }
    }
}
