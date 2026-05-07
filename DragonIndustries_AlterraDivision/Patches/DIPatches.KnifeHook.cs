using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(Knife))]
    [HarmonyPatch(nameof(Knife.OnToolUseAnim))]
    public static class KnifeHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Call,
                    "Utils",
                    "PlayFMODAsset",
                    false,
                    new Type[] { typeof(FMODAsset), typeof(Transform), typeof(float) }
                );
                codes.Insert(
                    idx + 1,
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.DIAlterra.DIHooks",
                        nameof(DIHooks.OnKnifed),
                        false,
                        typeof(GameObject)
                    )
                );
                codes.Insert(idx + 1, new CodeInstruction(OpCodes.Ldloc_1));
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