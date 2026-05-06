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
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                int idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Stfld,
                    "FiltrationMachine",
                    "timeRemainingSalt"
                );
                idx = InstructionHandlers.getLastOpcodeBefore(codes, idx, OpCodes.Ldloc_S);
                codes.InsertRange(
                    idx + 1,
                    new InsnList {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        InstructionHandlers.createMethodCall(
                            "ReikaKalseki.Ecocean.ECHooks",
                            nameof(ECHooks.GetWaterFilterSaltTickTime),
                            false,
                            typeof(float),
                            typeof(FiltrationMachine)
                        )
                    }
                );
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