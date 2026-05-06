using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Reefbalance;

internal static partial class RBPatches {
    [HarmonyPatch(typeof(Eatable))]
    [HarmonyPatch("GetFoodValue")]
    public static class DecayRateAndTimePatch {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try { /*
            int sub = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Sub);
            InsnList inject = new InsnList();
            inject.add(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand("ReikaKalseki.Reefbalance.ReefbalanceMod", "onRoomFindMachine"));
            codes.InsertRange(sub+1, inject);
            */
                codes.Clear();
                codes.add(OpCodes.Ldarg_0);
                codes.add(OpCodes.Ldarg_0);
                codes.add(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand("Eatable", "foodValue"));
                codes.invoke(
                    "ReikaKalseki.Reefbalance.ReefbalanceMod",
                    "getFoodValue",
                    false,
                    typeof(Eatable),
                    typeof(float)
                );
                codes.add(OpCodes.Ret);
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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