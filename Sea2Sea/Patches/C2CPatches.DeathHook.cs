using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch(nameof(Player.OnKill))]
    public static class DeathHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Call,
                    "Player",
                    "set_allowSaving",
                    true,
                    new[] { typeof(bool) }
                );
                codes.Insert(
                    idx + 1,
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.SeaToSea.C2CHooks",
                        nameof(C2CHooks.OnDeath),
                        false,
                        Type.EmptyTypes
                    )
                );
                codes.RemoveAt(idx + 1); //remove ldarg0
                codes.RemoveAt(idx + 1); //remove ldarg0
                codes.RemoveAt(idx + 1); //remove ldc.r4 5
                codes.RemoveAt(idx + 1); //remove call ResetPlayerOnDeath
                codes.RemoveAt(idx + 1); //remove call StartCoroutine
                codes.RemoveAt(idx + 1); //remove pop
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