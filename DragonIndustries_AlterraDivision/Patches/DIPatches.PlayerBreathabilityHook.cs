using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch(nameof(Player.CanBreathe))]
    public static class PlayerBreathabilityHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchEveryReturnPre(InjectCallback);
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

        private static void InjectCallback(InsnList codes, int idx) {
            codes.Insert(
                idx,
                InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.CanPlayerBreathe),
                    false,
                    typeof(bool),
                    typeof(Player)
                )
            );
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
        }
    }
}