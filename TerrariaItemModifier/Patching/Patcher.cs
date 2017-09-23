using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TerrariaItemModifier.Util;
using System.Threading;

namespace TerrariaItemModifier.Patching {
	/**<summary>An exception thrown when the patcher is unable to locate the instructions to change.</summary>*/
	public class PatcherException : Exception {
		public PatcherException(string message) : base(message) { }
	}
	/**<summary>An exception thrown when the executable has already been patched.</summary>*/
	public class AlreadyPatchedException : Exception {
		public AlreadyPatchedException() : base("This executable has already been patched!") { }
	}

	/**<summary>Tha class for handling modification to the Terraria executable.</summary>*/
	public static class Patcher {
		//========== CONSTANTS ===========
		#region Constants

		/**<summary>The lst of required dlls and files to transfer.</summary>*/
		public static readonly string[] RequireFiles = {
			"ItemModification.dll"
		};
		/**<summary>The name of the static field used to signal the exe has been patched.</summary>*/
		public const string AlreadyPatchedStaticField = "TriggersItemModifier";

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
		//=========== MEMBERS ============
		#region Members

		/**<summary>The Terraria assembly.</summary>*/
		private static AssemblyDefinition AsmDefinition;
		/**<summary>The Terraria module.</summary>*/
		private static ModuleDefinition ModDefinition;

		/**<summary>The Terraria.Main type.</summary>*/
		private static TypeDefinition Main;
		/**<summary>The Terraria.Item type.</summary>*/
		private static TypeDefinition Item;

		/**<summary>The ItemModifier type.</summary>*/
		private static TypeDefinition ItemModifier;

		#endregion
		//=========== PATCHING ===========
		#region Patching
		//--------------------------------
		#region Patch & Restore

		/**<summary>Restores the Terraria backup.</summary>*/
		public static void Restore(bool removeFiles) {
			File.Copy(BackupPath, ExePath, true);

			if (removeFiles) {
				RemoveRequiredFiles();
			}
		}
		/**<summary>Patches the Terraria executable.</summary>*/
		public static void Patch() {
			// Backup the file first
			if (!File.Exists(BackupPath)) {
				File.Copy(ExePath, BackupPath, false);
			}

			// Do this first so we don't bork the executable if copying fails
			CopyRequiredFiles();

			// Load the assembly
			var resolver = new EmbeddedAssemblyResolver();
			var parameters = new ReaderParameters{ AssemblyResolver = resolver };
			AsmDefinition = AssemblyDefinition.ReadAssembly(ExePath, parameters);
			ModDefinition = AsmDefinition.MainModule;

			// Get links to Terraria types that will have their functions modified
			Main = IL.GetTypeDefinition(ModDefinition, "Main");
			Item = IL.GetTypeDefinition(ModDefinition, "Item");

			// Get link and import ItemModifier type
			ItemModifier = ModDefinition.Import(typeof(ItemModifier)).Resolve();

			// Check if we've already been patched
			if (IL.GetFieldDefinition(Main, AlreadyPatchedStaticField, false) != null)
				throw new AlreadyPatchedException();

			// Add a static field to let us know this exe has already been patched
			var objectType = IL.GetTypeReference(ModDefinition, "System.Object");
			IL.AddStaticField(Main, AlreadyPatchedStaticField, objectType);

			// Patch Terraria
			Patch_Main_LoadPlayers();
			Patch_Item_SetDefaults();

			// Save the modifications
			AsmDefinition.Write(ExePath);
			// Wait for the exe to be closed by AsmDefinition.Write()
			Thread.Sleep(400);
			IL.MakeLargeAddressAware(ExePath);
		}

		#endregion
		//--------------------------------
		#region Required Files

		/**<summary>Copies the required dlls and files to the Terraria folder.</summary>*/
		private static void CopyRequiredFiles() {
			try {
				foreach (string file in RequireFiles) {
					//string source = Path.Combine(AppDirectory, file);
					string destination = Path.Combine(ExeDirectory, file);
					//File.Copy(source, destination, true);
					EmbeddedResources.Extract(destination, file);
				}
			}
			catch (Exception ex) {
				throw new IOException("Error while trying to copy over required files.", ex);
			}
		}
		/**<summary>Removes all required dlls and files from the Terraria folder.</summary>*/
		private static void RemoveRequiredFiles() {
			try {
				foreach (string file in RequireFiles) {
					string path = Path.Combine(ExeDirectory, file);
					if (File.Exists(path))
						File.Delete(path);
				}
			}
			catch {
				// Oh well, no harm done if we don't remove these
			}
		}

		#endregion
		//--------------------------------
		#region Exception Throwing

		/**<summary>Performs a check to see if the starting point was found. Throws an exception otherwise.</summary>*/
		private static void CheckFailedToFindStart(int start, int index, string function) {
			if (start == -1)
				throw new PatcherException("Failed to find starting point '" + (index + 1) + "' for " + function);
		}
		/**<summary>Performs a check to see if the ending point was found. Throws an exception otherwise.</summary>*/
		private static void CheckFailedToFindEnd(int end, int index, string function) {
			if (end == -1)
				throw new PatcherException("Failed to find ending point '" + (index + 1) + "' for " + function);
		}
		/**<summary>Performs a check to see if the local variable was found. Throws an exception otherwise.</summary>*/
		private static void CheckFailedToFindVariable(VariableDefinition varDef, string varName, string function) {
			if (varDef == null)
				throw new PatcherException("Failed to find local variable '" + varName + "' for " + function);
		}

		#endregion
		//--------------------------------
		#region Patchers

		/**<summary>Patches loading of modifications xml file. Also resets Recipe created item defaults.</summary>*/
		private static void Patch_Main_LoadPlayers() {
			var loadPlayers = IL.GetMethodDefinition(Main, "LoadPlayers", 0);

			var onLoadModifications = ModDefinition.Import(IL.GetMethodDefinition(ItemModifier, "OnLoadModifications"));

			// Essentially make it so modifications are reloaded every time a player re-enters the world
			IL.MethodPrepend(loadPlayers,
				Instruction.Create(OpCodes.Call, onLoadModifications)
			);
		}
		/**<summary>Loads the modifications to the items.</summary>*/
		private static void Patch_Item_SetDefaults() {
			var setDefaults = IL.GetMethodDefinition(Item, "SetDefaults", 2);

			var onSetItemDefaults = ModDefinition.Import(IL.GetMethodDefinition(ItemModifier, "OnSetItemDefaults"));

			// Modify item defaults after set by the game
			IL.MethodReplaceEnd(setDefaults, IL.InstructionCount(setDefaults) - 1,
				Instruction.Create(OpCodes.Ldarg_0),
				Instruction.Create(OpCodes.Call, onSetItemDefaults),
				// We need to replace ret since there are gotos leading towards it
				Instruction.Create(OpCodes.Ret)
			);
		}

		#endregion
		//--------------------------------
		#endregion
	}
}
