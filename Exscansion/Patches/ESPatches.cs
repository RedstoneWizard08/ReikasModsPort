//Working with Lists and Collections
//Working with Lists and Collections
//For data read/write methods
//More advanced manipulation of lists/collections

//Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.Exscansion;

internal static partial class ESPatches {
    // TODO
    // [HarmonyPatch(typeof(MapRoomFunctionality))]
    // [HarmonyPatch(nameof(MapRoomFunctionality.GetScanInterval))]
    // public static class ScannerSpeedPatch {
    //     static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    //         InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
    //         InsnList codes = new InsnList(instructions);
    //         try {
    //             RangePatchLib.replaceBaseSpeedReference(codes);
    //             RangePatchLib.replaceSpeedBonusReference(codes);
    //             InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
    //         } catch (Exception e) {
    //             InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
    //             FileLog.Log(e.Message);
    //             FileLog.Log(e.StackTrace);
    //             FileLog.Log(e.ToString());
    //         }
    //
    //         return codes.AsEnumerable();
    //     }
    // }

    // TODO
    // [HarmonyPatch(typeof(uGUI_ResourceTracker))]
    // [HarmonyPatch(nameof(uGUI_ResourceTracker.UpdateVisibility))]
    // public static class PingHUDVisibility {
    //     static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    //         InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
    //         InsnList codes = new InsnList(instructions);
    //         try {
    //             CodeInstruction call = InstructionHandlers.createMethodCall(
    //                 "ReikaKalseki.Exscansion.ESHooks",
    //                 "updatePingHUDVisibility",
    //                 false,
    //                 typeof(uGUI_ResourceTracker)
    //             );
    //             codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_0), call);
    //             InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
    //         } catch (Exception e) {
    //             InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
    //             FileLog.Log(e.Message);
    //             FileLog.Log(e.StackTrace);
    //             FileLog.Log(e.ToString());
    //         }
    //
    //         return codes.AsEnumerable();
    //     }
    // }
}