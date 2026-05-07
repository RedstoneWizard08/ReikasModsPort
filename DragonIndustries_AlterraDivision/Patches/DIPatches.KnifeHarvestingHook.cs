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
    public static class KnifeHarvestingHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Call,
                    "Knife",
                    "GiveResourceOnDamage",
                    false,
                    new Type[] { typeof(GameObject), typeof(bool), typeof(bool) }
                );
                codes[idx].operand = InstructionHandlers.ConvertMethodOperand(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.DoKnifeHarvest),
                    false,
                    typeof(Knife),
                    typeof(GameObject),
                    typeof(bool),
                    typeof(bool)
                );
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