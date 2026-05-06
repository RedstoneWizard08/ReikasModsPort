using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(WaterParkCreature))]
    [HarmonyPatch(nameof(WaterParkCreature.Born))]
    public static class WaterParkFix {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Callvirt,
                    "UnityEngine.GameObject",
                    "SetActive",
                    true,
                    new Type[] { typeof(bool) }
                );
                codes.Insert(
                    idx + 1,
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.DIAlterra.DIHooks",
                        nameof(DIHooks.OnEggHatched),
                        false,
                        typeof(GameObject)
                    )
                );
                codes.Insert(idx + 1, new CodeInstruction(OpCodes.Ldloc_0));
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