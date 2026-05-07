using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(DamageSystem))]
    [HarmonyPatch(nameof(DamageSystem.CalculateDamage))]
    public static class DamageCalcHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(0, 0, OpCodes.Ret);
                codes.Insert(
                    idx,
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.DIAlterra.DIHooks",
                        nameof(DIHooks.RecalculateDamage),
                        false,
                        typeof(float),
                        typeof(DamageType),
                        typeof(GameObject),
                        typeof(GameObject)
                    )
                );
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_3));
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_2));
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
                //already present//codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
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