using System;
using ReikaKalseki.DIAlterra;
using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea;

public class LiquidBreathingSystem {

	public static readonly LiquidBreathingSystem Instance = new();

	internal static readonly float ItemValue = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 1800 : 2700;
	//seconds
	internal static readonly float TankCharge = 10 * 60;
	//how much time you can spend (total) of liquid before returning to a base with a charger
	internal static readonly float TankCapacity = 2.5F * 60;
	//per "air tank" before you need to go back to a powered air-filled space

	private static readonly string CustomHUDText = "CF<size=30>X</size>•O<size=30>Y</size>";

	private LiquidBreathingHUDMeters _meters;

	private Texture2D _baseO2BarTexture;
	private Color _baseO2BarColor;
	private Texture2D _baseO2BubbleTexture;
	private float _baseOverlayAlpha2;
	private float _baseOverlayAlpha1;
	private string _baseLabel;

	private float _lastRechargeRebreatherTime = -1;
	private float _rechargingTintStrength;

	private float _forceAllowO2 = 0;

	private float _lastUnequippedTime = -1;

	private bool _hasTemporaryKharaaTreatment;
	private bool _startedUsingTemporaryKharaaTreatment;
	public float KharaaTreatmentRemainingTime { get; private set; }

	private LiquidBreathingSystem() {

	}

	public void OnEquip() {
		var inf = Player.main.GetComponent<InfectedMixin>();
		inf.SetInfectedAmount(Mathf.Max(inf.infectedAmount, 0.25F));
	}

	public void OnUnequip() {
		var amt = Player.main.oxygenMgr.GetOxygenAvailable();
		_lastUnequippedTime = DayNightCycle.main.timePassedAsFloat;
		Player.main.oxygenMgr.RemoveOxygen(amt/*-1*/);
		//SNUtil.writeToChat("Removed "+amt+" oxygen, player now has "+Player.main.oxygenMgr.GetOxygenAvailable());
		SoundManager.playSoundAt(SoundManager.buildSound(Player.main.IsUnderwater() ? "event:/player/Puke_underwater" : "event:/player/Puke"), Player.main.lastPosition, false, 12);
	}

	public float GetLastUnequippedTime() {
		return _lastUnequippedTime;
	}

	public void RefreshGui() {
		_lastRechargeRebreatherTime = DayNightCycle.main.timePassedAsFloat;
	}
	/*
    public void refillPlayerO2Bar(Player p, float amt) {
		forceAllowO2 += amt;
		if (amt > 0)
			p.oxygenMgr.AddOxygen(amt);
		onAddO2ToBar();
		//SNUtil.writeToChat("Added "+add);
    }*/
	/*
    internal void onAddO2ToBar(float amt) {
	    if (!hasLimited)
		    return;
	    Oxygen o = getTankTank();
	    float rem = o.oxygenAvailable-75;
	    if (rem > 0)
		    o.RemoveOxygen(rem);
    }*/

	public float GetFuelLevel() {
		var b = GetTankBattery();
		return b ? b.charge : 0;
	}

	public float GetAvailableFuelSpace() {
		var b = GetTankBattery();
		return b ? b.capacity - b.charge : 0;
	}

	private Battery GetTankBattery() {
		var tank = Inventory.main.equipment.GetItemInSlot("Tank");
		if (tank.item.GetTechType() != C2CItems.liquidTank.TechType)
			return null;
		var b = tank.item.gameObject.GetComponent<Battery>();
		return b;
	}

	private Oxygen GetTankTank() {
		var tank = Inventory.main.equipment.GetItemInSlot("Tank");
		if (tank.item.GetTechType() != C2CItems.liquidTank.TechType)
			return null;
		var b = tank.item.gameObject.GetComponent<Oxygen>();
		return b;
	}

	public float RechargePlayerLiquidBreathingFuel(float amt) {
		var b = GetTankBattery();
		if (!b)
			return 0;
		var add = Mathf.Min(amt, b.capacity - b.charge);
		if (add > 0) {
			b.charge += add;
			RefreshGui();
		}
		return add;
	}

	public bool IsLiquidBreathingActive(Player ep) {
		if (IsInPoweredArea(ep))
			return false;
		var v = ep.GetVehicle();
		if (v && !v.IsPowered())
			return true;
		var sub = ep.currentSub;
		return (sub && sub.powerRelay && !sub.powerRelay.IsPowered()) || ep.IsUnderwater() || ep.IsSwimming();
	}

	public bool IsO2BarAbleToFill(Player ep) {
		return !HasTankButNoMask() && !EnvironmentalDamageSystem.Instance.IsPlayerRecoveringFromPressure() && (!HasLiquidBreathing() || IsInPoweredArea(ep) || !IsLiquidBreathingActive(ep));
	}

	public bool IsInPoweredArea(Player p) {
		if (!p)
			return false;
		if (p.IsUnderwater() || p.IsSwimming())
			return false;
		if (p.currentEscapePod && p.currentEscapePod == EscapePod.main && Story.StoryGoalManager.main && EscapePod.main.fixPanelGoal != null && Story.StoryGoalManager.main.IsGoalComplete(EscapePod.main.fixPanelGoal.key))
			return true;
		var v = p.GetVehicle();
		if (v && v.IsPowered())
			return true;
		var sub = p.currentSub;
		return (sub && sub.powerRelay && sub.powerRelay.IsPowered()) || p.precursorOutOfWater;
	}

	public bool TryFillPlayerO2Bar(Player p, ref float amt, bool force = false) {
		if (HasTankButNoMask()) {
			amt = 0;
			return false;
		}
		if (!HasLiquidBreathing())
			return true;
		if (!force) {
			if (!IsInPoweredArea(p)) {
				amt = 0;
				return false;
			}
		}
		var b = GetTankBattery();
		if (!b) {
			amt = 0;
			return false;
		}
		amt = Mathf.Min(amt, b.charge);
		//if (hasReducedCapacity()) does not work reliably
		//	amt = Mathf.Min(amt, 75-getTankTank().oxygenAvailable);
		if (amt > 0)
			b.charge -= amt;
		//SNUtil.writeToChat(amt+" > "+b.charge);
		return amt > 0;
	}

	public bool HasTankButNoMask() {
		return Inventory.main.equipment.GetTechTypeInSlot("Head") != C2CItems.rebreatherV2.TechType && Inventory.main.equipment.GetTechTypeInSlot("Tank") == C2CItems.liquidTank.TechType;
	}

	public bool HasLiquidBreathing() {
		return Inventory.main.equipment.GetTechTypeInSlot("Head") == C2CItems.rebreatherV2.TechType && Inventory.main.equipment.GetTechTypeInSlot("Tank") == C2CItems.liquidTank.TechType;
	}

	public void CheckLiquidBreathingSupport(OxygenArea a) {
		var oxy = a.gameObject.GetComponent<OxygenAreaWithLiquidSupport>();
		//SNUtil.writeToChat("Check pipe: "+oxy+" > "+(oxy != null ? oxy.supplier+"" : "null"));
		if (oxy != null && oxy.Supplier != null && DayNightCycle.main.timePassedAsFloat - oxy.LastVerify < 5) {
			RefillFrom(oxy.Supplier, Time.deltaTime);
		}
	}

	public void RefillFrom(RebreatherRechargerLogic lgc, float seconds) {
		if (HasLiquidBreathing()) {
			var add = lgc.Consume(GetAvailableFuelSpace(), seconds);
			var added = RechargePlayerLiquidBreathingFuel(add);
			lgc.Refund(add - added); //if somehow added less than space, refund it
		}
	}

	public static GameObject GetO2Label(uGUI_OxygenBar gui) {
		return gui.gameObject.getChildObject("OxygenTextLabel");
	}

	public void UpdateOxygenGUI(uGUI_OxygenBar gui) {
		var bar = gui.bar;
		var t = GetO2Label(gui).GetComponent<Text>();
		var tn = gui.gameObject.getChildObject("OxygenTextValue").GetComponent<Text>();
		if (_baseO2BarTexture == null) {
			_baseO2BarTexture = bar.texture;
			_baseO2BarColor = bar.borderColor;
			_baseO2BubbleTexture = bar.overlay;
			_baseOverlayAlpha1 = gui.overlay1Alpha;
			_baseOverlayAlpha2 = gui.overlay2Alpha;
			_baseLabel = t.text; //O<size=30>2</size>
			//RenderUtil.dumpTexture("o2bar_core", baseO2BarTexture);
			//RenderUtil.dumpTexture("o2bar_bubble", baseO2BubbleTexture);
		}

		var pink = HasLiquidBreathing();

		bar.edgeWidth = pink ? 0.25F : 0.2F;
		bar.borderWidth = pink ? 0.1F : 0.2F;
		bar.borderColor = pink ? new Color(1, 0.6F, 0.82F) : _baseO2BarColor;
		bar.texture = pink ? TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/HUD/o2bar_liquid") : _baseO2BarTexture;
		bar.overlay = pink ? TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/HUD/o2bar_liquid_bubble") : _baseO2BubbleTexture;
		bar.overlay1Alpha = pink ? Math.Min(1, _baseOverlayAlpha1 * 2) : _baseOverlayAlpha1;
		bar.overlay2Alpha = pink ? Math.Min(1, _baseOverlayAlpha2 * 2) : _baseOverlayAlpha2;
		t.text = pink ? CustomHUDText /*"O<size=30>2</size><size=20>(aq)</size>"*/ : _baseLabel;
		var inactive = !IsLiquidBreathingActive(Player.main);
		var tc = Color.white;
		if (pink) {
			if (inactive) {
				tn.text = "-";
				tc = Color.gray;
			}
			else {
				tc = SNUtil.IsPlayerCured()
					? Color.green
					: IsKharaaTreatmentActive()
						? KharaaTreatmentRemainingTime > 0 ? Color.white : Color.yellow
						: Color.Lerp(Color.red, Color.yellow, 0.5F);
			}
		}
		tn.color = tc;
		bar.color = Color.white;

		var time = DayNightCycle.main.timePassedAsFloat;
		_rechargingTintStrength = time - _lastRechargeRebreatherTime <= 0.5
			? Math.Min(1, _rechargingTintStrength * 1.01F + 0.025F)
			: Math.Max(0, _rechargingTintStrength * 0.992F - 0.0125F);
		if (pink && _rechargingTintStrength > 0) {
			var f = 1 - 0.33F * (0.5F + _rechargingTintStrength * 0.5F);
			bar.color = new Color(f, f, 1);
		}
	}

	public bool IsO2BarFlashingRed() {
		return Player.main.GetDepth() >= 400 && EnvironmentalDamageSystem.Instance.IsPlayerInOcean() && Inventory.main.equipment.GetTechTypeInSlot("Head") != C2CItems.rebreatherV2.TechType;
	}

	public void ApplyToBasePipes(RebreatherRechargerLogic machine, Transform seabase) {
		foreach (Transform child in seabase) {
			var root = child.gameObject.GetComponent<IPipeConnection>();
			if (root != null) {
				for (var i = 0; i < OxygenPipe.pipes.Count; i++) {
					var p = OxygenPipe.pipes[i];
					if (p && p.oxygenProvider != null && p.GetRoot() == root && p.oxygenProvider.activeInHierarchy) {
						var oxy = p.oxygenProvider.EnsureComponent<OxygenAreaWithLiquidSupport>();
						oxy.Supplier = machine;
						oxy.LastVerify = DayNightCycle.main.timePassedAsFloat;
						//SNUtil.writeToChat("Enable oxy area @ "+oxy.lastVerify);
					}
				}
			}
		}
	}

	internal bool UseKharaaTreatment() {
		var dur = GetTreatmentDuration();
		if (KharaaTreatmentRemainingTime > Mathf.Max(dur * 0.01F, 60))
			return false;
		MoraleSystem.instance.shiftMorale(15);
		KharaaTreatmentRemainingTime = dur;
		return true;
	}

	internal bool ApplyTemporaryKharaaTreatment() {
		if (_hasTemporaryKharaaTreatment || !HasLiquidBreathing() || !IsInPoweredArea(Player.main))
			return false;
		MoraleSystem.instance.shiftMorale(2);
		_hasTemporaryKharaaTreatment = true;
		return true;
	}

	public float GetTreatmentDuration() {
		return SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 2400 : 7200;
	}

	private bool IsKharaaTreatmentActive() {
		return KharaaTreatmentRemainingTime > 0 || _hasTemporaryKharaaTreatment;
	}

	internal void TickLiquidBreathing(bool has, bool active) {
		var ep = Player.main;
		if (!ep || !DIHooks.IsWorldLoaded())
			return;
		if (!_meters) {
			var o2Bar = UnityEngine.Object.FindObjectOfType<uGUI_OxygenBar>();
			if (o2Bar) {
				var go = new GameObject("LiquidBreathingMeters");
				_meters = go.EnsureComponent<LiquidBreathingHUDMeters>();
				go.transform.SetParent(o2Bar.transform, false);
				go.transform.localPosition = Vector3.zero;
			}
		}
		if (_meters) {
			_meters.gameObject.SetActive(has);
			if (has) {
				var b = Inventory.main.equipment.GetItemInSlot("Tank").item.GetComponent<Battery>();
				_meters.PrimaryTankBar.CurrentTime = b.charge;
				_meters.PrimaryTankBar.CurrentFillLevel = b.charge / b.capacity;
				_meters.TreatmentBar.CurrentTime = _hasTemporaryKharaaTreatment ? -1 : KharaaTreatmentRemainingTime;
				_meters.TreatmentBar.CurrentFillLevel = _hasTemporaryKharaaTreatment ? 1 : KharaaTreatmentRemainingTime / GetTreatmentDuration();
			}
		}
		if (has && active) {
			if (_startedUsingTemporaryKharaaTreatment && (IsInPoweredArea(ep) || !HasLiquidBreathing())) {
				ClearTempTreatment();
			}
			if (_hasTemporaryKharaaTreatment && !_startedUsingTemporaryKharaaTreatment && !IsInPoweredArea(ep) && HasLiquidBreathing()) {
				_startedUsingTemporaryKharaaTreatment = true;
				SNUtil.WriteToChat("Kharaa treatment engaged");
			}
			if (ep.infectedMixin.IsInfected() && KharaaTreatmentRemainingTime > 0) {
				KharaaTreatmentRemainingTime = Mathf.Max(0, KharaaTreatmentRemainingTime - Time.deltaTime);
			}
		}
		else if (!has || _startedUsingTemporaryKharaaTreatment) {
			ClearTempTreatment();
		}
	}

	public void ClearTempTreatment() {
		if (_hasTemporaryKharaaTreatment) {
			_hasTemporaryKharaaTreatment = false;
			_startedUsingTemporaryKharaaTreatment = false;
			SNUtil.WriteToChat("Weak kharaa treatment cleared");
		}
	}

	public bool HasReducedCapacity() {
		return !IsKharaaTreatmentActive() && !SNUtil.IsPlayerCured() && HasLiquidBreathing();
	}

	private class OxygenAreaWithLiquidSupport : MonoBehaviour {

		internal RebreatherRechargerLogic Supplier;
		internal float LastVerify;

	}

	public class LiquidBreathingHUDMeters : MonoBehaviour {

		internal LiquidBreathingHUDMeterUnit PrimaryTankBar;
		internal LiquidBreathingHUDMeterUnit TreatmentBar;

		private void Update() {
			if (!PrimaryTankBar) {
				PrimaryTankBar = CreateBar("Primary");
				PrimaryTankBar.LeftSide = true;
			}
			if (!TreatmentBar) {
				TreatmentBar = CreateBar("Treatment");
			}
			TreatmentBar.gameObject.SetActive(!SNUtil.IsPlayerCured());
		}

		private LiquidBreathingHUDMeterUnit CreateBar(string name) {
			var go = new GameObject(name);
			go.transform.SetParent(transform, false);
			go.layer = gameObject.layer;
			return go.EnsureComponent<LiquidBreathingHUDMeterUnit>();
		}

	}

	public class LiquidBreathingHUDMeterUnit : MonoBehaviour {

		internal GameObject FillBar;
		internal Image Background;
		internal Image Foreground;

		internal Text Timer;

		internal bool LeftSide;

		internal float CurrentFillLevel;
		internal float CurrentTime;

		public float overrideValue = -1;
		public float overrideTime = -1;

		public Color currentColor;

		private void Rebuild() {
			TextureManager.refresh();
			Foreground.sprite = Sprite.Create(TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/LiquidBreathingFuelBar"), new Rect(0, 0, 128, 128), Vector2.zero);
		}

		private void Update() {
			if (!FillBar) {
				FillBar = new GameObject("Bar");
				FillBar.transform.SetParent(transform, false);
				FillBar.layer = gameObject.layer;
				FillBar.transform.localPosition = Vector3.zero;//new Vector3(0.55F, -0.55F, 0);
				FillBar.transform.localScale = Vector3.one * 2.5F;
				var go = new GameObject("Background");
				go.transform.SetParent(FillBar.transform, false);
				go.layer = gameObject.layer;
				Background = go.AddComponent<Image>();
				Background.sprite = Sprite.Create(TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/LiquidBreathingFuelBarBack"), new Rect(0, 0, 128, 128), Vector2.zero);
				Background.rectTransform.offsetMin = new Vector2(-32f, -32f);
				Background.rectTransform.offsetMax = new Vector2(32f, 32f);
				var go2 = new GameObject("Foreground");
				go2.transform.SetParent(FillBar.transform, false);
				go2.layer = gameObject.layer;
				Foreground = go2.AddComponent<Image>();
				Foreground.sprite = Sprite.Create(TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/LiquidBreathingFuelBar"), new Rect(0, 0, 128, 128), Vector2.zero);
				Foreground.rectTransform.offsetMin = new Vector2(-32f, -32f);
				Foreground.rectTransform.offsetMax = new Vector2(32f, 32f);
				Foreground.type = Image.Type.Filled;
				Foreground.fillMethod = Image.FillMethod.Radial360;
				Foreground.fillClockwise = true;
				Foreground.fillOrigin = (int)Image.Origin360.Bottom;
				Foreground.transform.rotation = Quaternion.identity;
				FillBar.SetActive(true);
			}
			if (!Timer) {
				var lbl = GetO2Label(gameObject.FindAncestor<uGUI_OxygenBar>()).clone().setName("SideBarText");
				Timer = lbl.GetComponent<Text>();
				Timer.transform.SetParent(transform, false);
				Timer.transform.localPosition = Vector3.zero;
				Timer.transform.rotation = Quaternion.identity;
				Timer.alignment = LeftSide ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
				Timer.resizeTextForBestFit = false;
				Timer.fontSize = 30;
			}

			if (LeftSide) {
				FillBar.transform.localRotation = Quaternion.Euler(0, 0, 202.5F);
				Foreground.transform.localRotation = Quaternion.Euler(0, 0, 157.5F);
				if (Timer)
					Timer.transform.localPosition = new Vector3(-60, 64, 0);
			}
			else {
				FillBar.transform.localRotation = Quaternion.Euler(0, 0, 22.5F);
				Foreground.transform.localRotation = Quaternion.Euler(0, 0, -22.5F);
				Foreground.fillClockwise = false;
				if (Timer)
					Timer.transform.localPosition = new Vector3(60, 64, 0);
			}

			var f = CurrentFillLevel * 0.395F + 0.0625F; //this fraction is because it does not fill the bar, slightly more than 3/8 to use up texture
			if (overrideValue >= 0)
				f = overrideValue;
			Foreground.fillAmount = f;
			currentColor = CurrentFillLevel < 0.5F ? new Color(1, CurrentFillLevel * 2, 0, 1) : new Color(1 - (CurrentFillLevel - 0.5F) * 2, 1, 0, 1);
			if (CurrentFillLevel < 0.1) { //make flash if very low
				var ff = (1+Mathf.Sin(DayNightCycle.main.timePassedAsFloat*Mathf.Deg2Rad*(float)MathUtil.linterpolate(CurrentFillLevel, 0.1, 0, 240, 4800, true)))*0.5F;
				currentColor = Color.Lerp(currentColor, Color.white, ff);
			}
			Foreground.color = currentColor;

			if (Timer) {
				f = CurrentTime;
				if (overrideTime >= 0)
					f = overrideTime;
				if (f >= 0) {
					var ts = TimeSpan.FromSeconds(f);
					Timer.text = f >= 3600 ? ts.ToString(@"hh\:mm\:ss") : ts.ToString(@"mm\:ss");
				}
				else {
					Timer.text = "N/A";
				}
			}
		}

	}

}