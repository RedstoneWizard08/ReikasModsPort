namespace ReikaKalseki.Auroresource;

public class DrillableMeteorite : DrillableResourceArea {

	public DrillableMeteorite() : base(AuroresourceMod.locale.getEntry("DrillableMeteorite"), 24) {
		this.addDrop(TechType.Titanium, 400);
		this.addDrop(TechType.Copper, 250);
		this.addDrop(TechType.Nickel, 180);
		this.addDrop(TechType.Lead, 180);
		this.addDrop(TechType.Silver, 150);
		this.addDrop(TechType.Gold, 100);
		this.addDrop(TechType.MercuryOre, 40);
	}

}