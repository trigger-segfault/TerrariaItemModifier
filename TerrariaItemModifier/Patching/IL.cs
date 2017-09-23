using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace TerrariaItemModifier.Patching {

	/**<summary>Local variable opcodes.</summary>*/
	public enum LocOpCodes {
		/**<summary>No special local variable opcode.</summary>*/
		None,

		/**<summary>Ldarg_0-3, Ldarg_S, Ldarg.</summary>*/
		Ldarg,
		/**<summary>Ldarga_S, Ldarga.</summary>*/
		Ldarga,
		/**<summary>Starg_S, Starg.</summary>*/
		Starg,

		/**<summary>Ldloc_0-3, Ldloc_S, Ldloc.</summary>*/
		Ldloc,
		/**<summary>Ldloca_S, Ldloca.</summary>*/
		Ldloca,
		/**<summary>Stloc_0-3, Stloc_S, Stloc.</summary>*/
		Stloc,
	};

	//https://github.com/dougbenham/TerrariaPatcher/blob/master/IL.cs
	/**<summary>The main helper class for scanning and modifying assemblies.</summary>*/
	public static class IL {
		//=========== CHECKING ===========
		#region Checking
		//--------------------------------
		#region Operand Check Structure

		/**<summary>Options for checking.</summary>*/
		public enum CheckModes {
			Normal,
			Variable,
			Repeat,
			Skip
		}
		/**<summary>The delegate for checking instructions.</summary>*/
		public delegate bool InstructionCheck(int index, object operand);
		/**<summary>A structure for checking the operand of an instruction.</summary>*/
		public struct OperandCheck {
			/**<summary>The opcode to check.</summary>*/
			public OpCode OpCode;
			/**<summary>The local variable opcode to check.</summary>*/
			public LocOpCodes LocOpCode;
			/**<summary>the function to call to check.</summary>*/
			public InstructionCheck Check;
			/**<summary>The mode for checking.</summary>*/
			public CheckModes Mode;
			/**<summary>The amount to skip or repeat.</summary>*/
			public int InstructionCount;
			/**<summary>Creates an Operand Check.</summary>*/
			public OperandCheck(OpCode opCode, InstructionCheck check, CheckModes mode = CheckModes.Normal) {
				OpCode = opCode;
				LocOpCode = LocOpCodes.None;
				Check = check;
				Mode = mode;
				InstructionCount = -1;
			}
			/**<summary>Creates an Operand Check.</summary>*/
			public OperandCheck(LocOpCodes locOpCode, InstructionCheck check, CheckModes mode = CheckModes.Normal) {
				OpCode = OpCodes.Nop;
				LocOpCode = locOpCode;
				Check = check;
				Mode = mode;
				InstructionCount = -1;
			}
			/**<summary>Creates an Operand Check.</summary>*/
			public OperandCheck(CheckModes mode, int amount = -1) {
				OpCode = OpCodes.Nop;
				LocOpCode = LocOpCodes.None;
				Check = (i, op) => { return true; };
				Mode = mode;
				InstructionCount = amount;
			}
			/**<summary>Compares the opcodes.</summary>*/
			public bool OpCodeMatches(OpCode opCode) {
				if (LocOpCode != LocOpCodes.None)
					return LocOpCode.Matches(opCode);
				else
					return opCode == OpCode;
			}
		}

		#endregion
		//--------------------------------
		#region Local OpCodes

		/**<summary>Gets the referenced parameter from the instruction.</summary>*/
		private static ParameterDefinition GetParameterFromInstruction(MethodDefinition method, Instruction instruction) {
			switch (instruction.OpCode.Code) {
			case Code.Ldarg_0:
				return method.Parameters[0];
			case Code.Ldarg_1:
				return method.Parameters[1];
			case Code.Ldarg_2:
				return method.Parameters[2];
			case Code.Ldarg_3:
				return method.Parameters[3];

			default:
				return instruction.Operand as ParameterDefinition;
			}
		}
		/**<summary>Gets the referenced variable from the instruction.</summary>*/
		private static VariableDefinition GetVariableFromInstruction(MethodDefinition method, Instruction instruction) {
			switch (instruction.OpCode.Code) {
			case Code.Ldloc_0:
			case Code.Stloc_0:
				return method.Body.Variables[0];
			case Code.Ldloc_1:
			case Code.Stloc_1:
				return method.Body.Variables[1];
			case Code.Ldloc_2:
			case Code.Stloc_2:
				return method.Body.Variables[2];
			case Code.Ldloc_3:
			case Code.Stloc_3:
				return method.Body.Variables[3];

			default:
				return instruction.Operand as VariableDefinition;
			}
		}

		/**<summary>Compares the opcode to the local variable opcode.</summary>*/
		private static bool Matches(this LocOpCodes locOpCode, OpCode opCode) {
			switch (locOpCode) {
			case LocOpCodes.None:
				return true;
			case LocOpCodes.Ldarg:
				switch (opCode.Code) {
				case Code.Ldarg_0:
				case Code.Ldarg_1:
				case Code.Ldarg_2:
				case Code.Ldarg_3:
				case Code.Ldarg_S:
				case Code.Ldarg:
					return true;
				}
				break;
			case LocOpCodes.Ldarga:
				switch (opCode.Code) {
				case Code.Ldarga_S:
				case Code.Ldarga:
					return true;
				}
				break;
			case LocOpCodes.Starg:
				switch (opCode.Code) {
				case Code.Starg_S:
				case Code.Starg:
					return true;
				}
				break;
			case LocOpCodes.Ldloc:
				switch (opCode.Code) {
				case Code.Ldloc_0:
				case Code.Ldloc_1:
				case Code.Ldloc_2:
				case Code.Ldloc_3:
				case Code.Ldloc_S:
				case Code.Ldloc:
					return true;
				}
				break;
			case LocOpCodes.Ldloca:
				switch (opCode.Code) {
				case Code.Ldloca_S:
				case Code.Ldloca:
					return true;
				}
				break;
			case LocOpCodes.Stloc:
				switch (opCode.Code) {
				case Code.Stloc_0:
				case Code.Stloc_1:
				case Code.Stloc_2:
				case Code.Stloc_3:
				case Code.Stloc_S:
				case Code.Stloc:
					return true;
				}
				break;
			}
			return false;
		}

		#endregion
		//--------------------------------
		#region Check Creators
		//--------------------------------
		#region Special

		/**<summary>Checks for just an opCode.</summary>*/
		public static OperandCheck CheckNoOp(OpCode opCode) {
			return new OperandCheck(opCode, (i, op) => { return op == null; });
		}
		/**<summary>Checks for a repeating instruction after the last instruction.</summary>*/
		public static OperandCheck CheckRepeat(int instructionCount = 1) {
			return new OperandCheck(CheckModes.Repeat, instructionCount);
		}
		/**<summary>Skips the instructions.</summary>*/
		public static OperandCheck CheckSkip(int instructionCount = 1) {
			return new OperandCheck(CheckModes.Skip, instructionCount);
		}
		/**<summary>Skips the instructions.</summary>*/
		public static OperandCheck CheckSkipIndefinite() {
			return new OperandCheck(CheckModes.Skip, -1);
		}

		#endregion
		//--------------------------------
		#region Variables

		/**<summary>Checks for just an opCode.</summary>*/
		public static OperandCheck VarCheck(LocOpCodes locOpCode) {
			return new OperandCheck(locOpCode, (i, op) => { return true; }, CheckModes.Variable);
		}
		/**<summary>Checks with a function.</summary>*/
		public static OperandCheck VarCheck(LocOpCodes locOpCode, InstructionCheck check) {
			return new OperandCheck(locOpCode, check, CheckModes.Variable);
		}

		/**<summary>Checks for just an opCode.</summary>*/
		public static OperandCheck VarCheck(OpCode opCode) {
			return new OperandCheck(opCode, (i, op) => { return true; }, CheckModes.Variable);
		}
		/**<summary>Checks with a function.</summary>*/
		public static OperandCheck VarCheck(OpCode opCode, InstructionCheck check) {
			return new OperandCheck(opCode, check, CheckModes.Variable);
		}

		#endregion
		//--------------------------------
		#region Normal

		/**<summary>Checks for just an opCode.</summary>*/
		public static OperandCheck Check(LocOpCodes locOpCode) {
			return new OperandCheck(locOpCode, (i, op) => { return true; });
		}
		/**<summary>Checks with a function.</summary>*/
		public static OperandCheck Check(LocOpCodes locOpCode, InstructionCheck check) {
			return new OperandCheck(locOpCode, check);
		}

		/**<summary>Checks for just an opCode.</summary>*/
		public static OperandCheck Check(OpCode opCode) {
			return new OperandCheck(opCode, (i, op) => { return true; });
		}
		/**<summary>Checks with a function.</summary>*/
		public static OperandCheck Check(OpCode opCode, InstructionCheck check) {
			return new OperandCheck(opCode, check);
		}
		/**<summary>Checks an opcode and double.</summary>*/
		public static OperandCheck Check(OpCode opCode, double value) {
			return new OperandCheck(opCode, (i, op) => { return (op is double && (double)op == value); });
		}
		/**<summary>Checks an opcode and float.</summary>*/
		public static OperandCheck Check(OpCode opCode, float value) {
			return new OperandCheck(opCode, (i, op) => { return (op is float && (float)op == value); });
		}
		/**<summary>Checks an opcode and long.</summary>*/
		public static OperandCheck Check(OpCode opCode, long value) {
			return new OperandCheck(opCode, (i, op) => { return (op is long && (long)op == value); });
		}
		/**<summary>Checks an opcode and int.</summary>*/
		public static OperandCheck Check(OpCode opCode, int value) {
			return new OperandCheck(opCode, (i, op) => { return (op is int && (int)op == value); });
		}
		/**<summary>Checks an opcode and byte.</summary>*/
		public static OperandCheck Check(OpCode opCode, byte value) {
			return new OperandCheck(opCode, (i, op) => { return (op is byte && (byte)op == value); });
		}
		/**<summary>Checks an opcode and sbyte.</summary>*/
		public static OperandCheck Check(OpCode opCode, sbyte value) {
			return new OperandCheck(opCode, (i, op) => { return (op is sbyte && (sbyte)op == value); });
		}
		/**<summary>Checks an opcode and string.</summary>*/
		public static OperandCheck Check(OpCode opCode, string value) {
			return new OperandCheck(opCode, (i, op) => { return (op is string && (string)op == value); });
		}

		#endregion
		//--------------------------------
		#region Reference/Definition

		/**<summary>Checks an opcode and parameter.</summary>*/
		public static OperandCheck Check(LocOpCodes locOpCode, ParameterDefinition parameter) {
			return new OperandCheck(locOpCode, (i, op) => { return (op as ParameterDefinition == parameter || op == null); });
		}
		/**<summary>Checks an opcode and variable.</summary>*/
		public static OperandCheck Check(LocOpCodes locOpCode, VariableDefinition variable) {
			return new OperandCheck(locOpCode, (i, op) => { return (op as VariableDefinition == variable || op == null); });
		}

		/**<summary>Checks an opcode and parameter.</summary>*/
		public static OperandCheck Check(OpCode opCode, ParameterDefinition parameter) {
			return new OperandCheck(opCode, (i, op) => { return (op as ParameterDefinition == parameter); });
		}
		/**<summary>Checks an opcode and variable.</summary>*/
		public static OperandCheck Check(OpCode opCode, VariableDefinition variable) {
			return new OperandCheck(opCode, (i, op) => { return (op as VariableDefinition == variable); });
		}

		/**<summary>Checks an opcode and field reference.</summary>*/
		public static OperandCheck Check(OpCode opCode, FieldReference field) {
			return new OperandCheck(opCode, (i, op) => { return (op is FieldReference && ((FieldReference)op).FullName == field.FullName); });
		}
		/**<summary>Checks an opcode and method reference.</summary>*/
		public static OperandCheck Check(OpCode opCode, MethodReference method) {
			return new OperandCheck(opCode, (i, op) => { return (op is MethodReference && ((MethodReference)op).FullName == method.FullName); });
		}
		/**<summary>Checks an opcode and type reference.</summary>*/
		public static OperandCheck Check(OpCode opCode, TypeReference type) {
			return new OperandCheck(opCode, (i, op) => { return (op is TypeReference && ((TypeReference)op).FullName == type.FullName); });
		}
		/**<summary>Checks an opcode and call site.</summary>*/
		public static OperandCheck Check(OpCode opCode, CallSite callSite) {
			return new OperandCheck(opCode, (i, op) => { return (op is CallSite && ((CallSite)op).FullName == callSite.FullName); });
		}

		/**<summary>Checks an opcode and field reference.</summary>*/
		public static OperandCheck CheckField(OpCode opCode, string contains) {
			return new OperandCheck(opCode, (i, op) => { return (op is FieldReference && ((FieldReference)op).FullName.Contains(contains)); });
		}
		/**<summary>Checks an opcode and method reference.</summary>*/
		public static OperandCheck CheckMethod(OpCode opCode, string contains) {
			return new OperandCheck(opCode, (i, op) => { return (op is MethodReference && ((MethodReference)op).FullName.Contains(contains)); });
		}
		/**<summary>Checks an opcode and type reference.</summary>*/
		public static OperandCheck CheckType(OpCode opCode, string contains) {
			return new OperandCheck(opCode, (i, op) => { return (op is TypeReference && ((TypeReference)op).FullName.Contains(contains)); });
		}

		#endregion
		//--------------------------------
		#endregion
		//--------------------------------
		#endregion
		//=========== METHODS ============
		#region Methods
		//--------------------------------
		#region Getters

		/**<summary>Gets the number of instructions in the method.</summary>*/
		public static int InstructionCount(MethodDefinition method) {
			var il = method.Body.GetILProcessor();
			return il.Body.Instructions.Count;
		}

		#endregion
		//--------------------------------
		#region Prepend

		/**<summary>Prepents to the beginning of the specified method.</summary>*/
		public static int MethodPrepend(MethodDefinition method, params Instruction[] instructions) {
			return MethodInsert(method, 0, instructions);
		}

		#endregion
		//--------------------------------
		#region Append

		/**<summary>Appends to the end of the specified method.</summary>*/
		public static int MethodAppend(MethodDefinition method, params Instruction[] instructions) {
			return MethodInsert(method, InstructionCount(method), instructions);
		}

		#endregion
		//--------------------------------
		#region Insert

		/**<summary>Appends to the index of the specified method.</summary>*/
		public static int MethodInsert(MethodDefinition method, int index, params Instruction[] instructions) {
			var il = method.Body.GetILProcessor();

			foreach (var inst in instructions) {
				il.Body.Instructions.Insert(index, inst);
				index++;
			}
			return index;
		}
		
		#endregion
		//--------------------------------
		#region Replace

		/**<summary>Replaces all of the specified method.</summary>*/
		public static int MethodReplace(MethodDefinition method, params Instruction[] instructions) {
			var il = method.Body.GetILProcessor();

			il.Body.Instructions.Clear();
			foreach (var inst in instructions) {
				il.Body.Instructions.Add(inst);
			}
			return instructions.Length;
		}
		/**<summary>Replaces a single instruction of the specified method.</summary>*/
		public static int MethodReplace(MethodDefinition method, int index, params Instruction[] instructions) {
			var il = method.Body.GetILProcessor();

			il.Body.Instructions.RemoveAt(index);
			foreach (var inst in instructions) {
				il.Body.Instructions.Insert(index, inst);
				index++;
			}
			return index;
		}
		/**<summary>Replaces a part of the specified method.</summary>*/
		public static int MethodReplaceRange(MethodDefinition method, int start, int end, params Instruction[] instructions) {
			var il = method.Body.GetILProcessor();

			for (int i = start; i < end; i++) {
				il.Body.Instructions.RemoveAt(start);
			}
			foreach (var inst in instructions) {
				il.Body.Instructions.Insert(start, inst);
				start++;
			}
			return start;
		}
		/**<summary>Replaces the start of the specified method.</summary>*/
		public static int MethodReplaceStart(MethodDefinition method, int end, params Instruction[] instructions) {
			return MethodReplaceRange(method, 0, end, instructions);
		}
		/**<summary>Replaces the end of the specified method.</summary>*/
		public static int MethodReplaceEnd(MethodDefinition method, int start, params Instruction[] instructions) {
			return MethodReplaceRange(method, start, InstructionCount(method), instructions);
		}

		#endregion
		//--------------------------------
		#region Remove

		/**<summary>Clears the specified method of all instructions.</summary>*/
		public static void MethodClear(MethodDefinition method) {
			var il = method.Body.GetILProcessor();
			il.Body.Instructions.Clear();
		}
		/**<summary>Removes a single instruction in the specified method.</summary>*/
		public static void MethodRemove(MethodDefinition method, int index) {
			var il = method.Body.GetILProcessor();
			il.Body.Instructions.RemoveAt(index);
		}
		/**<summary>Removes a range of instructions in the specified method.</summary>*/
		public static void MethodRemoveRange(MethodDefinition method, int start, int end) {
			var il = method.Body.GetILProcessor();

			for (int i = start; i < end; i++) {
				il.Body.Instructions.RemoveAt(start);
			}
		}
		/**<summary>Removes the start of the specified method.</summary>*/
		public static void MethodRemoveStart(MethodDefinition method, int end) {
			MethodRemoveRange(method, 0, end);
		}
		/**<summary>Removes the end of the specified method.</summary>*/
		public static void MethodRemoveEnd(MethodDefinition method, int start) {
			MethodRemoveRange(method, start, InstructionCount(method));
		}

		#endregion
		//--------------------------------
		#endregion
		//=========== SCANNING ===========
		#region Scanning
		//--------------------------------
		#region OpCodes

		/**<summary>Scans for the nth opcode pattern.</summary>*/
		private static int ScanForNthOpCodePattern(MethodDefinition method, int n, int start, bool end, params OpCode[] checks) {
			var il = method.Body.GetILProcessor();

			for (int x = start; x < il.Body.Instructions.Count - checks.Length; x++) {
				if (il.Body.Instructions[x].OpCode != checks[0])
					continue;

				for (int y = 0; y < checks.Length; y++) {
					if (il.Body.Instructions[x + y].OpCode != checks[y])
						break;
					if (y == checks.Length - 1) {
						if (n == 0)
							return x + (end ? checks.Length : 0);
						n--;
					}
				}
			}

			return -1;
		}

		/**<summary>Scans for the opcode pattern.</summary>*/
		public static int ScanForOpCodePattern(MethodDefinition method, params OpCode[] checks) {
			return ScanForNthOpCodePattern(method, 0, 0, false, checks);
		}
		/**<summary>Scans for the opcode pattern.</summary>*/
		public static int ScanForOpCodePattern(MethodDefinition method, int start, params OpCode[] checks) {
			return ScanForNthOpCodePattern(method, 0, start, false, checks);
		}
		/**<summary>Scans for the nth opcode pattern.</summary>*/
		public static int ScanForNthOpCodePattern(MethodDefinition method, int n, params OpCode[] checks) {
			return ScanForNthOpCodePattern(method, n, 0, false, checks);
		}
		/**<summary>Scans for the nth opcode pattern.</summary>*/
		private static int ScanForNthOpCodePattern(MethodDefinition method, int n, int start, params OpCode[] checks) {
			return ScanForNthOpCodePattern(method, n, start, false, checks);
		}

		/**<summary>Scans for the end of the opcode pattern.</summary>*/
		public static int ScanForOpCodePatternEnd(MethodDefinition method, params OpCode[] checks) {
			return ScanForNthOpCodePattern(method, 0, 0, true, checks);
		}
		/**<summary>Scans for the end of the opcode pattern.</summary>*/
		public static int ScanForOpCodePatternEnd(MethodDefinition method, int start, params OpCode[] checks) {
			return ScanForNthOpCodePattern(method, 0, start, true, checks);
		}
		/**<summary>Scans for the end of the nth opcode pattern.</summary>*/
		public static int ScanForNthOpCodePatternEnd(MethodDefinition method, int n, params OpCode[] checks) {
			return ScanForNthOpCodePattern(method, n, 0, true, checks);
		}
		/**<summary>Scans for the end of the nth opcode pattern.</summary>*/
		public static int ScanForNthOpCodePatternEnd(MethodDefinition method, int n, int start, params OpCode[] checks) {
			return ScanForNthOpCodePattern(method, n, start, true, checks);
		}

		#endregion
		//--------------------------------
		#region Instructions

		/**<summary>Scans for the nth instruction pattern.</summary>*/
		private static int ScanForNthInstructionPattern(MethodDefinition method, int n, int start, bool end, int checkIndex, params OperandCheck[] checks) {
			var il = method.Body.GetILProcessor();

			for (int x = start; x < il.Body.Instructions.Count - checks.Length; x++) {
				if (!checks[checkIndex].OpCodeMatches(il.Body.Instructions[x].OpCode))
					continue;

				CheckModes mode = CheckModes.Normal;
				int countLeft = 0;
				int index = 0;
				for (int y = checkIndex; y < checks.Length; y++) {
					Instruction instruction = il.Body.Instructions[x + index];
					OperandCheck check = checks[y];

					// Check for a new mode
					if (check.Mode == CheckModes.Repeat) {
						if (mode != CheckModes.Repeat) {
							countLeft = check.InstructionCount;
							mode = CheckModes.Repeat;
						}
						check = checks[y - 1];
					}
					else if (check.Mode == CheckModes.Skip) {
						if (check.InstructionCount == -1) {
							// Recursively search for the next section of instructions
							int returnIndex = ScanForNthInstructionPattern(method, n, x + index, end, y + 1, checks);
							if (returnIndex == -1) {
								// Abandon all hope
								return -1;
							}
							else {
								if (n == 0) {
									if (end) return returnIndex;
									else return x;
								}
								n--;
							}
						}
						else if (check.Mode != CheckModes.Skip) {
							// Just skip x instructions
							countLeft = check.InstructionCount;
							mode = CheckModes.Skip;
						}
					}

					// Decrement the count left
					if (countLeft > 0) {
						countLeft--;
						bool skipping = (mode == CheckModes.Skip);
						if (countLeft == 0)
							mode = CheckModes.Normal;
						else
							y--;
						if (skipping)
							continue;
					}

					// Check the instruction
					if (!check.OpCodeMatches(instruction.OpCode))
						break;
					else if (!check.Check.Invoke(index, instruction.Operand))
						break;
					index++;
					if (y == checks.Length - 1 && countLeft == 0) {
						if (n == 0)
							return x + (end ? index : 0);
						n--;
					}
				}
			}

			return -1;
		}

		/**<summary>Scans for the instruction pattern.</summary>*/
		public static int ScanForInstructionPattern(MethodDefinition method, params OperandCheck[] checks) {
			return ScanForNthInstructionPattern(method, 0, 0, false, 0, checks);
		}
		/**<summary>Scans for the instruction pattern.</summary>*/
		public static int ScanForInstructionPattern(MethodDefinition method, int start, params OperandCheck[] checks) {
			return ScanForNthInstructionPattern(method, 0, start, false, 0, checks);
		}
		/**<summary>Scans for the nth instruction pattern.</summary>*/
		public static int ScanForNthInstructionPattern(MethodDefinition method, int n, params OperandCheck[] checks) {
			return ScanForNthInstructionPattern(method, n, 0, false, 0, checks);
		}
		/**<summary>Scans for the nth instruction pattern.</summary>*/
		public static int ScanForNthInstructionPattern(MethodDefinition method, int n, int start, params OperandCheck[] checks) {
			return ScanForNthInstructionPattern(method, n, start, false, 0, checks);
		}

		/**<summary>Scans for the end of the instruction pattern.</summary>*/
		public static int ScanForInstructionPatternEnd(MethodDefinition method, params OperandCheck[] checks) {
			return ScanForNthInstructionPattern(method, 0, 0, true, 0, checks);
		}
		/**<summary>Scans for the end of the instruction pattern.</summary>*/
		public static int ScanForInstructionPatternEnd(MethodDefinition method, int start, params OperandCheck[] checks) {
			return ScanForNthInstructionPattern(method, 0, start, true, 0, checks);
		}
		/**<summary>Scans for the end of the nth instruction pattern.</summary>*/
		public static int ScanForNthInstructionPatternEnd(MethodDefinition method, int n, params OperandCheck[] checks) {
			return ScanForNthInstructionPattern(method, n, 0, true, 0, checks);
		}
		/**<summary>Scans for the end of the nth instruction pattern.</summary>*/
		public static int ScanForNthInstructionPatternEnd(MethodDefinition method, int n, int start, params OperandCheck[] checks) {
			return ScanForNthInstructionPattern(method, n, start, true, 0, checks);
		}

		#endregion
		//--------------------------------
		#region Variables

		/**<summary>Scans for the nth instruction pattern.</summary>*/
		private static VariableDefinition ScanForNthVariablePattern(MethodDefinition method, int n, int start, int checkIndex, VariableDefinition initialVarDef, params OperandCheck[] checks) {
			var il = method.Body.GetILProcessor();

			for (int x = start; x < il.Body.Instructions.Count - checks.Length; x++) {
				if (!checks[checkIndex].OpCodeMatches(il.Body.Instructions[x].OpCode))
					continue;

				CheckModes mode = CheckModes.Normal;
				int countLeft = 0;
				int index = 0;
				VariableDefinition varDef = initialVarDef;
				for (int y = checkIndex; y < checks.Length; y++) {
					Instruction instruction = il.Body.Instructions[x + index];
					OperandCheck check = checks[y];

					// Check for a new mode
					if (check.Mode == CheckModes.Repeat) {
						if (mode != CheckModes.Repeat) {
							countLeft = check.InstructionCount;
							mode = CheckModes.Repeat;
						}
						check = checks[y - 1];
					}
					else if (check.Mode == CheckModes.Skip) {
						if (check.InstructionCount == -1) {
							// Recursively search for the next section of instructions
							var returnVarDef = ScanForNthVariablePattern(method, n, x + index, y + 1, varDef, checks);
							if (returnVarDef != null) {
								if (n == 0)
									return returnVarDef;
								n--;
							}
							else {
								// Abandon all hope
								if (initialVarDef != null)
									return null;
								// Continue from the initial non-recursive loop
								break;
							}
						}
						else if (check.Mode != CheckModes.Skip) {
							// Just skip x instructions
							countLeft = check.InstructionCount;
							mode = CheckModes.Skip;
						}
					}

					// Decrement the count left
					if (countLeft > 0) {
						countLeft--;
						bool skipping = (mode == CheckModes.Skip);
						if (countLeft == 0)
							mode = CheckModes.Normal;
						else
							y--;
						if (skipping)
							continue;
					}

					// Check the instruction
					if (!check.OpCodeMatches(instruction.OpCode))
						break;
					else if (!check.Check.Invoke(index, instruction.Operand))
						break;
					else if (check.Mode == CheckModes.Variable) {
						if (varDef == null)
							varDef = GetVariableFromInstruction(method, instruction);
						else if (varDef != GetVariableFromInstruction(method, instruction))
							break;
					}
					index++;
					if (y == checks.Length - 1 && countLeft == 0) {
						if (n == 0)
							return varDef;
						n--;
					}
				}
			}

			return null;
		}

		/**<summary>Scans for the variable inside the instruction set.</summary>*/
		public static VariableDefinition ScanForVariablePattern(MethodDefinition method, params OperandCheck[] checks) {
			return ScanForNthVariablePattern(method, 0, 0, 0, null, checks);
		}
		/**<summary>Scans for the variable inside the instruction set.</summary>*/
		public static VariableDefinition ScanForVariablePattern(MethodDefinition method, int start, params OperandCheck[] checks) {
			return ScanForNthVariablePattern(method, 0, start, 0, null, checks);
		}
		/**<summary>Scans for the variable inside the instruction set.</summary>*/
		public static VariableDefinition ScanForNthVariablePattern(MethodDefinition method, int n, params OperandCheck[] checks) {
			return ScanForNthVariablePattern(method, n, 0, 0, null, checks);
		}

		#endregion
		//--------------------------------
		#endregion
		//========= DEFINITIONS ==========
		#region Definitions

		/**<summary>Gets the type name and end name from a full name of a type's property method or field.</summary>*/
		private static string[] GetFullNames(string fullName, bool verbose = true) {
			int end = fullName.LastIndexOf('.');
			for (int i = end - 1; i >= 1; i++) {
				if (fullName[i] == '.') {
					return new string[] { fullName.Substring(i, end - i), fullName.Substring(end + 1) };
				}
			}
			if (verbose)
				throw new Exception("Failed to get type name of '" + fullName + "'.");
			return null;
		}

		/**<summary>Gets the definition of an assembly's module.</summary>*/
		public static ModuleDefinition GetModuleDefinition(AssemblyDefinition definition, string fullyQualifiedName, bool verbose = true) {
			ModuleDefinition module = definition.Modules.FirstOrDefault(p => p.FullyQualifiedName == fullyQualifiedName);

			if (module == null && verbose) {
				module = definition.MainModule;
				throw new Exception(string.Format("Failed to locate {0} reference!", fullyQualifiedName));
			}

			return module;
		}
		/**<summary>Gets the definition of a modfule's type.</summary>*/
		public static TypeDefinition GetTypeDefinition(ModuleDefinition moduleDefinition, string typeName, bool verbose = true) {
			var result = (from TypeDefinition t in moduleDefinition.Types
						  where t.Name == typeName
						  select t).FirstOrDefault();

			if (result == null && verbose)
				throw new Exception(string.Format("Failed to locate {0} type!", typeName));

			return result;
		}
		/**<summary>Gets the definition of a modfule's type. Only use this instead of GetTypeDefinition() when the type is not within the Terraria module (eg. an XNA type).</summary>*/
		public static TypeReference GetTypeReference(ModuleDefinition moduleDefinition, string fullName, bool verbose = true) {
			TypeReference reference;

			if (!moduleDefinition.TryGetTypeReference(fullName, out reference) && verbose)
				throw new Exception(string.Format("Failed to locate {0} type!", fullName));

			return reference;
		}
		/**<summary>Gets the definition of a type's field.</summary>*/
		public static FieldDefinition GetFieldDefinition(TypeDefinition t, string fieldName, bool verbose = true) {
			var result = (from FieldDefinition f in t.Fields
						  where f.Name == fieldName
						  select f).FirstOrDefault();

			if (result == null && verbose)
				throw new Exception(string.Format("Failed to locate {0}.{1} field!", t.FullName, fieldName));

			return result;
		}
		/**<summary>Gets the definition of a type's field.</summary>*/
		public static FieldDefinition GetFieldDefinition(ModuleDefinition moduleDefinition, string fullName, bool verbose = true) {
			string[] fullNames = GetFullNames(fullName, verbose);
			TypeDefinition t = GetTypeDefinition(moduleDefinition, fullNames[0], verbose);
			var result = (from FieldDefinition f in t.Fields
						  where f.Name == fullNames[1]
						  select f).FirstOrDefault();

			if (result == null && verbose)
				throw new Exception(string.Format("Failed to locate {0}.{1} field!", t.FullName, fullNames[1]));

			return result;
		}
		/**<summary>Gets the definition of a type's property.</summary>*/
		public static PropertyDefinition GetPropertyDefinition(TypeDefinition t, string propName, bool verbose = true) {
			var result = (from PropertyDefinition p in t.Properties
						  where p.Name == propName
						  select p).FirstOrDefault();

			if (result == null && verbose)
				throw new Exception(string.Format("Failed to locate {0}.{1} property!", t.FullName, propName));

			return result;
		}
		/**<summary>Gets the definition of a type's property.</summary>*/
		public static PropertyDefinition GetPropertyDefinition(ModuleDefinition moduleDefinition, string fullName, bool verbose = true) {
			string[] fullNames = GetFullNames(fullName, verbose);
			TypeDefinition t = GetTypeDefinition(moduleDefinition, fullNames[0], verbose);
			var result = (from PropertyDefinition p in t.Properties
						  where p.Name == fullNames[1]
						  select p).FirstOrDefault();

			if (result == null && verbose)
				throw new Exception(string.Format("Failed to locate {0}.{1} property!", t.FullName, fullNames[1]));

			return result;
		}
		/**<summary>Gets the definition of a type's method.</summary>*/
		public static MethodDefinition GetMethodDefinition(TypeDefinition t, string methodName, int parameterCount = -1, bool verbose = true) {
			var result = (from MethodDefinition m in t.Methods
						  where m.Name == methodName && (parameterCount == -1 || m.Parameters.Count + m.GenericParameters.Count == parameterCount)
						  select m).FirstOrDefault();

			if (result == null && verbose)
				throw new Exception(string.Format("Failed to locate {0}.{1}() method!", t.FullName, methodName));

			return result;
		}
		/**<summary>Gets the definition of a type's method.</summary>*/
		public static MethodDefinition GetMethodDefinition(TypeDefinition t, string methodName, bool isStatic, int parameterCount = -1, bool verbose = true) {
			var result = (from MethodDefinition m in t.Methods
						  where m.Name == methodName && m.IsStatic == isStatic && (parameterCount == -1 || m.Parameters.Count + m.GenericParameters.Count == parameterCount)
						  select m).FirstOrDefault();

			if (result == null && verbose)
				throw new Exception(string.Format("Failed to locate {0}.{1}() method!", t.FullName, methodName));

			return result;
		}

		#endregion
		//======== STATIC FIELDS =========
		#region Static Fields

		/**<summary>Adds a new static field to a type.</summary>*/
		public static FieldDefinition AddStaticField(ModuleDefinition moduleDefinition, TypeDefinition classType, string field, Type type, object value = null) {
			return AddStaticField(classType, field, moduleDefinition.Import(type), value);
		}
		/**<summary>Adds a new static field to a type.</summary>*/
		public static FieldDefinition AddStaticField(TypeDefinition classType, string field, TypeReference type, object value = null) {
			var classStaticConstructor = GetMethodDefinition(classType, ".cctor");
			if (classStaticConstructor == null) return null;

			var fld = new FieldDefinition(field, FieldAttributes.Static | FieldAttributes.Public, type);
			classType.Fields.Add(fld);

			if (value != null) {
				var il = classStaticConstructor.Body.GetILProcessor();
				var first = il.Body.Instructions[0];

				if (type.Name == "String")
					il.InsertBefore(first, il.Create(OpCodes.Ldstr, (string)value));
				else if (type.Name == "Int32")
					il.InsertBefore(first, il.Create(OpCodes.Ldc_I4, (int)value));
				else if (type.Name == "Boolean")
					il.InsertBefore(first, il.Create(OpCodes.Ldc_I4, (bool)value ? 1 : 0));
				else if (type.Name == "Single")
					il.InsertBefore(first, il.Create(OpCodes.Ldc_R4, (Single)value));
				else if (value is Instruction)
					il.InsertBefore(first, (Instruction)value);
				else
					throw new Exception(string.Format("AddStaticField(): Unrecognized type '{0}'!", type.FullName));

				il.InsertBefore(first, il.Create(OpCodes.Stsfld, fld));
			}

			return fld;
		}
		/**<summary>Modifies a type's static field.</summary>*/
		public static void ModifyStaticField(TypeDefinition classType, string field, object newValue) {
			var classStaticConstructor = GetMethodDefinition(classType, ".cctor");
			if (classStaticConstructor == null) return;

			if (newValue is string) {
				ModifyStaticField(classStaticConstructor, field, instr => {
					instr.OpCode = OpCodes.Ldstr;
					instr.Operand = newValue;
				});
			}
			else if (newValue is int || newValue is bool) {
				ModifyStaticField(classStaticConstructor, field, instr => {
					instr.OpCode = OpCodes.Ldc_I4;
					instr.Operand = newValue;
				});
			}
			else if (newValue is float) {
				ModifyStaticField(classStaticConstructor, field, instr => {
					instr.OpCode = OpCodes.Ldc_R4;
					instr.Operand = newValue;
				});
			}
			else
				throw new Exception(string.Format("ModifyStaticField(): Unrecognized type '{0}'!", newValue.GetType().FullName));
		}
		/**<summary>Modifies a type's static field.</summary>*/
		public static void ModifyStaticField(MethodDefinition classStaticConstructor, string field, Action<Instruction> set) {
			Instruction last = null;
			foreach (var instr in classStaticConstructor.Body.Instructions) {
				if (instr.OpCode == OpCodes.Stsfld && instr.Operand != null && instr.Operand.ToString().ToLower().Contains(field.ToLower())) {
					set(last);
					return;
				}
				last = instr;
			}
		}

		#endregion
		//===== LARGE ADDRESS AWARE ======
		#region Large Address Aware

		/**<summary>Patches the executable to allow more memory usage. This is needed after Mono.cecil writes to the assembly.</summary>*/
		public static void MakeLargeAddressAware(string file) {
			using (var stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite)) {
				const int IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x20;

				var reader = new BinaryReader(stream);
				var writer = new BinaryWriter(stream);

				if (reader.ReadInt16() != 0x5A4D)       //No MZ Header
					return;

				reader.BaseStream.Position = 0x3C;
				var peloc = reader.ReadInt32();         //Get the PE header location.

				reader.BaseStream.Position = peloc;
				if (reader.ReadInt32() != 0x4550)       //No PE header
					return;

				reader.BaseStream.Position += 0x12;

				var position = reader.BaseStream.Position;
				var flags = reader.ReadInt16();
				bool isLAA = (flags & IMAGE_FILE_LARGE_ADDRESS_AWARE) == IMAGE_FILE_LARGE_ADDRESS_AWARE;
				if (isLAA)                          //Already Large Address Aware
					return;

				flags |= IMAGE_FILE_LARGE_ADDRESS_AWARE;

				writer.Seek((int)position, SeekOrigin.Begin);
				writer.Write(flags);
				writer.Flush();
			}
		}
		#endregion
		//=========== VARIOUS ============
		#region Various

		/**<summary>Gets the version of the assembly.</summary>*/
		public static Version GetAssemblyVersion(string path) {
			return AssemblyDefinition.ReadAssembly(path).Name.Version;
		}
		/**<summary>Makes a type public.</summary>*/
		public static void MakeTypePublic(TypeDefinition typeDefinition, bool includeNestedTypes = true) {
			if (includeNestedTypes) {
				typeDefinition.NestedTypes.ToList().ForEach(definition => {
					if (definition.FullName != typeDefinition.FullName) // avoid infinite recursion
						MakeTypePublic(definition);
				});
			}
			if (typeDefinition.IsNested)
				typeDefinition.IsNestedPublic = true;
			else
				typeDefinition.IsPublic = true;
			foreach (var field in typeDefinition.Fields)
				field.IsPublic = true;
			foreach (var method in typeDefinition.Methods) {
				if (!method.IsSpecialName)
					method.IsPublic = true;
			}
		}
		/**<summary>Creates a copy of an instruction.</summary>*/
		public static Instruction CloneInstruction(Instruction original) {
			if (original.Operand is double)
				return Instruction.Create(original.OpCode, (double)original.Operand);
			else if (original.Operand is float)
				return Instruction.Create(original.OpCode, (float)original.Operand);
			else if (original.Operand is long)
				return Instruction.Create(original.OpCode, (long)original.Operand);
			else if (original.Operand is int)
				return Instruction.Create(original.OpCode, (int)original.Operand);
			else if (original.Operand is byte)
				return Instruction.Create(original.OpCode, (byte)original.Operand);
			else if (original.Operand is sbyte)
				return Instruction.Create(original.OpCode, (sbyte)original.Operand);
			else if (original.Operand is string)
				return Instruction.Create(original.OpCode, original.Operand as string);

			else if (original.Operand is ParameterDefinition)
				return Instruction.Create(original.OpCode, original.Operand as ParameterDefinition);
			else if (original.Operand is VariableDefinition)
				return Instruction.Create(original.OpCode, original.Operand as VariableDefinition);
			else if (original.Operand is TypeReference)
				return Instruction.Create(original.OpCode, original.Operand as TypeReference);
			else if (original.Operand is MethodReference)
				return Instruction.Create(original.OpCode, original.Operand as MethodReference);
			else if (original.Operand is FieldReference)
				return Instruction.Create(original.OpCode, original.Operand as FieldReference);
			else if (original.Operand is CallSite)
				return Instruction.Create(original.OpCode, original.Operand as CallSite);

			else if (original.Operand is Instruction)
				return Instruction.Create(original.OpCode, IL.CloneInstruction(original.Operand as Instruction));
			else if (original.Operand is Instruction[]) {
				Instruction[] set = new Instruction[(original.Operand as Instruction[]).Length];

				for (int i = 0; i < set.Length; i++)
					set[i] = IL.CloneInstruction((original.Operand as Instruction[])[i]);

				return Instruction.Create(original.OpCode, set);
			}

			else if (original.Operand == null)
				return Instruction.Create(original.OpCode);

			return null;
		}

		#endregion
	}
}
