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
		//========== CONSTANTS ===========
		#region Constants

		/**<summary>The name of the config file.</summary>*/
		public const string ConfigName = "ItemModifications.xml";
		/**<summary>The path of the config file.</summary>*/
		public static readonly string ConfigPath = Path.Combine(Environment.CurrentDirectory, ConfigName);
		/**<summary>The path of the error log.</summary>*/
		public static readonly string ErrorLogPath = Path.Combine(Environment.CurrentDirectory, "ItemModificationErrorLog.txt");

		#endregion
		//=========== MEMBERS ============
		#region Members

		/**<summary>The items to be modified.</summary>*/
		public static Dictionary<int, ItemModification> ItemMods = new Dictionary<int, ItemModification>();

		#endregion
		//============ HOOKS =============
		#region Hooks

		/**<summary>Called when item defaults are setup.</summary>*/
		public static void OnSetItemDefaults(Item item) {
			if (ItemMods.ContainsKey(item.type)) {
				ItemModification itemMod = ItemMods[item.type];

				itemMod.ModifyItem(item);

				// Set localizations
				/*if (itemMod.name != null)
					SetItemName(item.type, itemMod.name);
				if (itemMod.tooltip != null)
					SetItemTooltip(item.type, itemMod.tooltip);*/
			}
		}
		/**<summary>Called on Main.LoadPlayers to reload all of the modifications.</summary>*/
		public static void OnSetupModifications() {
			if (File.Exists(ConfigPath))
				LoadModifications();
			else
				SaveModifications();
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
					if (attribute != null && int.TryParse(attribute.InnerText, out type)) {
						if (ItemMods.ContainsKey(type))
							continue;
						ItemModification item = new ItemModification();
						item.LoadXml(node);

						// Set localizations
						if (item.name != null)
							SetItemName(type, item.name);
						if (item.tooltip != null)
							SetItemTooltip(type, item.tooltip);

						ItemMods.Add(type, item);
					}
				}
			}
			catch (Exception ex) {
				StreamWriter writer = new StreamWriter(ErrorLogPath, true);
				writer.WriteLine("--------------------------------");
				writer.WriteLine("Time: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
				writer.WriteLine("Message: " + ex.Message);
				writer.WriteLine("ToString: " + ex.ToString());
				writer.WriteLine();

				writer.Close();
			}
		}
		/**<summary>Saves an example modifications xml doc.</summary>*/
		private static void SaveModifications() {
			try {
				XmlDocument doc = new XmlDocument();
				doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

				doc.AppendChild(doc.CreateComment("Variable List"));
				doc.AppendChild(doc.CreateComment(" https://github.com/trigger-death/TerrariaItemModifier/wiki/Variable-List "));

				XmlElement mods = doc.CreateElement("ItemModifications");
				doc.AppendChild(mods);

				mods.AppendChild(doc.CreateComment("Example Modification: Magical Harp"));
				mods.AppendChild(doc.CreateComment("<Item ID=\"494\">\n" +
					"    <AutoReuse>False</AutoReuse>\n" +
					"    <ManaUse>0</ManaUse>\n" +
					"    <UseAnimation>3</UseAnimation>\n" +
					"    <UseTime>3</UseTime>\n" +
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
			// Access Lang's private tooltip cache
			FieldInfo field = typeof(Lang).GetField("_itemNameCache", BindingFlags.NonPublic | BindingFlags.Static);
			LocalizedText[] _itemNameCache = (LocalizedText[])field.GetValue(null);

			// Get the name of the item
			LocalizedText locText = _itemNameCache[ItemID.FromNetId((short)type)];

			// Access Value's private setter and set the name
			PropertyInfo property = typeof(LocalizedText).GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
			property.SetValue(locText, name);

			// Hooray for reflection!
		}
		/**<summary>Sets the name of an item.</summary>*/
		private static void SetItemTooltip(int type, string text) {
			// Access Lang's private tooltip cache
			FieldInfo field = typeof(Lang).GetField("_itemTooltipCache", BindingFlags.NonPublic | BindingFlags.Static);
			ItemTooltip[] _itemTooltipCache = (ItemTooltip[])field.GetValue(null);

			// Get the tooltip of the item
			//ItemTooltip itemTooltip = _itemTooltipCache[ItemID.FromNetId((short)type)];


			ConstructorInfo ctor = typeof(ItemTooltip).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
			ItemTooltip itemTooltip = (ItemTooltip)ctor.Invoke(new object[] {});
			ctor = typeof(LocalizedText).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
			LocalizedText locText = (LocalizedText)ctor.Invoke(new object[] { "ItemTooltip" + type.ToString(), text });
			field = typeof(ItemTooltip).GetField("_text", BindingFlags.NonPublic | BindingFlags.Instance);
			field.SetValue(itemTooltip, locText);

			// Set the text of the tooltip
			/*field = typeof(ItemTooltip).GetField("_text", BindingFlags.NonPublic | BindingFlags.Instance);
			LocalizedText locText = (LocalizedText)field.GetValue(itemTooltip);
			if (locText == null) {
				ctor = typeof(LocalizedText).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
				locText = (LocalizedText)ctor.Invoke(new object[] { "ItemTooltip" + type.ToString(), text });
				field.SetValue(itemTooltip, locText);
				
				locText = (LocalizedText)FormatterServices.GetUninitializedObject(typeof(LocalizedText));
				field.SetValue(itemTooltip, locText);
				field = typeof(LocalizedText).GetField("Key", BindingFlags.Public | BindingFlags.Instance);
				field.SetValue(locText, "");
				PropertyInfo property = typeof(LocalizedText).GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
				property.SetValue(locText, text);
			}
			else {
				PropertyInfo property = typeof(LocalizedText).GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
				property.SetValue(locText, text);
			}*/

			// Set it so that the tooltip will be revalidated the first time it's needed.
			field = typeof(ItemTooltip).GetField("_lastCulture", BindingFlags.NonPublic | BindingFlags.Instance);
			field.SetValue(itemTooltip, null);

			_itemTooltipCache[ItemID.FromNetId((short)type)] = itemTooltip;

			// Hooray for reflection!
		}

		#endregion
	}
}
