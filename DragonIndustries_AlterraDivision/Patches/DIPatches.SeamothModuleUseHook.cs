using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch(nameof(SeaMoth.OnUpgradeModuleUse))]
    public static class SeamothModuleUseHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.patchInitialHook(injectModuleHook());
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }

        private static CodeInstruction[] injectModuleHook() {
            InsnList codes = [];
            codes.add(OpCodes.Ldarg_0);
            codes.add(OpCodes.Ldarg_1);
            codes.add(OpCodes.Ldarg_2);
            codes.invoke(
                "ReikaKalseki.DIAlterra.DIHooks",
                "useSeamothModule",
                false,
                typeof(SeaMoth),
                typeof(TechType),
                typeof(int)
            );
            return codes.ToArray();
        }
    }
}