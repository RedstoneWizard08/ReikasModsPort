using System;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

[Obsolete]
public class ColliderPrefabLink : MonoBehaviour {

	public GameObject root { private set; get; }/*

	public bool isPlayer {private set; get;}
	public LiveMixin living {private set; get;}
	public Vehicle vehicle {private set; get;}
	public Creature creature {private set; get;}*/

	internal void init(PrefabIdentifier pi) {
		root = pi.gameObject;/*

		switch(pi.ClassId) {
			case "ba3fb98d-e408-47eb-aa6c-12e14516446b":
			case "1c34945a-656d-4f70-bf86-8bc101a27eee":

				break;
		}*/
	}

}