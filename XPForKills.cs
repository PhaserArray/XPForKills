﻿using Rocket.API.Collections;
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
		private static XPForKills Instance;
		private static XPForKillsConfiguration Config;

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
                        if (Config.CheckSteamGroupTeamkill && player.SteamGroupID.Equals(murderer.SteamGroupID))
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
                    else if (deathCause == EDeathCause.ZOMBIE || deathCause == EDeathCause.SPIT || deathCause == EDeathCause.ACID || deathCause == EDeathCause.SPARK || deathCause == EDeathCause.BURNER || deathCause == EDeathCause.BOULDER) {
                        ZombiePenalty(player);
                    }
                    else if (deathCause == EDeathCause.BREATH)
                    {
                        BreathPlay(player);
                    }
                    else if (deathCause == EDeathCause.BURNING)
                    {
                        FirePenalty(player);
                    }
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
			var killReward = (int)(Config.KillXP * limbModifier);
			if (killReward != 0)
			{
				var realXP = ChangeExperience(murderer, killReward);
				UnturnedChat.Say(murderer, Instance.Translate("experience_kill_reward", victim.CharacterName, realXP));
			}
		}

        // TODO: The penalty functions are very similar to each other,
        // I should try to collapse them into a single function in the
        // future.
        // lmao i have no idea what phaser did lololoolollol
        private void FirePenalty(UnturnedPlayer player)
        {
            if (Config.FireXP != 0)
            {
                var realXP = ChangeExperience(player, Config.FireXP);
                UnturnedChat.Say(player, Instance.Translate("experience_fire_penalty", -realXP));
            }
        }
        private void BreathPlay(UnturnedPlayer player)
        {
            if (Config.DrownXP != 0)
            {
                var realXP = ChangeExperience(player, Config.DrownXP);
                UnturnedChat.Say(player, Instance.Translate("experience_drown_penalty", -realXP));
            }
        }
        private void ZombiePenalty(UnturnedPlayer player)
        {
            if (Config.ZombieXP != 0)
            {
                var realXP = ChangeExperience(player, Config.ZombieXP);
                UnturnedChat.Say(player, Instance.Translate("experience_zombie_penalty", -realXP));
            }
        }

        private void DeathPenalty(UnturnedPlayer player)
		{
			if (Config.DeathXP != 0)
			{
				var realXP = ChangeExperience(player, Config.DeathXP);
				UnturnedChat.Say(player, Instance.Translate("experience_death_penalty", -realXP));
			}
		}

		private void SuicidePenalty(UnturnedPlayer player)
		{
			if (Config.SuicideXP != 0)
			{
				var realXP = ChangeExperience(player, Config.SuicideXP);
				UnturnedChat.Say(player, Instance.Translate("experience_suicide_penalty", -realXP));
			}
		}

		private void TeamkillPenalty(UnturnedPlayer player)
		{
			if (Config.TeamkillXP != 0)
			{
				var realXP = ChangeExperience(player, Config.TeamkillXP);
				UnturnedChat.Say(player, Instance.Translate("experience_teamkill_penalty", -realXP));
			}
		}

		// Returns the change that was actually made.
		public int ChangeExperience(UnturnedPlayer player, int change)
		{
			// Unity's Mathf.Clamp had some precision issues
			// with larger number (mostly in the hundreds of
			// millions or even billions), I'm gonna use this
			// instead, hopefully it works better.
			long exp = player.Experience + change;
			player.Experience = (exp < uint.MinValue) ? uint.MinValue : (exp > uint.MaxValue) ? uint.MaxValue : (uint)exp;
			return (int)(change + player.Experience - exp);
		}

		public float GetLimbModifier(ELimb limb)
		{
			// There's probably a better way to connect the 
			// limb to the experience modifier but this works.
			// I'm not sure if the feet, hand, front and back 
			// limbs are actually used.
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

		public override TranslationList DefaultTranslations
		{
			get
			{
				return new TranslationList(){
					{"experience_kill_reward", "You killed {0} and gained {1} experience!"},
					{"experience_death_penalty", "You died and lost {0} experience!"},
					{"experience_suicide_penalty", "You killed yourself and lost {0} experience!"},
					{"experience_teamkill_penalty", "You killed a teammate and lost {0} experience!"}
                    {"experience_drown_penalty", "You breathed water and lost {0} experience!"}
                    { "experience_fire_penalty", "You got roasted and lost {0} experience!"}
                };
			}
		}
	}
}
