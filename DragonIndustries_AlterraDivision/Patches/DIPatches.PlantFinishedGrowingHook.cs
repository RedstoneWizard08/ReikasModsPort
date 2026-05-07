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
    [HarmonyPatch(nameof(GrowingPlant.SpawnGrownModelAsync), MethodType.Enumerator)]
    public static class PlantFinishedGrowingHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchEveryReturnPre(injectCallback);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
                InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.LogErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
    
            return codes.AsEnumerable();
        }
    
        private static void injectCallback(InsnList codes, int idx) {
            codes.Insert(
                idx,
                InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.OnFarmedPlantGrowDone),
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