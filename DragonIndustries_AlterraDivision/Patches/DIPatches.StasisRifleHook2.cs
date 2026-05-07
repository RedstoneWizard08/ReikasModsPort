using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(StasisSphere))]
    [HarmonyPatch(nameof(StasisSphere.UnfreezeAll))]
    public static class StasisRifleHook2 {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Call,
                    "StasisSphere",
                    "Unfreeze",
                    true,
                    new Type[] { typeof(Rigidbody) }
                );
                codes[idx] = InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.OnStasisUnfreeze),
                    false,
                    typeof(StasisSphere),
                    typeof(Rigidbody)
                );
                InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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