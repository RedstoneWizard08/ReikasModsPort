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
    [HarmonyPatch("UpdateSpawn")]
    public static class VoidLeviathanTypeHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Ldfld,
                    "VoidGhostLeviathansSpawner",
                    "ghostLeviathanPrefab"
                ) - 1;
                while (!(codes[idx].opcode == OpCodes.Call && ((MethodInfo)codes[idx].operand).Name == "Instantiate"))
                    codes.RemoveAt(idx);
                codes[idx] = InstructionHandlers.createMethodCall(
                    "ReikaKalseki.SeaToSea.C2CHooks",
                    "getVoidLeviathan",
                    false,
                    typeof(VoidGhostLeviathansSpawner),
                    typeof(Vector3)
                );
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_2));
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
                //FileLog.Log("levitype Codes are "+InstructionHandlers.toString(codes));
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
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