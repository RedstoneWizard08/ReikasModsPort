using System;
using System.Collections.Generic;
using Story;

namespace ReikaKalseki.DIAlterra;

public class FirstObtainmentSystem {

	public static readonly FirstObtainmentSystem instance = new();

	private static readonly Dictionary<TechType, List<Action>> events = new();

	private FirstObtainmentSystem() {

	}

	public void registerEvent(TechType tt, Action a) {
		var li = events.ContainsKey(tt) ? events[tt] : [];
		li.Add(a);
		events[tt] = li;
	}

	public void onPickup(TechType tt) {
		var key = getGoal(tt);
		if (!StoryGoalManager.main.IsGoalComplete(key)) {
			StoryGoal.Execute(key, Story.GoalType.Story);
			if (events.ContainsKey(tt)) {
				foreach (var a in events[tt]) {
					a.Invoke();
				}
			}
		}
	}

	public static string getGoal(TechType tt) {
		return "FirstCollect_" + tt.AsString();
	}
}