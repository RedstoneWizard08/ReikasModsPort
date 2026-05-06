using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(uGUI_OxygenBar))]
    [HarmonyPatch("LateUpdate")]
    public static class O2BarTick {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.patchInitialHook(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.SeaToSea.C2CHooks",
                        "tickO2Bar",
                        false,
                        typeof(uGUI_OxygenBar)
                    )
                );
                var idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stloc_S, 4);
                codes.Insert(
                    idx,
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.SeaToSea.C2CHooks",
                        "getO2RedPulseTime",
                        false,
                        typeof(float)
                    )
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