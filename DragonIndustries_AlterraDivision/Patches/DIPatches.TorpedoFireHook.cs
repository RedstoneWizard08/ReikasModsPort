using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(Vehicle))]
    [HarmonyPatch(nameof(Vehicle.TorpedoShot))]
    public static class TorpedoFireHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Callvirt,
                    "Bullet",
                    "Shoot",
                    true,
                    new Type[] { typeof(Vector3), typeof(Quaternion), typeof(float), typeof(float) }
                );
                codes[idx] = InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.DoShootTorpedo),
                    false,
                    new Type[] {
                        typeof(Bullet), typeof(Vector3), typeof(Quaternion), typeof(float), typeof(float),
                        typeof(Vehicle),
                    }
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