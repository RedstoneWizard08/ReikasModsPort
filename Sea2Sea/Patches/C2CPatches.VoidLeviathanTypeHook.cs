using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(VoidGhostLeviathansSpawner))]
    [HarmonyPatch(nameof(VoidGhostLeviathansSpawner.UpdateSpawn))]
    public static class VoidLeviathanTypeHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Ldfld,
                    "VoidGhostLeviathansSpawner",
                    "ghostLeviathanPrefab"
                ) - 1;
                while (!(codes[idx].opcode == OpCodes.Call && ((MethodInfo)codes[idx].operand).Name == "Instantiate"))
                    codes.RemoveAt(idx);
                codes[idx] = InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.SeaToSea.C2CHooks",
                    nameof(C2CHooks.GetVoidLeviathan),
                    false,
                    typeof(VoidGhostLeviathansSpawner),
                    typeof(Vector3)
                );
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_2));
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
                //FileLog.Log("levitype Codes are "+InstructionHandlers.toString(codes));
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