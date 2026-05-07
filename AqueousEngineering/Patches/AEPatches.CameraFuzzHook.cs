using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering;

public static partial class AEPatches {
    [HarmonyPatch(typeof(uGUI_CameraDrone))]
    [HarmonyPatch(nameof(uGUI_CameraDrone.LateUpdate))]
    public static class CameraFuzzHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                for (var i = 0; i < codes.Count; i++) {
                    var ci = codes[i];
                    if (ci.opcode == OpCodes.Callvirt) {
                        var mi = (MethodInfo)ci.operand;
                        if (mi.Name == "GetScreenDistance") {
                            ci.operand = InstructionHandlers.ConvertMethodOperand(
                                "ReikaKalseki.AqueousEngineering.AEHooks",
                                nameof(AEHooks.GetCameraDistanceForRenderFX),
                                false,
                                typeof(MapRoomCamera),
                                typeof(MapRoomScreen)
                            );
                        }
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