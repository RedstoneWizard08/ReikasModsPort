using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(TimeCapsule))]
    [HarmonyPatch(nameof(TimeCapsule.Collect))]
    public static class TimeCapsuleBypassPrevention {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = [];
            try {
                codes.add(OpCodes.Ldarg_0);
                codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "collectTimeCapsule", false, typeof(TimeCapsule));
                codes.add(OpCodes.Ret);
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
    }
}