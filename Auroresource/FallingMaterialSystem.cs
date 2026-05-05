using System;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.Auroresource;

public class FallingMaterialSystem {
    public static readonly FallingMaterialSystem Instance = new();

    private static readonly SoundManager.SoundData EntrySound = SoundManager.registerSound(
        AuroresourceMod.ModDLL,
        "debrisentry",
        "Sounds/debris-entry.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 9999); }
    );

    private static readonly SoundManager.SoundData AlertSound = SoundManager.registerSound(
        AuroresourceMod.ModDLL,
        "debrisalert",
        "Sounds/debris-alert.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 9999); }
    );

    internal static readonly SoundManager.SoundData SplashSound = SoundManager.registerSound(
        AuroresourceMod.ModDLL,
        "debrissplash",
        "Sounds/debris-splash.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 9999); }
    );

    private static readonly Vector3 MountainIslandPoint1 = new(360, 0, 1040);
    private static readonly Vector3 MountainIslandPoint2 = new(347, 0, 909);
    private static readonly float MountainIslandPointRadius = 80;
    private static readonly Vector3 FloatingIslandCenter = new(-747, 0, -1061);
    private static readonly float FloatingIslandRadius = 150;

    private readonly WeightedRandom<TechType> _items = new WeightedRandom<TechType>();

    internal FallingMaterial FallingMaterial;
    internal FallingMaterialSpawner FallingMaterialSpawner;

    private SignalManager.ModSignal _signal;

    private float _nextReEntry = -1;

    private FallingMaterialSpawnerTag _currentSpawner;
    private FallingMaterialCountdownTag _countdown;

    public string TimerText { get; private set; }

    public static event Action<GameObject, float> TimerBeginEvent;
    public static event Action<GameObject> EntryEvent;
    public static event Action<GameObject, Pickupable> ImpactEvent;

    private FallingMaterialSystem() {
    }

    internal void Register() {
        var e = AuroresourceMod.Locale.getEntry("FallingMaterialSpawner");
        TimerText = e.getString("timer");

        FallingMaterial = new FallingMaterial();
        FallingMaterial.Register();
        FallingMaterialSpawner = new FallingMaterialSpawner(e);
        FallingMaterialSpawner.Register();

        _signal = SignalManager.createSignal(e);
        _signal.register(
            null, /*SpriteManager.Get(SpriteManager.Group.Pings, "Sunbeam")*/
            TextureManager.getSprite(AuroresourceMod.ModDLL, "Textures/impact-signal"),
            Vector3.zero
        );
    }

    public void AddMaterial(TechType item, float weight) {
        _items.addEntry(item, weight);
    }

    public void Clear() {
        _items.clear();
    }

    internal void Tick(float time, float dT) {
        if (DIHooks.GetWorldAge() < 1)
            return;
        if (_items.isEmpty())
            return;

        if (!_countdown) {
            var find = UnityEngine.Object.FindObjectOfType<uGUI_SunbeamCountdown>();
            if (find) {
                var go2 = find.gameObject.clone();
                _countdown = go2.EnsureComponent<FallingMaterialCountdownTag>();
                var gui = go2.GetComponent<uGUI_SunbeamCountdown>();
                _countdown.TimerText = gui.countdownText;
                _countdown.TitleText = gui.countdownTitle;
                _countdown.TitleText.text = TimerText;
                _countdown.Holder = gui.countdownHolder;
                _countdown.gameObject.name = "FallingMaterialCountdown";
                _countdown.transform.SetParent(find.transform.parent);
                _countdown.transform.position = find.transform.position;
                _countdown.transform.rotation = find.transform.rotation;
                _countdown.transform.localScale = find.transform.localScale;
                _countdown.Holder.transform.position = find.countdownHolder.transform.position;
                _countdown.Holder.transform.rotation = find.countdownHolder.transform.rotation;
                _countdown.Holder.transform.localScale = find.countdownHolder.transform.localScale;
                go2.removeComponent<uGUI_SunbeamCountdown>();
            }
        }

        if (_countdown) {
            if (_currentSpawner)
                _countdown.SetTime(_currentSpawner.NumberToSpawn, _currentSpawner.TimeLeft);
            else
                _countdown.Holder.SetActive(false);
        }

        if (_nextReEntry <= 0) {
            ScheduleNextReEntry(time);
        } else if (time >= _nextReEntry && IsPlayerInValidBiome()) {
            //spawnItem();
            QueueSpawn();
            ScheduleNextReEntry(time);
        }
    }

    internal bool IsPlayerInValidBiome() {
        var pos = Player.main.transform.position;
        if (Player.main.precursorOutOfWater || pos.y < -600 || Creature.prisonAquriumBounds.Contains(pos))
            return false;
        var bb = BiomeBase.GetBiome(pos);
        return bb != VanillaBiomes.Void && bb != VanillaBiomes.Lostriver && bb != VanillaBiomes.Cove &&
               bb != VanillaBiomes.Ilz && bb != VanillaBiomes.Alz;
    }

    internal void QueueSpawn() {
        if (_items.isEmpty())
            return;
        if (_currentSpawner)
            return;
        var go = ObjectUtil.createWorldObject(FallingMaterialSpawner.ClassID);
        go.transform.position = SelectRandomPosition();
        _currentSpawner = go.EnsureComponent<FallingMaterialSpawnerTag>();
        _currentSpawner.Initialize();
        _countdown.SetTime(_currentSpawner.NumberToSpawn, _currentSpawner.TimeLeft);
        TimerBeginEvent?.Invoke(_currentSpawner.gameObject, _currentSpawner.TimeLeft);
    }

    private Vector3 SelectRandomPosition() {
        var sel = MathUtil.getRandomVectorAround(
            Player.main.transform.position.setY(0),
            new Vector3(1200, 0, 1200)
        );
        while (!IsValidPosition(sel)) {
            sel = MathUtil.getRandomVectorAround(Player.main.transform.position.setY(0), new Vector3(1200, 0, 1200));
        }

        return sel.setY(-2);
    }

    public bool IsValidPosition(Vector3 sel) {
        return !VanillaBiomes.Void.IsInBiome(sel.setY(-5)) && !IsCloseToExclusion(sel);
    }

    private bool IsCloseToExclusion(Vector3 sel) {
        return Vector3.Distance(sel, FloatingIslandCenter) <= FloatingIslandRadius ||
               MathUtil.getDistanceToLineSegment(sel, MountainIslandPoint1, MountainIslandPoint2) <=
               MountainIslandPointRadius || WorldUtil.isInsideAurora2D(sel);
    }

    public float GetTimeUntilNextEntry() {
        return _nextReEntry - DayNightCycle.main.timePassedAsFloat;
    }

    private void ScheduleNextReEntry(float time) {
        _nextReEntry = time + UnityEngine.Random.Range(20F, 60F) * 60 /
            AuroresourceMod.ModConfig.getFloat(ARConfig.ConfigEntries.REENTRY_RATE); //default every 20-60 min
    }

    internal void SpawnItem() {
        SpawnItem(MathUtil.getRandomVectorAround(Vector3.zero, new Vector3(1500, 0, 1500)));
    }

    internal void SpawnItem(Vector3 pos, int num = 1) {
        if (_items.isEmpty() || !Player.main)
            return;
        var soundPos = Vector3.zero;
        for (var i = 0; i < num; i++) {
            var go = ObjectUtil.createWorldObject(FallingMaterial.ClassID);
            if (!go)
                return;
            go.transform.position = MathUtil.getRandomVectorAround(pos, new Vector3(100, 0, 100))
                .setY(UnityEngine.Random.Range(500F, 1500F));
            soundPos = go.transform.position;
            foreach (var p in go.GetComponentsInChildren<ParticleSystem>()) {
                if (p)
                    p.Play();
            }

            var item = ObjectUtil.createWorldObject(_items.getRandomEntry());
            if (!item)
                return;
            item.transform.SetParent(go.transform);
            item.transform.localPosition = Vector3.zero;
            var tag = go.EnsureComponent<FallingMaterialTag>();
            tag.Velocity = MathUtil.getRandomVectorAround(Vector3.zero, 20).setY(-24);
            EntryEvent?.Invoke(go);
        }

        var playerPos = Player.main.transform.position;
        if (playerPos.y >= -50 || (pos.setY(playerPos.y) - playerPos).magnitude < 200)
            SoundManager.playSoundAt(EntrySound, soundPos, false, 9999);
        if (_signal != null)
            _signal.deactivate();
        else
            SNUtil.log("Could not deactivate null signal in FallingMaterial::spawnItem");
        if (_currentSpawner)
            _currentSpawner.gameObject.destroy(false);
        _currentSpawner = null;
        if (_countdown && _countdown.Holder)
            _countdown.Holder.SetActive(false);
    }

    internal void ModifyScannableList(uGUI_MapRoomScanner gui) {
        if (HasFinderUpgrade(gui)) {
            gui.availableTechTypes.Clear();
            gui.availableTechTypes.Add(FallingMaterialSpawner.TechType);
        } else {
            gui.availableTechTypes.Remove(FallingMaterialSpawner.TechType);
        }
    }

    internal void TickMapRoom(MapRoomFunctionality map) {
        //SNUtil.writeToChat("Tick map room "+map+" @ "+map.transform.position+" > "+map.scanActive+" & "+map.typeToScan+" & "+hasFinderUpgrade(map)+" OF "+currentSpawner);
        if (map.scanActive && map.typeToScan == FallingMaterialSpawner.TechType && HasFinderUpgrade(map)) {
            if (_currentSpawner) {
                _signal.move(_currentSpawner.transform.position);
                _signal.attachToObject(_currentSpawner.gameObject);
                _signal.activate();
                if (!_countdown.Holder.activeSelf)
                    SoundManager.playSoundAt(AlertSound, _currentSpawner.transform.position, false, 9999);
                _countdown.Holder.SetActive(true);
            }
        }
    }

    internal bool HasFinderUpgrade(uGUI_MapRoomScanner gui) {
        return HasFinderUpgrade(gui.mapRoom);
    }

    internal bool HasFinderUpgrade(MapRoomFunctionality map) {
        return map.storageContainer.container.GetCount(AuroresourceMod.MeteorDetector.TechType) > 0;
    }

    internal void Impact(FallingMaterialTag tag, Pickupable pp) {
        ImpactEvent?.Invoke(tag.gameObject, pp);
    }
}

public class FallingMaterial : CustomPrefab {
    [SetsRequiredMembers]
    internal FallingMaterial() : base("FallingMaterial", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var go = new GameObject("FallingMaterial(Clone)");
        var meteor = VFXSunbeam.main.burningChunkPrefabs[1].clone();
        meteor.transform.SetParent(go.transform);
        meteor.transform.localPosition = Vector3.zero;
        meteor.transform.rotation = Quaternion.Euler(90, UnityEngine.Random.Range(0F, 360F), 0);
        meteor.removeComponent<VFXStopAfterSeconds>();
        meteor.removeComponent<VFXFallingChunk>();
        go.EnsureComponent<PrefabIdentifier>().classId = Info.ClassID;
        go.EnsureComponent<TechTag>().type = Info.TechType;
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
        go.EnsureComponent<FallingMaterialTag>();
        return go;
    }
}

public class FallingMaterialSpawner : CustomPrefab {
    [SetsRequiredMembers]
    internal FallingMaterialSpawner(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
        SetGameObject(GetGameObject);
        AddOnRegister(() => {
                SaveSystem.addSaveHandler(
                    Info.ClassID,
                    new SaveSystem.ComponentFieldSaveHandler<FallingMaterialSpawnerTag>().addField("timeLeft")
                );
            }
        );
    }

    public GameObject GetGameObject() {
        var go = new GameObject("FallingMaterialSpawner(Clone)");
        var pi = go.EnsureComponent<PrefabIdentifier>();
        pi.classId = Info.ClassID;
        go.EnsureComponent<TechTag>().type = Info.TechType;
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
        go.EnsureComponent<FallingMaterialSpawnerTag>();
        /*
        ResourceTracker rt = go.EnsureComponent<ResourceTracker>();
        rt.techType = TechType;
        rt.overrideTechType = TechType;
        rt.prefabIdentifier = pi;
        rt.pickupable = null;
        rt.rb = null;
        */
        return go;
    }

    protected Sprite GetItemSprite() {
        return TextureManager.getSprite(AuroresourceMod.ModDLL, "Textures/falling-material");
    }
}

internal class FallingMaterialTag : MonoBehaviour {
    internal Vector3 Velocity = Vector3.down * 24;

    private bool _isDestroyed;

    private void Update() {
        if (Ocean.GetDepthOf(gameObject) > 1) {
            Velocity *= 0.88F;
            if (!_isDestroyed && Velocity.magnitude < 0.25) {
                _isDestroyed = true;
                foreach (var p in GetComponentsInChildren<ParticleSystem>())
                    p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                var pp = GetComponentInChildren<Pickupable>();
                pp.transform.SetParent(null);
                gameObject.destroy(false, 3);
                var dist = Vector3.Distance(Player.main.transform.position, transform.position);
                if (Player.main.transform.position.y >= -100 || dist <= 200) {
                    var depthvol = MathUtil.linterpolate(-Player.main.transform.position.y, 50, 200, 1, 0, true);
                    var distvol = MathUtil.linterpolate(dist, 200, 350, 1, 0, true);
                    var vol = (float)Math.Max(depthvol, distvol);
                    SoundManager.playSoundAt(FallingMaterialSystem.SplashSound, transform.position, false, 9999, vol);
                }

                FallingMaterialSystem.Instance.Impact(this, pp);
            }
        } else {
            var dT = Time.deltaTime;
            transform.position += Velocity * dT;
            transform.up = -Velocity.normalized;
            Velocity += Vector3.down * dT * 2;
        }
    }
}

internal class FallingMaterialSpawnerTag : MonoBehaviour {
    public float TimeLeft { get; private set; }

    public int NumberToSpawn { get; private set; }

    internal void Initialize() {
        TimeLeft = UnityEngine.Random.Range(5F, 15F) * 60 *
                   AuroresourceMod.ModConfig.getFloat(ARConfig.ConfigEntries.REENTRY_WARNING);
        NumberToSpawn = Math.Max(1, UnityEngine.Random.Range(1, 6) - 2); //1, 1, 1, 2, 3
    }

    private void Update() {
        if (TimeLeft >= 0) {
            TimeLeft -= Time.deltaTime;
            if (TimeLeft <= 0) {
                ForceSpawn();
            }
        }
    }

    public void ForceSpawn() {
        FallingMaterialSystem.Instance.SpawnItem(transform.position, NumberToSpawn);
        gameObject.destroy(false);
    }
}

internal class FallingMaterialCountdownTag : MonoBehaviour {
    private int _currentTime;

    internal GameObject Holder;
    internal TextMeshProUGUI TitleText;
    internal TextMeshProUGUI TimerText;

    internal void SetTime(int num, float time) {
        var t = (int)time;
        if (t == _currentTime) return;
        _currentTime = t;
        var ts = TimeSpan.FromSeconds(_currentTime);
        TimerText.text = $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        TitleText.text = string.Format(FallingMaterialSystem.Instance.TimerText, num, "\n");
    }
}