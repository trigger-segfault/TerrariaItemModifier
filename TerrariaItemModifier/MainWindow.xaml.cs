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
using System.IO;
using Path = System.IO.Path;
using TerrariaItemModifier.Properties;
using Microsoft.Win32;
using System.Xml;
using System.Diagnostics;
using TerrariaItemModifier.Util;

namespace TerrariaItemModifier {
	/**<summary>The main window running Terraria Item Modifier.</summary>*/
	public partial class MainWindow : Window {
		//========== CONSTANTS ===========
		#region Constants

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

			// Disable drag/drop text in textboxes so you can scroll their contents easily
			DataObject.AddCopyingHandler(textBoxExe, (sender, e) => { if (e.IsDragDrop) e.CancelCommand(); });
		}

		#endregion
		//=========== SETTINGS ===========
		#region Settings

		/**<summary>Loads the application settings.</summary>*/
		private void LoadSettings() {
			Patcher.ExePath = Settings.Default.ExePath;
			if (string.IsNullOrEmpty(Patcher.ExePath)) {
				Patcher.ExePath = "";
				if (!string.IsNullOrEmpty(TerrariaLocator.TerrariaPath)) {
					Patcher.ExePath = TerrariaLocator.TerrariaPath;
				}
			}
			textBoxExe.Text = Patcher.ExePath;
		}
		/**<summary>Saves the application settings.</summary>*/
		private void SaveSettings() {
			Settings.Default.ExePath = Patcher.ExePath;
			Settings.Default.Save();
		}

		#endregion
		//=========== HELPERS ============
		#region Helpers
		
		/**<summary>Saves the xml to be modified for use in Terraria.</summary>*/
		private void SaveItemModificationsXml() {
			try {
				string configPath = Path.Combine(Patcher.ExeDirectory, ItemModifier.ConfigName);
				if (File.Exists(configPath))
					return;

				// Only create one, don't overwrite a user's existing modifications
				if (!File.Exists(configPath)) {
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
				Path.GetDirectoryName(Patcher.ExePath);
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
			if (!File.Exists(Patcher.ExePath)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find Terraria executable!", "Missing Exe");
				return;
			}
			result = TriggerMessageBox.Show(this, MessageIcon.Question, "Are you sure you want to patch the current Terraria executable?", "Patch Terraria", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.No)
				return;
			try {
				Patcher.Patch();
				SaveItemModificationsXml();
				TriggerMessageBox.Show(this, MessageIcon.Info, "Terraria successfully patched!", "Terraria Patched");
			}
			catch (AlreadyPatchedException) {
				TriggerMessageBox.Show(this, MessageIcon.Error, "This executable has already been patched by Item Modifier! Use Restore & Patch instead.", "Already Patched");
			}
			catch (Exception ex) {
				result = TriggerMessageBox.Show(this, MessageIcon.Error, "An error occurred while patching Terraria! Would you like to see the error?", "Patch Error", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex, true);
				return;
			}
		}
		private void OnRestore(object sender = null, RoutedEventArgs e = null) {
			MessageBoxResult result;
			bool cleanup = false;
			if (!ValidPathTest())
				return;
			if (!File.Exists(Patcher.BackupPath)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find Terraria backup!", "Missing Backup");
				return;
			}
			result = TriggerMessageBox.Show(this, MessageIcon.Question, "Would you like to restore the current Terraria executable to its backup and cleanup the required files or just restore the backup?", "Restore Terraria", MessageBoxButton.YesNoCancel, "Cleanup & Restore", "Restore Only");
			if (result == MessageBoxResult.Cancel)
				return;
			cleanup = result == MessageBoxResult.Yes;
			if (File.Exists(Patcher.ExePath) && IL.GetAssemblyVersion(Patcher.BackupPath) < IL.GetAssemblyVersion(Patcher.ExePath)) {
				result = TriggerMessageBox.Show(this, MessageIcon.Warning, "The backed up Terraria executable is an older game version. Are you sure you want to restore it?", "Older Version", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.No)
					return;
			}
			try {
				Patcher.Restore(cleanup);
				// Clean up directory and remove config file
				if (cleanup) {
					string configPath = Path.Combine(Patcher.ExeDirectory, ItemModifier.ConfigName);
					string logPath = Path.Combine(Patcher.ExeDirectory, ErrorLogger.LogName);
					if (File.Exists(configPath))
						File.Delete(configPath);
					if (File.Exists(logPath))
						File.Delete(logPath);
				}
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
			if (!File.Exists(Patcher.BackupPath)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find Terraria backup!", "Missing Backup");
				return;
			}
			result = TriggerMessageBox.Show(this, MessageIcon.Question, "Are you sure you want to restore Terraria from its backup and then patch it?", "Patch & Restore Terraria", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.No)
				return;
			if (File.Exists(Patcher.ExePath) && IL.GetAssemblyVersion(Patcher.BackupPath) < IL.GetAssemblyVersion(Patcher.ExePath)) {
				result = TriggerMessageBox.Show(this, MessageIcon.Warning, "The backed up Terraria executable is an older game version. Are you sure you want to restore it?", "Older Version", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.No)
					return;
			}
			try {
				Patcher.Restore(false);
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
			catch (AlreadyPatchedException) {
				TriggerMessageBox.Show(this, MessageIcon.Error, "The backup executable has already been patched by Item Modifier!", "Already Patched");
			}
			catch (Exception ex) {
				result = TriggerMessageBox.Show(this, MessageIcon.Error, "An error occurred while patching Terraria! Would you like to see the error?", "Patch Error", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex, true);
				return;
			}

			SaveItemModificationsXml();
		}

		#endregion
		//--------------------------------
		#region Settings

		private void OnExeBrowse(object sender, RoutedEventArgs e) {
			OpenFileDialog fileDialog = new OpenFileDialog();

			fileDialog.Title = "Find Terraria Executable";
			fileDialog.AddExtension = true;
			fileDialog.DefaultExt = ".exe";
			fileDialog.Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*";
			fileDialog.FilterIndex = 0;
			fileDialog.CheckFileExists = true;
			try {
				fileDialog.InitialDirectory = Path.GetFullPath(Patcher.ExeDirectory);
			}
			catch { }
			var result = fileDialog.ShowDialog(this);
			if (result.HasValue && result.Value) {
				Patcher.ExePath = fileDialog.FileName;
				textBoxExe.Text = fileDialog.FileName;
				SaveSettings();
			}
		}
		private void OnExeChanged(object sender, TextChangedEventArgs e) {
			Patcher.ExePath = textBoxExe.Text;
		}

		#endregion
		//--------------------------------
		#region Menu Items

		private void OnLaunchTerraria(object sender, RoutedEventArgs e) {
			try {
				if (File.Exists(Patcher.ExePath))
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
				if (Directory.Exists(Patcher.ExeDirectory))
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
