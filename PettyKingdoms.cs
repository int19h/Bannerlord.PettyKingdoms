using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Int19h.Bannerlord.PettyKingdoms {
    public static class PettyKingdoms {
        private static TextObject GetPolityName(CultureObject culture, Settlement capital) {
            var text = new TextObject(
                culture.StringId switch {
                    "aserai" =>
                        "{=PVhGQ6QH}Emirate of {CAPITAL}",
                    "battania" =>
                        "{=OE333RZB}Coiced of {CAPITAL}",
                    "empire" =>
                        "{=ucTClFY1}Despotate of {CAPITAL}",
                    "khuzait" =>
                        "{=ERyfR5VC}Ulus of {CAPITAL}",
                    "sturgia" =>
                        "{=8LoRBACH}Principality of {CAPITAL}",
                    "vlandia" =>
                        "{=pfYBvZ0B}Duchy of {CAPITAL}",
                    _ =>
                        "{=LGvW6yLI}Kingdom of {CAPITAL}",
                }
            );
            text.SetTextVariable("CAPITAL", capital.Name);
            return text;
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("create", "petty_kingdoms")]
        public static string Create(List<string>? strings = null) {
            var clans =
                from c in Clan.All
                where c != Clan.PlayerClan && c.Kingdom != null
                where !c.IsRebelClan && !c.IsClanTypeMercenary
                orderby (c.Kingdom.RulingClan == c) ? 1 : 0
                select c;

            foreach (var clan in clans) {
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
                if (!clan.Fiefs.Any(f => f.IsTown)) {
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
                    fief?.GarrisonParty?.MemberRoster?.Reset();
                }

                var oldKingdom = clan.Kingdom;
                ChangeKingdomAction.ApplyByLeaveWithRebellionAgainstKingdom(clan, false);

                var capital = clan.Fiefs.OrderByDescending(f => f.Prosperity)
                    .FirstOrDefault(f => f.IsTown)?.Settlement;
                if (capital == null) {
                    SetClanColor(clan, palettes.NeutralColor);
                    continue;
                }

                SetClanColor(clan, palettes[clan.Culture].ClaimColor());
                var nameObject = GetPolityName(clan.Culture, capital);
                Campaign.Current.KingdomManager.CreateKingdom(nameObject, nameObject, clan.Culture, clan, encyclopediaTitle: nameObject);

                if (!oldKingdom.Clans.Any()) {
                    DestroyKingdomAction.Apply(oldKingdom);
                }
            }

            return "";
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
