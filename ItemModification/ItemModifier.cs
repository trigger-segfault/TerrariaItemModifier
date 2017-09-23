using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace TerrariaItemModifier {


	/**<summary>The class that handles changes to items.</summary>*/
	public static class ItemModifier {
		//=========== CLASSES ============
		#region Classes

		/**<summary>Aquire this all ahead of time to reduce reflection slowdown.</summary>*/
		private static class TerrariaReflection {
			//=========== MEMBERS ============
			#region Members

			/**<summary>private ItemTooltip()</summary>*/
			public static ConstructorInfo ItemToolTip_ctor;

			/**<summary>internal LocalizedText(string key, string text)</summary>*/
			public static ConstructorInfo LocalizedText_ctor;

			/**<summary>private static LocalizedText[] Lang._itemNameCache</summary>*/
			public static FieldInfo Lang_itemNameCache;
			/**<summary>private static ItemTooltip[] Lang._itemTooltipCache</summary>*/
			public static FieldInfo Lang_itemTooltipCache;
			/**<summary>private LocalizedText ItemTooltip._text</summary>*/
			public static FieldInfo ItemTooltip_text;
			/**<summary>private GameCulture ItemTooltip._lastCulture</summary>*/
			public static FieldInfo ItemTooltip_lastCulture;

			/**<summary>internal void LocalizedText.SetValue(string text)</summary>*/
			public static MethodInfo LocalizedText_SetValue;

			#endregion
			//========= CONSTRUCTORS =========
			#region Constructors

			/**<summary>Aquire all of the reflection infos ahead of time to reduce reflection slowdown.</summary>*/
			static TerrariaReflection() {
				ItemToolTip_ctor			= typeof(ItemTooltip).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
				LocalizedText_ctor			= typeof(LocalizedText).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();

				Lang_itemNameCache			= typeof(Lang).GetField("_itemNameCache", BindingFlags.NonPublic | BindingFlags.Static);
				Lang_itemTooltipCache       = typeof(Lang).GetField("_itemTooltipCache", BindingFlags.NonPublic | BindingFlags.Static);
				ItemTooltip_text			= typeof(ItemTooltip).GetField("_text", BindingFlags.NonPublic | BindingFlags.Instance);
				ItemTooltip_lastCulture     = typeof(ItemTooltip).GetField("_lastCulture", BindingFlags.NonPublic | BindingFlags.Instance);

				LocalizedText_SetValue		= typeof(LocalizedText).GetMethod("SetValue", BindingFlags.NonPublic | BindingFlags.Instance);

				// Hooray for reflection!
			}

			#endregion
		}

		#endregion
		//========== CONSTANTS ===========
		#region Constants

		/**<summary>The name of the config file.</summary>*/
		public const string ConfigName = "ItemModifications.xml";
		/**<summary>The path of the config file.</summary>*/
		public static readonly string ConfigPath = Path.Combine(Environment.CurrentDirectory, ConfigName);

		#endregion
		//=========== MEMBERS ============
		#region Members

		/**<summary>The items to be modified.</summary>*/
		private static Dictionary<int, ItemModification> ItemMods = new Dictionary<int, ItemModification>();

		#endregion
		//========= CONSTRUCTORS =========
		#region Constructors

		/**<summary>Change a main static field once and not every time we load modifications.</summary>*/
		static ItemModifier() {
			// Make NPCs releasable (catchable does not seem to affect catching NPCs with bug nets)
			for (int i = 0; i < Main.npcCatchable.Length; i++) {
				Main.npcCatchable[i] = true;
			}
		}

		#endregion
		//============ HOOKS =============
		#region Hooks

		/**<summary>Called when item defaults are setup.</summary>*/
		public static void OnSetItemDefaults(Item item) {
			if (ItemMods.ContainsKey(item.type)) {
				ItemModification itemMod = ItemMods[item.type];

				itemMod.ModifyItem(item);

				ErrorLogger.Close();
			}
		}
		/**<summary>Called on Main.LoadPlayers to reload all of the modifications.</summary>*/
		public static void OnLoadModifications() {
			if (File.Exists(ConfigPath)) {
				LoadModifications();
				ReloadRecipeCraftedItems();
			}
			else {
				SaveExampleModifications();
			}
		}

		#endregion
		//=========== LOADING ============
		#region Loading

		/**<summary>Loads or reloads the modifications xml doc.</summary>*/
		private static void LoadModifications() {
			try {

				ItemMods.Clear();

				XmlDocument doc = new XmlDocument();
				XmlAttribute attribute;
				XmlNode node;
				int type;
				doc.Load(ConfigPath);

				XmlNodeList nodeList = doc.SelectNodes("/ItemModifications/Item");
				for (int i = 0; i < nodeList.Count; i++) {
					node = nodeList[i];

					attribute = node.Attributes["ID"];
					if (attribute != null) {
						if (!int.TryParse(attribute.InnerText, out type)) {
							if (!ErrorLogger.IsOpen)
								ErrorLogger.Open();

							ErrorLogger.WriteErrorHeader();
							ErrorLogger.WriteLine("Error when reading XML.");
							ErrorLogger.WriteLine("ItemID: " + attribute.InnerText + " could not be parsed.");
							ErrorLogger.WriteLine();
							continue;
						}
						if (ItemMods.ContainsKey(type)) {
							if (!ErrorLogger.IsOpen)
								ErrorLogger.Open();

							ErrorLogger.WriteErrorHeader();
							ErrorLogger.WriteLine("Error when reading XML.");
							ErrorLogger.WriteLine("ItemID: " + type.ToString() + " already exists.");
							ErrorLogger.WriteLine();
							continue;
						}
						ItemModification item = new ItemModification(type);
						item.LoadXml(node);

						if (item.VarList.ContainsKey("Name")) {
							SetItemName(type, item.VarList["Name"].Value as string);
						}
						if (item.VarList.ContainsKey("Tooltip")) {
							SetItemTooltip(type, item.VarList["Tooltip"].Value as string);
						}

						ItemMods.Add(type, item);
					}
				}
			}
			catch (Exception ex) {
				ErrorLogger.Open();
				ErrorLogger.WriteErrorHeader();
				ErrorLogger.WriteLine("Error when parsing XML.");
				ErrorLogger.WriteLine("Exception: ");
				ErrorLogger.WriteLine(ex.ToString());
				ErrorLogger.WriteLine();
			}

			// Close the error log if needed
			ErrorLogger.Close();
		}
		/**<summary>Saves an example modifications xml doc.</summary>*/
		private static void SaveExampleModifications() {
			try {
				XmlDocument doc = new XmlDocument();
				doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

				doc.AppendChild(doc.CreateComment("Variable List"));
				doc.AppendChild(doc.CreateComment(" https://github.com/trigger-death/TerrariaItemModifier/wiki/Variable-List "));

				XmlElement mods = doc.CreateElement("ItemModifications");
				doc.AppendChild(mods);

				mods.AppendChild(doc.CreateComment("Example Modification: Magical Harp"));
				mods.AppendChild(doc.CreateComment("<Item ID=\"494\">\r\n" +
					"    <Name>Loud &amp; Obnoxious Harp</Name>\r\n" +
					"    <Tooltip>'Turn that racket down you darn kids!'</Tooltip>\r\n" +
					"    <AutoReuse>False</AutoReuse>\r\n" +
					"    <ManaUse>0</ManaUse>\r\n" +
					"    <UseAnimation>3</UseAnimation>\r\n" +
					"    <UseTime>3</UseTime>\r\n" +
					"  </Item>"
				));
				doc.Save(ConfigPath);
			}
			catch { }
		}

		#endregion
		//=========== HELPERS ============
		#region Helpers

		/**<summary>Sets the name of an item.</summary>*/
		private static void SetItemName(int type, string name) {
			// Access Lang's item name cache
			LocalizedText[] _itemNameCache = (LocalizedText[])TerrariaReflection.Lang_itemNameCache.GetValue(null);

			// Get the cached localized text item name
			LocalizedText locText = _itemNameCache[ItemID.FromNetId((short)type)];

			// Set the localized text of the cached item name
			TerrariaReflection.LocalizedText_SetValue.Invoke(locText, new object[] { name });

			// Create a new LocalizedText
			//LocalizedText locText = (LocalizedText)TerrariaReflection.ctor_LocalizedText.Invoke(new object[] { "ItemName" + type.ToString(), name });

			// Assign the new LocalizedText to Lang's item name cache
			//_itemNameCache[ItemID.FromNetId((short)type)] = locText;

			// Hooray for reflection!
		}
		/**<summary>Sets the name of an item.</summary>*/
		private static void SetItemTooltip(int type, string tooltip) {
			// Access Lang's item tooltip cache
			ItemTooltip[] _itemTooltipCache = (ItemTooltip[])TerrariaReflection.Lang_itemTooltipCache.GetValue(null);

			// Get the cached item tooltip
			ItemTooltip itemTooltip = _itemTooltipCache[ItemID.FromNetId((short)type)];
			
			// Tooltip has not been assigned. Let's assign a new one
			if (itemTooltip == ItemTooltip.None) {
				// Create a new item tooltip
				itemTooltip = (ItemTooltip)TerrariaReflection.ItemToolTip_ctor.Invoke(new object[] { });

				// Create the tooltip's new localized text
				LocalizedText _text = (LocalizedText)TerrariaReflection.LocalizedText_ctor.Invoke(new object[] { "", tooltip });

				// Assign the tooltip's new localized text
				TerrariaReflection.ItemTooltip_text.SetValue(itemTooltip, _text);
				
				// Assign the new item tooltip to the cache
				_itemTooltipCache[ItemID.FromNetId((short)type)] = itemTooltip;
			}
			else {
				// Get the tooltip's localized text
				LocalizedText _text = (LocalizedText)TerrariaReflection.ItemTooltip_text.GetValue(itemTooltip);

				// Set the text of the tooltip's localized text
				TerrariaReflection.LocalizedText_SetValue.Invoke(_text, new object[] { tooltip });

				// Set it so that the tooltip will be revalidated the first time it's needed.
				TerrariaReflection.ItemTooltip_lastCulture.SetValue(itemTooltip, null);
			}



			// Create a new ItemTooltip
			/*ItemTooltip itemTooltip = (ItemTooltip)TerrariaReflection.ctor_ItemToolTip.Invoke(new object[] {});
			// Create a new LocalizedText
			LocalizedText locText = (LocalizedText)TerrariaReflection.ctor_LocalizedText.Invoke(new object[] { "ItemTooltip" + type.ToString(), tooltip });

			TerrariaReflection._text_ItemTooltip.SetValue(itemTooltip, locText);

			// Assign the new ItemTooltip to Lang's item tooltip cache
			_itemTooltipCache[ItemID.FromNetId((short)type)] = itemTooltip;*/

			// Hooray for reflection!
		}
		/**<summary>Modify Recipe createItems since they are setup before Item Modifier is loaded.</summary>*/
		private static void ReloadRecipeCraftedItems() {
			foreach (Recipe recipe in Main.recipe) {
				// Save the stack count because it will be reset with SetDefaults
				int stack = recipe.createItem.stack;
				recipe.createItem.SetDefaults(recipe.createItem.type, false);
				// Change the stack back to what it was
				recipe.createItem.stack = stack;
			}
		}

		#endregion
	}
}
