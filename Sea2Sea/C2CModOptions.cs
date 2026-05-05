using System;
using System.Collections.Generic;
using Nautilus.Options;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class C2CModOptions : ModOptions {
    private readonly Dictionary<string, Keybind> bindings = new Dictionary<string, Keybind>();

    public static readonly string PROPGUNSWAP = "PropGunSwap";

    public C2CModOptions() : base(SeaToSeaMod.MOD_KEY.from('.')) {
        addBinding(PROPGUNSWAP, "(Pro/Re)pulsion Gun Swap", KeyCode.Backslash);
    }

    private void addBinding(string id, string name, KeyCode def) {
        var opt = ModChoiceOption<KeyCode>.Create(id, name, (KeyCode[])typeof(KeyCode).GetEnumValues(), def);

        opt.OnChanged += (s, e) => { bindings[e.Id].selectedKey = e.Key; };
        AddItem(opt);

        bindings[id] = new Keybind(id, def);
    }

    public KeyCode getBinding(string id) {
        return bindings.ContainsKey(id) ? bindings[id].selectedKey : KeyCode.None;
    }

    private class Keybind {
        public readonly string optionID;
        public KeyCode selectedKey;

        internal Keybind(string s, KeyCode def) {
            optionID = s;
            selectedKey = def;
        }
    }
}