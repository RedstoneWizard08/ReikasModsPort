using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class WorldgenLog {

	//private readonly string saveSlot;
	//private readonly string saveFile;

	private static readonly List<string> queue = [];

	static WorldgenLog() {
		//saveSlot = SNUtil.getCurrentSaveDir();
		//saveFile = Path.Combine(saveSlot, "Worldgen.log");

		// TODO
		// IngameMenuHandler.Main.RegisterOnLoadEvent(queue.Clear);
		// IngameMenuHandler.Main.RegisterOnSaveEvent(save);
	}

	public static void log(GameObject go) {
		log(go.name + " (" + ObjectUtil.tryGetObjectIdentifiers(go, out var classID, out var tt) + ") @ " + go.transform.position + " / " + go.transform.eulerAngles);
	}

	public static void log(string s) {
		queue.Add(s);
		SNUtil.log(s, SNUtil.diDLL);
	}

	private static void save() {
		var file = Path.Combine(SNUtil.getCurrentSaveDir(), "Worldgen.log");
		File.AppendAllText(file, string.Join("\n", queue));
		queue.Clear();
	}

}