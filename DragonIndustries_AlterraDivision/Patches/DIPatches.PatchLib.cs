using System;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    private static class PatchLib {
        internal static void addEquipmentAllowedHook(InsnList codes, params CodeInstruction[] getItem) {
            var idx = InstructionHandlers.getInstruction(
                codes,
                0,
                0,
                OpCodes.Call,
                "Equipment",
                "IsCompatible",
                false,
                new Type[] { typeof(EquipmentType), typeof(EquipmentType) }
            );
            codes[idx] = InstructionHandlers.createMethodCall(
                "ReikaKalseki.DIAlterra.DIHooks",
                "isEquipmentApplicable",
                false,
                typeof(EquipmentType),
                typeof(EquipmentType),
                typeof(Equipment),
                typeof(Pickupable)
            );
            codes.InsertRange(idx, getItem);
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
        }

        internal static void patchPropulsability(InsnList codes, int idx, bool mass, CodeInstruction go = null) {
            InsnList add = [go == null ? new CodeInstruction(OpCodes.Ldarg_1) : go];
            add.add(OpCodes.Ldarg_0);
            add.add(mass ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            add.invoke(
                "ReikaKalseki.DIAlterra.DIHooks",
                "getMaxPropulsible",
                false,
                typeof(float),
                typeof(GameObject),
                typeof(MonoBehaviour),
                typeof(bool)
            );
            codes.InsertRange(idx + 1, add);
        }

        internal static void patchVisualItemSize(InsnList codes, bool useSelfContainer = false) {
            patchVisualItemSize(
                codes,
                useSelfContainer,
                true,
                useSelfContainer
                    ? [typeof(TechType), typeof(InventoryItem), typeof(IItemsContainer)]
                    : [typeof(TechType), typeof(InventoryItem)]
            );
        }

        internal static void patchVisualItemSize(
            InsnList codes,
            bool ldSelf = false,
            bool ldArg1 = true,
            params Type[] args
        ) {
            for (var i = codes.Count - 1; i >= 0; i--) {
                if (codes[i].opcode == OpCodes.Call) {
                    var m = (MethodInfo)codes[i].operand;
                    if (m != null && m.DeclaringType.Name == "CraftData" && m.Name == "GetItemSize") {
                        var call = InstructionHandlers.convertMethodOperand(
                            "ReikaKalseki.DIAlterra.DIHooks",
                            "getItemDisplaySize",
                            false,
                            args
                        );
                        codes[i].operand = call;
                        if (ldSelf)
                            codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        if (ldArg1)
                            codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
                    }
                }
            }
            //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
            /*
            int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "CraftData", "GetItemSize", false, new Type[]{typeof(TechType)});
            codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "getItemDisplaySize", false, typeof(TechType), typeof(InventoryItem));
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));*/
        }

        internal static void redirectPowerHook(InsnList codes) {
            var idx = InstructionHandlers.getInstruction(
                codes,
                0,
                0,
                OpCodes.Call,
                "PowerSystem",
                "AddEnergy",
                false,
                new Type[] { typeof(IPowerInterface), typeof(float), typeof(float).MakeByRefType() }
            );
            codes[idx].operand = InstructionHandlers.convertMethodOperand(
                "ReikaKalseki.DIAlterra.DIHooks",
                "addPowerToSeabaseDelegate",
                false,
                typeof(IPowerInterface),
                typeof(float),
                typeof(float).MakeByRefType(),
                typeof(MonoBehaviour)
            );
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
        }

        internal static void injectEMPHook(InsnList codes, int idx) {
            var arg = codes[idx - 3]; //-1 is getfield time, -2 is loadarg0 to get that field
            idx -= 4;
            codes.Insert(
                idx + 1,
                InstructionHandlers.createMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    "onEMPHit",
                    false,
                    typeof(EMPBlast),
                    typeof(MonoBehaviour)
                )
            );
            codes.Insert(idx + 1, new CodeInstruction(arg.opcode, arg.operand));
            codes.Insert(idx + 1, new CodeInstruction(OpCodes.Ldarg_0));
        }

        internal static void injectTickHook(InsnList codes, string name, Type arg) {
            codes.patchInitialHook(
                new CodeInstruction(OpCodes.Ldarg_0),
                InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", name, false, arg)
            );
        }
    }
}