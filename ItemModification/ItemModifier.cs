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

			public static ConstructorInfo ctor_ItemToolTip;
			public static ConstructorInfo ctor_LocalizedText;

			public static FieldInfo _itemNameCache_Lang;
			public static FieldInfo _itemTooltipCache_Lang;
			public static FieldInfo _text_ItemTooltip;
			public static FieldInfo _lastCulture_ItemTooltip;

			#endregion
			//========= CONSTRUCTORS =========
			#region Constructors

			/**<summary>Aquire all of the reflection infos ahead of time to reduce reflection slowdown.</summary>*/
			static TerrariaReflection() {
				ctor_ItemToolTip		 = typeof(ItemTooltip).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
				ctor_LocalizedText		 = typeof(LocalizedText).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();

				_itemNameCache_Lang		 = typeof(Lang).GetField("_itemNameCache", BindingFlags.NonPublic | BindingFlags.Static);
				_itemTooltipCache_Lang	 = typeof(Lang).GetField("_itemTooltipCache", BindingFlags.NonPublic | BindingFlags.Static);
				_text_ItemTooltip		 = typeof(ItemTooltip).GetField("_text", BindingFlags.NonPublic | BindingFlags.Instance);
				_lastCulture_ItemTooltip = typeof(ItemTooltip).GetField("_lastCulture", BindingFlags.NonPublic | BindingFlags.Instance);

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
		//============ HOOKS =============
		#region Hooks

		/**<summary>Called when item defaults are setup.</summary>*/
		public static void OnSetItemDefaults(Item item) {
			if (ItemMods.ContainsKey(item.type)) {
				ItemModification itemMod = ItemMods[item.type];

				itemMod.ModifyItem(item);

				if (ErrorLogger.IsOpen)
					ErrorLogger.Close();
			}
		}
		/**<summary>Called on Main.LoadPlayers to reload all of the modifications.</summary>*/
		public static void OnSetupModifications() {
			if (File.Exists(ConfigPath))
				LoadModifications();
			else
				SaveExampleModifications();
		}

		#endregion
		//=========== LOADING ============
		#region Loading

		/**<summary>Loads or reloads the modifications xml doc.</summary>*/
		private static void LoadModifications() {
			try {
				// Make NPCs releasable (catchable does not seem to affect catching NPCs with bug nets)
				for (int i = 0; i < Main.npcCatchable.Length; i++) {
					Main.npcCatchable[i] = true;
				}

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
				if (!ErrorLogger.IsOpen)
					ErrorLogger.Open();
				ErrorLogger.WriteErrorHeader();
				ErrorLogger.WriteLine("Error when parsing XML.");
				ErrorLogger.WriteLine("Exception: ");
				ErrorLogger.WriteLine(ex.ToString());
				ErrorLogger.WriteLine();
			}

			// Close the error log if needed
			if (ErrorLogger.IsOpen)
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
			LocalizedText[] _itemNameCache = (LocalizedText[])TerrariaReflection._itemNameCache_Lang.GetValue(null);

			// Create a new LocalizedText
			LocalizedText locText = (LocalizedText)TerrariaReflection.ctor_LocalizedText.Invoke(new object[] { "ItemName" + type.ToString(), name });

			// Assign the new LocalizedText to Lang's item name cache
			_itemNameCache[ItemID.FromNetId((short)type)] = locText;

			// Hooray for reflection!
		}
		/**<summary>Sets the name of an item.</summary>*/
		private static void SetItemTooltip(int type, string tooltip) {
			// Access Lang's item tooltip cache
			ItemTooltip[] _itemTooltipCache = (ItemTooltip[])TerrariaReflection._itemTooltipCache_Lang.GetValue(null);

			// Create a new ItemTooltip
			ItemTooltip itemTooltip = (ItemTooltip)TerrariaReflection.ctor_ItemToolTip.Invoke(new object[] {});
			// Create a new LocalizedText
			LocalizedText locText = (LocalizedText)TerrariaReflection.ctor_LocalizedText.Invoke(new object[] { "ItemTooltip" + type.ToString(), tooltip });

			// Set the text of the tooltip
			TerrariaReflection._text_ItemTooltip.SetValue(itemTooltip, locText);
			// Set it so that the tooltip will be revalidated the first time it's needed.
			TerrariaReflection._lastCulture_ItemTooltip.SetValue(itemTooltip, null);

			// Assign the new ItemTooltip to Lang's item tooltip cache
			_itemTooltipCache[ItemID.FromNetId((short)type)] = itemTooltip;

			// Hooray for reflection!
		}

		#endregion
	}
}
