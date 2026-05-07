using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Reefbalance;

internal static partial class RBPatches {
    [HarmonyPatch(typeof(SeamothStorageContainer))]
    [HarmonyPatch(nameof(SeamothStorageContainer.Init))]
    public static class SeamothStorageBoost {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try { /*
            int sub = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Sub);
            InsnList inject = new InsnList();
            inject.add(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand("ReikaKalseki.Reefbalance.ReefbalanceMod", "onRoomFindMachine"));
            codes.InsertRange(sub+1, inject);
            */
                codes.PatchInitialHook(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.Reefbalance.ReefbalanceMod",
                        nameof(ReefbalanceMod.InitializeSeamothStorage),
                        false,
                        typeof(SeamothStorageContainer)
                    )
                );
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