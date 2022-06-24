using TaleWorlds.CampaignSystem;

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
        }
    }
}
