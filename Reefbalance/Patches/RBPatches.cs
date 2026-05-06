/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 04/11/2019
 * Time: 11:28 AM
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

//Working with Lists and Collections
//Working with Lists and Collections
//For data read/write methods
//More advanced manipulation of lists/collections

//Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.Reefbalance;

internal static partial class RBPatches {
    /*
    [HarmonyPatch(typeof(Creature))]
    [HarmonyPatch(nameof(Creature.Start))]
    public static class CreatureActivateHook {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = new InsnList(instructions);
            try {/*
                int sub = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Sub);
                InsnList inject = new InsnList();
                inject.add(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand("ReikaKalseki.Reefbalance.ReefbalanceMod", "onRoomFindMachine"));
                codes.InsertRange(sub+1, inject);
                */ /*
                codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(1, InstructionHandlers.createMethodCall("ReikaKalseki.Reefbalance.ReefbalanceMod", "onCreatureActivate", false, typeof(Creature)));
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
    }*/
}