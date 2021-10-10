using Rocket.API;
using XPForKills;

namespace PhaserArray.XPForKills
{
	public class XPForKillsConfiguration : IRocketPluginConfiguration
	{

		public bool CheckSteamGroupTeamkill;
		public bool DisableMessages;
		public string NoTeamkillPenaltyPermission;

		public int KillXP;
		public float HeadModifier;
		public float TorsoModifier;
		public float ArmModifier;
		public float LegModifier;

		public int TeamkillXP;
		public int SuicideXP;
		public int DeathXP;
        	public int BreathXP;
        	public int ZombieXP;
        	public int FireXP;

		public GunMultiplierModel[] GunMultipliers { get; set; }

		public void LoadDefaults()
		{
			CheckSteamGroupTeamkill = true;
			DisableMessages = false;
			NoTeamkillPenaltyPermission = "XPForKills.NoTeamkillPenalty";

			KillXP = 50;
			HeadModifier = 1.5f;
			TorsoModifier = 1.0f;
			ArmModifier = 0.75f;
			LegModifier = 0.5f;

			TeamkillXP = -500;
			SuicideXP = -50;
			DeathXP = -50;
			BreathXP = -50;
            		ZombieXP = -50;
            		FireXP = -50;

			GunMultipliers = new GunMultiplierModel[]
			{
				new GunMultiplierModel()
				{
					Id = 363,
					Multiplier = 0.5
				}
			};
		}
	}
}
