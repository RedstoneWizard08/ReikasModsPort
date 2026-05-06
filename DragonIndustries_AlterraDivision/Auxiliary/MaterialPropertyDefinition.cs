using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class MaterialPropertyDefinition {

	private static readonly Dictionary<string, ShaderPropertyDefinition> shaderPropTypes = new();

	static MaterialPropertyDefinition() { //this is not exhaustive but it covers most of the commonly used ones
		addShaderProperty(new ShaderPropertyDefinition("_Fresnel", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_Shininess", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_SpecInt", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_Mode", typeof(float)));

		addShaderProperty(new ShaderPropertyDefinition("_Color", typeof(Color)));
		addShaderProperty(new ShaderPropertyDefinition("_Color2", typeof(Color)));
		addShaderProperty(new ShaderPropertyDefinition("_Color3", typeof(Color)));

		addShaderProperty(new ShaderPropertyDefinition("_SpecColor", typeof(Color)));
		addShaderProperty(new ShaderPropertyDefinition("_GlowColor", typeof(Color)));
		addShaderProperty(new ShaderPropertyDefinition("_SquaresColor", typeof(Color)));

		addShaderProperty(new ShaderPropertyDefinition("_GlowStrength", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_GlowStrengthNight", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_EmissionLM", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("__EmissionLMNight", typeof(float)));

		addShaderProperty(new ShaderPropertyDefinition("_EnableGlow", typeof(int)));
		addShaderProperty(new ShaderPropertyDefinition("_EnableLighting", typeof(int)));
		addShaderProperty(new ShaderPropertyDefinition("_EnableLightmap", typeof(int)));

		addShaderProperty(new ShaderPropertyDefinition("_ZWrite", typeof(int)));
		addShaderProperty(new ShaderPropertyDefinition("_Cutoff", typeof(int)));

		addShaderProperty(new ShaderPropertyDefinition("_SrcBlend", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_DstBlend", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_SrcBlend2", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_DstBlend2", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_AddSrcBlend", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_AddDstBlend", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_AddSrcBlend2", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_AddDstBlend2", typeof(float)));

		addShaderProperty(new ShaderPropertyDefinition("_FillSack", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_OverlayStrength", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_Hypnotize", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_SquaresTile", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_SquaresSpeed", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_SquaresIntensityPow", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_Built", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_BuildLinear", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_NoiseThickness", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_NoiseStr", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_WaveUpMin", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_Fallof", typeof(float))); //missing f 
		addShaderProperty(new ShaderPropertyDefinition("_RopeGravity", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_minYpos", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_maxYpos", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_Displacement", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_BurstStrength", typeof(float)));
		addShaderProperty(new ShaderPropertyDefinition("_ClipRange", typeof(float)));

		addShaderProperty(new ShaderPropertyDefinition("_DetailIntensities", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_LightmapStrength", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_ColorStrength", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_GlowMaskSpeed", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_ScrollSpeed", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_NoiseSpeed", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_FakeSSSparams", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_FakeSSSSpeed", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_BuildParams", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_Scale", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_Frequency", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_Speed", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_ObjectUp", typeof(Vector4)));
		addShaderProperty(new ShaderPropertyDefinition("_Range", typeof(Vector4)));
	}

	private static void addShaderProperty(ShaderPropertyDefinition def) {
		shaderPropTypes[def.name] = def;
	}

	public string name;

	public readonly Dictionary<string, TextureDefinition> textures = new();
	public readonly HashSet<string> shaderFlags = [];
	public readonly Dictionary<string, ShaderProperty> shaderProperties = new();

	public Color color;
	public int renderQueue;
	public MaterialGlobalIlluminationFlags illumFlags;

	public MaterialPropertyDefinition(string n) {
		name = n.Replace(" (Instance)", "");
	}

	public MaterialPropertyDefinition(Material m) : this(m.mainTexture.name) {
		color = m.color;
		renderQueue = m.renderQueue;
		illumFlags = m.globalIlluminationFlags;
		foreach (var tex in m.GetTexturePropertyNames()) {
			textures[tex] = new TextureDefinition(m, tex);
		}
		foreach (var shd in m.shaderKeywords) {
			shaderFlags.Add(shd);
		}
		foreach (var shd in shaderPropTypes.Values) {
			shaderProperties[shd.name] = new ShaderProperty(shd, m);
		}
	}

	public void readFromFile(Assembly a, string folder) {
		var defs = Path.Combine(folder, "defs.xml");
		var doc = new XmlDocument();
		doc.Load(defs);
		var texs = (XmlElement)doc.DocumentElement.GetElementsByTagName("textures")[0];
		var flags = (XmlElement)doc.DocumentElement.GetElementsByTagName("flags")[0];
		var props = (XmlElement)doc.DocumentElement.GetElementsByTagName("properties")[0];
		foreach (var e in texs.GetDirectElementsByTagName("entry")) {
			var tex = new TextureDefinition();
			tex.readFromFile(e);
			textures[tex.name] = tex;
			tex.texture = TextureManager.getTexture(a, Path.Combine(folder, tex.name));
		}
		foreach (var e in props.GetDirectElementsByTagName("entry")) {
			var shd = new ShaderProperty(shaderPropTypes[e.GetProperty("name")]);
			shd.readFromFile(e);
			shaderProperties[shd.definition.name] = shd;
		}
		foreach (var e in flags.GetDirectElementsByTagName("entry")) {
			shaderFlags.Add(e.InnerText);
		}
		color = doc.DocumentElement.GetColor("color", true).Value;
		renderQueue = doc.DocumentElement.GetInt("renderQueue", 0, false);
		illumFlags = (MaterialGlobalIlluminationFlags)doc.DocumentElement.GetInt("illumFlags", 0, false);
	}

	public void writeToFile(string folder) {
		Directory.CreateDirectory(folder);
		var defs = Path.Combine(folder, "defs.xml");
		var doc = new XmlDocument();
		var rootnode = doc.CreateElement("Root");
		doc.AppendChild(rootnode);
		var texs = doc.CreateElement("textures");
		var flags = doc.CreateElement("flags");
		var props = doc.CreateElement("properties");
		foreach (var tex in textures.Values) {
			var e = doc.CreateElement("entry");
			tex.writeToFile(e);
			texs.AppendChild(e);
			RenderUtil.dumpTexture(SNUtil.DiDLL, tex.name, (Texture2D)tex.texture, folder);
		}
		foreach (var s in shaderFlags) {
			flags.AddProperty("entry", s);
		}
		foreach (var shd in shaderProperties.Values) {
			var e = doc.CreateElement("entry");
			shd.writeToFile(e);
			props.AppendChild(e);
		}
		doc.DocumentElement.AddProperty("color", color);
		doc.DocumentElement.AddProperty("renderQueue", renderQueue);
		doc.DocumentElement.AddProperty("illumFlags", (int)illumFlags);
		doc.DocumentElement.AppendChild(texs);
		doc.DocumentElement.AppendChild(flags);
		doc.DocumentElement.AppendChild(props);
		doc.Save(defs);
	}

	public void applyToMaterial(Material m, bool useTex = true, bool vars = true) {
		m.name = name;
		m.color = color;
		if (vars) {
			m.renderQueue = renderQueue;
			m.globalIlluminationFlags = illumFlags;
		}
		if (useTex) {
			foreach (var tex in textures.Values) {
				tex.applyToMaterial(m);
			}
		}
		if (vars) {
			foreach (var shd in shaderProperties.Values) {
				shd.applyToMaterial(m);
			}
			foreach (var flag in m.shaderKeywords) {
				m.DisableKeyword(flag);
			}
			foreach (var flag in shaderFlags) {
				m.EnableKeyword(flag);
			}
		}
	}

	public class ShaderPropertyDefinition {

		public readonly string name;
		public readonly Type valueType;

		public ShaderPropertyDefinition(string n, Type tt) {
			name = n;
			valueType = tt;
		}

		internal object loadValue(XmlElement e) {
			if (valueType == typeof(int))
				return e.GetInt(name, 0, false);
			if (valueType == typeof(float))
				return e.GetFloat(name, float.NaN);
			//if (valueType == typeof(float[]))
			//	return e.GetFloatArray(name);
			return valueType == typeof(Vector4) ? e.GetVector4(name) : valueType == typeof(Color) ? e.GetColor(name, true) : (object)null;
		}

		internal void saveValue(XmlElement e, object val) {
			if (valueType == typeof(int))
				e.AddProperty(name, (int)val);
			if (valueType == typeof(float))
				e.AddProperty(name, (float)val);
			//if (valueType == typeof(float[]))
			//e.addProperty(name, val);
			if (valueType == typeof(Vector4))
				e.AddProperty(name, (Vector4)val);
			if (valueType == typeof(Color))
				e.AddProperty(name, (Color)val);
		}

		internal object getValue(Material m) {
			return valueType == typeof(int)
				? m.GetInt(name)
				: valueType == typeof(float)
					? m.GetFloat(name)
					: valueType == typeof(float[])
						? m.GetFloatArray(name)
						: valueType == typeof(Vector4) ? m.GetVector(name) : valueType == typeof(Color) ? m.GetColor(name) : (object)null;
		}

		internal void applyValue(Material m, object val) {
			if (valueType == typeof(int))
				m.SetInt(name, Convert.ToInt32(val));
			if (valueType == typeof(float))
				m.SetFloat(name, Convert.ToSingle(val));
			if (valueType == typeof(float[]))
				m.SetFloatArray(name, (float[])val);
			if (valueType == typeof(Vector4))
				m.SetVector(name, (Vector4)val);
			if (valueType == typeof(Color))
				m.SetColor(name, (Color)val);
		}

	}

	public class ShaderProperty {

		public readonly ShaderPropertyDefinition definition;

		public object value;

		public ShaderProperty(ShaderPropertyDefinition n) {
			definition = n;
		}

		public ShaderProperty(ShaderPropertyDefinition n, Material m) : this(n) {
			value = definition.getValue(m);
		}

		public void applyToMaterial(Material m) {
			try {
				definition.applyValue(m, value);
			}
			catch (Exception ex) {
				SNUtil.Log("Could not apply shader property " + definition.name + " [" + value + "]: " + ex);
			}
		}

		public void writeToFile(XmlElement e) {
			e.AddProperty("name", definition.name);
			definition.saveValue(e, value);
		}

		public void readFromFile(XmlElement e) {
			value = definition.loadValue(e);
		}

	}

	public class TextureDefinition {

		public string name;

		public Texture texture;
		public Vector2 scale;
		public Vector2 offset;

		public TextureDefinition() {

		}

		public TextureDefinition(Material m, string tex) {
			name = tex;
			texture = m.GetTexture(tex);
			scale = m.GetTextureScale(tex);
			offset = m.GetTextureOffset(tex);
		}

		public void applyToMaterial(Material m) {
			m.SetTexture(name, texture);
			m.SetTextureScale(name, scale);
			m.SetTextureOffset(name, offset);
		}

		public void writeToFile(XmlElement e) {
			e.AddProperty("name", name);
			e.AddProperty("scale", scale.WithZ(0));
			e.AddProperty("offset", offset.WithZ(0));
		}

		public void readFromFile(XmlElement e) {
			name = e.GetProperty("name");
			scale = e.GetVector("scale").Value.XY();
			offset = e.GetVector("offset").Value.XY();
		}

	}

}