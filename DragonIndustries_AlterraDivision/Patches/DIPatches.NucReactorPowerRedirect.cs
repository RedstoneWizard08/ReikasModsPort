using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(BaseNuclearReactor))]
    [HarmonyPatch(nameof(BaseNuclearReactor.Update))]
    public static class NucReactorPowerRedirect {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            if (codes.Count == 1 && codes[0].opcode == OpCodes.Ret) {
                FileLog.Log("Skipping patch " + MethodBase.GetCurrentMethod().DeclaringType + ", Update() was cleared");
                return codes;
            }

            try {
                PatchLib.RedirectPowerHook(codes);
                InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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