using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Exscansion;

internal static partial class ESPatches {
    private static class RangePatchLib {
        internal static void replaceMaxRangeReference(InsnList codes) {
            InsnList li = [
                InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    nameof(ESHooks.GetScannerMaxRange),
                    false,
                    new string[0]
                ),
            ];
            codes.ReplaceConstantWithMethodCall(500F, li);
        }

        internal static void replaceBaseRangeReference(InsnList codes) {
            InsnList li = [
                InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    nameof(ESHooks.GetScannerBaseRange),
                    false,
                    new string[0]
                ),
            ];
            codes.ReplaceConstantWithMethodCall(300F, li);
        }

        internal static void replaceRangeBonusReference(InsnList codes) {
            InsnList li = [
                InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    nameof(ESHooks.GetRangeUpgradeValue),
                    false,
                    new string[0]
                ),
            ];
            codes.ReplaceConstantWithMethodCall(50F, li);
        }

        internal static void replaceBaseSpeedReference(InsnList codes) {
            InsnList li = [
                InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    nameof(ESHooks.GetScannerBaseSpeed),
                    false,
                    new string[0]
                ),
            ];
            codes.ReplaceConstantWithMethodCall(14F, li);
        }

        internal static void replaceSpeedBonusReference(InsnList codes) {
            InsnList li = [
                InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    nameof(ESHooks.GetSpeedUpgradeValue),
                    false,
                    new string[0]
                ),
            ];
            codes.ReplaceConstantWithMethodCall(3F, li);
        }
    }
}