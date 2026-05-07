using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(Crafter))]
    [HarmonyPatch(nameof(Crafter.Craft))]
    public static class FabSpeedHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Callvirt,
                    "CrafterLogic",
                    "Craft",
                    true,
                    new Type[] { typeof(TechType), typeof(float) }
                );
                codes.InsertRange(
                    idx,
                    new InsnList {
                        new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1),
                        InstructionHandlers.CreateMethodCall(
                            "ReikaKalseki.DIAlterra.DIHooks",
                            nameof(DIHooks.GetCrafterTime),
                            false,
                            typeof(float),
                            typeof(Crafter),
                            typeof(TechType)
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