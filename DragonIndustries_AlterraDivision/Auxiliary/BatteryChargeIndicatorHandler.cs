using System.Reflection;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class BatteryChargeIndicatorHandler {
    private static MonoBehaviour batteryChargeModHandler;
    private static MethodInfo batteryChargeModUpdate;

    private static bool failedToFind;

    public static void resyncChargeIndicators() {
        // if (QModManager.API.QModServices.Main.ModPresent("batteryChargeIndicator")) {
        // 	if (!failedToFind && !batteryChargeModHandler) {
        // 		Type t = InstructionHandlers.getTypeBySimpleName("batteryMod.BatteryIndicatorManager");
        // 		batteryChargeModHandler = (MonoBehaviour)UnityEngine.Object.FindObjectOfType(t);
        // 		batteryChargeModUpdate = t.GetMethod("UpdateAll");
        // 		if (!batteryChargeModHandler)
        // 			failedToFind = true;
        // 	}
        // 	if (batteryChargeModHandler && batteryChargeModUpdate != null)
        // 		batteryChargeModUpdate.Invoke(batteryChargeModHandler, new object[0]);
        // }
    }
}