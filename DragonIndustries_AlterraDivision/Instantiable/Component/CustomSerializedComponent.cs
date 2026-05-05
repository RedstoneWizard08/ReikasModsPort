using System.Xml;

namespace ReikaKalseki.DIAlterra;

public interface CustomSerializedComponent {

	void saveToXML(XmlElement e);
	void readFromXML(XmlElement e);

}