using System.Reflection.Emit;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static class PatchLib {
    /*
	internal static void patchCellGet(InsnList codes) {
		for (int i = 0; i < codes.Count; i++) {
			if (codes[i].opcode == OpCodes.Call) {
				MethodInfo m = (MethodInfo)codes[i].operand;
				if (m.Name == "GetCells" && m.DeclaringType.Name.EndsWith("BatchCells", StringComparison.InvariantCulture)) {
					CodeInstruction inner = codes[i+2];
					if (inner.opcode == OpCodes.Call) {
						MethodInfo mi = (MethodInfo)inner.operand;
						if (mi.Name == "Get") {
							inner.operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.SeaToSea.C2CHooks", "getEntityCellForInt3", false, typeof(Array3<EntityCell>), typeof(Int3), typeof(BatchCells));
							codes.Insert(i+2, new CodeInstruction(OpCodes.Ldarg_0));
							FileLog.Log("Patched GET at "+i);
						}
					}
					inner = codes[i+3];
					if (inner.opcode == OpCodes.Call) {
						MethodInfo mi = (MethodInfo)inner.operand;
						if (mi.Name == "Set") {
							inner.operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.SeaToSea.C2CHooks", "setEntityCellForInt3", false, typeof(Array3<EntityCell>), typeof(Int3), typeof(EntityCell), typeof(BatchCells));
							codes.Insert(i+3, new CodeInstruction(OpCodes.Ldarg_0));
							FileLog.Log("Patched SET at "+i);
						}
					}
				}
			}
		}
	}*/
    internal static void hookKeyTerminalInteractable(InsnList codes) {
        var idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PrecursorKeyTerminal", "slotted");
        codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "cannotClickKeyTerminal", false, typeof(PrecursorKeyTerminal));
    }
}