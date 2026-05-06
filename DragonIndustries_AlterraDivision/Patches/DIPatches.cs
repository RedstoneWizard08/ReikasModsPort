//Working with Lists and Collections
//Working with Lists and Collections
//For data read/write methods
//More advanced manipulation of lists/collections
using Nautilus.Handlers;

//Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    /*
    [HarmonyPatch(typeof(uGUI_PopupNotification))]
    [HarmonyPatch("Set")]
    public static class DebugTechPopup {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onPopup", false, typeof(uGUI_PopupNotification)));
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }
    */

    /*
    [HarmonyPatch(typeof(Builder))]
    [HarmonyPatch("CheckAsSubModule")]
    public static class ConstructableBuildabilityHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Builder", "CheckTag", false, new Type[]{typeof(Collider)});
                codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.DIAlterra.DIHooks", "interceptConstructability", false, typeof(Collider));
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }*/

    /*
    [HarmonyPatch(typeof(WaterscapeVolume))]
    [HarmonyPatch("PreRender")]
    public static class WaterFogShaderHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList();
            try {
                //int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "uSkyManager", "GetLightColor", true, new Type[0]);
                //codes.InsertRange(idx+1, new InsnList{new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "interceptChosenColor", false, typeof(Color), typeof(WaterscapeVolume), typeof(Camera))});
                codes.add(OpCodes.Ldarg_0);
                codes.add(OpCodes.Ldarg_1);
                codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "interceptChosenFog", false, typeof(WaterscapeVolume), typeof(Camera));
                codes.add(OpCodes.Ret);
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }*/
    /*
    [HarmonyPatch(typeof(WaterBiomeManager))]
    [HarmonyPatch("RasterizeAtmosphereVolumes")]
    public static class WaterFogShaderHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {/*
                int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "WaterBiomeManager", "GetEmissiveTextureValue", true, new Type[]{typeof(WaterscapeVolume.Settings)});
                CodeInstruction settings = codes[idx-1];
                InsnList add = new InsnList{
                    new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_2), new CodeInstruction(settings.opcode, settings.operand),
                    InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "overrideFog", false, typeof(WaterBiomeManager), typeof(Vector3), typeof(bool), typeof(WaterscapeVolume.Settings))
                };
                codes.InsertRange(idx+2, add);*/ /*
                InsnList add = new InsnList{
                    new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_2),
                    InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onFogRasterized", false, typeof(WaterBiomeManager), typeof(Vector3), typeof(bool))
                };
                InstructionHandlers.patchInitialHook(add);
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }
    */

    /*
    [HarmonyPatch(typeof(PowerRelay))]
    [HarmonyPatch("GetMaxPower")]
    public static class SeabasePowerCapacityHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ret);
                codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getPowerRelayCapacity", false, typeof(float), typeof(PowerRelay)));
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }
    */

    /*
    [HarmonyPatch(typeof(uGUI_ItemsContainer))]
    [HarmonyPatch("OnAddItem")]
    public static class ItemVisualSizeHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                PatchLib.patchVisualItemSize(codes);
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(uGUI_ItemsContainerView))]
    [HarmonyPatch("OnAddItem")]
    public static class ItemVisualSizeHookView {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                PatchLib.patchVisualItemSize(codes);
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(ItemsContainer))]
    [HarmonyPatch("UnsafeAdd")]
    public static class ItemFunctionalSizeHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                PatchLib.patchVisualItemSize(codes, true);
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(InventoryItem))]
    [HarmonyPatch("get_height")]
    public static class InvItemHeightHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                PatchLib.patchVisualItemSize(codes, true, false, new Type[]{typeof(TechType), typeof(InventoryItem)});
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(InventoryItem))]
    [HarmonyPatch("get_width")]
    public static class InvItemWidthHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                PatchLib.patchVisualItemSize(codes, true, false, new Type[]{typeof(TechType), typeof(InventoryItem)});
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }*/

    /*
    [HarmonyPatch(typeof(PropulsionCannon))]
    [HarmonyPatch("UpdateTargetPosition")]
    public static class PropulsionGrabPositionFix3 {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "getPropulsionMoveToPoint", false, typeof(Vector3), typeof(PropulsionCannon)));
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }
    */

    /* DO NOT ENABLE, CAUSES ALL GUI PINGS TO DISAPPEAR WHEN THEIR CAUSAL GO DERENDERS/UNLOADS AT DISTANCE
    [HarmonyPatch(typeof(ResourceTracker))]
    [HarmonyPatch("OnDestroy")]
    public static class ResourceTrackerDestroyUnregisterFix {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                codes.patchInitialHook(new InsnList(){
                                                        new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ResourceTracker", "Unregister", true, new Type[0])
                });
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }*/
    /*
    [HarmonyPatch(typeof(Vehicle))]
    [HarmonyPatch("set_docked")]
    public static class DockingDebug {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "logDockingVehicle", false, typeof(Vehicle), typeof(bool)));
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }
    */

    /*
    [HarmonyPatch(typeof(Equipment))]
    [HarmonyPatch("AllowedToAdd")]
    public static class EquipmentApplicabilityHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                PatchLib.addEquipmentAllowedHook(codes, new CodeInstruction(OpCodes.Ldarg_2));
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(Equipment))]
    [HarmonyPatch("GetCompatibleSlotDefault")]
    public static class EquipmentApplicabilityHook2 {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                PatchLib.addEquipmentAllowedHook(codes);
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(Equipment))]
    [HarmonyPatch("GetFreeSlot")]
    public static class EquipmentApplicabilityHook3 {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                PatchLib.addEquipmentAllowedHook(codes);
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(Equipment))]
    [HarmonyPatch("GetSlots")]
    public static class EquipmentApplicabilityHook4 {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                PatchLib.addEquipmentAllowedHook(codes);
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }
    */

    /*
    [HarmonyPatch(typeof(PrecursorTeleporter))]
    [HarmonyPatch("BeginTeleportPlayer")]
    public static class ArchTeleportHookPre {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList();
            try {
                codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onArchTeleportPre", false, new Type[]{typeof(PrecursorTeleporter), typeof(GameObject)}));
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(PrecursorTeleporter))]
    [HarmonyPatch("BeginTeleportPlayer")]
    public static class ArchTeleportHookPre {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onArchTeleportPre", false, new Type[]{typeof(PrecursorTeleporter), typeof(GameObject)}));
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }*/

    /*
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("GetOxygenPerBreath")]
    public static class PlayerO2Use {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList();
            try {
                codes.add(OpCodes.Ldarg_0);
                codes.add(OpCodes.Ldarg_1);
                codes.add(OpCodes.Ldarg_2);
                codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "getPlayerO2Use", false, typeof(Player), typeof(float), typeof(int));
                codes.add(OpCodes.Ret);
                FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch (Exception e) {
                FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }
    */

    /*
    [HarmonyPatch(typeof(CraftData))]
    [HarmonyPatch("GetCraftTime")]
    public static class CraftingSpeed {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList();
            try {
                codes.add(OpCodes.Ldarg_0);
                codes.add(OpCodes.Ldarg_1);
                codes.invoke("ReikaKalseki.DIAlterra.DIHooks", "getFabricatorTime", false, typeof(TechType), typeof(float).MakeByRefType());
                codes.add(OpCodes.Ret);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            }
            catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }
    */
    /*
    [HarmonyPatch(typeof(EscapePodCinematicControl))]
    [HarmonyPatch("OnIntroStart")]
    public static class IntroStartHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.DIAlterra.DIHooks", "onAuroraSpawn", false, typeof(CrashedShipExploder)));
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            }
            catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }
            return codes.AsEnumerable();
        }
    }*/
}