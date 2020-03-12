﻿using Rocket.API;

namespace PhaserArray.XPForKills
{
	public class XPForKillsConfiguration : IRocketPluginConfiguration
	{
		public int KillXP;
		public float HeadModifier;
		public float TorsoModifier;
		public float ArmModifier;
		public float LegModifier;

		public bool CheckSteamGroupTeamkill;
		public int TeamkillXP;
		public int SuicideXP;
		public int DeathXP;
        public int DrownXP;
        public int ZombieXP;
        public int FireXP;
		public void LoadDefaults()
		{
			KillXP = 50;
			HeadModifier = 1.5f;
			TorsoModifier = 1.0f;
			ArmModifier = 0.75f;
			LegModifier = 0.5f;

			CheckSteamGroupTeamkill = true;
			TeamkillXP = -500;
			SuicideXP = -50;
			DeathXP = -25;
            DrownXP = -1000;
            ZombieXP = -2000;
            FireXP = -2000;
        }
	}
}
