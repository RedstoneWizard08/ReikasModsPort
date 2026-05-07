using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(ExosuitClawArm))]
    [HarmonyPatch(nameof(ExosuitClawArm.TryUse))]
    public static class PrawnGrabDelayRemoval {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                for (var i = codes.Count - 1; i >= 0; i--) {
                    var ci = codes[i];
                    if (ci.opcode == OpCodes.Ldstr && (string)ci.operand == "use_tool") {
                        codes.InsertRange(
                            i + 2,
                            new InsnList {
                                new CodeInstruction(OpCodes.Ldarg_0),
                                InstructionHandlers.CreateMethodCall("ExosuitClawArm", "OnPickup", true, new Type[0]),
                            }
                        );
                    }
                }

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