using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(UnderwaterMotor))]
    [HarmonyPatch("UpdateMove")]
    public static class AffectSwimSpeed {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try { /*
            int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "Inventory", "GetHeldTool", true, new Type[0]);
            while (codes[idx].opcode != OpCodes.Ldloc_1)
                idx--;
            codes.Insert(idx, new CodeInstruction(OpCodes.Stloc_0));
            codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getSwimSpeed", false, typeof(float)));
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_0));
            */
                var idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stloc_S, 9);
                codes.Insert(
                    idx,
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.DIAlterra.DIHooks",
                        "getSwimSpeed",
                        false,
                        typeof(float)
                    )
                );
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