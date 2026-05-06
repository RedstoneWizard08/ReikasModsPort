using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(KeypadDoorConsole))]
    [HarmonyPatch("NumberButtonPress")]
    public static class OnKeypadButtonPress {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldstr, "ResetNumberField");
                idx = InstructionHandlers.getFirstOpcode(codes, idx, OpCodes.Call);
                codes.InsertRange(
                    idx + 1,
                    new InsnList {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        InstructionHandlers.createMethodCall(
                            "ReikaKalseki.SeaToSea.C2CHooks",
                            "onKeypadFailed",
                            false,
                            typeof(KeypadDoorConsole)
                        ),
                    }
                );
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