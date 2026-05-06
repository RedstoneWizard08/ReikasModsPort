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
    [HarmonyPatch(nameof(StasisSphere.LateUpdate))]
    public static class StasisRifleHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Call,
                    "StasisSphere",
                    "Freeze",
                    true,
                    new Type[] { typeof(Collider), typeof(Rigidbody).MakeByRefType() }
                );
                codes[idx] = InstructionHandlers.createMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    "onStasisFreeze",
                    false,
                    typeof(StasisSphere),
                    typeof(Collider),
                    typeof(Rigidbody).MakeByRefType()
                );

                idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Call,
                    "StasisSphere",
                    "Unfreeze",
                    true,
                    new Type[] { typeof(Rigidbody) }
                );
                codes[idx] = InstructionHandlers.createMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    "onStasisUnfreeze",
                    false,
                    typeof(StasisSphere),
                    typeof(Rigidbody)
                );
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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