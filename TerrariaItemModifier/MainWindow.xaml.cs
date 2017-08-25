using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TerrariaItemModifier.Patching;
using TerrariaItemModifier.Windows;
using IOPath = System.IO.Path;
using IOFile = System.IO.File;
using IODirectory = System.IO.Directory;
using TerrariaItemModifier.Properties;
using Microsoft.Win32;
using System.Xml;
using System.Diagnostics;

namespace TerrariaItemModifier {
	/**<summary>The main window running Terraria Item Modifier.</summary>*/
	public partial class MainWindow : Window {
		//========== CONSTANTS ===========
		#region Constants

		/**<summary>The possibly paths to the Terraria executable.</summary>*/
		private static readonly string[] PossibleTerrariaPaths = {
			@"C:\Program Files (x86)\Steam\steamapps\common\Terraria\Terraria.exe",
			@"C:\Program Files\Steam\steamapps\common\Terraria\Terraria.exe",
			@"C:\Steam\steamapps\common\Terraria\Terraria.exe"
		};

		#endregion
		//=========== MEMBERS ============
		#region Members

		#endregion
		//========= CONSTRUCTORS =========
		#region Constructors

		/**<summary>Constructs the main window.</summary>*/
		public MainWindow() {
			InitializeComponent();

			LoadSettings();
		}

		#endregion
		//=========== SETTINGS ===========
		#region Settings

		/**<summary>Loads the application settings.</summary>*/
		private void LoadSettings() {
			if (Settings.Default.ExePath == null || Settings.Default.ExePath == "") {
				if (TryFindTerrariaPath())
					SaveSettings();
			}
			else {
				Patcher.ExePath = Settings.Default.ExePath;
				textBoxExe.Text = Settings.Default.ExePath;
			}
		}
		/**<summary>Saves the application settings.</summary>*/
		private void SaveSettings() {
			Settings.Default.ExePath = Patcher.ExePath;
			Settings.Default.Save();
		}

		#endregion
		//=========== HELPERS ============
		#region Helpers

		/**<summary>Tries to find one of the default paths to Terraria.</summary>*/
		private bool TryFindTerrariaPath() {
			foreach (string path in PossibleTerrariaPaths) {
				if (IOFile.Exists(path)) {
					Patcher.ExePath = path;
					textBoxExe.Text = path;
					return true;
				}
			}
			return false;
		}
		/**<summary>Saves the xml to be modified for use in Terraria.</summary>*/
		private void SaveItemModificationsXml() {
			try {
				string configPath = IOPath.Combine(Patcher.ExeDirectory, ItemModifier.ConfigName);
				// Only create one, don't overwrite a user's existing modifications
				if (!IOFile.Exists(configPath)) {
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
					doc.Save(configPath);
				}
			}
			catch { }
		}
		/**<summary>Checks if the path is valid.</summary>*/
		private bool ValidPathTest() {
			if (Patcher.ExePath == "") {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "The Terraria path cannot be empty!", "Invalid Path");
				return false;
			}
			try {
				IOPath.GetDirectoryName(Patcher.ExePath);
				return true;
			}
			catch (ArgumentException) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "You must enter a valid Terraria path!", "Invalid Path");
				return false;
			}
		}

		#endregion
		//============ EVENTS ============
		#region Events
		//--------------------------------
		#region Regular

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			SaveSettings();
		}
		private void OnPatch(object sender = null, RoutedEventArgs e = null) {
			MessageBoxResult result;
			if (!ValidPathTest())
				return;
			if (!IOFile.Exists(Patcher.ExePath)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find Terraria executable!", "Missing Exe");
				return;
			}
			result = TriggerMessageBox.Show(this, MessageIcon.Question, "Are you sure you want to patch the current Terraria executable?", "Patch Terraria", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.No)
				return;
			if (!IOFile.Exists(Patcher.BackupPath)) {
				result = TriggerMessageBox.Show(this, MessageIcon.Question, "Would you like to create a backup of the current Terraria executable?", "Backup Terraria", MessageBoxButton.YesNoCancel);

				if (result == MessageBoxResult.Yes) {
					try {
						IOFile.Copy(Patcher.ExePath, Patcher.BackupPath);
					}
					catch (Exception ex) {
						result = TriggerMessageBox.Show(this, MessageIcon.Error, "An error occurred while trying to backup Terraria! Would you like to see the error?", "Backup Error", MessageBoxButton.YesNo);
						if (result == MessageBoxResult.Yes)
							ErrorMessageBox.Show(ex, true);
						return;
					}
				}
				else if (result == MessageBoxResult.Cancel) {
					return;
				}
			}
			try {
				Patcher.Patch();
				TriggerMessageBox.Show(this, MessageIcon.Info, "Terraria successfully patched!", "Terraria Patched");
			}
			catch (Exception ex) {
				result = TriggerMessageBox.Show(this, MessageIcon.Error, "An error occurred while patching Terraria! Would you like to see the error?", "Patch Error", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex, true);
				return;
			}

			SaveItemModificationsXml();
		}
		private void OnRestore(object sender = null, RoutedEventArgs e = null) {
			MessageBoxResult result;
			if (!ValidPathTest())
				return;
			if (IL.GetAssemblyVersion(Patcher.BackupPath) < IL.GetAssemblyVersion(Patcher.ExePath)) {
				result = TriggerMessageBox.Show(this, MessageIcon.Warning, "The backed up Terraria executable is an older game version. Are you sure you want to restore it?", "Older Version", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.No)
					return;
			}
			result = TriggerMessageBox.Show(this, MessageIcon.Question, "Are you sure you want to restore the current Terraria executable to its backup?", "Restore Terraria", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.No)
				return;
			if (!IOFile.Exists(Patcher.BackupPath)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find Terraria backup!", "Missing Backup");
				return;
			}
			try {
				Patcher.Restore();
				TriggerMessageBox.Show(this, MessageIcon.Info, "Terraria successfully restored!", "Terraria Restored");
			}
			catch (Exception ex) {
				result = TriggerMessageBox.Show(this, MessageIcon.Error, "An error occurred while restoring Terraria! Would you like to see the error?", "Restore Error", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex, true);
			}
		}
		private void OnRestoreAndPatch(object sender, RoutedEventArgs e) {
			MessageBoxResult result;
			if (!ValidPathTest())
				return;
			if (IL.GetAssemblyVersion(Patcher.BackupPath) < IL.GetAssemblyVersion(Patcher.ExePath)) {
				result = TriggerMessageBox.Show(this, MessageIcon.Warning, "The backed up Terraria executable is an older game version. Are you sure you want to restore it?", "Older Version", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.No)
					return;
			}
			result = TriggerMessageBox.Show(this, MessageIcon.Question, "Are you sure you want to restore Terraria from its backup and then patch it?", "Patch & Restore Terraria", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.No)
				return;
			if (!IOFile.Exists(Patcher.BackupPath)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find Terraria backup!", "Missing Backup");
				return;
			}
			else if (!IOFile.Exists(Patcher.ExePath)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find Terraria executable!", "Missing Exe");
				return;
			}
			try {
				Patcher.Restore();
			}
			catch (Exception ex) {
				result = TriggerMessageBox.Show(this, MessageIcon.Error, "An error occurred while restoring Terraria! Would you like to see the error?", "Restore Error", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex, true);
				return;
			}
			try {
				Patcher.Patch();
				TriggerMessageBox.Show(this, MessageIcon.Info, "Terraria successfully restored and patched!", "Terraria Repatched");
			}
			catch (Exception ex) {
				result = TriggerMessageBox.Show(this, MessageIcon.Error, "An error occurred while patching Terraria! Would you like to see the error?", "Patch Error", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex, true);
				return;
			}

			SaveItemModificationsXml();
		}
		private void OnExeBrowse(object sender, RoutedEventArgs e) {
			OpenFileDialog fileDialog = new OpenFileDialog();

			fileDialog.Title = "Find Terraria Executable";
			fileDialog.AddExtension = true;
			fileDialog.DefaultExt = ".exe";
			fileDialog.Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*";
			fileDialog.FilterIndex = 0;
			fileDialog.CheckFileExists = true;
			if (Patcher.ExePath != "")
				fileDialog.InitialDirectory = Patcher.ExeDirectory;

			var result = fileDialog.ShowDialog(this);
			if (result.HasValue && result.Value) {
				Patcher.ExePath = fileDialog.FileName;
				textBoxExe.Text = fileDialog.FileName;
				SaveSettings();
			}
		}

		#endregion
		//--------------------------------
		#region Menu Items

		private void OnLaunchTerraria(object sender, RoutedEventArgs e) {
			try {
				if (IOFile.Exists(Patcher.ExePath))
					Process.Start(Patcher.ExePath);
				else
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not locate the Terraria executable! Cannot launch Terraria.", "Missing Executable");
			}
			catch {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "The current path to Terraria is invalid! Cannot launch Terraria.", "Invalid Path");
			}
		}
		private void OnOpenTerrariaFolder(object sender, RoutedEventArgs e) {
			try {
				if (IODirectory.Exists(Patcher.ExeDirectory))
					Process.Start(Patcher.ExeDirectory);
				else
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not locate the Terraria folder! Cannot open folder.", "Missing Folder");
			}
			catch {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "The current path to Terraria is invalid! Cannot open folder.", "Invalid Path");
			}
		}
		private void OnExit(object sender, RoutedEventArgs e) {
			Close();
		}

		private void OnAbout(object sender, RoutedEventArgs e) {
			AboutWindow.Show(this);
		}
		private void OnHelp(object sender, RoutedEventArgs e) {
			Process.Start("https://github.com/trigger-death/TerrariaItemModifier/wiki");
		}
		private void OnCredits(object sender, RoutedEventArgs e) {
			CreditsWindow.Show(this);
		}
		private void OnViewOnGitHub(object sender, RoutedEventArgs e) {
			Process.Start("https://github.com/trigger-death/TerrariaItemModifier");
		}
		private void OnVariableList(object sender, RoutedEventArgs e) {
			Process.Start("https://github.com/trigger-death/TerrariaItemModifier/wiki/Variable-List");
		}

		#endregion
		//--------------------------------
		#endregion
	}
}
