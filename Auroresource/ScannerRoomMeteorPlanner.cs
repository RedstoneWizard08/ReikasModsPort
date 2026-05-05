using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Auroresource;

public class ScannerRoomMeteorPlanner : BasicCraftingItem {
    [SetsRequiredMembers]
    internal ScannerRoomMeteorPlanner() : base(
        AuroresourceMod.Locale.getEntry("ScannerRoomMeteorPlanner"),
        "6d1d97a5-75b8-49ef-8944-393d387a37a0"
    ) {
        unlockRequirement = TechType.Unobtanium;
        sprite = TextureManager.getSprite(AuroresourceMod.ModDLL, "Textures/planner");
        craftingTime = 8;
        inventorySize = new Vector2int(2, 2);
        this.addIngredient(TechType.MapRoomCamera, 1);
        this.addIngredient(TechType.SeamothSonarModule, 1);
        this.addIngredient(TechType.TitaniumIngot, 1);
        this.addIngredient(TechType.Polyaniline, 1);
    }

    public override TechGroup GroupForPDA => TechGroup.MapRoomUpgrades;

    public sealed override TechCategory CategoryForPDA => TechCategory.MapRoomUpgrades;

    public override CraftTree.Type FabricatorType => CraftTree.Type.MapRoom;

    public override string[] StepsToFabricatorTab => [];
}