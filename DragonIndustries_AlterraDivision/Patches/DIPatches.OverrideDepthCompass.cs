using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch(nameof(uGUI_DepthCompass.UpdateDepth))]
    public static class OverrideDepthCompass {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try { /*
            for (int i = codes.Count-1; i >= 0; i--) {
                if (codes[i].opcode == OpCodes.Call) {
                    MethodInfo mi = (MethodInfo)codes[i].operand;
                    if (mi.Name == "FloorToInt") {
                        codes.Insert(i, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getCompassDepth", false, typeof(float)));
                    }
                }
            }*/
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Call,
                    "uGUI_DepthCompass",
                    "GetDepthInfo",
                    true,
                    new Type[] { typeof(int).MakeByRefType(), typeof(int).MakeByRefType() }
                );
                /*
                InsnList li = new InsnList();
                li.add(OpCodes.Ldarg_0);
                li.add(OpCodes.Ldloc_S, 0);
                li.invoke("ReikaKalseki.DIAlterra.DIHooks", "getCompassDepth", false, typeof(uGUI_DepthCompass), typeof(int).MakeByRefType());
                //li.add(OpCodes.Stloc_S, 0);
                codes.InsertRange(idx+2, li);*/

                codes[idx].operand = InstructionHandlers.ConvertMethodOperand(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.GetCompassDepth),
                    false,
                    typeof(uGUI_DepthCompass),
                    typeof(int).MakeByRefType(),
                    typeof(int).MakeByRefType()
                );
                InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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