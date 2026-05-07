using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch(nameof(SeaMoth.OnUpgradeModuleChange))]
    public static class SeamothModuleHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchEveryReturnPre(InjectSmModuleHook);
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

        private static void InjectSmModuleHook(InsnList codes, int idx) {
            codes.Insert(
                idx,
                InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.UpdateSeamothModules),
                    false,
                    typeof(SeaMoth),
                    typeof(int),
                    typeof(TechType),
                    typeof(bool)
                )
            );
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_3));
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_2));
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
        }
    }
}