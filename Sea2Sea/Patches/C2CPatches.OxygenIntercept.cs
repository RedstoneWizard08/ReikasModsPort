using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(OxygenManager))]
    [HarmonyPatch(nameof(OxygenManager.AddOxygen))]
    public static class OxygenIntercept {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchInitialHook(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.SeaToSea.C2CHooks",
                        nameof(C2CHooks.AddO2ToPlayer),
                        false,
                        typeof(OxygenManager),
                        typeof(float)
                    ),
                    new CodeInstruction(OpCodes.Starg_S, 1)
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