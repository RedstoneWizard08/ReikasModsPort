using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    // TODO: I think this was relocated to AlterMaxSpeed() and was changed a ton
    // [HarmonyPatch(typeof(UnderwaterMotor))]
    // [HarmonyPatch(nameof(UnderwaterMotor.UpdateMove))]
    // public static class AffectSeaglideSpeed {
    //     private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    //         InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
    //         var codes = new InsnList(instructions);
    //         try {
    //             var idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 1.45F);
    //             codes.Insert(
    //                 idx + 1,
    //                 InstructionHandlers.createMethodCall(
    //                     "ReikaKalseki.SeaToSea.C2CHooks",
    //                     nameof(C2CHooks.GetSeaglideSpeed),
    //                     false,
    //                     typeof(float)
    //                 )
    //             );
    //             //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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