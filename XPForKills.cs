using Rocket.API.Collections;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.Core.Plugins;
using Rocket.Core.Logging;
using SDG.Unturned;

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
			Logger.Log("Plugin Loaded");
		}

		protected override void Unload()
		{
			UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
			Logger.Log("Plugin Unloaded");
		}

		public void OnPlayerDeath(UnturnedPlayer player, EDeathCause deathCause, ELimb limb, Steamworks.CSteamID murdererID)
		{
			// From observations, ID is only invalid if the player was killed by an admin command.
			if (!murdererID.IsValid()) return;

			// Killed
			if (player.CSteamID != murdererID)
			{
				var murderer = UnturnedPlayer.FromCSteamID(murdererID);
				// Murderer exists (doesn't exist in cases like bleeding and explosions)
				if (murderer.Player != null)
				{
					// Teamkilling
					if (Config.CheckSteamGroupTeamkill && player.SteamGroupID.Equals(murderer.SteamGroupID))
					{
						ApplyPenalty(player, Config.TeamkillXP, Instance.Translate("experience_teamkill_penalty"));
					}
					// Killed by Player
					else
					{
						KillReward(murderer, player, limb);
						ApplyPenalty(player, Config.DeathXP, Instance.Translate("experience_death_penalty"));
					}
				}
				// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
				else switch (deathCause)
				{
					// Killed by a zombie
					case EDeathCause.ZOMBIE:
					case EDeathCause.SPIT:
					case EDeathCause.ACID:
					case EDeathCause.SPARK:
					case EDeathCause.BURNER:
					case EDeathCause.BOULDER:
						ApplyPenalty(player, Config.ZombieXP, Instance.Translate("experience_zombie_penalty"));
						break;
					// Killed by suffocation (either high altitude or under water)
					case EDeathCause.BREATH:
						ApplyPenalty(player, Config.BreathXP, Instance.Translate("experience_breath_penalty"));
						break;
					// Killed by fire
					case EDeathCause.BURNING:
						ApplyPenalty(player, Config.FireXP, Instance.Translate("experience_fire_penalty"));
						break;
					// Killed by any other reason
					default:
						ApplyPenalty(player, Config.DeathXP, Instance.Translate("experience_death_penalty"));
						break;
				}
			}
			// Suicide
			else
			{
				ApplyPenalty(player, Config.SuicideXP, Instance.Translate("experience_suicide_penalty"));
			}
		}
		
		private void KillReward(UnturnedPlayer murderer, UnturnedPlayer victim, ELimb limb)
		{
			var limbModifier = GetLimbModifier(limb);
			var killReward = (int)(Config.KillXP * limbModifier);

			if (killReward == 0) return;
			var realXPDelta = ChangeExperience(murderer, killReward);

			if (!Config.DisableMessages) return;
			UnturnedChat.Say(murderer, Instance.Translate("experience_kill_reward", victim.CharacterName, realXPDelta));
		}

		private void ApplyPenalty(UnturnedPlayer player, int experienceDelta, string chatMessage)
		{
			if (experienceDelta == 0) return;
			var realXPDelta = ChangeExperience(player, experienceDelta);

			if (!Config.DisableMessages) return;
			UnturnedChat.Say(player, string.Format(chatMessage, -realXPDelta));
		}

		// Returns the change that was actually made.
		public int ChangeExperience(UnturnedPlayer player, int change)
		{
			// Unity's Mathf.Clamp had some precision issues
			// with larger number (mostly in the hundreds of
			// millions or even billions), I'm gonna use this
			// instead, hopefully it works better.
			var exp = player.Experience + change;
			player.Experience = (exp < uint.MinValue) ? uint.MinValue : (exp > uint.MaxValue) ? uint.MaxValue : (uint)exp;
			return (int)(change + player.Experience - exp);
		}

		public float GetLimbModifier(ELimb limb)
		{
			switch (limb)
			{
				case ELimb.LEFT_FOOT:
					return Config.LegModifier;
				case ELimb.LEFT_LEG:
					return Config.LegModifier;
				case ELimb.RIGHT_FOOT:
					return Config.LegModifier;
				case ELimb.RIGHT_LEG:
					return Config.LegModifier;
				case ELimb.LEFT_HAND:
					return Config.ArmModifier;
				case ELimb.LEFT_ARM:
					return Config.ArmModifier;
				case ELimb.RIGHT_HAND:
					return Config.ArmModifier;
				case ELimb.RIGHT_ARM:
					return Config.ArmModifier;
				case ELimb.LEFT_BACK:
					return Config.TorsoModifier;
				case ELimb.RIGHT_BACK:
					return Config.TorsoModifier;
				case ELimb.LEFT_FRONT:
					return Config.TorsoModifier;
				case ELimb.RIGHT_FRONT:
					return Config.TorsoModifier;
				case ELimb.SPINE:
					return Config.TorsoModifier;
				case ELimb.SKULL:
					return Config.HeadModifier;
				default:
					return Config.TorsoModifier;
			}
		}

		public override TranslationList DefaultTranslations => new TranslationList
		{
			{"experience_kill_reward", "You killed {0} and gained {1} experience!"},
			{"experience_death_penalty", "You died and lost {0} experience!"},
			{"experience_suicide_penalty", "You killed yourself and lost {0} experience!"},
			{"experience_teamkill_penalty", "You killed a teammate and lost {0} experience!"},
			{"experience_breath_penalty", "You lost your breath and {0} experience!"},
			{"experience_fire_penalty", "You got roasted and lost {0} experience!"},
			{"experience_zombie_penalty", "A zombie stole {0} experience from your body!"},
		};
	}
}
