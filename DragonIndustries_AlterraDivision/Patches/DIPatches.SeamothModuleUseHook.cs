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
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchInitialHook(InjectModuleHook());
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
                InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.LogErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }

        private static CodeInstruction[] InjectModuleHook() {
            InsnList codes = [];
            codes.Add(OpCodes.Ldarg_0);
            codes.Add(OpCodes.Ldarg_1);
            codes.Add(OpCodes.Ldarg_2);
            codes.Invoke(
                "ReikaKalseki.DIAlterra.DIHooks",
                nameof(DIHooks.UseSeamothModule),
                false,
                typeof(SeaMoth),
                typeof(TechType),
                typeof(int)
            );
            return codes.ToArray();
        }
    }
}