//Working with Lists and Collections
//Working with Lists and Collections
//For data read/write methods
//More advanced manipulation of lists/collections

//Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.Ecocean;

internal static partial class ECPatches {
    /*
    [HarmonyPatch(typeof(SeamothTorpedoWhirlpool))]
    [HarmonyPatch("Update")]
    public static class TickVortexTorpedo {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {
                int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldnull);
                codes.InsertRange(idx, new InsnList{new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.Ecocean.ECHooks", "tickVortexTorpedo", false, typeof(SeamothTorpedoWhirlpool))});
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