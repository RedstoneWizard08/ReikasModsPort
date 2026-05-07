using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Reefbalance;

internal static partial class RBPatches {
    [HarmonyPatch(typeof(DataboxSpawner))]
    [HarmonyPatch(nameof(DataboxSpawner.Start))]
    public static class DataboxDuplicateRemovalHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Call,
                    "KnownTech",
                    "Contains",
                    false,
                    new Type[] { typeof(TechType) }
                );
                codes[idx] = InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.Reefbalance.ReefbalanceMod",
                    nameof(ReefbalanceMod.DeleteDuplicateDatabox),
                    false,
                    typeof(TechType)
                );
                InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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