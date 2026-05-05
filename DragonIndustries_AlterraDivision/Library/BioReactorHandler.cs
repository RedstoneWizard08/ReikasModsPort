namespace ReikaKalseki.DIAlterra;

public class BioReactorHandler {
    public static void SetBioReactorCharge(TechType type, float value) {
        BaseBioReactor.charge[type] = value;
    }
}