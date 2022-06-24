using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Int19h.Bannerlord.PettyKingdoms {
    public class SubModule : MBSubModuleBase {
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject) {
            base.OnGameStart(game, gameStarterObject);
            if (gameStarterObject is CampaignGameStarter campaignStarter) {
                campaignStarter.AddBehavior(new PettyKingdomsCampaignBehavior());
            }
        }

        public override void OnAfterGameInitializationFinished(Game game, object starterObject) {
            base.OnAfterGameInitializationFinished(game, starterObject);
            if (Campaign.Current != null) {
                var behavior = Campaign.Current.GetCampaignBehavior<PettyKingdomsCampaignBehavior>();
                if (!behavior.WasApplied) {
                    PettyKingdoms.Create();
                    behavior.WasApplied = true;
                }
            }
        }
    }
}