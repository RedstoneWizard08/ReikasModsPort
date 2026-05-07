using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Exscansion;

internal static partial class ESPatches {
    [HarmonyPatch(typeof(ResourceTracker))]
    [HarmonyPatch(nameof(ResourceTracker.Start))]
    public static class ScannerFilteringHook2 {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try { /*
            codes.add(OpCodes.Ldarg_0);
            codes.invoke("ReikaKalseki.Exscansion.ESHooks", "registerResourceTracker", false, typeof(ResourceTracker));
            codes.add(OpCodes.Ret);*/
                codes.PatchInitialHook(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.Exscansion.ESHooks",
                        nameof(ESHooks.InitializeResourceTracker),
                        false,
                        typeof(ResourceTracker)
                    )
                );
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