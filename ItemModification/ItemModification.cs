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
			List<string> unrecognized = new List<string>();
			List<string> parsingErrors = new List<string>();
			List<string> alreadyDefined = new List<string>();
			bool looseText = false;

			// Read all variables
			XmlNodeList nodeList = node.ChildNodes;
			foreach (XmlNode varNode in nodeList) {
				string varName = varNode.Name;
				if (varName == "#comment")
					continue;
				else if (varName == "#text")
					looseText = true;
				try {
					if (VarInfoList.ContainsKey(varName)) {
						VarInfo varInfo = VarInfoList[varName];
						object value = ParseVariable(varInfo.Type, varNode.InnerText);
						if (value is Exception)
							parsingErrors.Add(varInfo.Type.ToString() + " " + varName + ": " + ((Exception)value).Message);
						else
							VarList.Add(varName, new Variable(varInfo, value));
					}
					else {
						unrecognized.Add(varName);
					}
				}
				catch (KeyNotFoundException) {
					unrecognized.Add(varName);
				}
				catch (ArgumentException) {
					alreadyDefined.Add(varName);
				}
			}

			// Log errors
			if (unrecognized.Count > 0 || parsingErrors.Count > 0 || alreadyDefined.Count > 0) {
				if (!ErrorLogger.IsOpen)
					ErrorLogger.Open();
				ErrorLogger.WriteErrorHeader();
				ErrorLogger.WriteLine("There were errors when reading ItemID: " + ID.ToString() + ".");
				if (looseText) {
					ErrorLogger.WriteLine("Loose text is present inside the element.");
				}
				if (unrecognized.Count > 0) {
					ErrorLogger.WriteLine();
					ErrorLogger.WriteLine("Unrecognized Variables:");
					foreach (string s in unrecognized) {
						ErrorLogger.WriteLine("- " + s);
					}
				}
				if (parsingErrors.Count > 0) {
					ErrorLogger.WriteLine();
					ErrorLogger.WriteLine("Parsing Errors:");
					foreach (string s in parsingErrors) {
						ErrorLogger.WriteLine("- " + s);
					}
				}
				if (alreadyDefined.Count > 0) {
					ErrorLogger.WriteLine();
					ErrorLogger.WriteLine("Already Defined:");
					foreach (string s in alreadyDefined) {
						ErrorLogger.WriteLine("- " + s);
					}
				}
				ErrorLogger.WriteLine();
			}
		}

		#endregion
		//========= MODIFICATION =========
		#region Modification

		/**<summary>Modifies the item's members.</summary>*/
		public void ModifyItem(Item item) {
			bool loggedError = false;

			// Set item members for each variable present in the collection
			foreach (var varPair in VarList) {
				try {
					switch (varPair.Value.Setter) {
					case VarSetters.Normal:
						varPair.Value.SetValue(item);
						break;
					}
				}
				catch (Exception ex) {
					if (!loggedError) {
						ErrorLogger.Open();
						ErrorLogger.WriteErrorHeader();
						ErrorLogger.WriteLine("There were errors when modifying ItemID: " + ID.ToString() + ".");
						ErrorLogger.WriteLine();
						ErrorLogger.WriteLine("Variables:");
						loggedError = true;
					}
					ErrorLogger.WriteLine("*" + varPair.Key + ": " + ex.Message);
				}
			}
			
			// A blank line to finish off this error
			if (loggedError)
				ErrorLogger.WriteLine();
		}

		#endregion
		//=========== HELPERS ============
		#region Helpers

		/**<summary>Parses a variable based on its type.</summary>*/
		private object ParseVariable(VarTypes type, string text) {
			try {
				switch (type) {
				case VarTypes.Bool:		return bool.Parse(text);
				case VarTypes.Byte:		return byte.Parse(text);
				case VarTypes.Short:	return short.Parse(text);
				case VarTypes.Int:		return int.Parse(text);
				case VarTypes.Float:	return float.Parse(text);
				case VarTypes.String:	return text;
				case VarTypes.Color:	return ParseColor(text);
				case VarTypes.UseSound:	return ParseUseSound(text);
				}
				return null;
			}
			catch (Exception ex) {
				return ex;
			}
		}
		/**<summary>Parses a Color.</summary>*/
		private Color ParseColor(string s) {
			string[] channels = s.Split(',');
			if (channels.Length != 3)
				throw new FormatException("Incorrect amount of commas in color.");

			byte r, g, b;
			if (!byte.TryParse(channels[0], out r)) throw new ArgumentOutOfRangeException("R channel not between 0 and 255.");
			if (!byte.TryParse(channels[1], out g)) throw new ArgumentOutOfRangeException("G channel not between 0 and 255.");
			if (!byte.TryParse(channels[2], out b)) throw new ArgumentOutOfRangeException("B channel not between 0 and 255.");
			return new Color(r, g, b, 0);
		}
		/**<summary>Parses a UseSound.</summary>*/
		private LegacySoundStyle ParseUseSound(string s) {
			try {
				// Use reflection to get the correct LegacySoundStyle
				FieldInfo field = typeof(SoundID).GetField(s, BindingFlags.Public | BindingFlags.Static);
				return (LegacySoundStyle)field.GetValue(null);
			}
			catch {
				throw new ArgumentException("No LegacySoundStyle with that name exists in SoundID.");
			}
		}

		#endregion
	}
}
