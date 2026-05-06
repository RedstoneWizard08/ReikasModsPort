using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(SolarPanel))]
    [HarmonyPatch("Update")]
    public static class SolarPanelPowerRedirect {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = [];
            try { /* BZ code
            int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "PowerRelay", "ModifyPower", true, new Type[]{typeof(float), typeof(float).MakeByRefType()});
            codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "addPowerToSeabaseDelegate", false, typeof(IPowerInterface), typeof(float), typeof(float).MakeByRefType(), typeof(MonoBehaviour));
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
            */
                /*
                int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "PowerSource", "power");
                codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "addPowerToSeabaseDelegateViaPowerSourceSet", false, typeof(PowerSource), typeof(float), typeof(MonoBehaviour));
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
                */
                codes.add(OpCodes.Ldarg_0);
                codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "updateSolarPanel", false, typeof(SolarPanel));
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