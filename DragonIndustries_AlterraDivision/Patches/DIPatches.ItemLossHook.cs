using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch(nameof(Inventory.LoseItems))]
    public static class ItemLossHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchInitialHook(
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.DIAlterra.DIHooks",
                        nameof(DIHooks.OnItemsLost),
                        false,
                        new Type[0]
                    )
                );
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));

                //this is a bugfix, they stop at second last item for some reason
                var idx = InstructionHandlers.GetFirstOpcode(codes, 0, OpCodes.Sub);
                codes.RemoveAt(idx);
                codes.RemoveAt(idx - 1);
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