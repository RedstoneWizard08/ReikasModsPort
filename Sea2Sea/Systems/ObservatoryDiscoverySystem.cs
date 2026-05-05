namespace ReikaKalseki.SeaToSea;

public class ObservatoryDiscoverySystem {

	public static readonly ObservatoryDiscoverySystem instance = new();

	private ObservatoryDiscoverySystem() {

	}

	public void tick(Player ep) {

	}

	private enum BiomeTypes {
		SHALLOW,
		MODERATE,
		DEEP,
		LOST,
		//ILZ,
			
	}
}