using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(InventoryConsoleCommands))]
    [HarmonyPatch(nameof(InventoryConsoleCommands.OnConsoleCommand_item))]
    public static class ItemSpawnHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Call,
                    "CraftData",
                    "InstantiateFromPrefab",
                    false,
                    new Type[] { typeof(TechType), typeof(bool) }
                );
                codes[idx] = InstructionHandlers.createMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    "createSpawnedItem",
                    false,
                    typeof(TechType),
                    typeof(bool)
                );
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }
    }
}