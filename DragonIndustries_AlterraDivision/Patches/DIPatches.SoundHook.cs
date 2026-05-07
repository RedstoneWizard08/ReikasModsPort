using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(FMOD_CustomEmitter))]
    [HarmonyPatch(nameof(FMOD_CustomEmitter.OnPlay))]
    public static class SoundHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
                //int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "FMOD_CustomEmitter", "OnPlay", true, new Type[0]);
                var ci = InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.OnFModEmitterPlay),
                    false,
                    typeof(FMOD_CustomEmitter)
                );
                codes.PatchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), ci);
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