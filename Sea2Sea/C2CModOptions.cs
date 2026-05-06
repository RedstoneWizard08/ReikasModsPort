using System;
using System.Collections.Generic;
using Nautilus.Options;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class C2CModOptions : ModOptions {
    private readonly Dictionary<string, Keybind> _bindings = new();

    public const string PropGunSwap = "PropGunSwap";

    public C2CModOptions() : base(SeaToSeaMod.ModKey.From('.')) {
        AddBinding(PropGunSwap, "(Pro/Re)pulsion Gun Swap", KeyCode.Backslash);
    }

    private void AddBinding(string id, string name, KeyCode def) {
        var opt = ModChoiceOption<KeyCode>.Create(id, name, (KeyCode[])typeof(KeyCode).GetEnumValues(), def);

        opt.OnChanged += (s, e) => { _bindings[e.Id].SelectedKey = e.Value; };
        AddItem(opt);

        _bindings[id] = new Keybind(id, def);
    }

    public KeyCode GetBinding(string id) {
        return _bindings.TryGetValue(id, out var binding) ? binding.SelectedKey : KeyCode.None;
    }

    private class Keybind {
        public readonly string OptionID;
        public KeyCode SelectedKey;

        internal Keybind(string s, KeyCode def) {
            OptionID = s;
            SelectedKey = def;
        }
    }
}