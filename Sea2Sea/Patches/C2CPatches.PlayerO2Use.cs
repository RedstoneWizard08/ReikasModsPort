using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("GetOxygenPerBreath")]
    public static class PlayerO2Use {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = [];
            try {
                codes.add(OpCodes.Ldarg_0);
                codes.add(OpCodes.Ldarg_1);
                codes.add(OpCodes.Ldarg_2);
                codes.invoke(
                    "ReikaKalseki.SeaToSea.C2CHooks",
                    "getPlayerO2Use",
                    false,
                    typeof(Player),
                    typeof(float),
                    typeof(int)
                );
                codes.add(OpCodes.Ret);
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }
    }
}