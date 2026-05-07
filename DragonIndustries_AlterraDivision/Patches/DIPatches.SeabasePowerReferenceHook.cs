using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(PowerRelay))]
    [HarmonyPatch(nameof(PowerRelay.AddInboundPower))]
    public static class SeabasePowerReferenceHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchEveryReturnPre(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.DIAlterra.DIHooks",
                        nameof(DIHooks.LinkPowerRelayToBase),
                        false,
                        typeof(PowerRelay),
                        typeof(IPowerInterface)
                    )
                );
                FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
            } catch (Exception e) {
                FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }
    }
}