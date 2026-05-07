using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(ExosuitClawArm))]
    [HarmonyPatch(nameof(ExosuitClawArm.OnPickupAsync), MethodType.Enumerator)]
    public static class OnPrawnPickup {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.GetInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Callvirt,
                    "ItemsContainer",
                    "UnsafeAdd",
                    true,
                    new Type[] { typeof(InventoryItem) }
                );
                codes.InsertRange(
                    idx + 1,
                    new InsnList {
                        new CodeInstruction(OpCodes.Ldloc_1),
                        InstructionHandlers.CreateMethodCall(
                            "ReikaKalseki.DIAlterra.DIHooks",
                            nameof(DIHooks.OnPrawnItemPickedUp),
                            false,
                            typeof(Pickupable)
                        ),
                    }
                );
                InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.LogErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }
    }
}