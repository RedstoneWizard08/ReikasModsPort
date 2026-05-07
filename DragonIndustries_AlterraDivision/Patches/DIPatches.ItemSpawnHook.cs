using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    // TODO: Fix harmony shitting itself with enumerators
    // [HarmonyPatch(typeof(InventoryConsoleCommands))]
    // [HarmonyPatch(nameof(InventoryConsoleCommands.ItemCmdSpawnAsync), MethodType.Enumerator)]
    // public static class ItemSpawnHook {
    //     private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    //         InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
    //         var codes = new InsnList(instructions);
    //         try {
    //             var idx = codes.GetInstruction(
    //                 0,
    //                 0,
    //                 OpCodes.Call,
    //                 "CraftData",
    //                 "InstantiateFromPrefabAsync",
    //                 false,
    //                 new Type[] { typeof(TechType), typeof(bool) }
    //             );
    //             codes[idx] = InstructionHandlers.CreateMethodCall(
    //                 "ReikaKalseki.DIAlterra.DIHooks",
    //                 nameof(DIHooks.CreateSpawnedItem),
    //                 false,
    //                 typeof(TechType),
    //                 typeof(bool)
    //             );
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