using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(Vehicle))]
    [HarmonyPatch(nameof(Vehicle.GetStorageInSlot))]
    public static class SeamothStorageAccessHooks {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = [];
            try {
                codes.Add(OpCodes.Ldarg_0);
                codes.Add(OpCodes.Ldarg_1);
                codes.Add(OpCodes.Ldarg_2);
                codes.Invoke(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.GetVehicleStorageInSlot),
                    false,
                    typeof(Vehicle),
                    typeof(int),
                    typeof(TechType)
                );
                codes.Add(OpCodes.Ret);
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