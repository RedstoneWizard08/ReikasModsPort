using System.Collections.Generic;

namespace ReikaKalseki.DIAlterra;

public class TreeCategories {
    internal static readonly Dictionary<TechCategory, TechGroup> customCategories = new();

    public static void Register(TechCategory category, TechGroup group) {
        customCategories[category] = group;
    }
}