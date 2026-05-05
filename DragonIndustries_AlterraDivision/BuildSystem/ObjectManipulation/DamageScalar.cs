/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Xml;

namespace ReikaKalseki.DIAlterra;

public sealed class DamageScalar : ModifyComponent<MeleeAttack> {

	private double scale = 1;

	public override void modifyComponent(MeleeAttack c) {
		c.biteDamage *= (float)scale;
	}

	public override void loadFromXML(XmlElement e) {
		scale = double.Parse(e.InnerText);
	}

	public override void saveToXML(XmlElement e) {
		e.InnerText = scale.ToString();
	}

}