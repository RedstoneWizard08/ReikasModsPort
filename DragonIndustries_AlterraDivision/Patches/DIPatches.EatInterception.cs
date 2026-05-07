using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.ExecuteItemAction), [typeof(ItemAction), typeof(InventoryItem)])]
    public static class EatInterception {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Callvirt,
                    "Survival",
                    "Eat",
                    true,
                    new[] { typeof(GameObject) }
                );
                
                codes[idx].operand = InstructionHandlers.ConvertMethodOperand(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.TryEat),
                    false,
                    typeof(Survival),
                    typeof(GameObject)
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