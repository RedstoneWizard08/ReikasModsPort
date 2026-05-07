using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    // TODO: Fix this too, I think harmony doesn't like enumerators
    // [HarmonyPatch(typeof(WaterParkCreature))]
    // [HarmonyPatch(nameof(WaterParkCreature.BornAsync), MethodType.Enumerator)]
    // public static class WaterParkFix {
    //     private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    //         InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
    //         var codes = new InsnList(instructions);
    //         try {
    //             var idx = InstructionHandlers.GetInstruction(
    //                 codes,
    //                 0,
    //                 0,
    //                 OpCodes.Callvirt,
    //                 "UnityEngine.GameObject",
    //                 "SetActive",
    //                 true,
    //                 new Type[] { typeof(bool) }
    //             );
    //             codes.Insert(
    //                 idx + 1,
    //                 InstructionHandlers.CreateMethodCall(
    //                     "ReikaKalseki.DIAlterra.DIHooks",
    //                     nameof(DIHooks.OnEggHatched),
    //                     false,
    //                     typeof(GameObject)
    //                 )
    //             );
    //             codes.Insert(idx + 1, new CodeInstruction(OpCodes.Ldloc_0));
    //             InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
    //         } catch (Exception e) {
    //             InstructionHandlers.LogErroredPatch(MethodBase.GetCurrentMethod());
    //             FileLog.Log(e.Message);
    //             FileLog.Log(e.StackTrace);
    //             FileLog.Log(e.ToString());
    //         }
    //
    //         return codes.AsEnumerable();
    //     }
    // }
}