using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(CollectShiny))]
    [HarmonyPatch(nameof(CollectShiny.UpdateShinyTarget))]
    public static class StalkerPlatinumSeekingHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                for (var i = codes.Count - 1; i >= 0; i--) {
                    var ci = codes[i];
                    if (ci.opcode == OpCodes.Stfld && ((FieldInfo)ci.operand).Name == "shinyTarget") {
                        codes.Insert(
                            i,
                            InstructionHandlers.CreateMethodCall(
                                "ReikaKalseki.SeaToSea.C2CHooks",
                                nameof(C2CHooks.GetStalkerShinyTarget),
                                false,
                                typeof(GameObject),
                                typeof(CollectShiny)
                            )
                        );
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
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