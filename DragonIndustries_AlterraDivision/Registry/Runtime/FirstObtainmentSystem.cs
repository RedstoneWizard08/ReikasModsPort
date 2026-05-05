using System;
using System.Collections.Generic;
using Story;

namespace ReikaKalseki.DIAlterra;

public class FirstObtainmentSystem {

	public static readonly FirstObtainmentSystem instance = new FirstObtainmentSystem();

	private static readonly Dictionary<TechType, List<Action>> events = new Dictionary<TechType, List<Action>>();

	private FirstObtainmentSystem() {

	}

	public void registerEvent(TechType tt, Action a) {
		List<Action> li = events.ContainsKey(tt) ? events[tt] : new List<Action>();
		li.Add(a);
		events[tt] = li;
	}

	public void onPickup(TechType tt) {
		string key = getGoal(tt);
		if (!StoryGoalManager.main.IsGoalComplete(key)) {
			StoryGoal.Execute(key, Story.GoalType.Story);
			if (events.ContainsKey(tt)) {
				foreach (Action a in events[tt]) {
					a.Invoke();
				}
			}
		}
	}

	public static string getGoal(TechType tt) {
		return "FirstCollect_" + tt.AsString();
	}
}