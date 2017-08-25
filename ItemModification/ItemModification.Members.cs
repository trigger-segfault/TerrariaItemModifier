using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace TerrariaItemModifier {
	/**<summary>Modification settings for an item.</summary>*/
	public partial class ItemModification {
		//=========== MEMBERS ============
		#region Members
		//--------------------------------
		#region Categorization

		/**<summary>IsMelee</summary>*/
		public bool? melee;
		/**<summary>IsRanged</summary>*/
		public bool? ranged;
		/**<summary>IsThrown</summary>*/
		public bool? thrown;
		/**<summary>IsMagic</summary>*/
		public bool? magic;
		/**<summary>IsSummon</summary>*/
		public bool? summon;
		/**<summary>IsSentry</summary>*/
		public bool? sentry;
		/**<summary>IsDD2Summon</summary>*/
		public bool? DD2Summon;
		/**<summary>IsAccessory</summary>*/
		public bool? accessory;

		#endregion
		//--------------------------------
		#region Combat

		/**<summary>AmmoID</summary>*/
		public int? ammo;
		/**<summary>BuffID</summary>*/
		public int? buffType;
		/**<summary>BuffTime</summary>*/
		public int? buffTime;
		/**<summary>CritChance</summary>*/
		public int? crit;
		/**<summary>Damage</summary>*/
		public int? damage;
		/**<summary>Defense</summary>*/
		public int? defense;
		/**<summary>HealLife</summary>*/
		public int? healLife;
		/**<summary>HealMana</summary>*/
		public int? healMana;
		/**<summary>Knockback</summary>*/
		public float? knockBack;
		/**<summary>LifeRegen</summary>*/
		public int? lifeRegen;
		/**<summary>ManaIncrease</summary>*/
		public int? manaIncrease;
		/**<summary>ManaUse</summary>*/
		public int? mana;
		/**<summary>ProjectileID</summary>*/
		public int? shoot;
		/**<summary>ProjectileSpeed</summary>*/
		public float? shootSpeed;
		/**<summary>RequiredAmmoID</summary>*/
		public int? useAmmo;

		#endregion
		//--------------------------------
		#region Details

		/**<summary>Name</summary>*/
		public string name;
		/**<summary>Tooltip</summary>*/
		public string tooltip;
		/**<summary>Rarity</summary>*/
		public int? rare;
		/**<summary>IsExpert</summary>*/
		public bool? expert;
		/**<summary>IsMaterial</summary>*/
		public bool? material;
		/**<summary>IsQuestItem</summary>*/
		public bool? questItem;
		/**<summary>IsVanity</summary>*/
		public bool? vanity;
		/**<summary>NotAmmo</summary>*/
		public bool? notAmmo;
		/**<summary>Value</summary>*/
		public int? value;

		#endregion
		//--------------------------------
		#region Grab Ranges

		/**<summary>CoinGrabRange</summary>*/
		//public int? coinGrabRange;
		/**<summary>LifeGrabRange</summary>*/
		//public int? lifeGrabRange;
		/**<summary>ManaGrabRange</summary>*/
		//public int? manaGrabRange;

		#endregion
		//--------------------------------
		#region Restrictions

		/**<summary>IsConsumable</summary>*/
		public bool? consumable;
		/**<summary>IsExpertOnly</summary>*/
		public bool? expertOnly;
		/**<summary>IsHealingPotion</summary>*/
		public bool? potion;
		/**<summary>IsUniqueItem</summary>*/
		public bool? uniqueStack;
		/**<summary>MaxStackSize</summary>*/
		public int? maxStack;
		/**<summary>UnusableUnderwater</summary>*/
		public bool? noWet;

		#endregion
		//--------------------------------
		#region Tools

		/**<summary>AxePower</summary>*/
		public int? axe;
		/**<summary>BaitPower</summary>*/
		public int? bait;
		/**<summary>FishingPower</summary>*/
		public int? fishingPole;
		/**<summary>HammerPower</summary>*/
		public int? hammer;
		/**<summary>IsHoldableLight</summary>*/
		public bool? flame;
		/**<summary>IsWiring</summary>*/
		public bool? mech;
		/**<summary>SpawnNPCID</summary>*/
		public short? makeNPC;
		/**<summary>PickaxePower</summary>*/
		public int? pick;
		/**<summary>PlacedTileID</summary>*/
		public int? createTile;
		/**<summary>PlacedWallID</summary>*/
		public int? createWall;
		/**<summary>PlaceStyle</summary>*/
		public int? placeStyle;
		/**<summary>TileRange</summary>*/
		public int? tileBoost;
		/**<summary>TileWandResourceID</summary>*/
		public int? tileWand;

		#endregion
		//--------------------------------
		#region Using

		/**<summary>AutoReuse</summary>*/
		public bool? autoReuse;
		/**<summary>CanChangeDirection</summary>*/
		public bool? useTurn;
		/**<summary>ReuseDelay</summary>*/
		public int? reuseDelay;
		/**<summary>UseAnimation</summary>*/
		public int? useAnimation;
		/**<summary>UseStyle</summary>*/
		public int? useStyle;
		/**<summary>UseTime</summary>*/
		public int? useTime;

		#endregion
		//--------------------------------
		#region Visuals

		/**<summary>Alpha</summary>*/
		public int? alpha;
		/**<summary>Color</summary>*/
		public Color? color;
		/**<summary>HoldStyle</summary>*/
		public int? holdStyle;
		/**<summary>NoSwingAnimation</summary>*/
		public bool? noMelee;
		/**<summary>NoUseGraphic</summary>*/
		public bool? noUseGraphic;
		/**<summary>Scale</summary>*/
		public float? scale;
		/**<summary>UseSound</summary>*/
		public string UseSound;

		#endregion
		//--------------------------------
		#endregion
	}
}
