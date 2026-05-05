using Nautilus.Assets;

namespace ReikaKalseki.DIAlterra;

public static class CustomPrefabExt {
    extension(CustomPrefab pfb) {
        public string ClassID => pfb.Info.ClassID;
        public TechType TechType => pfb.Info.TechType;
    }
}