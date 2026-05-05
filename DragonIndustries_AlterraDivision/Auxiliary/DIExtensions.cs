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
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class DIExtensions {
    public static string setLeadingCase(this string s, bool upper) {
        return (upper ? char.ToUpperInvariant(s[0]) : char.ToLowerInvariant(s[0])) + s.Substring(1);
    }

    public static string ensureNonNull(this string s) {
        return s == null ? "" : s;
    }

    public static string from(this string s, char cut) {
        return s.Substring(s.IndexOf(cut) + 1);
    }

    public static List<string[]> polySplit(this string s, char s1, char s2) {
        List<string[]> li = [];
        var parts = s.Split(s1);
        for (var i = 0; i < parts.Length; i++) {
            li.Add(parts[i].Split(s2));
        }

        return li;
    }

    public static List<List<string[]>> polySplit(this string s, char s1, char s2, char s3) {
        List<List<string[]>> li0 = [];
        var parts = s.Split(s1);
        for (var i = 0; i < parts.Length; i++) {
            var p = parts[i].Split(s2);
            List<string[]> li2 = [];
            for (var k = 0; k < p.Length; k++) {
                li2.Add(p[k].Split(s3));
            }

            li0.Add(li2);
        }

        return li0;
    }

    public static bool contains(this string s, Regex r) {
        return r.IsMatch(s);
    }

    public static E convertEnum<E>(this Enum e, E fallback) where E : struct {
        return Enum.TryParse<E>(e.ToString(), out var ret) ? ret : fallback;
    }

    public static bool intersects(this SphereCollider sc, SphereCollider other) {
        var pos1 = sc.transform.position + sc.center;
        var pos2 = other.transform.position + other.center;
        var r = Mathf.Min(sc.radius, other.radius);
        return (pos2 - pos1).sqrMagnitude <= r * r;
    }

    public static Vector3 getWorldCenter(this SphereCollider sc) {
        return sc.center + sc.transform.position;
    }

    public static Vector3 getWorldCenter(this BoxCollider sc) {
        return sc.center + sc.transform.position;
    }

    public static Vector3 getWorldCenter(this CapsuleCollider sc) {
        return sc.center + sc.transform.position;
    }

    public static Sprite setTexture(this Sprite s, Texture2D tex) {
        return Sprite.Create(tex, s.textureRect, s.pivot, s.pixelsPerUnit, 0, SpriteMeshType.FullRect, s.border);
    }

    public static VECTOR toFMODVector(this Vector3 vec) {
        var ret = new VECTOR {
            x = vec.x,
            y = vec.y,
            z = vec.z,
        };
        return ret;
    }

    public static Vector4 setXYZ(this Vector4 vec, Vector3 xyz) {
        vec.x = xyz.x;
        vec.y = xyz.y;
        vec.z = xyz.z;
        return new Vector4(xyz.x, xyz.y, xyz.z, vec.w);
    }

    public static Color exponent(this Color c, float exp) {
        return new Color(Mathf.Pow(c.r, exp), Mathf.Pow(c.g, exp), Mathf.Pow(c.b, exp), Mathf.Pow(c.a, exp));
    }

    public static Color asColor(this Vector3 c) {
        return new Color(c.x, c.y, c.z);
    }

    public static Vector3 exponent(this Vector3 c, float exp) {
        return new Vector3(Mathf.Pow(c.x, exp), Mathf.Pow(c.y, exp), Mathf.Pow(c.z, exp));
    }

    public static Vector3 modulo(this Vector3 c, float size) {
        return new Vector3((c.x % size + size) % size, (c.y % size + size) % size, (c.z % size + size) % size);
    }

    public static Color asColor(this Vector4 c) {
        return new Color(c.x, c.y, c.z, c.w);
    }

    public static Vector4 exponent(this Vector4 c, float exp) {
        return new Vector4(Mathf.Pow(c.x, exp), Mathf.Pow(c.y, exp), Mathf.Pow(c.z, exp), Mathf.Pow(c.w, exp));
    }

    public static Vector3 toVector(this Color c) {
        return new Vector3(c.r, c.g, c.b);
    }

    public static Vector4 toVectorA(this Color c) {
        return new Vector4(c.r, c.g, c.b, c.a);
    }

    public static int toARGB(this Color c) {
        var a = Mathf.RoundToInt(c.a * 255) & 0xFF;
        var r = Mathf.RoundToInt(c.r * 255) & 0xFF;
        var g = Mathf.RoundToInt(c.g * 255) & 0xFF;
        var b = Mathf.RoundToInt(c.b * 255) & 0xFF;
        return (a << 24) | (r << 16) | (g << 8) | b;
    }

    public static Color toColor(this Color32 c) {
        return new Color(c.r / 255F, c.g / 255F, c.b / 255F, c.a / 255F);
    }

    public static Color32 toColor32(this Color c) {
        return new Color32(
            (byte)Mathf.Round(c.r * 255F),
            (byte)Mathf.Round(c.g * 255F),
            (byte)Mathf.Round(c.b * 255F),
            (byte)Mathf.Round(c.a * 255F)
        );
    }

    public static Vector3 getXYZ(this Vector4 vec) {
        return new Vector3(vec.x, vec.y, vec.z);
    }

    public static Vector3 setLength(this Vector3 vec, double amt) {
        return vec.normalized * (float)amt;
    }

    public static Vector3 addLength(this Vector3 vec, double amt) {
        return vec.setLength(vec.magnitude + amt);
    }

    public static Vector3 setY(this Vector3 vec, double y) {
        return new Vector3(vec.x, (float)y, vec.z);
    }

    public static Vector3 Rotated(this Vector3 vector, Quaternion rotation, Vector3 pivot = default(Vector3)) {
        return rotation * (vector - pivot) + pivot;
    }

    public static Vector3 Rotated(this Vector3 vector, Vector3 rotation, Vector3 pivot = default(Vector3)) {
        return Rotated(vector, Quaternion.Euler(rotation), pivot);
    }

    public static Vector3 Rotated(this Vector3 vector, float x, float y, float z, Vector3 pivot = default(Vector3)) {
        return Rotated(vector, Quaternion.Euler(x, y, z), pivot);
    }

    public static XmlElement addProperty(this XmlNode xml, string name, Quaternion quat) {
        var n = xml.OwnerDocument.CreateElement(name);
        n.addProperty("x", quat.x);
        n.addProperty("y", quat.y);
        n.addProperty("z", quat.z);
        n.addProperty("w", quat.w);
        xml.AppendChild(n);
        return n;
    }

    public static XmlElement addProperty(this XmlNode xml, string name, Vector3 vec) {
        var n = xml.OwnerDocument.CreateElement(name);
        n.addProperty("x", vec.x);
        n.addProperty("y", vec.y);
        n.addProperty("z", vec.z);
        xml.AppendChild(n);
        return n;
    }

    public static XmlElement addProperty(this XmlNode xml, string name, Vector4 vec) {
        var quat = new Quaternion {
            x = vec.x,
            y = vec.y,
            z = vec.z,
            w = vec.w,
        };
        return addProperty(xml, name, quat);
    }

    public static XmlElement addProperty(this XmlNode xml, string name, Color c) {
        var n = xml.OwnerDocument.CreateElement(name);
        n.addProperty("r", c.r);
        n.addProperty("g", c.g);
        n.addProperty("b", c.b);
        n.addProperty("a", c.a);
        xml.AppendChild(n);
        return n;
    }

    public static XmlElement addProperty(this XmlNode xml, string name, int value) {
        return xml.addProperty(name, value.ToString(CultureInfo.InvariantCulture));
    }

    public static XmlElement addProperty(this XmlNode xml, string name, double value) {
        return xml.addProperty(name, value.ToString(CultureInfo.InvariantCulture));
    }

    public static XmlElement addProperty(this XmlNode xml, string name, bool value) {
        return xml.addProperty(name, value.ToString(CultureInfo.InvariantCulture));
    }

    public static XmlElement addProperty(this XmlNode xml, string name, string value = null) {
        var n = xml.OwnerDocument.CreateElement(name);
        if (value != null)
            n.InnerText = value;
        xml.AppendChild(n);
        return n;
    }

    public static double getFloat(this XmlElement xml, string name, double fallback) {
        var s = xml.getProperty(name, true);
        return string.IsNullOrEmpty(s)
            ? double.IsNaN(fallback) ? throw new Exception("No matching tag '" + name + "'! " + xml.format()) : fallback
            : double.Parse(xml.getProperty(name), CultureInfo.InvariantCulture);
    }

    public static int getInt(this XmlElement xml, string name, int fallback, bool allowFallback = true) {
        var s = xml.getProperty(name, allowFallback);
        var nul = string.IsNullOrEmpty(s);
        return nul && !allowFallback ? throw new Exception("No matching tag '" + name + "'! " + xml.format()) :
            nul ? fallback : int.Parse(s, CultureInfo.InvariantCulture);
    }

    public static bool getBoolean(this XmlElement xml, string name) {
        return xml.getBoolean(name, out var trash);
    }

    public static bool getBoolean(this XmlElement xml, string name, out XmlElement elem) {
        var prop = xml.getProperty(name, out elem, true);
        return !string.IsNullOrEmpty(prop) && bool.Parse(prop);
    }

    public static string getProperty(this XmlElement xml, string name, bool allowNull = false) {
        return xml.getProperty(name, out var trash, allowNull);
    }

    public static int getRandomInt(this XmlElement xml, string name) {
        var li = xml.getDirectElementsByTagName(name);
        if (li.Count == 1) {
            var min = li[0].getInt("min", 0, true);
            var max = li[0].getInt("max", -1, false);
            return UnityEngine.Random.Range(min, max);
        } else {
            throw new Exception(
                "You must have exactly one matching named element for getRandomInt '" + name + "'! " + xml.format()
            );
        }
    }

    public static float getRandomFloat(this XmlElement xml, string name) {
        var li = xml.getDirectElementsByTagName(name);
        if (li.Count == 1) {
            var min = li[0].getFloat("min", double.NaN);
            var max = li[0].getFloat("max", double.NaN);
            return UnityEngine.Random.Range((float)min, (float)max);
        } else {
            throw new Exception(
                "You must have exactly one matching named element for getRandomFloat '" + name + "'! " + xml.format()
            );
        }
    }

    public static string getProperty(this XmlElement xml, string name, out XmlElement elem, bool allowNull = false) {
        var li = xml.getDirectElementsByTagName(name);
        if (li.Count == 1) {
            elem = li[0];
            return li[0].InnerText;
        } else if (li.Count == 0 && allowNull) {
            elem = null;
            return null;
        } else {
            throw new Exception(
                "You must have exactly one matching named tag for getProperty '" + name + "'! " + xml.format()
            );
        }
    }

    public static Vector3? getVector(this XmlElement xml, string name, bool allowNull = false) {
        return getVector(xml, name, out var trash, allowNull);
    }

    public static Vector4? getVector4(this XmlElement xml, string name, bool allowNull = false) {
        var quat = getQuaternion(xml, name, allowNull);
        if (quat == null || !quat.HasValue)
            return null;
        var vec = new Vector4 {
            x = quat.Value.x,
            y = quat.Value.y,
            z = quat.Value.z,
            w = quat.Value.w,
        };
        return vec;
    }

    public static Vector3? getVector(this XmlElement xml, string name, out XmlElement elem, bool allowNull = false) {
        var li = xml.getDirectElementsByTagName(name);
        if (li.Count == 1) {
            var x = li[0].getFloat("x", double.NaN);
            var y = li[0].getFloat("y", double.NaN);
            var z = li[0].getFloat("z", double.NaN);
            elem = li[0];
            return new Vector3((float)x, (float)y, (float)z);
        } else if (li.Count == 0 && allowNull) {
            elem = null;
            return null;
        } else {
            throw new Exception(
                "You must have exactly one matching named element for getVector '" + name + "'! " + xml.format()
            );
        }
    }

    public static Quaternion? getQuaternion(this XmlElement xml, string name, bool allowNull = false) {
        var li = xml.getDirectElementsByTagName(name);
        if (li.Count == 1) {
            var x = li[0].getFloat("x", double.NaN);
            var y = li[0].getFloat("y", double.NaN);
            var z = li[0].getFloat("z", double.NaN);
            var w = li[0].getFloat("w", double.NaN);
            return new Quaternion((float)x, (float)y, (float)z, (float)w);
        } else {
            return li.Count == 0 && allowNull
                ? (Quaternion?)null
                : throw new Exception(
                    "You must have exactly one matching named element for getQuaternion '" + name + "'! " + xml.format()
                );
        }
    }

    public static Color? getColor(this XmlElement xml, string name, bool includeAlpha, bool allowNull = false) {
        var li = xml.getDirectElementsByTagName(name);
        if (li.Count == 1) {
            var r = li[0].getFloat("r", double.NaN);
            var g = li[0].getFloat("g", double.NaN);
            var b = li[0].getFloat("b", double.NaN);
            var a = includeAlpha ? li[0].getFloat("a", double.NaN) : 1;
            return new Color((float)r, (float)g, (float)b, (float)a);
        } else {
            return li.Count == 0 && allowNull
                ? (Color?)null
                : throw new Exception(
                    "You must have exactly one matching named element for getColor '" + name + "'! " + xml.format()
                );
        }
    }

    public static List<XmlElement> getDirectElementsByTagName(this XmlElement xml, string name) {
        List<XmlElement> li = [];
        foreach (XmlNode e in xml.ChildNodes) {
            if (e is XmlElement element && element.Name == name)
                li.Add(element);
        }

        return li;
    }

    public static XmlNodeList getAllChildrenIn(this XmlElement xml, string name) {
        var li = xml.getDirectElementsByTagName(name);
        return li.Count == 1 ? li[0].ChildNodes : null;
    }

    public static bool hasProperty(this XmlElement xml, string name) {
        var li = xml.getDirectElementsByTagName(name);
        return li.Count == 1;
    }

    public static string format(this XmlElement xml) {
        return xml.OuterXml;
    }

    public static XmlElement addChild(this XmlElement e, string name) {
        var e2 = e.OwnerDocument.CreateElement(name);
        e.AppendChild(e2);
        return e2;
    }

    public static Int3 roundToInt3(this Vector3 vec) {
        return new Int3((int)Mathf.Floor(vec.x), (int)Mathf.Floor(vec.y), (int)Mathf.Floor(vec.z));
    }

    public static bool isEnumerable(this object o) {
        return o != null && o is IEnumerable && o.GetType().IsGenericType &&
               o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(IEnumerable<>));
    }

    public static bool isList(this object o) {
        return o != null && o is IList && o.GetType().IsGenericType &&
               o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
    }

    public static bool isDictionary(this object o) {
        return o != null && o is IDictionary && o.GetType().IsGenericType &&
               o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
    }

    /*
    public static string toDebugString(this IDictionary<object, object> dict) {
        return "{" + string.Join(",", dict.Select(kv => kv.Key + "=" + stringify(kv.Value)).ToArray()) + "}";
    }

    public static string toDebugString(this IEnumerable<object> c) {
        return "[" + string.Join(",", c.Select<object, string>(stringify).ToArray()) + "]";
    }
    */
    public static string toDebugString<K, V>(this IDictionary<K, V> dict, string separator = ",") {
        return dict.Count + ":{" + string.Join(
            separator,
            dict.Select(kv => kv.Key + "=" + stringify(kv.Value)).ToArray()
        ) + "}"; //return toDebugString((IDictionary<object, object>)dict);
    }

    public static string toDebugString<E>(this IEnumerable<E> c, string separator = ",") {
        return c.Count() + ":[" + string.Join(separator, c.Select(e => stringify(e)).ToArray()) +
               "]"; //return toDebugString((IEnumerable<object>)c);
    }

    public static E pop<E>(this IList<E> c) {
        var ret = c[0];
        c.RemoveAt(0);
        return ret;
    }

    public static E getRandomEntry<E>(this IEnumerable<E> c) {
        if (c is IList<E> li)
            return li.Count == 0 ? default(E) : li.GetRandom();
        return c.ElementAt(UnityEngine.Random.Range(0, c.Count()));
    }

    public static Vector3 getClosest(this IEnumerable<Vector3> li, Vector3 pos) {
        var ret = Vector3.zero;
        var distSq = float.PositiveInfinity;
        foreach (var v in li) {
            var dd = (v - pos).sqrMagnitude;
            if (dd < distSq) {
                distSq = dd;
                ret = v;
            }
        }

        return ret;
    }

    public static E[] addToArray<E>(this E[] arr, E add) {
        List<E> li = [..arr.ToList(), add];
        return li.ToArray();
    }

    public static bool overlaps<E>(this ICollection<E> c, ICollection<E> other) {
        foreach (var e in c) {
            if (other.Contains(e)) {
                return true;
            }
        }

        return false;
    }

    public static string stringify(object obj) {
        if (obj == null)
            return "null";
        else if (obj.isDictionary())
            return "dict:" + ((IDictionary<object, object>)obj).toDebugString();
        else if (obj.isEnumerable())
            return "enumerable:" + ((IEnumerable<object>)obj).toDebugString();
        else if (obj is Attribute ar)
            return "attr '" + ar.GetType().Name + "'";
        return obj.ToString();
    }

    public static void copySprites(this UnityEngine.UI.Button b, UnityEngine.UI.Button other) {
        b.image = other.image;
        //UnityEngine.UI.Toggle tg1 = b.GetComponent<UnityEngine.UI.Toggle>();
        //UnityEngine.UI.Toggle tg2 = other.GetComponent<UnityEngine.UI.Toggle>();
        var sprs = other.spriteState;
        var hover = sprs.highlightedSprite;
        var img = other.GetComponent<UnityEngine.UI.Image>();
        var sprs2 = b.spriteState;
        sprs2.highlightedSprite = hover;
        sprs2.selectedSprite = hover;
        b.spriteState = sprs2;
        b.transition = other.transition;
    }

    public static T copyStruct<T>(this T comp, T from) where T : struct {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                    BindingFlags.FlattenHierarchy;

        foreach (var pinfo in typeof(T).GetProperties(flags)) {
            if (pinfo.CanWrite) {
                try {
                    pinfo.SetValue(comp, pinfo.GetValue(from, null), null);
                } catch {
                }
            }
        }

        foreach (var finfo in typeof(T).GetFields(flags)) {
            finfo.SetValue(comp, finfo.GetValue(from));
        }

        return comp;
    }

    public static T copyObject<T>(this T comp, T from) where T : class {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                    BindingFlags.FlattenHierarchy;

        foreach (var pinfo in typeof(T).GetProperties(flags)) {
            if (pinfo.CanWrite) {
                try {
                    pinfo.SetValue(comp, pinfo.GetValue(from, null), null);
                } catch {
                }
            }
        }

        foreach (var finfo in typeof(T).GetFields(flags)) {
            finfo.SetValue(comp, finfo.GetValue(from));
        }

        return comp;
    }

    public static string toDetailedString(this WaterscapeVolume.Settings s) {
        return
            $"Start={s.startDistance:0.0000}, Murk={s.murkiness:0.0000}, Absorb={s.absorption:0.0000}, AmbScale={s.ambientScale:0.0000}, Emissive={s.emissive.ToString("0.0000")}, EmisScale={s.emissiveScale:0.0000}, Scatter={s.scattering:0.0000}, ScatterColor={s.scatteringColor.ToString("0.0000")}, Sun={s.sunlightScale:0.0000}, Temp={s.temperature}";
    }

    public static float getLifespan(this StasisSphere s) {
        return s.time * s.fieldEnergy;
    }

    public static void clearAttackTarget(this AttackLastTarget a) {
        var c = a.GetComponent<Creature>();
        if (c)
            c.Aggression.Add(-1);
        a.StopAttack();
        a.currentTarget = null;
        a.lastTarget.SetTarget(null);
    }

    public static VehicleAccelerationModifier addSpeedModifier(this Vehicle v) {
        var ret = v.gameObject.AddComponent<VehicleAccelerationModifier>();
        v.accelerationModifiers = v.GetComponentsInChildren<VehicleAccelerationModifier>();
        return ret;
    }

    private static readonly Type craftDataPatcher =
        InstructionHandlers.getTypeBySimpleName("SMLHelper.V2.Patchers.CraftDataPatcher");

    private static readonly Type knownTechPatcher =
        InstructionHandlers.getTypeBySimpleName("SMLHelper.V2.Patchers.KnownTechPatcher");

    private static readonly Type craftTreePatcher =
        InstructionHandlers.getTypeBySimpleName("SMLHelper.V2.Patchers.CraftTreePatcher");

    private static readonly Type craftingNode =
        InstructionHandlers.getTypeBySimpleName("SMLHelper.V2.Crafting.CraftingNode");

    public static TechType getEgg(this TechType creature) {
        if (creature == TechType.None)
            return TechType.None;
        var e = CustomEgg.getEgg(creature);
        if (e != null)
            return e.creatureToSpawn;
        var name = creature.AsString();
        if (Enum.TryParse(name + "Egg", true, out TechType ret))
            return ret;
        return TechType.None;
    }

    public static TechType getCookedCounterpart(this TechType raw) {
        return TechData.GetProcessed(raw);
        // if (TechData.GetProcessed(raw) != null)
        //     return TechData.GetProcessed(raw);
        // IDictionary<TechType, TechType> dict = getPatcherDict<TechType>("CustomCookedCreatureList", craftDataPatcher);
        // if (dict != null && dict.ContainsKey(raw))
        //     return dict[raw];
        // else
        //     return TechType.None;
    }

    public static Vector2int getItemSize(this TechType item) {
        return TechData.GetItemSize(item);
        // if (TechData.GetItemSize(item) != null)
        // 	return TechData.GetItemSize(item);
        // IDictionary<TechType, Vector2int> dict = getPatcherDict<Vector2int>("CustomItemSizes", craftDataPatcher);
        // if (dict != null && dict.ContainsKey(item))
        // 	return dict[item];
        // else
        // 	return new Vector2int(1, 1);
    }

    private static IDictionary<TechType, E> getPatcherDict<E>(string name, Type from) {
        return (IDictionary<TechType, E>)from.GetField(
            name,
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
        ).GetValue(null);
    }

    public static void removeUnlockTrigger(this TechType item, ProgressionTrigger checkToDisable = null) {
        KnownTechHandler.RemoveAllCurrentAnalysisTechEntry(item);
        ((ICollection<TechType>)knownTechPatcher.GetField(
            "UnlockedAtStart",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
        ).GetValue(null)).Remove(item);
        if (checkToDisable != null)
            RecipeUtil.techsToRemoveIf[item] = checkToDisable;
    }

    public static void preventCraftNodeAddition(this TechType rec, CraftTree.Type tree = CraftTree.Type.Fabricator) {
        SNUtil.log("Removing all prepared craft nodes for '" + rec.AsString() + "'");

        var li = (IList)craftTreePatcher.GetField(
            "CraftingNodes",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
        ).GetValue(null);

        var ttP = craftingNode.GetProperty(
            "TechType",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );
        var pathP = craftingNode.BaseType.GetProperty(
            "Path",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        var any = false;
        for (var i = li.Count - 1; i >= 0; i--) {
            var o = li[i];
            if (o == null)
                continue;
            var tt = (TechType)ttP.GetValue(o);
            if (tt == rec) {
                li.RemoveAt(i);
                var path = (string[])pathP.GetValue(o);
                if (path == null)
                    path = [];
                path = path.addToArray(tt.AsString());
                SNUtil.log("Removing craft node " + o.ToString() + " @ " + string.Join("/", path));
                CraftTreeHandler.RemoveNode(tree, path);
                any = true;
            }
        }

        if (!any) {
            var nodelist = "";
            foreach (var o in li) {
                if (o == null)
                    continue;
                var tt = (TechType)ttP.GetValue(o);
                var path = (string[])pathP.GetValue(o);
                if (path == null)
                    path = [];
                path = path.addToArray(tt.AsString());
                nodelist += tt.AsString() + " @ " + string.Join("/", path) + "\n";
            }

            SNUtil.log("No craft nodes for '" + rec.AsString() + "' found! Queued Nodes:\n" + nodelist);
        }
    }
}