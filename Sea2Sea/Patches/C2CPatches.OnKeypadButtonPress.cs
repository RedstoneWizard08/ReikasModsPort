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
    [HarmonyPatch(nameof(KeypadDoorConsole.NumberButtonPress))]
    public static class OnKeypadButtonPress {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(0, 0, OpCodes.Ldstr, "ResetNumberField");
                idx = InstructionHandlers.GetFirstOpcode(codes, idx, OpCodes.Call);
                codes.InsertRange(
                    idx + 1,
                    new InsnList {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        InstructionHandlers.CreateMethodCall(
                            "ReikaKalseki.SeaToSea.C2CHooks",
                            nameof(C2CHooks.OnKeypadFailed),
                            false,
                            typeof(KeypadDoorConsole)
                        ),
                    }
                );
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