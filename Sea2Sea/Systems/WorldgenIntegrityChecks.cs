using System;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public static class WorldgenIntegrityChecks {
    private static bool xmlLoadFailure;

    private static readonly List<string> currentErrorText = [];

    public static bool checkWorldgenIntegrity(bool flag) {
        xmlLoadFailure = SeaToSeaMod.WorldGen.getCount() <= 0;
        if (flag || xmlLoadFailure || SeaToSeaMod.MushroomBioFragment.fragmentCount <= 0 ||
            SeaToSeaMod.GeyserCoral.fragmentCount <= 0 || DataboxTypingMap.instance.isEmpty()) {
            currentErrorText.Clear();
            currentErrorText.Add(
                "C2C worldgen failed to initialize, and all progression is invalid! Do not continue playing!"
            );
            if (xmlLoadFailure)
                currentErrorText.Add("Main worldgen DB failed to load");
            DIHooks.SetWarningText(currentErrorText);
            return true;
        }

        return false;
    }

    public static void throwError() {
        throw new Exception(currentErrorText.ToDebugString());
    }
}