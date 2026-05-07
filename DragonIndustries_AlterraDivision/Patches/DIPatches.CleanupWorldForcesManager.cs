using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(WorldForcesManager))]
    [HarmonyPatch(nameof(WorldForcesManager.FixedUpdate))]
    public static class CleanupWorldForcesManager {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Callvirt,
                    "WorldForces",
                    "DoFixedUpdate",
                    true,
                    new Type[0]
                );
                codes[idx] = InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.TickWorldForces),
                    false,
                    typeof(WorldForces)
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