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
    [HarmonyPatch(typeof(Current))]
    [HarmonyPatch(nameof(Current.Update))]
    public static class CurrentTick {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Callvirt,
                    "UnityEngine.Rigidbody",
                    "AddForce",
                    true,
                    new Type[] { typeof(Vector3), typeof(ForceMode) }
                );
                codes[idx].operand = InstructionHandlers.ConvertMethodOperand(
                    "ReikaKalseki.Ecocean.ECHooks",
                    nameof(ECHooks.ApplyCurrentForce),
                    false,
                    typeof(Rigidbody),
                    typeof(Vector3),
                    typeof(ForceMode),
                    typeof(Current)
                );
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
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