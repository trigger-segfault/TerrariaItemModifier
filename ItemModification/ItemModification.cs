using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace TerrariaItemModifier {
	/**<summary>Modification settings for an item.</summary>*/
	public partial class ItemModification {
		//=========== LOADING ============
		#region Loading

		/**<summary>Loads the item modification from xml.</summary>*/
		public void LoadXml(XmlNode node) {
			#region Categorization
			
			/**<summary>IsMelee</summary>*/
			Read(node, "IsMelee", ref melee);
			/**<summary>IsRanged</summary>*/
			Read(node, "IsRanged", ref ranged);
			/**<summary>IsThrown</summary>*/
			Read(node, "IsThrown", ref thrown);
			/**<summary>IsMagic</summary>*/
			Read(node, "IsMagic", ref magic);
			/**<summary>IsSummon</summary>*/
			Read(node, "IsSummon", ref summon);
			/**<summary>IsSentry</summary>*/
			Read(node, "IsSentry", ref sentry);
			/**<summary>IsDD2Summon</summary>*/
			Read(node, "IsDD2Summon", ref DD2Summon);
			/**<summary>IsAccessory</summary>*/
			Read(node, "IsAccessory", ref accessory);
			/**<summary>IsVanity</summary>*/
			Read(node, "IsVanity", ref vanity);
			/**<summary>IsPotion</summary>*/
			Read(node, "IsPotion", ref potion);

			#endregion
			//--------------------------------
			#region Combat

			/**<summary>AmmoID</summary>*/
			Read(node, "AmmoID", ref ammo);
			/**<summary>BuffID</summary>*/
			Read(node, "BuffID", ref buffType);
			/**<summary>BuffTime</summary>*/
			Read(node, "BuffTime", ref buffTime);
			/**<summary>CritChance</summary>*/
			Read(node, "CritChance", ref crit);
			/**<summary>Damage</summary>*/
			Read(node, "Damage", ref damage);
			/**<summary>Defense</summary>*/
			Read(node, "Defense", ref defense);
			/**<summary>HealLife</summary>*/
			Read(node, "HealLife", ref healLife);
			/**<summary>HealMana</summary>*/
			Read(node, "HealMana", ref healMana);
			/**<summary>Knockback</summary>*/
			Read(node, "Knockback", ref knockBack);
			/**<summary>LifeRegen</summary>*/
			Read(node, "LifeRegen", ref lifeRegen);
			/**<summary>ManaIncrease</summary>*/
			Read(node, "ManaIncrease", ref manaIncrease);
			/**<summary>ManaUse</summary>*/
			Read(node, "ManaUse", ref mana);
			/**<summary>NotAmmo</summary>*/
			Read(node, "NotAmmo", ref notAmmo);
			/**<summary>ProjectileID</summary>*/
			Read(node, "ProjectileID", ref shoot);
			/**<summary>ProjectileSpeed</summary>*/
			Read(node, "ProjectileSpeed", ref shootSpeed);
			/**<summary>RequiredAmmoID</summary>*/
			Read(node, "RequiredAmmoID", ref useAmmo);

			#endregion
			//--------------------------------
			#region Details

			/**<summary>Name</summary>*/
			Read(node, "Name", ref name);
			/**<summary>Tooltip</summary>*/
			Read(node, "Tooltip", ref tooltip);
			/**<summary>Rarity</summary>*/
			Read(node, "Rarity", ref rare);
			/**<summary>IsExpert</summary>*/
			Read(node, "IsExpert", ref expert);
			/**<summary>IsQuestItem</summary>*/
			Read(node, "IsQuestItem", ref questItem);
			/**<summary>Value</summary>*/
			Read(node, "Value", ref value);

			#endregion
			//--------------------------------
			#region Ranges

			/**<summary>CoinGrabRange</summary>*/
			//Read(node, "CoinGrabRange", ref coinGrabRange);
			/**<summary>LifeGrabRange</summary>*/
			//Read(node, "LifeGrabRange", ref lifeGrabRange);
			/**<summary>ManaGrabRange</summary>*/
			//Read(node, "ManaGrabRange", ref manaGrabRange);

			#endregion
			//--------------------------------
			#region Restrictions

			/**<summary>IsConsumable</summary>*/
			Read(node, "IsConsumable", ref consumable);
			/**<summary>IsUniqueItem</summary>*/
			Read(node, "IsUniqueItem", ref uniqueStack);
			/**<summary>IsExpertOnly</summary>*/
			Read(node, "IsExpertOnly", ref expertOnly);
			/**<summary>MaxStackSize</summary>*/
			Read(node, "MaxStackSize", ref maxStack);
			/**<summary>IsUnusableUnderwater</summary>*/
			Read(node, "IsUnusableUnderwater", ref noWet);

			#endregion
			//--------------------------------
			#region Tools

			/**<summary>AxePower</summary>*/
			Read(node, "AxePower", ref axe);
			/**<summary>BaitPower</summary>*/
			Read(node, "BaitPower", ref bait);
			/**<summary>FishingPower</summary>*/
			Read(node, "FishingPower", ref fishingPole);
			/**<summary>HammerPower</summary>*/
			Read(node, "HammerPower", ref hammer);
			/**<summary>IsHoldableLight</summary>*/
			Read(node, "IsHoldableLight", ref flame);
			/**<summary>SpawnNPCID</summary>*/
			Read(node, "SpawnNPCID", ref makeNPC);
			/**<summary>PickaxePower</summary>*/
			Read(node, "PickaxePower", ref pick);
			/**<summary>PlacedTileID</summary>*/
			Read(node, "PlacedTileID", ref createTile);
			/**<summary>PlacedWallID</summary>*/
			Read(node, "PlacedWallID", ref createWall);
			/**<summary>PlaceStyle</summary>*/
			Read(node, "PlaceStyle", ref placeStyle);
			/**<summary>TileRange</summary>*/
			Read(node, "TileRange", ref tileBoost);
			/**<summary>TileWandResourceID</summary>*/
			Read(node, "TileWandResourceID", ref tileWand);

			#endregion
			//--------------------------------
			#region Using

			/**<summary>AutoReuse</summary>*/
			Read(node, "AutoReuse", ref autoReuse);
			/**<summary>CanChangeDirection</summary>*/
			Read(node, "CanChangeDirection", ref useTurn);
			/**<summary>ReuseDelay</summary>*/
			Read(node, "ReuseDelay", ref reuseDelay);
			/**<summary>UseAnimation</summary>*/
			Read(node, "UseAnimation", ref useAnimation);
			/**<summary>UseStyle</summary>*/
			Read(node, "UseStyle", ref useStyle);
			/**<summary>UseTime</summary>*/
			Read(node, "UseTime", ref useTime);

			#endregion
			//--------------------------------
			#region Visuals

			/**<summary>Alpha</summary>*/
			Read(node, "Alpha", ref alpha);
			/**<summary>Color</summary>*/
			Read(node, "Color", ref color);
			/**<summary>HoldStyle</summary>*/
			Read(node, "HoldStyle", ref holdStyle);
			/**<summary>NoSwingAnimation</summary>*/
			Read(node, "NoSwingAnimation", ref noMelee);
			/**<summary>NoUseGraphic</summary>*/
			Read(node, "NoUseGraphic", ref noUseGraphic);
			/**<summary>Scale</summary>*/
			Read(node, "Scale", ref scale);
			/**<summary>UseSound</summary>*/
			Read(node, "UseSound", ref UseSound);

			#endregion
		}
		/**<summary>Saves the item modification to xml.</summary>*/
		public void SaveXml(XmlDocument doc, XmlElement item) {
			#region Categorization

			/**<summary>IsMelee</summary>*/
			Write(doc, item, "IsMelee", melee);
			/**<summary>IsRanged</summary>*/
			Write(doc, item, "IsRanged", ranged);
			/**<summary>IsThrown</summary>*/
			Write(doc, item, "IsThrown", thrown);
			/**<summary>IsMagic</summary>*/
			Write(doc, item, "IsMagic", magic);
			/**<summary>IsSummon</summary>*/
			Write(doc, item, "IsSummon", summon);
			/**<summary>IsSentry</summary>*/
			Write(doc, item, "IsSentry", sentry);
			/**<summary>IsDD2Summon</summary>*/
			Write(doc, item, "IsDD2Summon", DD2Summon);
			/**<summary>IsAccessory</summary>*/
			Write(doc, item, "IsAccessory", accessory);
			/**<summary>IsVanity</summary>*/
			Write(doc, item, "IsVanity", vanity);
			/**<summary>IsPotion</summary>*/
			Write(doc, item, "IsPotion", potion);

			#endregion
			//--------------------------------
			#region Combat

			/**<summary>AmmoID</summary>*/
			Write(doc, item, "AmmoID", ammo);
			/**<summary>BuffID</summary>*/
			Write(doc, item, "BuffID", buffType);
			/**<summary>BuffTime</summary>*/
			Write(doc, item, "BuffTime", buffTime);
			/**<summary>CritChance</summary>*/
			Write(doc, item, "CritChance", crit);
			/**<summary>Damage</summary>*/
			Write(doc, item, "Damage", damage);
			/**<summary>Defense</summary>*/
			Write(doc, item, "Defense", defense);
			/**<summary>HealLife</summary>*/
			Write(doc, item, "HealLife", healLife);
			/**<summary>HealMana</summary>*/
			Write(doc, item, "HealMana", healMana);
			/**<summary>Knockback</summary>*/
			Write(doc, item, "Knockback", knockBack);
			/**<summary>LifeRegen</summary>*/
			Write(doc, item, "LifeRegen", lifeRegen);
			/**<summary>ManaIncrease</summary>*/
			Write(doc, item, "ManaIncrease", manaIncrease);
			/**<summary>ManaUse</summary>*/
			Write(doc, item, "ManaUse", mana);
			/**<summary>NotAmmo</summary>*/
			Write(doc, item, "NotAmmo", notAmmo);
			/**<summary>ProjectileID</summary>*/
			Write(doc, item, "ProjectileID", shoot);
			/**<summary>ProjectileSpeed</summary>*/
			Write(doc, item, "ProjectileSpeed", shootSpeed);
			/**<summary>RequiredAmmoID</summary>*/
			Write(doc, item, "RequiredAmmoID", useAmmo);

			#endregion
			//--------------------------------
			#region Details

			/**<summary>Name</summary>*/
			Write(doc, item, "Name", name);
			/**<summary>Tooltip</summary>*/
			Write(doc, item, "Tooltip", tooltip);
			/**<summary>Rarity</summary>*/
			Write(doc, item, "Rarity", rare);
			/**<summary>IsExpert</summary>*/
			Write(doc, item, "IsExpert", expert);
			/**<summary>IsQuestItem</summary>*/
			Write(doc, item, "IsQuestItem", questItem);
			/**<summary>Value</summary>*/
			Write(doc, item, "Value", value);

			#endregion
			//--------------------------------
			#region Ranges

			/**<summary>CoinGrabRange</summary>*/
			//Write(doc, item, "CoinGrabRange", coinGrabRange);
			/**<summary>LifeGrabRange</summary>*/
			//Write(doc, item, "LifeGrabRange", lifeGrabRange);
			/**<summary>ManaGrabRange</summary>*/
			//Write(doc, item, "ManaGrabRange", manaGrabRange);

			#endregion
			//--------------------------------
			#region Restrictions

			/**<summary>IsConsumable</summary>*/
			Write(doc, item, "IsConsumable", consumable);
			/**<summary>IsUniqueItem</summary>*/
			Write(doc, item, "IsUniqueItem", uniqueStack);
			/**<summary>IsExpertOnly</summary>*/
			Write(doc, item, "IsExpertOnly", expertOnly);
			/**<summary>MaxStackSize</summary>*/
			Write(doc, item, "MaxStackSize", maxStack);
			/**<summary>UnusableUnderwater</summary>*/
			Write(doc, item, "UnusableUnderwater", noWet);

			#endregion
			//--------------------------------
			#region Tools

			/**<summary>AxePower</summary>*/
			Write(doc, item, "AxePower", axe);
			/**<summary>BaitPower</summary>*/
			Write(doc, item, "BaitPower", bait);
			/**<summary>FishingPower</summary>*/
			Write(doc, item, "FishingPower", fishingPole);
			/**<summary>HammerPower</summary>*/
			Write(doc, item, "HammerPower", hammer);
			/**<summary>IsHoldableLight</summary>*/
			Write(doc, item, "IsHoldableLight", flame);
			/**<summary>SpawnNPCID</summary>*/
			Write(doc, item, "SpawnNPCID", makeNPC);
			/**<summary>PickaxePower</summary>*/
			Write(doc, item, "PickaxePower", pick);
			/**<summary>PlacedTileID</summary>*/
			Write(doc, item, "PlacedTileID", createTile);
			/**<summary>PlacedWallID</summary>*/
			Write(doc, item, "PlacedWallID", createWall);
			/**<summary>PlaceStyle</summary>*/
			Write(doc, item, "PlaceStyle", placeStyle);
			/**<summary>TileRange</summary>*/
			Write(doc, item, "TileRange", tileBoost);
			/**<summary>TileWandResourceID</summary>*/
			Write(doc, item, "TileWandResourceID", tileWand);

			#endregion
			//--------------------------------
			#region Using

			/**<summary>AutoReuse</summary>*/
			Write(doc, item, "AutoReuse", autoReuse);
			/**<summary>CanChangeDirection</summary>*/
			Write(doc, item, "CanChangeDirection", useTurn);
			/**<summary>ReuseDelay</summary>*/
			Write(doc, item, "ReuseDelay", reuseDelay);
			/**<summary>UseAnimation</summary>*/
			Write(doc, item, "UseAnimation", useAnimation);
			/**<summary>UseStyle</summary>*/
			Write(doc, item, "UseStyle", useStyle);
			/**<summary>UseTime</summary>*/
			Write(doc, item, "UseTime", useTime);

			#endregion
			//--------------------------------
			#region Visuals

			/**<summary>Alpha</summary>*/
			Write(doc, item, "Alpha", alpha);
			/**<summary>Color</summary>*/
			Write(doc, item, "Color", color);
			/**<summary>HoldStyle</summary>*/
			Write(doc, item, "HoldStyle", holdStyle);
			/**<summary>NoSwingAnimation</summary>*/
			Write(doc, item, "NoSwingAnimation", noMelee);
			/**<summary>NoUseGraphic</summary>*/
			Write(doc, item, "NoUseGraphic", noUseGraphic);
			/**<summary>Scale</summary>*/
			Write(doc, item, "Scale", scale);
			/**<summary>UseSound</summary>*/
			Write(doc, item, "UseSound", UseSound);

			#endregion
		}

		#endregion
		//========= MODIFICATION =========
		#region Modification

		/**<summary>Modifies the item's members.</summary>*/
		public void ModifyItem(Item item) {
			#region Categorization

			/**<summary>IsMelee</summary>*/
			Modify(melee, ref item.melee);
			/**<summary>IsRanged</summary>*/
			Modify(ranged, ref item.ranged);
			/**<summary>IsThrown</summary>*/
			Modify(thrown, ref item.thrown);
			/**<summary>IsMagic</summary>*/
			Modify(magic, ref item.magic);
			/**<summary>IsSummon</summary>*/
			Modify(summon, ref item.summon);
			/**<summary>IsSentry</summary>*/
			Modify(sentry, ref item.sentry);
			/**<summary>IsDD2Summon</summary>*/
			Modify(DD2Summon, ref item.DD2Summon);
			/**<summary>IsAccessory</summary>*/
			Modify(accessory, ref item.accessory);
			/**<summary>IsVanity</summary>*/
			Modify(vanity, ref item.vanity);
			/**<summary>IsPotion</summary>*/
			Modify(potion, ref item.potion);

			#endregion
			//--------------------------------
			#region Combat

			/**<summary>AmmoID</summary>*/
			Modify(ammo, ref item.ammo);
			/**<summary>BuffID</summary>*/
			Modify(buffType, ref item.buffType);
			/**<summary>BuffTime</summary>*/
			Modify(buffTime, ref item.buffTime);
			/**<summary>CritChance</summary>*/
			Modify(crit, ref item.crit);
			/**<summary>Damage</summary>*/
			Modify(damage, ref item.damage);
			/**<summary>Defense</summary>*/
			Modify(defense, ref item.defense);
			/**<summary>HealLife</summary>*/
			Modify(healLife, ref item.healLife);
			/**<summary>HealMana</summary>*/
			Modify(healMana, ref item.healMana);
			/**<summary>Knockback</summary>*/
			Modify(knockBack, ref item.knockBack);
			/**<summary>LifeRegen</summary>*/
			Modify(lifeRegen, ref item.lifeRegen);
			/**<summary>ManaIncrease</summary>*/
			Modify(manaIncrease, ref item.manaIncrease);
			/**<summary>ManaUse</summary>*/
			Modify(mana, ref item.mana);
			/**<summary>NotAmmo</summary>*/
			Modify(notAmmo, ref item.notAmmo);
			/**<summary>ProjectileID</summary>*/
			Modify(shoot, ref item.shoot);
			/**<summary>ProjectileSpeed</summary>*/
			Modify(shootSpeed, ref item.shootSpeed);
			/**<summary>RequiredAmmoID</summary>*/
			Modify(useAmmo, ref item.useAmmo);

			#endregion
			//--------------------------------
			#region Details

			/**<summary>Name</summary>*/
			//Write(Name, ref item.name);
			// Set on ItemModifier.LoadModifications()
			/**<summary>Tooltip</summary>*/
			//Write(Tooltip, ref item.ToolTip);
			// Set on ItemModifier.LoadModifications()
			/**<summary>Rarity</summary>*/
			Modify(rare, ref item.rare);
			/**<summary>IsExpert</summary>*/
			Modify(expert, ref item.expert);
			/**<summary>IsQuestItem</summary>*/
			Modify(questItem, ref item.questItem);
			/**<summary>Value</summary>*/
			Modify(value, ref item.value);

			#endregion
			//--------------------------------
			#region Ranges

			/**<summary>CoinGrabRange</summary>*/
			//Write(coinGrabRange, ref item.coinGrabRange);
			// Apparently this is static
			/**<summary>LifeGrabRange</summary>*/
			//Write(lifeGrabRange, ref item.lifeGrabRange);
			// Apparently this is static
			/**<summary>ManaGrabRange</summary>*/
			//Write(manaGrabRange, ref item.manaGrabRange);
			// Apparently this is static

			#endregion
			//--------------------------------
			#region Restrictions

			/**<summary>IsConsumable</summary>*/
			Modify(consumable, ref item.consumable);
			/**<summary>IsUniqueItem</summary>*/
			Modify(uniqueStack, ref item.uniqueStack);
			/**<summary>IsExpertOnly</summary>*/
			Modify(expertOnly, ref item.expertOnly);
			/**<summary>MaxStackSize</summary>*/
			Modify(maxStack, ref item.maxStack);
			/**<summary>UnusableUnderwater</summary>*/
			Modify(noWet, ref item.noWet);

			#endregion
			//--------------------------------
			#region Tools

			/**<summary>AxePower</summary>*/
			Modify(axe, ref item.axe);
			/**<summary>BaitPower</summary>*/
			Modify(bait, ref item.bait);
			/**<summary>FishingPower</summary>*/
			Modify(fishingPole, ref item.fishingPole);
			/**<summary>HammerPower</summary>*/
			Modify(hammer, ref item.hammer);
			/**<summary>IsHoldableLight</summary>*/
			Modify(flame, ref item.flame);
			/**<summary>SpawnNPCID</summary>*/
			Modify(makeNPC, ref item.makeNPC);
			/**<summary>PickaxePower</summary>*/
			Modify(pick, ref item.pick);
			/**<summary>PlacedTileID</summary>*/
			Modify(createTile, ref item.createTile);
			/**<summary>PlacedWallID</summary>*/
			Modify(createWall, ref item.createWall);
			/**<summary>PlaceStyle</summary>*/
			Modify(placeStyle, ref item.placeStyle);
			/**<summary>TileRange</summary>*/
			Modify(tileBoost, ref item.tileBoost);
			/**<summary>TileWandResourceID</summary>*/
			Modify(tileWand, ref item.tileWand);

			#endregion
			//--------------------------------
			#region Using

			/**<summary>AutoReuse</summary>*/
			Modify(autoReuse, ref item.autoReuse);
			/**<summary>CanChangeDirection</summary>*/
			Modify(useTurn, ref item.useTurn);
			/**<summary>ReuseDelay</summary>*/
			Modify(reuseDelay, ref item.reuseDelay);
			/**<summary>UseAnimation</summary>*/
			Modify(useAnimation, ref item.useAnimation);
			/**<summary>UseStyle</summary>*/
			Modify(useStyle, ref item.useStyle);
			/**<summary>UseTime</summary>*/
			Modify(useTime, ref item.useTime);

			#endregion
			//--------------------------------
			#region Visuals

			/**<summary>Alpha</summary>*/
			Modify(alpha, ref item.alpha);
			/**<summary>Color</summary>*/
			Modify(color, ref item.color);
			/**<summary>HoldStyle</summary>*/
			Modify(holdStyle, ref item.holdStyle);
			/**<summary>NoSwingAnimation</summary>*/
			Modify(noMelee, ref item.noMelee);
			/**<summary>NoUseGraphic</summary>*/
			Modify(noUseGraphic, ref item.noUseGraphic);
			/**<summary>Scale</summary>*/
			Modify(scale, ref item.scale);
			/**<summary>UseSound</summary>*/
			//Write(UseSound, ref item.UseSound);
			// This needs to be updated when more sounds are added
			if (UseSound != null) {
				try {
					// Use reflection to get the correct LegacySoundStyle
					FieldInfo field = typeof(SoundID).GetField(UseSound, BindingFlags.Public | BindingFlags.Static);
					item.UseSound = (LegacySoundStyle)field.GetValue(null);
				}
				catch {
					// Failed to specify a valid sound
				}
			}

			#endregion
		}

		#endregion
		//=========== HELPERS ============
		#region Helpers
		//--------------------------------
		#region Reading

		/**<summary>Parses a string value.</summary>*/
		private void Read(XmlNode node, string name, ref string value) {
			if (node[name] != null)
				value = node[name].InnerText;
		}
		/**<summary>Parses a bool value.</summary>*/
		private void Read(XmlNode node, string name, ref bool? value) {
			bool parseValue;
			if (node[name] != null && bool.TryParse(node[name].InnerText, out parseValue))
				value = parseValue;
		}
		/**<summary>Parses a short value.</summary>*/
		public void Read(XmlNode node, string name, ref short? value) {
			short parseValue;
			if (node[name] != null && short.TryParse(node[name].InnerText, out parseValue))
				value = parseValue;
		}
		/**<summary>Parses an integer value.</summary>*/
		public void Read(XmlNode node, string name, ref int? value) {
			int parseValue;
			if (node[name] != null && int.TryParse(node[name].InnerText, out parseValue))
				value = parseValue;
		}
		/**<summary>Parses a float value.</summary>*/
		public void Read(XmlNode node, string name, ref float? value) {
			float parseValue;
			if (node[name] != null && float.TryParse(node[name].InnerText, out parseValue))
				value = parseValue;
		}
		/**<summary>Parses an XNA color value.</summary>*/
		public void Read(XmlNode node, string name, ref Color? value) {
			if (node[name] != null) {
				string text = node[name].InnerText;
				string[] channels = text.Split(',');
				if (channels.Length != 3)
					return;

				byte r, g, b;
				if (!byte.TryParse(channels[0], out r)) return;
				if (!byte.TryParse(channels[1], out g)) return;
				if (!byte.TryParse(channels[2], out b)) return;
				value = new Color(r, g, b, 0);
			}
		}

		#endregion
		//--------------------------------
		#region Writing

		/**<summary>Writes a string value.</summary>*/
		private void Write(XmlDocument doc, XmlElement item, string name, string value) {
			if (value != null) {
				XmlElement element = doc.CreateElement(name);
				element.AppendChild(doc.CreateTextNode(value));
				item.AppendChild(element);
			}
		}
		/**<summary>Writes a bool value.</summary>*/
		private void Write(XmlDocument doc, XmlElement item, string name, bool? value) {
			if (value.HasValue) {
				XmlElement element = doc.CreateElement(name);
				element.AppendChild(doc.CreateTextNode(value.Value.ToString()));
				item.AppendChild(element);
			}
		}
		/**<summary>Writes a short value.</summary>*/
		private void Write(XmlDocument doc, XmlElement item, string name, short? value) {
			if (value.HasValue) {
				XmlElement element = doc.CreateElement(name);
				element.AppendChild(doc.CreateTextNode(value.Value.ToString()));
				item.AppendChild(element);
			}
		}
		/**<summary>Writes an integer value.</summary>*/
		private void Write(XmlDocument doc, XmlElement item, string name, int? value) {
			if (value.HasValue) {
				XmlElement element = doc.CreateElement(name);
				element.AppendChild(doc.CreateTextNode(value.Value.ToString()));
				item.AppendChild(element);
			}
		}
		/**<summary>Writes a float value.</summary>*/
		private void Write(XmlDocument doc, XmlElement item, string name, float? value) {
			if (value.HasValue) {
				XmlElement element = doc.CreateElement(name);
				element.AppendChild(doc.CreateTextNode(value.Value.ToString()));
				item.AppendChild(element);
			}
		}
		/**<summary>Writes an XNA color value.</summary>*/
		private void Write(XmlDocument doc, XmlElement item, string name, Color? value) {
			if (value.HasValue) {
				XmlElement element = doc.CreateElement(name);
				element.AppendChild(doc.CreateTextNode(
					value.Value.R.ToString() + "," +
					value.Value.G.ToString() + "," +
					value.Value.B.ToString()
				));
				item.AppendChild(element);
			}
		}

		#endregion
		//--------------------------------
		#region Modifying

		/**<summary>Writes the string value to the item.</summary>*/
		public void Modify(string value, ref string output) {
			if (value != null)
				output = value;
		}
		/**<summary>Writes the bool value to the item.</summary>*/
		public void Modify(bool? value, ref bool output) {
			if (value.HasValue)
				output = value.Value;
		}
		/**<summary>Writes the short value to the item.</summary>*/
		public void Modify(short? value, ref short output) {
			if (value.HasValue)
				output = value.Value;
		}
		/**<summary>Writes the integer value to the item.</summary>*/
		public void Modify(int? value, ref int output) {
			if (value.HasValue)
				output = value.Value;
		}
		/**<summary>Writes the float value to the item.</summary>*/
		public void Modify(float? value, ref float output) {
			if (value.HasValue)
				output = value.Value;
		}
		/**<summary>Writes the XNA color value to the item.</summary>*/
		public void Modify(Color? value, ref Color output) {
			if (value.HasValue)
				output = value.Value;
		}

		#endregion
		//--------------------------------
		#endregion
	}
}
