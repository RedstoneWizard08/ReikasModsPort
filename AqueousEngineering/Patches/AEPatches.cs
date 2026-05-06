//Working with Lists and Collections
//Working with Lists and Collections
//For data read/write methods
//More advanced manipulation of lists/collections

//Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.AqueousEngineering;

public static partial class AEPatches {
    /* handled in NuclearReactorFuelSystem
    [HarmonyPatch(typeof(BaseNuclearReactor))]
    [HarmonyPatch(nameof(BaseNuclearReactor.Update))]
    public static class NuclearReactorPowerHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                PatchLib.addPowerGenHook("BaseNuclearReactor", codes);
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
}