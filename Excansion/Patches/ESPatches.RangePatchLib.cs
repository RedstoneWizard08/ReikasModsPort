using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Exscansion;

internal static partial class ESPatches {
    private static class RangePatchLib {
        internal static void replaceMaxRangeReference(InsnList codes) {
            InsnList li = [
                InstructionHandlers.createMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    "getScannerMaxRange",
                    false,
                    new string[0]
                ),
            ];
            codes.replaceConstantWithMethodCall(500F, li);
        }

        internal static void replaceBaseRangeReference(InsnList codes) {
            InsnList li = [
                InstructionHandlers.createMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    "getScannerBaseRange",
                    false,
                    new string[0]
                ),
            ];
            codes.replaceConstantWithMethodCall(300F, li);
        }

        internal static void replaceRangeBonusReference(InsnList codes) {
            InsnList li = [
                InstructionHandlers.createMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    "getRangeUpgradeValue",
                    false,
                    new string[0]
                ),
            ];
            codes.replaceConstantWithMethodCall(50F, li);
        }

        internal static void replaceBaseSpeedReference(InsnList codes) {
            InsnList li = [
                InstructionHandlers.createMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    "getScannerBaseSpeed",
                    false,
                    new string[0]
                ),
            ];
            codes.replaceConstantWithMethodCall(14F, li);
        }

        internal static void replaceSpeedBonusReference(InsnList codes) {
            InsnList li = [
                InstructionHandlers.createMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    "getSpeedUpgradeValue",
                    false,
                    new string[0]
                ),
            ];
            codes.replaceConstantWithMethodCall(3F, li);
        }
    }
}