using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(uGUI_Pings))]
    [HarmonyPatch("OnWillRenderCanvases")]
    public static class PingVisibilityHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var refInsn = codes[InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Ldfld,
                    "PingInstance",
                    "visible"
                ) - 1];
                for (var i = codes.Count - 1; i >= 0; i--) {
                    if (codes[i].opcode == OpCodes.Callvirt) {
                        var m = (MethodInfo)codes[i].operand;
                        if (m.Name == "SetIconAlpha" && m.DeclaringType.Name == "uGUI_Ping") {
                            injectHook(codes, i, refInsn, false);
                            i -= 4;
                        } else if (m.Name == "SetTextAlpha" && m.DeclaringType.Name == "uGUI_Ping") {
                            injectHook(codes, i, refInsn, true);
                            i -= 4;
                        }
                    }
                }

                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
            } catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }

        private static void injectHook(InsnList codes, int idx, CodeInstruction refInsn, bool isText) {
            //int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "uGUI_Ping", "SetIconAlpha");
            codes[idx] = InstructionHandlers.createMethodCall(
                "ReikaKalseki.SeaToSea.C2CHooks",
                "setPingAlpha",
                false,
                typeof(uGUI_Ping),
                typeof(float),
                typeof(PingInstance),
                typeof(bool)
            );
            codes.InsertRange(
                idx,
                [
                    new(refInsn.opcode, refInsn.operand),
                    new(isText ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0),
                ]
            );
            FileLog.Log("Injected ping alpha hook (" + isText + ") @ " + idx);
        }
    }
}