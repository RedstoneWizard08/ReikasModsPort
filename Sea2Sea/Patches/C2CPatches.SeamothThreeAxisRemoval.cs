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
    [HarmonyPatch(typeof(Vehicle))]
    [HarmonyPatch(nameof(Vehicle.ApplyPhysicsMove))]
    public static class SeamothThreeAxisRemoval {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Callvirt,
                    "UnityEngine.Transform",
                    "get_rotation",
                    true,
                    new Type[0]
                );
                //idx = InstructionHandlers.getLastOpcodeBefore(codes, idx, OpCodes.Stloc_2);
                idx = codes.GetInstruction(idx, 0, OpCodes.Ldloc_2) + 1;
                InsnList li = [];
                li.Add(OpCodes.Ldarg_0);
                li.Add(OpCodes.Ldloc_1);
                li.Invoke(
                    "ReikaKalseki.SeaToSea.C2CHooks",
                    nameof(C2CHooks.Get3AxisSpeed),
                    false,
                    typeof(float),
                    typeof(Vehicle),
                    typeof(Vector3)
                );
                codes.InsertRange(idx, li);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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