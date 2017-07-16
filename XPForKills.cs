using Rocket.API.Collections;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.Core.Plugins;
using SDG.Unturned;
using UnityEngine;
using System;

namespace PhaserArray.XPForKills
{
	public class XPForKills : RocketPlugin<XPForKillsConfiguration>
	{
		public static XPForKills Instance;
		public static XPForKillsConfiguration Config;

		protected override void Load()
		{
			Instance = this;
			Config = Instance.Configuration.Instance;
			UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
			Log("Plugin Loaded");
		}

		public void OnPlayerDeath(UnturnedPlayer player, EDeathCause deathCause, ELimb limb, Steamworks.CSteamID murdererID)
		{
			// From observations, ID is only invalid if the player was killed by an admin command.
			if (murdererID.IsValid())
			{
				// Killed
				if (player.CSteamID != murdererID)
				{
					var murderer = UnturnedPlayer.FromCSteamID(murdererID);
					// Murderer exists (doesn't exist in cases like bleeding and explosions)
					if (murderer.Player != null)
					{
						// Teamkilling
						if (Config.CheckSteamGroupTeamkill && player.SteamGroupID == murderer.SteamGroupID)
						{
							TeamkillPenalty(murderer);
						}
						// Killed by Player
						else
						{
							KillReward(murderer, player, limb);
							DeathPenalty(player);
						}
					}
					// Killed w/o Murderer
					else
					{
						DeathPenalty(player);
					}
				}
				// Suicide
				else
				{
					SuicidePenalty(player);
				}
			}
		}
		
		private void KillReward(UnturnedPlayer murderer, UnturnedPlayer victim, ELimb limb)
		{
			var limbModifier = GetLimbModifier(limb);
			var experience = (int)(Config.KillXP * limbModifier);
			UnturnedChat.Say(murderer, Instance.Translate("experience_kill_reward", victim.CharacterName, experience));
			ChangeExperience(murderer, experience);
		}

		private void DeathPenalty(UnturnedPlayer player)
		{
			if(Config.DeathXP != 0)
			{
				UnturnedChat.Say(player, Instance.Translate("experience_death_penalty", Mathf.Abs(Config.DeathXP)));
				ChangeExperience(player, Config.DeathXP);
			}
		}

		private void SuicidePenalty(UnturnedPlayer player)
		{
			if (Config.SuicideXP != 0)
			{
				UnturnedChat.Say(player, Instance.Translate("experience_suicide_penalty", Mathf.Abs(Config.SuicideXP)));
				ChangeExperience(player, Config.SuicideXP);
			}
		}

		private void TeamkillPenalty(UnturnedPlayer player)
		{
			if (Config.TeamkillXP != 0)
			{
				UnturnedChat.Say(player, Instance.Translate("experience_teamkill_penalty", Mathf.Abs(Config.TeamkillXP)));
				ChangeExperience(player, Config.TeamkillXP);
			}
		}

		// If I expand this plugin, a separate logger class would be nice.
		private void Log(string message)
		{
			Console.WriteLine("[XPForKills]: " + message);
		}

		public void ChangeExperience(UnturnedPlayer player, int change)
		{
			// The clamp should make sure that experience can't under- or overflow.
			player.Experience = (uint)Mathf.Clamp(player.Experience + change, uint.MinValue, uint.MaxValue);
		}

		public float GetLimbModifier(ELimb limb)
		{
			// There's probably a better way to connect the 
			// limb to the experience modifier but this works.
			// I'm not sure if the feet, hand, front and back 
			// limbs are actually used.
			switch (limb)
			{
				case SDG.Unturned.ELimb.LEFT_FOOT:
					return Config.LegModifier;
				case SDG.Unturned.ELimb.LEFT_LEG:
					return Config.LegModifier;
				case SDG.Unturned.ELimb.RIGHT_FOOT:
					return Config.LegModifier;
				case SDG.Unturned.ELimb.RIGHT_LEG:
					return Config.LegModifier;
				case SDG.Unturned.ELimb.LEFT_HAND:
					return Config.ArmModifier;
				case SDG.Unturned.ELimb.LEFT_ARM:
					return Config.ArmModifier;
				case SDG.Unturned.ELimb.RIGHT_HAND:
					return Config.ArmModifier;
				case SDG.Unturned.ELimb.RIGHT_ARM:
					return Config.ArmModifier;
				case SDG.Unturned.ELimb.LEFT_BACK:
					return Config.TorsoModifier;
				case SDG.Unturned.ELimb.RIGHT_BACK:
					return Config.TorsoModifier;
				case SDG.Unturned.ELimb.LEFT_FRONT:
					return Config.TorsoModifier;
				case SDG.Unturned.ELimb.RIGHT_FRONT:
					return Config.TorsoModifier;
				case SDG.Unturned.ELimb.SPINE:
					return Config.TorsoModifier;
				case SDG.Unturned.ELimb.SKULL:
					return Config.HeadModifier;
				default:
					return Config.DefaultModifier;
			}
		}

		public override TranslationList DefaultTranslations
		{
			get
			{
				return new TranslationList(){
					{"experience_kill_reward", "You killed {0} and gained {1} experience!"},
					{"experience_death_penalty", "You died and lost {0} experience!"},
					{"experience_suicide_penalty", "You killed yourself and lost {0} experience!"},
					{"experience_teamkill_penalty", "You killed a teammate and lost {0} experience!"}
				};
			}
		}
	}
}
