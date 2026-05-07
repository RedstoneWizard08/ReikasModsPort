using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using FMOD;
using Nautilus.Handlers;
using Nautilus.Patchers;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class DIExtensions {
    extension(string s) {
        public string SetLeadingCase(bool upper) {
            return (upper ? char.ToUpperInvariant(s[0]) : char.ToLowerInvariant(s[0])) + s.Substring(1);
        }

        public string EnsureNonNull() {
            return s ?? "";
        }

        public string From(char cut) {
            return s.Substring(s.IndexOf(cut) + 1);
        }

        public List<string[]> PolySplit(char s1, char s2) {
            List<string[]> li = [];
            var parts = s.Split(s1);
            li.AddRange(parts.Select(t => t.Split(s2)));

            return li;
        }

        public List<List<string[]>> PolySplit(char s1, char s2, char s3) {
            List<List<string[]>> li0 = [];
            var parts = s.Split(s1);
            foreach (var part in parts) {
                var p = part.Split(s2);
                List<string[]> li2 = [];
                li2.AddRange(p.Select(t => t.Split(s3)));
                li0.Add(li2);
            }

            return li0;
        }

        public bool Contains(Regex r) {
            return r.IsMatch(s);
        }
    }

    public static TE ConvertEnum<TE>(this Enum e, TE fallback) where TE : struct {
        return Enum.TryParse<TE>(e.ToString(), out var ret) ? ret : fallback;
    }

    extension(SphereCollider sc) {
        public bool Intersects(SphereCollider other) {
            var pos1 = sc.transform.position + sc.center;
            var pos2 = other.transform.position + other.center;
            var r = Mathf.Min(sc.radius, other.radius);
            return (pos2 - pos1).sqrMagnitude <= r * r;
        }

        public Vector3 GetWorldCenter() {
            return sc.center + sc.transform.position;
        }
    }

    public static Vector3 GetWorldCenter(this BoxCollider sc) {
        return sc.center + sc.transform.position;
    }

    public static Vector3 GetWorldCenter(this CapsuleCollider sc) {
        return sc.center + sc.transform.position;
    }

    public static Sprite SetTexture(this Sprite s, Texture2D tex) {
        return Sprite.Create(tex, s.textureRect, s.pivot, s.pixelsPerUnit, 0, SpriteMeshType.FullRect, s.border);
    }

    extension(Vector3 c) {
        public Color AsColor() {
            return new Color(c.x, c.y, c.z);
        }

        public Vector3 Exponent(float exp) {
            return new Vector3(Mathf.Pow(c.x, exp), Mathf.Pow(c.y, exp), Mathf.Pow(c.z, exp));
        }

        public Vector3 Modulo(float size) {
            return new Vector3((c.x % size + size) % size, (c.y % size + size) % size, (c.z % size + size) % size);
        }

        public VECTOR ToFMODVector() {
            var ret = new VECTOR {
                x = c.x,
                y = c.y,
                z = c.z,
            };
            return ret;
        }

        public Vector3 SetLength(double amt) {
            return c.normalized * (float)amt;
        }

        public Vector3 AddLength(double amt) {
            return c.SetLength(c.magnitude + amt);
        }

        public Vector3 SetY(double y) {
            return new Vector3(c.x, (float)y, c.z);
        }

        public Vector3 Rotated(Quaternion rotation, Vector3 pivot = default) {
            return rotation * (c - pivot) + pivot;
        }

        public Vector3 Rotated(Vector3 rotation, Vector3 pivot = default) {
            return c.Rotated(Quaternion.Euler(rotation), pivot);
        }

        public Vector3 Rotated(float x, float y, float z, Vector3 pivot = default) {
            return c.Rotated(Quaternion.Euler(x, y, z), pivot);
        }

        public Int3 RoundToInt3() {
            return new Int3((int)Mathf.Floor(c.x), (int)Mathf.Floor(c.y), (int)Mathf.Floor(c.z));
        }
    }

    extension(Vector4 c) {
        public Color AsColor() {
            return new Color(c.x, c.y, c.z, c.w);
        }

        public Vector4 Exponent(float exp) {
            return new Vector4(Mathf.Pow(c.x, exp), Mathf.Pow(c.y, exp), Mathf.Pow(c.z, exp), Mathf.Pow(c.w, exp));
        }

        public Vector4 SetXYZ(Vector3 xyz) {
            c.x = xyz.x;
            c.y = xyz.y;
            c.z = xyz.z;
            return new Vector4(xyz.x, xyz.y, xyz.z, c.w);
        }

        public Vector3 GetXYZ() {
            return new Vector3(c.x, c.y, c.z);
        }
    }

    extension(Color c) {
        public Vector3 ToVector() {
            return new Vector3(c.r, c.g, c.b);
        }

        public Vector4 ToVectorA() {
            return new Vector4(c.r, c.g, c.b, c.a);
        }

        public int ToArgb() {
            var a = Mathf.RoundToInt(c.a * 255) & 0xFF;
            var r = Mathf.RoundToInt(c.r * 255) & 0xFF;
            var g = Mathf.RoundToInt(c.g * 255) & 0xFF;
            var b = Mathf.RoundToInt(c.b * 255) & 0xFF;
            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        public Color32 ToColor32() {
            return new Color32(
                (byte)Mathf.Round(c.r * 255F),
                (byte)Mathf.Round(c.g * 255F),
                (byte)Mathf.Round(c.b * 255F),
                (byte)Mathf.Round(c.a * 255F)
            );
        }

        public Color Exponent(float exp) {
            return new Color(Mathf.Pow(c.r, exp), Mathf.Pow(c.g, exp), Mathf.Pow(c.b, exp), Mathf.Pow(c.a, exp));
        }
    }

    public static Color ToColor(this Color32 c) {
        return new Color(c.r / 255F, c.g / 255F, c.b / 255F, c.a / 255F);
    }

    extension(XmlNode xml) {
        public XmlElement AddProperty(string name, Quaternion quat) {
            var n = xml.OwnerDocument.CreateElement(name);
            n.AddProperty("x", quat.x);
            n.AddProperty("y", quat.y);
            n.AddProperty("z", quat.z);
            n.AddProperty("w", quat.w);
            xml.AppendChild(n);
            return n;
        }

        public XmlElement AddProperty(string name, Vector3 vec) {
            var n = xml.OwnerDocument.CreateElement(name);
            n.AddProperty("x", vec.x);
            n.AddProperty("y", vec.y);
            n.AddProperty("z", vec.z);
            xml.AppendChild(n);
            return n;
        }

        public XmlElement AddProperty(string name, Vector4 vec) {
            var quat = new Quaternion {
                x = vec.x,
                y = vec.y,
                z = vec.z,
                w = vec.w,
            };
            return xml.AddProperty(name, quat);
        }

        public XmlElement AddProperty(string name, Color c) {
            var n = xml.OwnerDocument.CreateElement(name);
            n.AddProperty("r", c.r);
            n.AddProperty("g", c.g);
            n.AddProperty("b", c.b);
            n.AddProperty("a", c.a);
            xml.AppendChild(n);
            return n;
        }

        public XmlElement AddProperty(string name, int value) {
            return xml.AddProperty(name, value.ToString(CultureInfo.InvariantCulture));
        }

        public XmlElement AddProperty(string name, double value) {
            return xml.AddProperty(name, value.ToString(CultureInfo.InvariantCulture));
        }

        public XmlElement AddProperty(string name, bool value) {
            return xml.AddProperty(name, value.ToString(CultureInfo.InvariantCulture));
        }

        public XmlElement AddProperty(string name, string value = null) {
            var n = xml.OwnerDocument.CreateElement(name);
            if (value != null)
                n.InnerText = value;
            xml.AppendChild(n);
            return n;
        }
    }

    extension(XmlElement xml) {
        public double GetFloat(string name, double fallback) {
            var s = xml.GetProperty(name, true);
            return string.IsNullOrEmpty(s)
                ? double.IsNaN(fallback) ? throw new Exception("No matching tag '" + name + "'! " + xml.Format()) : fallback
                : double.Parse(xml.GetProperty(name), CultureInfo.InvariantCulture);
        }

        public int GetInt(string name, int fallback, bool allowFallback = true) {
            var s = xml.GetProperty(name, allowFallback);
            var nul = string.IsNullOrEmpty(s);
            return nul && !allowFallback ? throw new Exception("No matching tag '" + name + "'! " + xml.Format()) :
                nul ? fallback : int.Parse(s, CultureInfo.InvariantCulture);
        }

        public bool GetBoolean(string name) {
            return xml.GetBoolean(name, out _);
        }

        public bool GetBoolean(string name, out XmlElement elem) {
            var prop = xml.GetProperty(name, out elem, true);
            return !string.IsNullOrEmpty(prop) && bool.Parse(prop);
        }

        public string GetProperty(string name, bool allowNull = false) {
            return xml.GetProperty(name, out _, allowNull);
        }

        public int GetRandomInt(string name) {
            var li = xml.GetDirectElementsByTagName(name);
            if (li.Count == 1) {
                var min = li[0].GetInt("min", 0, true);
                var max = li[0].GetInt("max", -1, false);
                return UnityEngine.Random.Range(min, max);
            }

            throw new Exception(
                "You must have exactly one matching named element for getRandomInt '" + name + "'! " + xml.Format()
            );
        }

        public float GetRandomFloat(string name) {
            var li = xml.GetDirectElementsByTagName(name);
            if (li.Count == 1) {
                var min = li[0].GetFloat("min", double.NaN);
                var max = li[0].GetFloat("max", double.NaN);
                return UnityEngine.Random.Range((float)min, (float)max);
            }

            throw new Exception(
                "You must have exactly one matching named element for getRandomFloat '" + name + "'! " + xml.Format()
            );
        }

        public string GetProperty(string name, out XmlElement elem, bool allowNull = false) {
            var li = xml.GetDirectElementsByTagName(name);
            if (li.Count == 1) {
                elem = li[0];
                return li[0].InnerText;
            }

            if (li.Count == 0 && allowNull) {
                elem = null;
                return null;
            }

            throw new Exception(
                "You must have exactly one matching named tag for getProperty '" + name + "'! " + xml.Format()
            );
        }

        public Vector3? GetVector(string name, bool allowNull = false) {
            return xml.GetVector(name, out _, allowNull);
        }

        public Vector4? GetVector4(string name, bool allowNull = false) {
            var quat = xml.GetQuaternion(name, allowNull);
            if (quat is null)
                return null;
            var vec = new Vector4 {
                x = quat.Value.x,
                y = quat.Value.y,
                z = quat.Value.z,
                w = quat.Value.w,
            };
            return vec;
        }

        public Vector3? GetVector(string name, out XmlElement elem, bool allowNull = false) {
            var li = xml.GetDirectElementsByTagName(name);
            if (li.Count == 1) {
                var x = li[0].GetFloat("x", double.NaN);
                var y = li[0].GetFloat("y", double.NaN);
                var z = li[0].GetFloat("z", double.NaN);
                elem = li[0];
                return new Vector3((float)x, (float)y, (float)z);
            }

            if (li.Count == 0 && allowNull) {
                elem = null;
                return null;
            }

            throw new Exception(
                "You must have exactly one matching named element for getVector '" + name + "'! " + xml.Format()
            );
        }

        public Quaternion? GetQuaternion(string name, bool allowNull = false) {
            var li = xml.GetDirectElementsByTagName(name);
            if (li.Count == 1) {
                var x = li[0].GetFloat("x", double.NaN);
                var y = li[0].GetFloat("y", double.NaN);
                var z = li[0].GetFloat("z", double.NaN);
                var w = li[0].GetFloat("w", double.NaN);
                return new Quaternion((float)x, (float)y, (float)z, (float)w);
            }

            return li.Count == 0 && allowNull
                ? null
                : throw new Exception(
                    "You must have exactly one matching named element for getQuaternion '" + name + "'! " + xml.Format()
                );
        }

        public Color? GetColor(string name, bool includeAlpha, bool allowNull = false) {
            var li = xml.GetDirectElementsByTagName(name);
            if (li.Count == 1) {
                var r = li[0].GetFloat("r", double.NaN);
                var g = li[0].GetFloat("g", double.NaN);
                var b = li[0].GetFloat("b", double.NaN);
                var a = includeAlpha ? li[0].GetFloat("a", double.NaN) : 1;
                return new Color((float)r, (float)g, (float)b, (float)a);
            }

            return li.Count == 0 && allowNull
                ? null
                : throw new Exception(
                    "You must have exactly one matching named element for getColor '" + name + "'! " + xml.Format()
                );
        }

        public List<XmlElement> GetDirectElementsByTagName(string name) {
            List<XmlElement> li = [];
            foreach (XmlNode e in xml.ChildNodes) {
                if (e is XmlElement element && element.Name == name)
                    li.Add(element);
            }

            return li;
        }

        public XmlNodeList GetAllChildrenIn(string name) {
            var li = xml.GetDirectElementsByTagName(name);
            return li.Count == 1 ? li[0].ChildNodes : null;
        }

        public bool HasProperty(string name) {
            var li = xml.GetDirectElementsByTagName(name);
            return li.Count == 1;
        }

        public string Format() {
            return xml.OuterXml;
        }

        public XmlElement AddChild(string name) {
            var e2 = xml.OwnerDocument.CreateElement(name);
            xml.AppendChild(e2);
            return e2;
        }
    }

    extension(object o) {
        public bool IsEnumerable() {
            return o is IEnumerable && o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(IEnumerable<>));
        }

        public bool IsList() {
            return o is IList && o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public bool IsDictionary() {
            return o is IDictionary && o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }
    }

    /*
    public static string toDebugString(this IDictionary<object, object> dict) {
        return "{" + string.Join(",", dict.Select(kv => kv.Key + "=" + stringify(kv.Value)).ToArray()) + "}";
    }

    public static string toDebugString(this IEnumerable<object> c) {
        return "[" + string.Join(",", c.Select<object, string>(stringify).ToArray()) + "]";
    }
    */
    public static string ToDebugString<TK, TV>(this IDictionary<TK, TV> dict, string separator = ",") {
        return dict.Count + ":{" + string.Join(
            separator,
            dict.Select(kv => kv.Key + "=" + Stringify(kv.Value)).ToArray()
        ) + "}"; //return toDebugString((IDictionary<object, object>)dict);
    }

    public static string ToDebugString<TE>(this IEnumerable<TE> c, string separator = ",") {
        return c.Count() + ":[" + string.Join(separator, c.Select(e => Stringify(e)).ToArray()) +
               "]"; //return toDebugString((IEnumerable<object>)c);
    }

    public static TE Pop<TE>(this IList<TE> c) {
        var ret = c[0];
        c.RemoveAt(0);
        return ret;
    }

    public static TE GetRandomEntry<TE>(this IEnumerable<TE> c) {
        if (c is IList<TE> li)
            return li.Count == 0 ? default : li.GetRandom();
        return c.ElementAt(UnityEngine.Random.Range(0, c.Count()));
    }

    public static Vector3 GetClosest(this IEnumerable<Vector3> li, Vector3 pos) {
        var ret = Vector3.zero;
        var distSq = float.PositiveInfinity;
        foreach (var v in li) {
            var dd = (v - pos).sqrMagnitude;
            if (!(dd < distSq)) continue;
            distSq = dd;
            ret = v;
        }

        return ret;
    }

    public static TE[] AddToArray<TE>(this TE[] arr, TE add) {
        List<TE> li = [..arr.ToList(), add];
        return li.ToArray();
    }

    public static bool Overlaps<TE>(this ICollection<TE> c, ICollection<TE> other) {
        return c.Any(other.Contains);
    }

    public static string Stringify(object obj) {
        if (obj == null)
            return "null";
        if (obj.IsDictionary())
            return "dict:" + ((IDictionary<object, object>)obj).ToDebugString();
        if (obj.IsEnumerable())
            return "enumerable:" + ((IEnumerable<object>)obj).ToDebugString();
        if (obj is Attribute ar)
            return "attr '" + ar.GetType().Name + "'";
        return obj.ToString();
    }

    public static void CopySprites(this UnityEngine.UI.Button b, UnityEngine.UI.Button other) {
        b.image = other.image;
        //UnityEngine.UI.Toggle tg1 = b.GetComponent<UnityEngine.UI.Toggle>();
        //UnityEngine.UI.Toggle tg2 = other.GetComponent<UnityEngine.UI.Toggle>();
        var sprs = other.spriteState;
        var hover = sprs.highlightedSprite;
        other.GetComponent<UnityEngine.UI.Image>();
        var sprs2 = b.spriteState;
        sprs2.highlightedSprite = hover;
        sprs2.selectedSprite = hover;
        b.spriteState = sprs2;
        b.transition = other.transition;
    }

    public static T CopyStruct<T>(this T comp, T from) where T : struct {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                   BindingFlags.FlattenHierarchy;

        foreach (var pinfo in typeof(T).GetProperties(flags)) {
            if (!pinfo.CanWrite) continue;
            try {
                pinfo.SetValue(comp, pinfo.GetValue(from, null), null);
            } catch {
                // ignored
            }
        }

        foreach (var finfo in typeof(T).GetFields(flags)) {
            finfo.SetValue(comp, finfo.GetValue(from));
        }

        return comp;
    }

    public static T CopyObject<T>(this T comp, T from) where T : class {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                   BindingFlags.FlattenHierarchy;

        foreach (var pinfo in typeof(T).GetProperties(flags)) {
            if (!pinfo.CanWrite) continue;
            try {
                pinfo.SetValue(comp, pinfo.GetValue(from, null), null);
            } catch {
                // ignored
            }
        }

        foreach (var finfo in typeof(T).GetFields(flags)) {
            finfo.SetValue(comp, finfo.GetValue(from));
        }

        return comp;
    }

    public static string ToDetailedString(this WaterscapeVolume.Settings s) {
        return
            $"Start={s.startDistance:0.0000}, Murk={s.murkiness:0.0000}, Absorb={s.absorption:0.0000}, AmbScale={s.ambientScale:0.0000}, Emissive={s.emissive.ToString("0.0000")}, EmisScale={s.emissiveScale:0.0000}, Scatter={s.scattering:0.0000}, ScatterColor={s.scatteringColor.ToString("0.0000")}, Sun={s.sunlightScale:0.0000}, Temp={s.temperature}";
    }

    public static float GetLifespan(this StasisSphere s) {
        return s.time * s.fieldEnergy;
    }

    public static void ClearAttackTarget(this AttackLastTarget a) {
        var c = a.GetComponent<Creature>();
        if (c)
            c.Aggression.Add(-1);
        a.StopAttack();
        a.currentTarget = null;
        a.lastTarget.SetTarget(null);
    }

    public static VehicleAccelerationModifier AddSpeedModifier(this Vehicle v) {
        var ret = v.gameObject.AddComponent<VehicleAccelerationModifier>();
        v.accelerationModifiers = v.GetComponentsInChildren<VehicleAccelerationModifier>();
        return ret;
    }

    extension(TechType creature) {
        public TechType GetEgg() {
            if (creature == TechType.None)
                return TechType.None;
            var e = CustomEgg.GetEgg(creature);
            if (e != null)
                return e.CreatureToSpawn;
            var name = creature.AsString();
            return Enum.TryParse(name + "Egg", true, out TechType ret) ? ret : TechType.None;
        }

        public TechType GetCookedCounterpart() {
            return TechData.GetProcessed(creature);
            // if (TechData.GetProcessed(raw) != null)
            //     return TechData.GetProcessed(raw);
            // IDictionary<TechType, TechType> dict = getPatcherDict<TechType>("CustomCookedCreatureList", craftDataPatcher);
            // if (dict != null && dict.ContainsKey(raw))
            //     return dict[raw];
            // else
            //     return TechType.None;
        }

        public Vector2int GetItemSize() {
            return TechData.GetItemSize(creature);
            // if (TechData.GetItemSize(item) != null)
            // 	return TechData.GetItemSize(item);
            // IDictionary<TechType, Vector2int> dict = getPatcherDict<Vector2int>("CustomItemSizes", craftDataPatcher);
            // if (dict != null && dict.ContainsKey(item))
            // 	return dict[item];
            // else
            // 	return new Vector2int(1, 1);
        }

        public void RemoveUnlockTrigger(ProgressionTrigger checkToDisable = null) {
            KnownTechHandler.RemoveAllCurrentAnalysisTechEntry(creature);
            KnownTechPatcher.UnlockedAtStart.Remove(creature);
            if (checkToDisable != null)
                RecipeUtil.techsToRemoveIf[creature] = checkToDisable;
        }

        public void PreventCraftNodeAddition(CraftTree.Type tree = CraftTree.Type.Fabricator) {
            SNUtil.Log("Removing all prepared craft nodes for '" + creature.AsString() + "'");

            var nodes = CraftTreePatcher.CraftingNodes;

            foreach (var key in nodes.Keys) {
                var any = false;
                var li = nodes[key];

                for (var i = li.Count - 1; i >= 0; i--) {
                    var o = li[i];
                    if (o == null)
                        continue;
                    var tt = o.TechType;
                    if (tt != creature) continue;
                    li.RemoveAt(i);
                    var path = o.Path ?? [];
                    path = path.AddToArray(tt.AsString());
                    SNUtil.Log("Removing craft node " + o + " @ " + string.Join("/", path));
                    CraftTreeHandler.RemoveNode(tree, path);
                    any = true;
                }

                if (any) continue;
                {
                    var nodelist = "";
                    foreach (var o in li) {
                        if (o == null)
                            continue;
                        var tt = o.TechType;
                        var path = o.Path ?? [];
                        path = path.AddToArray(tt.AsString());
                        nodelist += tt.AsString() + " @ " + string.Join("/", path) + "\n";
                    }

                    SNUtil.Log("No craft nodes for '" + creature.AsString() + "' found! Queued Nodes:\n" + nodelist);
                }
            }
        }
    }
}