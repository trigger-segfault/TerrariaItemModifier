using Microsoft.Xna.Framework;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace TerrariaItemModifier {
	//============ ENUMS =============
	#region Enums

	/**<summary>The different types of variables.</summary>*/
	public enum VarTypes {
		Bool,
		Byte,
		Short,
		Int,
		Float,
		String,
		Color,
		UseSound
	}
	/**<summary>The different types item member setters for variables.</summary>*/
	public enum VarSetters {
		None,
		Normal
	}

	#endregion
	//========== VARIABLES ===========
	#region Variables

	/**<summary>A info for a variable.</summary>*/
	public class VarInfo {
		/**<summary>The type of the variable.</summary>*/
		public readonly VarTypes Type;
		/**<summary>The internal field of the item variable.</summary>*/
		public readonly FieldInfo Field;
		/**<summary>The way to handle setting the item's member.</summary>*/
		public readonly VarSetters Setter;
		/**<summary>Constructs the base variable.</summary>*/
		public VarInfo(VarTypes type, string internalName, VarSetters setter = VarSetters.Normal) {
			this.Type = type;
			if (internalName != "")
				this.Field = typeof(Item).GetField(internalName);
			else
				this.Field = null;
			this.Setter = setter;
		}
	}

	/**<summary>A base variable type.</summary>*/
	public class Variable {
		/**<summary>Information about the variable.</summary>*/
		public readonly VarInfo Info;
		/**<summary>The variable value.</summary>*/
		public object Value;
		/**<summary>The type of the variable.</summary>*/
		public VarTypes Type {
			get { return Info.Type; }
		}
		/**<summary>The way to handle setting the item's member.</summary>*/
		public VarSetters Setter {
			get { return Info.Setter; }
		}
		/**<summary>Constructs the base variable.</summary>*/
		public Variable(VarInfo info, object value) {
			this.Info = info;
			this.Value = value;
		}
		/**<summary>Sets the value of the item.</summary>*/
		public void SetValue(Item item) {
			Info.Field.SetValue(item, Value);
		}
	}

	#endregion
}
