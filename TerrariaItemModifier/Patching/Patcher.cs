using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaItemModifier.Patching {
	/**<summary>Tha class for handling modification to the Terraria executable.</summary>*/
	public static class Patcher {
		//========== CONSTANTS ===========
		#region Constants

		/**<summary>The lst of required dlls and files to transfer.</summary>*/
		public static readonly string[] RequireFiles = {
			"ItemModification.dll"
		};

		#endregion
		//========== PROPERTIES ==========
		#region Properties

		/**<summary>The path to terraria's executable.</summary>*/
		public static string ExePath { get; set; } = "";
		/**<summary>Gets the path to terraria's backup.</summary>*/
		public static string BackupPath {
			get { return Path.Combine(ExeDirectory, Path.GetFileName(ExePath) + ".bak"); }
		}
		/**<summary>Gets the directory of the Terraria executable.</summary>*/
		public static string ExeDirectory {
			get { return Path.GetDirectoryName(ExePath); }
		}
		/**<summary>Gets the directory of this application.</summary>*/
		public static string AppDirectory {
			get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
		}

		#endregion
		//=========== PATCHING ===========
		#region Patching

		/**<summary>Restores the Terraria backup.</summary>*/
		public static void Restore() {
			File.Copy(BackupPath, ExePath, true);
		}
		/**<summary>Patches the Terraria executable.</summary>*/
		public static void Patch() {
			// Backup the file first
			if (!File.Exists(BackupPath)) {
				File.Copy(ExePath, BackupPath, false);
			}

			// Load the assembly
			var AsmDefinition = AssemblyDefinition.ReadAssembly(ExePath);
			var ModDefinition = AsmDefinition.MainModule;
			
			// Get links to Terraria functions
			var main = IL.GetTypeDefinition(ModDefinition, "Main");
			var loadPlayers = IL.GetMethodDefinition(main, "LoadPlayers", 0);

			var item = IL.GetTypeDefinition(ModDefinition, "Item");
			var setDefaults = IL.GetMethodDefinition(item, "SetDefaults", 2);

			// Get links to ItemModification functions
			var itemModifier = ModDefinition.Import(typeof(ItemModifier)).Resolve();
			var onSetupModifications = ModDefinition.Import(IL.GetMethodDefinition(itemModifier, "OnSetupModifications"));
			var onSetItemDefaults = ModDefinition.Import(IL.GetMethodDefinition(itemModifier, "OnSetItemDefaults"));

			// Essentially make it so modifications are reloaded every time a player re-enters the world
			IL.MethodPrepend(loadPlayers, new[] {
				Instruction.Create(OpCodes.Call, onSetupModifications)
			});

			// Modify item defaults after set by the game
			IL.MethodAppend(setDefaults, setDefaults.Body.Instructions.Count - 1, 1, new[] {
				Instruction.Create(OpCodes.Ldarg_0),
				Instruction.Create(OpCodes.Call, onSetItemDefaults),
				Instruction.Create(OpCodes.Ret)
			});
			/*IL.MethodAppend(setDefaults, setDefaults.Body.Instructions.Count - 1, 0, new[] {
				Instruction.Create(OpCodes.Ldarg_0),
				Instruction.Create(OpCodes.Call, onSetItemDefaults)
			});*/

			// Save the modifications
			AsmDefinition.Write(ExePath);
			IL.MakeLargeAddressAware(ExePath);

			CopyRequiredFiles();
		}
		/**<summary>Copies the required dlls and files to the Terraria folder.</summary>*/
		private static void CopyRequiredFiles() {
			try {
				foreach (string dll in RequireFiles) {
					string source = Path.Combine(AppDirectory, dll);
					string destination = Path.Combine(ExeDirectory, dll);
					File.Copy(source, destination, true);
				}
			}
			catch (Exception) {
				throw new IOException("Error while trying to copy over required files.");
			}
		}

		#endregion
	}
}
