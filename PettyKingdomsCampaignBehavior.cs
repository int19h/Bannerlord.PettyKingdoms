using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Int19h.Bannerlord.PettyKingdoms {
    internal class PettyKingdomsCampaignBehavior : CampaignBehaviorBase {
        public bool WasApplied;

        public override void RegisterEvents() {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        public override void SyncData(IDataStore dataStore) {
            dataStore.SyncData("PettyKingdomsCreated", ref WasApplied);
        }

        private void OnDailyTick() {
            foreach (var kingdom in Kingdom.All.ToArray()) {
                if (!kingdom.IsEliminated && kingdom.Fiefs.Count == 0 && kingdom.Clans.Count <= 1) {
                    ChangeKingdomAction.ApplyByLeaveWithRebellionAgainstKingdom(kingdom.RulingClan, false);
                    DestroyKingdomAction.Apply(kingdom);
                }
            }
        }
    }
}
