using System;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public static partial class AEPatches {
    private static class PatchLib {
        internal static void addPowerGenHook(string caller, InsnList codes) {
            var idx = InstructionHandlers.getInstruction(
                codes,
                0,
                0,
                OpCodes.Call,
                caller,
                "ProducePower",
                true,
                new Type[] { typeof(float) }
            );
            codes.InsertRange(
                idx + 1,
                new InsnList {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.AqueousEngineering.AEHooks",
                        "getReactorGeneration",
                        false,
                        typeof(float),
                        typeof(MonoBehaviour)
                    )
                }
            );
        }
    }
}