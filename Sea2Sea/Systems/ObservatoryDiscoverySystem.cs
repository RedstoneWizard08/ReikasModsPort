namespace ReikaKalseki.SeaToSea;

public class ObservatoryDiscoverySystem {

	public static readonly ObservatoryDiscoverySystem instance = new ObservatoryDiscoverySystem();

	private ObservatoryDiscoverySystem() {

	}

	public void tick(Player ep) {

	}

	enum BiomeTypes {
		SHALLOW,
		MODERATE,
		DEEP,
		LOST,
		//ILZ,
			
	}
}