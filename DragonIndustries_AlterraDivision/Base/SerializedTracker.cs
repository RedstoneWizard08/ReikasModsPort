using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Utility;

namespace ReikaKalseki.DIAlterra;

public class SerializedTracker<E> where E : SerializedTrackedEvent {

	private readonly string saveFileName;
	private readonly Func<XmlElement, E> parser;
	private readonly Func<string, E> legacyParser;

	protected readonly List<E> data = [];

	protected SerializedTracker(string name, bool loadWithGame, Func<XmlElement, E> f, Func<string, E> l) {
		saveFileName = name;
		parser = f;
		legacyParser = l;
		SaveUtils.RegisterOnSaveEvent(handleSave);
		if (loadWithGame)
			WaitScreenHandler.RegisterLoadTask("DIAlterra", handleLoad);
	}

	public void forAll(Action<E> a) {
		foreach (var e in data)
			a.Invoke(e);
	}

	public void forAllNewerThan(float thresh, Action<E> forEach) {
		var time = DayNightCycle.main.timePassedAsFloat;
		foreach (var obj in data) {
			var age = time - obj.eventTime;
			if (age < thresh)
				forEach.Invoke(obj);
		}
	}

	public string getData() {
		return data.ToDebugString();
	}

	public void handleSave() {
		var path = Path.Combine(SNUtil.GetCurrentSaveDir(), saveFileName);
		var content = new XmlDocument();
		content.AppendChild(content.CreateElement("Root"));
		data.Sort();
		foreach (var tt in data) {
			var e = content.DocumentElement.AddChild("Event");
			tt.saveToXML(e);
			e.AddProperty("eventTime", tt.eventTime);
		}
		content.Save(path);
	}

	public void handleLoad(WaitScreenHandler.WaitScreenTask task) {
		var dir = SNUtil.GetCurrentSaveDir();
		var path = Path.Combine(dir, saveFileName);
		if (!File.Exists(path))
			return;
		SNUtil.Log("Loading saved " + typeof(E).Name + "[] from " + path, SNUtil.DiDLL);
		clear();
		var ie = File.ReadLines(path);
		if (ie == null)
			return;
		var first = ie.FirstOrDefault(); //ReadLines is lazy-eval so this only actually reads the first line
		if (string.IsNullOrEmpty(first))
			return;
		if (first.Contains("<Root>")) {
			var doc = new XmlDocument();
			doc.Load(path);
			if (doc.DocumentElement == null)
				return;
			foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
				try {
					var tt = parser.Invoke(e);
					if (tt != null) {
						add(tt);
					}
				}
				catch (Exception ex) {
					SNUtil.Log("Failed to parse " + typeof(E).Name + " from XML '" + e.OuterXml + "': " + ex.ToString(), SNUtil.DiDLL);
				}
			}
		}
		else if (legacyParser != null) {
			foreach (var s in File.ReadAllLines(path)) {
				var e = legacyParser.Invoke(s);
				if (e != null) {
					add(e);
				}
			}
		}
	}

	protected virtual void add(E e) {
		data.Add(e);
	}

	protected virtual void clear() {
		data.Clear();
	}

}

public abstract class SerializedTrackedEvent : IComparable<SerializedTrackedEvent> {

	public readonly double eventTime;

	public string formatTime => Utils.PrettifyTime((int)eventTime);

	protected SerializedTrackedEvent(double t) {
		eventTime = t;
	}

	public abstract void saveToXML(XmlElement e);

	public int CompareTo(SerializedTrackedEvent e) {
		return eventTime.CompareTo(e.eventTime);
	}


}