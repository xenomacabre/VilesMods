using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DoorsExpanded
{
    // Copied from updated PawnShields
    public static class HarmonyExtensions
    {
        public static void SafeInsertRange(this List<CodeInstruction> instructions, int insertionIndex, IEnumerable<CodeInstruction> newInstructions,
            IEnumerable<Label> labelsToTransfer = null, IEnumerable<ExceptionBlock> blocksToTransfer = null)
        {
            var origInstruction = instructions[insertionIndex];
            instructions.InsertRange(insertionIndex, newInstructions);
            instructions[insertionIndex].TransferLabelsAndBlocksFrom(origInstruction, labelsToTransfer, blocksToTransfer);
        }

        public static void SafeReplaceRange(this List<CodeInstruction> instructions, int insertionIndex, int count, IEnumerable<CodeInstruction> newInstructions,
            IEnumerable<Label> labelsToTransfer = null, IEnumerable<ExceptionBlock> blocksToTransfer = null)
        {
            var origInstruction = instructions[insertionIndex];
            instructions.ReplaceRange(insertionIndex, count, newInstructions);
            instructions[insertionIndex].TransferLabelsAndBlocksFrom(origInstruction, labelsToTransfer, blocksToTransfer);
        }

        public static CodeInstruction TransferLabelsAndBlocksFrom(this CodeInstruction instruction, CodeInstruction otherInstruction,
            IEnumerable<Label> labelsToTransfer = null, IEnumerable<ExceptionBlock> blocksToTransfer = null)
        {
            if (labelsToTransfer is null)
                instruction.labels.AddRange(otherInstruction.labels.PopAll());
            else
                instruction.labels.AddRange(labelsToTransfer.Where(otherInstruction.labels.Remove));
            if (blocksToTransfer is null)
                instruction.blocks.AddRange(otherInstruction.blocks.PopAll());
            else
                instruction.blocks.AddRange(blocksToTransfer.Where(otherInstruction.blocks.Remove));
            return instruction;
        }

        public static bool IsBrfalse(this CodeInstruction instruction)
        {
            return instruction.opcode == OpCodes.Brfalse_S || instruction.opcode == OpCodes.Brfalse;
        }
    }

#pragma warning disable CA1822 // Mark members as static
    public class Locals
    {
        private readonly IList<LocalVariableInfo> locals;
        private readonly ILGenerator ilGenerator;

        public Locals(MethodBase method, ILGenerator ilGenerator)
        {
            locals = method.GetMethodBody().LocalVariables;
            this.ilGenerator = ilGenerator;
        }

        public LocalVar FromLdloc(CodeInstruction instruction)
        {
            if (IsLdloc(instruction, out var local))
                return local;
            throw new ArgumentException("Expected ldloc-type instruction, actual instruction: " + instruction);
        }

        public bool IsLdlocOrLdloca(CodeInstruction instruction)
        {
            return instruction.IsLdloc();
        }

        public bool IsLdloc(CodeInstruction instruction)
        {
            return
                instruction.opcode == OpCodes.Ldloc ||
                instruction.opcode == OpCodes.Ldloc_S ||
                instruction.opcode == OpCodes.Ldloc_0 ||
                instruction.opcode == OpCodes.Ldloc_1 ||
                instruction.opcode == OpCodes.Ldloc_2 ||
                instruction.opcode == OpCodes.Ldloc_3;
        }

        public bool IsLdloc(CodeInstruction instruction, out LocalVar local)
        {
            if (instruction.opcode == OpCodes.Ldloc || instruction.opcode == OpCodes.Ldloc_S)
                local = new LocalVar((LocalVariableInfo)instruction.operand); // note: LocalBuilder derives from LocalVariableInfo
            else if (instruction.opcode == OpCodes.Ldloc_0)
                local = new LocalVar(locals[0]);
            else if (instruction.opcode == OpCodes.Ldloc_1)
                local = new LocalVar(locals[1]);
            else if (instruction.opcode == OpCodes.Ldloc_2)
                local = new LocalVar(locals[2]);
            else if (instruction.opcode == OpCodes.Ldloc_3)
                local = new LocalVar(locals[3]);
            else
            {
                local = default;
                return false;
            }
            return true;
        }

        public bool IsLdloc(CodeInstruction instruction, LocalVar local)
        {
            return IsLdloc(instruction, out var otherLocal) && local == otherLocal;
        }

        public LocalVar FromLdloca(CodeInstruction instruction)
        {
            if (IsLdloca(instruction, out var local))
                return local;
            throw new ArgumentException("Expected ldloca-type instruction, actual instruction: " + instruction);
        }

        public bool IsLdloca(CodeInstruction instruction)
        {
            return instruction.opcode == OpCodes.Ldloca || instruction.opcode == OpCodes.Ldloca_S;
        }

        public bool IsLdloca(CodeInstruction instruction, out LocalVar local)
        {
            if (instruction.opcode == OpCodes.Ldloca || instruction.opcode == OpCodes.Ldloca_S)
            {
                local = new LocalVar((LocalVariableInfo)instruction.operand); // note: LocalBuilder derives from LocalVariableInfo
                return true;
            }
            else
            {
                local = default;
                return false;
            }
        }

        public bool IsLdloca(CodeInstruction instruction, LocalVar local)
        {
            return IsLdloca(instruction, out var otherLocal) && local == otherLocal;
        }

        public LocalVar FromStloc(CodeInstruction instruction)
        {
            if (IsStloc(instruction, out var local))
                return local;
            throw new ArgumentException("Expected stloc-type instruction, actual instruction: " + instruction);
        }

        public bool IsStloc(CodeInstruction instruction)
        {
            return instruction.IsStloc();
        }

        public bool IsStloc(CodeInstruction instruction, out LocalVar local)
        {
            if (instruction.opcode == OpCodes.Stloc || instruction.opcode == OpCodes.Stloc_S)
                local = new LocalVar((LocalVariableInfo)instruction.operand); // note: LocalBuilder derives from LocalVariableInfo
            else if (instruction.opcode == OpCodes.Stloc_0)
                local = new LocalVar(locals[0]);
            else if (instruction.opcode == OpCodes.Stloc_1)
                local = new LocalVar(locals[1]);
            else if (instruction.opcode == OpCodes.Stloc_2)
                local = new LocalVar(locals[2]);
            else if (instruction.opcode == OpCodes.Stloc_3)
                local = new LocalVar(locals[3]);
            else
            {
                local = default;
                return false;
            }
            return true;
        }

        public bool IsStloc(CodeInstruction instruction, LocalVar local)
        {
            return IsStloc(instruction, out var otherLocal) && local == otherLocal;
        }

        public LocalVar DeclareLocal(Type localType)
        {
            return new LocalVar(ilGenerator.DeclareLocal(localType));
        }
    }
#pragma warning restore CA1822 // Mark members as static

    public struct LocalVar : IEquatable<LocalVar>
    {
        private readonly LocalVariableInfo local;

        public bool IsPinned => local.IsPinned;

        public int LocalIndex => local.LocalIndex;

        public Type LocalType => local.LocalType;

        internal LocalVar(LocalVariableInfo local) => this.local = local;

        public CodeInstruction ToLdloc()
        {
            // ILGenerator.Emit(OpCodes.Ldloc, LocalBuilder) automatically emits the proper opcode and operand.
            if (local is LocalBuilder localBuilder)
                return new CodeInstruction(OpCodes.Ldloc, localBuilder);
            var index = LocalIndex;
            switch (index)
            {
                case 0:
                    return new CodeInstruction(OpCodes.Ldloc_0);
                case 1:
                    return new CodeInstruction(OpCodes.Ldloc_1);
                case 2:
                    return new CodeInstruction(OpCodes.Ldloc_2);
                case 3:
                    return new CodeInstruction(OpCodes.Ldloc_3);
                default:
                    if (index <= byte.MaxValue)
                        return new CodeInstruction(OpCodes.Ldloc_S, (byte)index);
                    else
                        return new CodeInstruction(OpCodes.Ldloc, (short)index);
            }
        }

        public CodeInstruction ToLdloca()
        {
            // ILGenerator.Emit(OpCodes.Ldloca, LocalBuilder) automatically emits the proper opcode and operand.
            if (local is LocalBuilder localBuilder)
                return new CodeInstruction(OpCodes.Ldloca, localBuilder);
            var index = LocalIndex;
            if (index <= byte.MaxValue)
                return new CodeInstruction(OpCodes.Ldloca_S, (byte)index);
            else
                return new CodeInstruction(OpCodes.Ldloca, (short)index);
        }

        public CodeInstruction ToStloc()
        {
            // ILGenerator.Emit(OpCodes.Stloc, LocalBuilder) automatically emits the proper opcode and operand.
            if (local is LocalBuilder localBuilder)
                return new CodeInstruction(OpCodes.Stloc, localBuilder);
            var index = LocalIndex;
            switch (index)
            {
                case 0:
                    return new CodeInstruction(OpCodes.Stloc_0);
                case 1:
                    return new CodeInstruction(OpCodes.Stloc_1);
                case 2:
                    return new CodeInstruction(OpCodes.Stloc_2);
                case 3:
                    return new CodeInstruction(OpCodes.Stloc_3);
                default:
                    if (index <= byte.MaxValue)
                        return new CodeInstruction(OpCodes.Stloc_S, (byte)index);
                    else
                        return new CodeInstruction(OpCodes.Stloc, (short)index);
            }
        }

        public override bool Equals(object obj) => obj is LocalVar other && LocalIndex == other.LocalIndex;

        public bool Equals(LocalVar other) => LocalIndex == other.LocalIndex;

        public static bool operator ==(LocalVar lhs, LocalVar rhs) => lhs.LocalIndex == rhs.LocalIndex;

        public static bool operator !=(LocalVar lhs, LocalVar rhs) => lhs.LocalIndex != rhs.LocalIndex;

        public override int GetHashCode() => LocalIndex;

        public override string ToString() => local.ToString();
    }
}
