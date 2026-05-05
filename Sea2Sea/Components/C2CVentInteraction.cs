using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class C2CVentInteraction : MonoBehaviour, IHandTarget {
    public void OnHandClick(GUIHand hand) {
        if (C2CProgression.instance.isPipeTravelEnabled(out bool invis)) {
            PipeTravelSystem.requestTravel(GetComponent<PrefabIdentifier>());
        } else if (!invis) {
            SoundManager.playSound("event:/env/keypad_wrong");
        }
    }

    public void OnHandHover(GUIHand hand) {
        if (C2CProgression.instance.isPipeTravelEnabled(out bool invis)) {
            HandReticle.main.SetText(HandReticle.TextType.Use, "VentClick", true); //is a locale key
            HandReticle.main.SetIcon(HandReticle.IconType.Interact);
        } else if (!invis) {
            HandReticle.main.SetText(HandReticle.TextType.Use, "VentClickDeny", true); //is a locale key
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny);
        }
    }
}