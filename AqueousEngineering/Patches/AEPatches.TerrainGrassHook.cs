using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering;

public static partial class AEPatches {
    [HarmonyPatch(typeof(VoxelandGrassBuilder))]
    [HarmonyPatch(nameof(VoxelandGrassBuilder.CreateUnityMeshes))]
    public static class TerrainGrassHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchEveryReturnPre(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.AqueousEngineering.AEHooks",
                        nameof(AEHooks.OnChunkGenGrass),
                        false,
                        typeof(IVoxelandChunk2)
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