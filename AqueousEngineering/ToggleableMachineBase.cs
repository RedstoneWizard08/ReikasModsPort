using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering;

public abstract class ToggleableMachineBase : CustomMachineLogic {

	private float lastButtonCheck = -1;

	internal bool isEnabled;

	protected override void load(System.Xml.XmlElement data) {
		isEnabled = data.getBoolean("toggled");
	}

	protected override void save(System.Xml.XmlElement data) {
		data.addProperty("toggled", isEnabled);
	}

	internal void toggle() {
		isEnabled = !isEnabled;
	}

	protected override void updateEntity(float seconds) {
		var time = DayNightCycle.main.timePassedAsFloat;
		if (time - lastButtonCheck >= 1 && sub) {
			lastButtonCheck = time;
			foreach (var panel in sub.GetComponentsInChildren<BaseControlPanelLogic>()) {
				panel.addButton(getButtonType());
			}
		}
		if (GameModeUtils.RequiresPower() && sub && sub.powerRelay.GetPower() < 0.1F)
			isEnabled = false;
	}

	protected abstract HolographicControl getButtonType();
}