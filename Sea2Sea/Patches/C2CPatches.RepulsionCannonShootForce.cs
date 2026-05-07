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
    [HarmonyPatch(typeof(RepulsionCannon))]
    [HarmonyPatch(nameof(RepulsionCannon.OnToolUseAnim))]
    public static class RepulsionCannonShootForce {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(0, 0, OpCodes.Ldc_R4, 70F);
                codes[idx] = InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.SeaToSea.C2CHooks",
                    nameof(C2CHooks.GetRepulsionCannonThrowForce),
                    false,
                    typeof(RepulsionCannon)
                );
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));

                idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Callvirt,
                    "UnityEngine.Rigidbody",
                    "get_mass",
                    true,
                    new Type[0]
                );
                codes.InsertRange(
                    idx + 2,
                    new InsnList {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldloc_S, 12),
                        InstructionHandlers.CreateMethodCall(
                            "ReikaKalseki.SeaToSea.C2CHooks",
                            nameof(C2CHooks.OnRepulsionCannonTryHit),
                            false,
                            typeof(RepulsionCannon),
                            typeof(Rigidbody)
                        ),
                    }
                ); //after the following add
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