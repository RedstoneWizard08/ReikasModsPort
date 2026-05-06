/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public abstract class SwapTexture : ManipulationBase {

	private readonly Dictionary<string, string> swaps = new();

	protected SwapTexture() {

	}

	protected abstract Texture2D getTexture(string name, string texType);

	protected void addSwap(string from, string to) {
		swaps[from] = to;
	}

	public override void applyToObject(GameObject go) {
		foreach (var r in go.GetComponentsInChildren<Renderer>()) {
			foreach (var m in r.materials) {
				if (m.mainTexture != null) {
					var put = swaps.ContainsKey(m.mainTexture.name) ? swaps[m.mainTexture.name] : null;
					if (put != null) {
						var tex2 = getTexture(put, "main");
						if (tex2 != null)
							m.mainTexture = tex2;
						//else
						//SNUtil.writeToChat("Could not find texture "+put);
					}
				}
				foreach (var n in m.GetTexturePropertyNames()) {
					var tex = m.GetTexture(n);
					if (tex is Texture2D) {
						var file = tex.name;
						var put = swaps.ContainsKey(file) ? swaps[file] : null;
						//SNUtil.writeToChat(n+" > "+file+" > "+put);
						if (put != null) {
							var tex2 = getTexture(put, n);
							//SNUtil.writeToChat(">>"+tex2);
							if (tex2 != null)
								m.SetTexture(n, tex2);
							else
								SNUtil.WriteToChat("Could not find texture " + put);
						}
					}
				}
			}
			r.UpdateGIMaterials();
		}
	}

	public sealed override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {
		swaps.Clear();
		foreach (XmlNode n2 in e.ChildNodes) {
			if (n2 is XmlElement e2) {
				swaps[e2.GetProperty("from")] = e2.GetProperty("to");
			}
		}
	}

	public override void saveToXML(XmlElement e) {
		foreach (var kvp in swaps) {
			var e2 = e.OwnerDocument.CreateElement("swap");
			e2.AddProperty("from", kvp.Key);
			e2.AddProperty("to", kvp.Value);
			e.AppendChild(e2);
		}
	}

}