using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering;

public static partial class AEPatches {
    [HarmonyPatch(typeof(Charger))]
    [HarmonyPatch(nameof(Charger.Update))]
    public static class ChargerSpeedHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                for (var i = codes.Count - 1; i >= 0; i--) {
                    if (InstructionHandlers.matchOperands(
                            codes[i].operand,
                            InstructionHandlers.convertFieldOperand("Charger", "chargeSpeed")
                        )) {
                        codes.InsertRange(
                            i + 1,
                            new InsnList {
                                new CodeInstruction(OpCodes.Ldarg_0),
                                InstructionHandlers.createMethodCall(
                                    "ReikaKalseki.AqueousEngineering.AEHooks",
                                    nameof(AEHooks.GetChargerSpeed),
                                    false,
                                    typeof(float),
                                    typeof(Charger)
                                ),
                            }
                        );
                    }
                }

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