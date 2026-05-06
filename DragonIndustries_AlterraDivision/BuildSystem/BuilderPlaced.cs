/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

[Serializable]
//[ProtoContract]
//[ProtoInclude(30000, typeof(BuilderPlaced))]
internal class BuilderPlaced : MonoBehaviour {

	[SerializeField]
	//[SerializeReference]
	internal PlacedObject placement;

	private void Start() {
		SNUtil.Log("Initialized builderplaced of " + placement, SNUtil.DiDLL);
	}

	private void Update() {

	}

}