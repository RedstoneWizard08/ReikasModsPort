using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(CrafterGhostModel))]
    [HarmonyPatch(nameof(CrafterGhostModel.SetupGhostModelAsync))]
    [HarmonyPatch(MethodType.Enumerator, typeof(TechType))]
    public static class CrafterGhostModelOverride {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchEveryReturnPre(InjectHook);
                InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.LogErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }

        private static void InjectHook(InsnList codes, int idx) {
            codes.Insert(
                idx,
                InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.SeaToSea.C2CHooks",
                    nameof(C2CHooks.GetCrafterGhostModel),
                    false,
                    typeof(GameObject),
                    typeof(TechType)
                )
            );
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
        }
    }
}