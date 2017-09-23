using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaItemModifier {
	public static class ErrorLogger {
		//========== CONSTANTS ===========
		#region Constants

		/**<summary>The name of the error log file.</summary>*/
		public static readonly string LogName = "ItemModificationErrorLog.txt";
		/**<summary>The path of the error log file.</summary>*/
		public static readonly string LogPath = Path.Combine(Environment.CurrentDirectory, LogName);

		#endregion
		//=========== MEMBERS ============
		#region Members
			
		/**<summary>The writer for the error log file.</summary>*/
		private static StreamWriter writer = null;

		#endregion
		//=========== WRITING ============
		#region Writing

		/**<summary>Returns true if the error log file is open.</summary>*/
		public static bool IsOpen {
			get { return writer != null; }
		}

		/**<summary>Opens the error log file.</summary>*/
		public static void Open() {
			if (writer != null) {
				try {
					writer = new StreamWriter(LogPath, true);
				}
				catch { }
			}
		}
		/**<summary>Closes the error log file.</summary>*/
		public static void Close() {
			try {
				if (writer != null) {
					writer.Close();
					writer = null;
				}
			}
			catch { }

		}
		/**<summary>Starts a new line.</summary>*/
		public static void WriteLine() {
			if (writer != null)
				writer.WriteLine();
		}
		/**<summary>Writes the text then starts a new line.</summary>*/
		public static void WriteLine(string text) {
			if (writer != null)
				writer.WriteLine(text);
		}
		/**<summary>Writes the text.</summary>*/
		public static void Write(string text) {
			if (writer != null)
				writer.Write(text);
		}
		
		/**<summary>Writes the standard error log header.</summary>*/
		public static void WriteErrorHeader() {
			if (writer != null) {
				writer.WriteLine("------------------------------------------------");
				writer.WriteLine("Time: " + DateTime.Now.ToString());
			}
		}
		/**<summary>Writes an exception.</summary>*/
		public static void WriteException(Exception ex) {
			bool wasOpen = IsOpen;
			Open();
			WriteErrorHeader();
			WriteLine(ex.Message);
			WriteLine();
			WriteLine(ex.ToString());
			if (!wasOpen)
				Close();
		}

		#endregion
	}
}
