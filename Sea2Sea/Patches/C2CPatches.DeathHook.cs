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
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getLastOpcodeBefore(codes, codes.Count, OpCodes.Ldstr);
                codes[idx] = InstructionHandlers.createMethodCall(
                    "ReikaKalseki.SeaToSea.C2CHooks",
                    nameof(C2CHooks.OnDeath),
                    false,
                    new Type[0]
                );
                codes.RemoveAt(idx + 1); //remove call StartCoroutine
                codes.RemoveAt(idx + 1); //remove pop
                codes.RemoveAt(idx - 1); //remove ldarg0
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
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