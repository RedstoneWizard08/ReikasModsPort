using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public static class WaterCurrent {
    public static void register() {
        var basic = new WaterCurrentBasic();
        basic.Register();
        new WaterCurrentStrong().Register();
        new WaterCurrentHot().Register();
        new WaterCurrentHotStrong().Register();
        new WaterCurrentImpassable().Register();

        XMLLocale.LocaleEntry e = EcoceanMod.locale.getEntry("WaterCurrent");
        EcoceanMod.waterCurrentCommon = EnumHandler.AddEntry<TechType>(e.key).WithPdaInfo(e.name, e.desc);
        SNUtil.AddPdaEntry(
            basic,
            5,
            e.getString("category"),
            e.pda,
            e.getString("header"),
            d => d.key = EcoceanMod.waterCurrentCommon
        );
    }
}

public abstract class WaterCurrentBase<T> : CustomPrefab where T : WaterCurrentTag {
    [SetsRequiredMembers]
    internal WaterCurrentBase() : base(
        "WaterCurrent_" + typeof(T).Name.Replace("CurrentTag", ""),
        "Water Current",
        ""
    ) {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        GameObject world = ObjectUtil.createWorldObject("42b38968-bd3a-4bfd-9d93-17078d161b29")
            .setName(Info.ClassID + "[Clone]");
        world.EnsureComponent<TechTag>().type = EcoceanMod.waterCurrentCommon;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        world.removeChildObject("xCurrenBubbles");
        world.EnsureComponent<TechTag>().type = EcoceanMod.waterCurrentCommon;
        world.layer = LayerID.Useable;
        world.EnsureComponent<T>();
        return world;
    }
}

public class WaterCurrentBasic : WaterCurrentBase<BasicCurrentTag> {
    [SetsRequiredMembers]
    public WaterCurrentBasic() {
    }
}

public class WaterCurrentStrong : WaterCurrentBase<StrongCurrentTag> {
    [SetsRequiredMembers]
    public WaterCurrentStrong() {
    }
}

public class WaterCurrentHot : WaterCurrentBase<HotCurrentTag> {
    [SetsRequiredMembers]
    public WaterCurrentHot() {
    }
}

public class WaterCurrentHotStrong : WaterCurrentBase<HotStrongCurrentTag> {
    [SetsRequiredMembers]
    public WaterCurrentHotStrong() {
    }
}

public class WaterCurrentImpassable : WaterCurrentBase<ImpassableCurrentTag> {
    [SetsRequiredMembers]
    public WaterCurrentImpassable() {
    }
}

public class BasicCurrentTag : WaterCurrentTag {
    internal BasicCurrentTag() : base(false, 10) {
    }
}

public class HotCurrentTag : WaterCurrentTag {
    internal HotCurrentTag() : base(true, 10) {
    }
}

public class StrongCurrentTag : WaterCurrentTag {
    internal StrongCurrentTag() : base(false, 18) {
    }
}

public class ImpassableCurrentTag : WaterCurrentTag {
    internal ImpassableCurrentTag() : base(false, 27.5F) {
    }
}

public class HotStrongCurrentTag : WaterCurrentTag {
    internal HotStrongCurrentTag() : base(true, 18) {
    }
}

public abstract class WaterCurrentTag : MonoBehaviour {
    internal readonly bool isHotWater;
    internal readonly float currentStrength;

    private float age;

    internal WaterCurrentTag(bool temp, float str) {
        isHotWater = temp;
        currentStrength = str;
    }

    private Current current;
    private Renderer render;

    private void Update() {
        if (!current)
            current = GetComponent<Current>();
        if (!render)
            render = GetComponentInChildren<Renderer>();

        age += Time.deltaTime;

        if (age > 1 && age < 2 && Vector3.Distance(transform.position, Vector3.zero) <= 10) {
            gameObject.destroy(false);
            return;
        }

        current.objectForce = currentStrength;
        current.activeAtDay = true;
        current.activeAtNight = true;
        if (isHotWater) {
            foreach (var rb in current.rigidbodyList) {
                if (rb.isPlayer()) {
                    rb.gameObject.FindAncestor<LiveMixin>().TakeDamage(
                        4 * Time.deltaTime,
                        rb.transform.position,
                        DamageType.Heat,
                        gameObject
                    );
                }
            }

            var c = new Color(1.25F, 1, 1);
            render.materials[0].SetColor("_Color", c);
            render.materials[0].color = c;
        } else if (currentStrength >= 20) {
            var c = new Color(1F, 1, 1.25F);
            render.materials[0].SetColor("_Color", c);
            render.materials[0].color = c;
        }
    }

    public float getCurrentStrength(Vector3 pos) {
        var len = transform.localScale.z * 21 / 2F + 2.5F;
        var pt1 = transform.position + transform.forward * len;
        var pt2 = transform.position - transform.forward * len;
        if (isInCylinder(pos, pt1, pt2))
            return 1;
        var dist = Mathf.Min(Vector3.Distance(pos, pt1), Vector3.Distance(pos, pt2));
        var f = dist / (64F * transform.localScale.z);
        if (currentStrength >= 20)
            f *= 0.75F;
        return f >= 1 ? 0 : Mathf.Sqrt(1 - f);
    }

    internal bool isInCylinder(Vector3 pos, Vector3 pt1, Vector3 pt2) {
        var vec = pt2 - pt1;
        return Vector3.Dot(pos - pt1, vec) >= 0 && Vector3.Dot(pos - pt2, vec) <= 0;
    }

    private void OnDestroy() {
    }

    private void OnDisable() {
    }
}