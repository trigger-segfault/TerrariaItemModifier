using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaItemModifier {
	/**<summary>Modification settings for an item.</summary>*/
	public partial class ItemModification {
		//======== STATIC MEMBERS ========
		#region Static Members

		/**<summary>The collection of modifiable variables.</summary>*/
		private static Dictionary<string, VarInfo> VarInfoList;

		#endregion
		//=========== MEMBERS ============
		#region Members

		/**<summary>The ID of the item.</summary>*/
		public int ID;
		/**<summary>The collection of modified variables.</summary>*/
		public Dictionary<string, Variable> VarList;

		#endregion
		//========= CONSTRUCTORS =========
		#region Constructors
			
		/**<summary>Constructs the item modification.</summary>*/
		public ItemModification(int id) {
			this.ID = id;
			this.VarList = new Dictionary<string, Variable>();
		}
		/**<summary>Initializes the variable list.</summary>*/
		static ItemModification() {
			VarInfoList = new Dictionary<string, VarInfo>();

			//--------------------------------
			#region Categorization

			AddVar(VarTypes.Bool,	"melee",		"IsMelee");
			AddVar(VarTypes.Bool,	"ranged",		"IsRanged");
			AddVar(VarTypes.Bool,	"thrown",		"IsThrown");
			AddVar(VarTypes.Bool,	"magic",		"IsMagic");
			AddVar(VarTypes.Bool,	"summon",		"IsSummon");
			AddVar(VarTypes.Bool,	"sentry",		"IsSentry");
			AddVar(VarTypes.Bool,	"accessory",	"IsAccessory");

			#endregion
			//--------------------------------
			#region Combat

			AddVar(VarTypes.Int,	"ammo",			"AmmoID");
			AddVar(VarTypes.Int,	"buffType",		"BuffID");
			AddVar(VarTypes.Int,	"buffTime",		"BuffTime");
			AddVar(VarTypes.Int,	"crit",			"CritChance");
			AddVar(VarTypes.Int,	"damage",		"Damage");
			AddVar(VarTypes.Int,	"defense",		"Defense");
			AddVar(VarTypes.Int,	"healLife",		"HealLife");
			AddVar(VarTypes.Int,	"healMana",		"HealMana");
			AddVar(VarTypes.Float,	"knockBack",	"Knockback");
			AddVar(VarTypes.Int,	"lifeRegen",	"LifeRegen");
			AddVar(VarTypes.Int,	"manaIncrease",	"ManaIncrease");
			AddVar(VarTypes.Int,	"mana",			"ManaUse");
			AddVar(VarTypes.Bool,	"noMelee",		"NoSwingDamage");
			AddVar(VarTypes.Int,	"shoot",		"ProjectileID");
			AddVar(VarTypes.Float,	"shootSpeed",	"ProjectileSpeed");
			AddVar(VarTypes.Int,	"useAmmo",		"RequiredAmmoID");

			#endregion
			//--------------------------------
			#region Details
			
			AddVar(VarTypes.String,	"",				"Name",			VarSetters.None); // Set by ItemModifier after reading
			AddVar(VarTypes.String,	"",				"Tooltip",		VarSetters.None); // Set by ItemModifier after reading
			AddVar(VarTypes.Bool,	"expert",		"IsExpert");
			AddVar(VarTypes.Bool,	"material",		"IsMaterial");
			AddVar(VarTypes.Bool,	"questItem",	"IsQuestItem");
			AddVar(VarTypes.Bool,	"vanity",		"IsVanity");
			AddVar(VarTypes.Bool,	"notAmmo",		"NotAmmo");
			AddVar(VarTypes.Int,	"rare",			"Rarity");
			AddVar(VarTypes.Int,	"value",		"Value");

			#endregion
			//--------------------------------
			#region Restrictions

			AddVar(VarTypes.Bool,	"consumable",	"IsConsumable");
			AddVar(VarTypes.Bool,	"DD2Summon",	"IsDD2Summon");
			AddVar(VarTypes.Bool,	"expertOnly",	"IsExpertOnly");
			AddVar(VarTypes.Bool,	"potion",		"IsHealingPotion");
			AddVar(VarTypes.Bool,	"uniqueStack",	"IsUniqueItem");
			AddVar(VarTypes.Int,	"maxStack",		"MaxStackSize");
			AddVar(VarTypes.Bool,	"noWet",		"UnusableUnderwater");

			#endregion
			//--------------------------------
			#region Tools

			AddVar(VarTypes.Int,	"axe",			"AxePower");
			AddVar(VarTypes.Int,	"bait",			"BaitPower");
			AddVar(VarTypes.Int,	"fishingPole",	"FishingPower");
			AddVar(VarTypes.Int,	"hammer",		"HammerPower");
			AddVar(VarTypes.Bool,	"flame",		"IsHoldableLight");
			AddVar(VarTypes.Bool,	"mech",			"IsWiring");
			AddVar(VarTypes.Short,	"makeNPC",		"SpawnNPCID");
			AddVar(VarTypes.Int,	"pick",			"PickaxePower");
			AddVar(VarTypes.Int,	"createTile",	"PlacedTileID");
			AddVar(VarTypes.Int,	"createWall",	"PlacedWallID");
			AddVar(VarTypes.Int,	"placeStyle",	"PlaceStyle");
			AddVar(VarTypes.Int,	"tileBoost",	"TileRange");
			AddVar(VarTypes.Int,	"tileWand",		"TileWandResourceID");

			#endregion
			//--------------------------------
			#region Using
			
			AddVar(VarTypes.Bool,	"autoReuse",	"AutoReuse");
			AddVar(VarTypes.Int,	"reuseDelay",	"ReuseDelay");
			AddVar(VarTypes.Int,	"useAnimation",	"UseAnimation");
			AddVar(VarTypes.Int,	"useStyle",		"UseStyle");
			AddVar(VarTypes.Int,	"useTime",		"UseTime");
			AddVar(VarTypes.Bool,	"useTurn",		"UseTurn");

			#endregion
			//--------------------------------
			#region Visuals
			
			AddVar(VarTypes.Int,	"alpha",		"Alpha");
			AddVar(VarTypes.Color,	"color",		"Color");
			AddVar(VarTypes.Int,	"holdStyle",	"HoldStyle");
			AddVar(VarTypes.Bool,	"noUseGraphic",	"NoUseGraphic");
			AddVar(VarTypes.Float,	"scale",		"Scale");
			AddVar(VarTypes.UseSound,"UseSound",	"UseSound"); // Set as a LegacySoundStyle
			AddVar(VarTypes.Int,	"width",		"Width");
			AddVar(VarTypes.Int,	"height",		"Height");

			#endregion
		}

		#endregion
		//=========== HELPERS ============
		#region Helpers
		
		/**<summary>Registers a variable.</summary>*/
		private static void AddVar(VarTypes type, string internalName, string externalName, VarSetters setter = VarSetters.Normal) {
			VarInfoList.Add(externalName, new VarInfo(type, internalName, setter));
		}

		#endregion
	}
}
