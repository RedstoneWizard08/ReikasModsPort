using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

internal static partial class ECPatches {
    [HarmonyPatch(typeof(MeleeAttack))]
    [HarmonyPatch(nameof(MeleeAttack.GetTarget))]
    public static class MeleeBaseTargetHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList();
            try {
                codes.Add(OpCodes.Ldarg_0);
                codes.Add(OpCodes.Ldarg_1);
                codes.Invoke(
                    "ReikaKalseki.Ecocean.ECHooks",
                    nameof(ECHooks.GetMeleeTarget),
                    false,
                    typeof(MeleeAttack),
                    typeof(Collider)
                );
                codes.Add(OpCodes.Ret);
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