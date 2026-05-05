namespace ReikaKalseki.DIAlterra;

public static class MiscExt {
    public static void SetInteractText(this HandReticle hand, string msg, bool translate = true) {
        hand.SetText(HandReticle.TextType.Use, msg, translate);
    }
}