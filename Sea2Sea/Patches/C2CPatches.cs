//For data read/write methods
//Working with Lists and Collections
//Working with Lists and Collections
//More advanced manipulation of lists/collections

//Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
	/*
[HarmonyPatch(typeof(GhostCrafter))]
[HarmonyPatch("Craft")]
public static class CraftingSpeed {

	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
		InsnList codes = new InsnList(instructions);
		try {
			int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "CraftData", "GetCraftTime", false, typeof(TechType), typeof(float).MakeByRefType());
			codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getFabricatorTime", false, typeof(TechType), typeof(float).MakeByRefType());
			//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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
[HarmonyPatch(typeof(TooltipFactory))]
[HarmonyPatch("Recipe")]
public static class CraftooltipHook {

	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
		InsnList codes = new InsnList(instructions);
		try {
			codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onCraftMenuTT", false, typeof(TechType)));
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
[HarmonyPatch(typeof(CrushDamage))]
[HarmonyPatch("CrushDamageUpdate")]
public static class CrushDamageAmount {

	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
		InsnList codes = new InsnList(instructions);
		try {
			int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "CrushDamage", "damagePerCrush");
			codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getCrushDamage", false, typeof(CrushDamage));
			//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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
}