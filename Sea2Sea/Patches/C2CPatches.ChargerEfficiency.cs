using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(Charger))]
    [HarmonyPatch(nameof(Charger.Update))]
    public static class ChargerEfficiency {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Call,
                    "PowerSystem",
                    "ConsumeEnergy",
                    false,
                    new Type[] {
                        typeof(IPowerInterface),
                        typeof(float),
                        typeof(float).MakeByRefType(),
                    }
                );
                codes[idx].operand = InstructionHandlers.ConvertMethodOperand(
                    "ReikaKalseki.SeaToSea.C2CHooks",
                    nameof(C2CHooks.ChargerConsumeEnergy),
                    false,
                    typeof(IPowerInterface),
                    typeof(float),
                    typeof(float).MakeByRefType(),
                    typeof(Charger)
                );
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
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