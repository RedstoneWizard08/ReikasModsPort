using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(GrowingPlant))]
    [HarmonyPatch("SpawnGrownModel")]
    public static class PlantFinishedGrowingHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.patchEveryReturnPre(injectCallback);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }

        private static void injectCallback(InsnList codes, int idx) {
            codes.Insert(
                idx,
                InstructionHandlers.createMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    "onFarmedPlantGrowDone",
                    false,
                    typeof(GrowingPlant),
                    typeof(GameObject)
                )
            );
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_0));
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
        }
    }
}