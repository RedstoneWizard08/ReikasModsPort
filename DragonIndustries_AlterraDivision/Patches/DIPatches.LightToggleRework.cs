using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(ToggleLights))]
    [HarmonyPatch(nameof(ToggleLights.CheckLightToggle))]
    public static class LightToggleRework {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                //int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "Player", "GetRightHandDown", true, new Type[0]);
                //codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "isRightHandDownForLightToggle", false, typeof(Player));
                var idx = codes.GetInstruction(0, 0, OpCodes.Ldc_R4, 0.25F);
                codes[idx].operand = -1F;
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