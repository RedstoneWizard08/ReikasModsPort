using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering;

public static partial class AEPatches {
    [HarmonyPatch(typeof(FiltrationMachine))]
    [HarmonyPatch(nameof(FiltrationMachine.UpdateFiltering))]
    public static class WaterFilterPowerCostHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                for (var i = codes.Count - 1; i >= 0; i--) {
                    if (codes[i].LoadsConstant(0.85F)) {
                        codes.InsertRange(
                            i + 1,
                            new InsnList {
                                new CodeInstruction(OpCodes.Ldarg_0),
                                InstructionHandlers.CreateMethodCall(
                                    "ReikaKalseki.AqueousEngineering.AEHooks",
                                    nameof(AEHooks.GetWaterFilterPowerCost),
                                    false,
                                    typeof(float),
                                    typeof(FiltrationMachine)
                                ),
                            }
                        );
                    }
                }

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