using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(BulkheadDoor))]
    [HarmonyPatch(nameof(BulkheadDoor.OnHandHover))]
    public static class BulkheadLaserCutterNotice {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = [];
            try { /*
            int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "HandReticle", "SetInteractText", true, new Type[]{typeof(string)});
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
            codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getBulkheadMouseoverText", false, typeof(string), typeof(BulkheadDoor)));
            FileLog.Log("Codes are "+InstructionHandlers.toString(codes));*/
                codes.Add(OpCodes.Ldarg_0);
                codes.Invoke("ReikaKalseki.DIAlterra.DIHooks", nameof(DIHooks.GetBulkheadMouseoverText), false, typeof(BulkheadDoor));
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