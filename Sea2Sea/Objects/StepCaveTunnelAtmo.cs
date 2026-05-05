using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class StepCaveTunnelAtmo : Spawnable {

	internal StepCaveTunnelAtmo() : base("StepCaveTunnelAtmo", "", "") {

	}

	public override GameObject GetGameObject() {
		GameObject world = ObjectUtil.createWorldObject("b179b366-4342-4545-aa4d-a86ad88b780e");
		world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
		return world;
	}

}