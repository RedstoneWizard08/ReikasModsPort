using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(Survival))]
    [HarmonyPatch(nameof(Survival.OnRespawn))]
    public static class RespawnHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchInitialHook(
                    new InsnList() {
                        new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1),
                        InstructionHandlers.CreateMethodCall(
                            "ReikaKalseki.DIAlterra.DIHooks",
                            nameof(DIHooks.OnRespawnPre),
                            false,
                            typeof(Survival),
                            typeof(Player)
                        ),
                    }
                );
                codes.PatchEveryReturnPre(
                    new InsnList() {
                        new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1),
                        InstructionHandlers.CreateMethodCall(
                            "ReikaKalseki.DIAlterra.DIHooks",
                            nameof(DIHooks.OnRespawnPost),
                            false,
                            typeof(Survival),
                            typeof(Player)
                        ),
                    }
                );
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