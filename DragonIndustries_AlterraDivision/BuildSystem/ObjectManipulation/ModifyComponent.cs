/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public abstract class ModifyComponent<T> : ManipulationBase where T : Component {

	public override sealed void applyToObject(PlacedObject go) {
		this.applyToObject(go.obj);
	}

	public override sealed void applyToObject(GameObject go) {
		T component = go.GetComponentInChildren<T>();
		if (component != null)
			this.modifyComponent(component);
	}

	public abstract void modifyComponent(T component);

}