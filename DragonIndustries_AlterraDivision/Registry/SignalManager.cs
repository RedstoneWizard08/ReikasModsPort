using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Nautilus.Assets;
using Nautilus.Handlers;
using Story;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class SignalManager {
    private static readonly Dictionary<string, ModSignal> signals = new();
    private static readonly Dictionary<PingType, string> types = new();

    static SignalManager() {
    }

    public static ModSignal getSignal(string id) {
        return signals.ContainsKey(id) ? signals[id] : null;
    }

    public static ModSignal createSignal(XMLLocale.LocaleEntry text) {
        return createSignal(text.key, text.name, text.desc, text.pda, text.getString("radio"));
    }

    public static ModSignal createSignal(string id, string name, string desc, string pda, string prompt) {
        if (signals.ContainsKey(id))
            throw new Exception("Signal ID '" + id + "' already in use!");
        var sig = new ModSignal(id, name, desc, pda, prompt);
        signals[sig.id] = sig;
        SNUtil.Log("Constructed signal " + sig);
        return sig;
    }

    public class ModSignal {
        public readonly string id;
        public readonly string name;
        public readonly string longName;
        public readonly string radioText;

        public readonly PDAManager.PDAPage pdaEntry;

        private StoryGoal radioMessage;

        public readonly Assembly ownerMod;

        public string storyGate { get; private set; }

        public Sprite icon { get; private set; }

        public Vector3 initialPosition { get; private set; }
        public float maxDistance { get; private set; }

        public PingType signalType { get; private set; }
        internal SignalHolder signalHolder { get; private set; }
        internal GenericSignalHolder genericSignalHolder { get; private set; }

        internal PingInstance signalInstance;
        internal SignalInitializer initializer;

        internal ModSignal(string id, string n, string desc, string pda, string prompt) {
            this.id = "signal_" + id;
            name = n;
            longName = desc;
            radioText = prompt;

            pdaEntry = string.IsNullOrEmpty(pda)
                ? null
                : PDAManager.createPage("signal_" + id, longName, pda, "DownloadedData");

            ownerMod = SNUtil.TryGetModDLL();
        }

        public ModSignal addRadioTrigger(string soundPath) {
            return addRadioTrigger(
                SoundManager.registerPDASound(SNUtil.TryGetModDLL(), "radio_" + id, soundPath).asset
            );
        }

        public ModSignal addRadioTrigger(FMODAsset sound) {
            setStoryGate("radio_" + id);
            radioMessage = SNUtil.AddRadioMessage(storyGate, radioText, sound);
            return this;
        }

        public ModSignal setStoryGate(string key) {
            storyGate = key;
            return this;
        }

        public ModSignal move(Vector3 pos) {
            initialPosition = pos;
            return this;
        }

        public void register(string pfb, Vector3 pos, float maxDist = -1) {
            register(pfb, SpriteManager.Get(SpriteManager.Group.Pings, "Signal"), pos, maxDist);
        }

        public void register(string pfb, Sprite icon, Vector3 pos, float maxDist = -1) {
            if (icon == null || icon == SpriteManager.defaultSprite)
                throw new Exception("Null icon is not allowed");
            signalType = EnumHandler.AddEntry<PingType>(id).WithIcon(icon);
            types[signalType] = id;
            CustomLocaleKeyDatabase.registerKey(id, "Signal");
            this.icon = icon;

            initialPosition = pos;
            maxDistance = maxDist;

            signalHolder = new SignalHolder(pfb, this).registerPrefab();
            genericSignalHolder = new GenericSignalHolder(this);
            genericSignalHolder.Register();

            if (pdaEntry != null)
                pdaEntry.register();
            SNUtil.Log("Registered signal " + this);
        }

        public GameObject spawnGenericSignalHolder(Vector3 pos) {
            var go = genericSignalHolder.GetGameObject();
            go.SetActive(true);

            go.transform.position = pos;
            var pi = go.GetComponent<PingInstance>();
            pi.origin = go.transform;
            pi.displayPingInManager = true;
            pi.SetVisible(true);
            return go;
        }

        public void addWorldgen(Quaternion? rot = null) {
            GenUtil.registerWorldgen(signalHolder.ClassID, initialPosition, rot);
        }

        public PingInstance attachToObject(GameObject go) {
            var lw = go.EnsureComponent<LargeWorldEntity>();
            lw.cellLevel = LargeWorldEntity.CellLevel.Global;

            go.SetActive(false);
            go.transform.position = initialPosition;

            signalInstance = go.EnsureComponent<PingInstance>();
            signalInstance.pingType = signalType;
            signalInstance.colorIndex = 0;
            signalInstance.origin = go.transform;
            signalInstance.minDist = 18;
            // signalInstance.maxDist = maxDistance >= 0 ? maxDistance : signalInstance.minDist;
            signalInstance.SetLabel(longName);

            var flag = true;
            if (storyGate != null)
                flag = StoryGoalManager.main.completedGoals.Contains(storyGate);

            signalInstance.displayPingInManager = flag;
            signalInstance.SetVisible(flag);

            initializer = go.EnsureComponent<SignalInitializer>();
            initializer.ping = signalInstance;
            initializer.signal = this;

            SNUtil.Log(
                "Initialized GO holder for signal " + id + " [" + flag + "]: " + go + " @ " + go.transform.position,
                SNUtil.DiDLL
            );

            go.SetActive(true);

            return signalInstance;
        }

        public void fireRadio() {
            if (radioMessage != null)
                StoryGoal.Execute(storyGate, radioMessage.goalType); //radioMessage.Trigger();
        }

        public bool isRadioFired() {
            return !string.IsNullOrEmpty(storyGate) && StoryGoalManager.main.completedGoals.Contains(storyGate);
        }

        public void activate(int delay = 0) {
            if (!signalInstance) {
                SNUtil.Log("Cannot disable mod signal " + this + " because it has no object/instance!");
                return;
            }

            var already = signalInstance.enabled;
            signalInstance.displayPingInManager = true;
            signalInstance.enabled = true;
            signalInstance.SetVisible(true);

            if (already)
                return;

            if (delay > 0)
                initializer.Invoke(nameof(SignalInitializer.triggerFX), delay);
            else
                initializer.triggerFX();

            if (pdaEntry != null)
                pdaEntry.unlock(false);
        }

        public void deactivate() { //Will not remove the PDA entry!
            if (!signalInstance)
                return;
            //signalInstance.displayPingInManager = false;
            signalInstance.enabled = false;
            signalInstance.SetVisible(false);
        }

        public bool isActive() {
            return signalInstance && signalInstance.isActiveAndEnabled;
        }

        public override string ToString() {
            return
                $"[ModSignal Id={id}, Name={name}, LongName={longName}, Radio={radioText}, PdaEntry={pdaEntry}, Icon={icon}, Mod={ownerMod}]";
        }
    }

    internal class SignalInitializer : MonoBehaviour {
        internal PingInstance ping;

        internal ModSignal signal;

        private void Start() {
            if (ping == null) {
                //SNUtil.log("Ping was null, refetch");
                ping = gameObject.GetComponentInParent<PingInstance>();
                //SNUtil.log("TT is now "+ping.pingType);
            }

            if (ping != null && signal == null) {
                //SNUtil.log("Signal was null, refetch");
                signal = getSignal(types[ping.pingType]);
            }

            SNUtil.Log("Starting signal init of " + signal + " / " + ping, SNUtil.DiDLL);
            signal.signalInstance = ping;
            signal.initializer = this;
            ping.SetLabel(signal.longName);

            var available = signal.storyGate == null ||
                            StoryGoalManager.main.completedGoals.Contains(signal.storyGate);
            ping.displayPingInManager = available;
            if (!available)
                ping.SetVisible(false);
        }

        internal void triggerFX() {
            SNUtil.Log("Firing signal unlock FX: " + signal.id);
            SoundManager.playSound("event:/player/signal_upload"); //"signal location uploaded to PDA"
            Subtitles.main.AddRawLongInternal(0, new StringBuilder("Signal location uploaded to PDA."), 0, 6, 6);
            //SNUtil.playSound("event:/tools/scanner/new_encyclopediea"); //triple-click	
        }
    }

    internal class SignalHolder : GenUtil.CustomPrefabImpl {
        private readonly ModSignal signal;

        [SetsRequiredMembers]
        internal SignalHolder(string template, ModSignal s) : base("signalholder_" + s.id, template) {
            signal = s;
        }

        public override void prepareGameObject(GameObject go, Renderer[] r) {
            signal.attachToObject(go);
        }

        internal SignalHolder registerPrefab() {
            Register();
            return this;
        }
    }

    internal class GenericSignalHolder : CustomPrefab {
        private readonly ModSignal signal;

        [SetsRequiredMembers]
        internal GenericSignalHolder(ModSignal s) : base("genericsignalholder_" + s.id, "", "") {
            signal = s;
            SetGameObject(GetGameObject);
        }

        public GameObject GetGameObject() {
            var go = new GameObject("Signal_" + signal.id + "(Clone)");
            go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
            go.EnsureComponent<TechTag>().type = Info.TechType;
            go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;

            var ping = go.EnsureComponent<PingInstance>();
            ping.pingType = signal.signalType; //PingType.Beacon;
            EnumHandler.AddEntry<PingType>("").WithIcon(null);
            //ping.displayPingInManager = false;
            ping.colorIndex = 0;
            ping.origin = go.transform;
            ping.minDist = 18f;
            // ping.maxDist = 1;

            var si = go.EnsureComponent<GenericSignalInitializer>();
            si.ping = ping;
            si.signal = signal;

            return go;
        }
    }

    internal class GenericSignalInitializer : MonoBehaviour {
        internal PingInstance ping;

        internal ModSignal signal;

        private void Start() {
            if (ping == null) {
                //SNUtil.log("Ping was null, refetch");
                ping = gameObject.FindAncestor<PingInstance>();
                //SNUtil.log("TT is now "+ping.pingType);
            }

            if (ping != null && signal == null) {
                //SNUtil.log("Signal was null, refetch");
                signal = getSignal(types[ping.pingType]);
            }

            SNUtil.Log("Starting signal init of " + signal + " / " + ping, SNUtil.DiDLL);
            ping.SetLabel(signal.longName);

            var available = signal.storyGate == null ||
                            StoryGoalManager.main.completedGoals.Contains(signal.storyGate);
            ping.displayPingInManager = available;
            if (!available)
                ping.SetVisible(false);
        }
    }
}