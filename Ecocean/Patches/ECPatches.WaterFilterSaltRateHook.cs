using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Ecocean;

internal static partial class ECPatches {
    [HarmonyPatch(typeof(FiltrationMachine))]
    [HarmonyPatch(nameof(FiltrationMachine.UpdateFiltering))]
    public static class WaterFilterSaltRateHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Stfld,
                    "FiltrationMachine",
                    "timeRemainingSalt"
                );
                idx = codes.GetLastOpcodeBefore(idx, OpCodes.Ldloc_S);
                codes.InsertRange(
                    idx + 1,
                    new InsnList {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        InstructionHandlers.CreateMethodCall(
                            "ReikaKalseki.Ecocean.ECHooks",
                            nameof(ECHooks.GetWaterFilterSaltTickTime),
                            false,
                            typeof(float),
                            typeof(FiltrationMachine)
                        )
                    }
                );
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