using System;
using System.Collections.Generic;
using System.Xml;
using Nautilus.Handlers;
using Story;

using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class StoryHandler : SerializedTracker<StoryHandler.StoryGoalRecord>, IStoryGoalListener {

	public static readonly StoryHandler instance = new();

	private readonly Dictionary<string, StoryGoalRecord> unlocks = new();

	private readonly Dictionary<ProgressionTrigger, DelayedProgressionEffect> triggers = new();
	private readonly List<IStoryGoalListener> listeners = [];

	private readonly List<StoryGoal> queuedTickedGoals = [];
	private readonly Dictionary<string, OnGoalUnlock> queuedChainedGoalRedirects = new();

	public bool disableStoryHooks = false;

	private StoryHandler() : base("StoryGoals.dat", false, parse, parseLegacy) {
		//load in world load//IngameMenuHandler.Main.RegisterOnLoadEvent(handleLoad);
		
		// TODO
		// IngameMenuHandler.Main.RegisterOnSaveEvent(this.handleSave);
	}

	private static StoryGoalRecord parse(XmlElement s) {
		return new StoryGoalRecord(s.GetProperty("goal"), s.GetFloat("eventTime", -1));
	}

	private static StoryGoalRecord parseLegacy(string s) {
		var parts = s.Split(',');
		return new StoryGoalRecord(parts[0], float.Parse(parts[1]));
	}

	public void addListener(Action<string> call) {
		listeners.Add(new DelegateGoalListener(call));
	}

	public void addListener(IStoryGoalListener ig) {
		listeners.Add(ig);
	}

	public void registerTrigger(ProgressionTrigger pt, DelayedProgressionEffect e) {
		triggers[pt] = e;
	}

	public void registerTickedGoal(StoryGoal g) {
		queuedTickedGoals.Add(g);
	}

	/// <remarks>Accepts null to deregister the hook entirely</remarks>
	public void registerChainedRedirect(string key, OnGoalUnlock redirect) {
		if (queuedChainedGoalRedirects.ContainsKey(key))
			throw new Exception("Story goal '" + key + "' is already being redirected to " + queuedChainedGoalRedirects[key]);
		queuedChainedGoalRedirects[key] = redirect;
	}

	public void onLoad() {
		if (!BiomeGoalTracker.main) {
			SNUtil.Log("Story biome goal tracker not initialized yet!", SNUtil.DiDLL);
			return;
		}
		var lgt = BiomeGoalTracker.main.gameObject.GetComponent<LocationGoalTracker>();
		var cg = lgt.gameObject.EnsureComponent<ConditionalLocationGoalTracker>();
		foreach (var g in queuedTickedGoals) {
			if (g is ConditionalLocationGoal clg) {
				SNUtil.Log("Registering conditional location goal '" + g.key + "' for position " + clg.position, SNUtil.DiDLL);
				cg.goals.Add(clg);
			}
			else if (g is LocationGoal lg) {
				SNUtil.Log("Registering location goal '" + g.key + "' for position " + lg.position + ": " + lg.location, SNUtil.DiDLL);
				lgt.goals.Add(lg);
			}
			else if (g is BiomeGoal bg) {
				SNUtil.Log("Registering discovery goal '" + g.key + "' for biome " + bg.biome, SNUtil.DiDLL);
				BiomeGoalTracker.main.goals.Add(bg);
			}
			else {
				SNUtil.Log("Unrecognized ticked goal '" + g.key + "' type: " + g.GetType().FullName + "!");
			}
		}
		var ut = lgt.gameObject.GetComponent<OnGoalUnlockTracker>();
		foreach (var kvp in queuedChainedGoalRedirects) {
			SNUtil.Log("Applying redirect for goal '" + kvp.Key + "': " + kvp.Value);
			if (kvp.Value == null) {
				ut.goalUnlocks.Remove(kvp.Key);
			}
			else {
				ut.goalUnlocks[kvp.Key] = kvp.Value;
			}
		}

		handleLoad(new WaitScreenHandler.WaitScreenTask("", null));
		foreach (var goal in StoryGoalManager.main.completedGoals) {
			if (!unlocks.ContainsKey(goal)) {
				add(new StoryGoalRecord(goal, -1));
			}
		}
	}

	public LocationGoal createLocationGoal(double x, double y, double z, double r, string key, float minStay = 0) {
		return createLocationGoal(new Vector3((float)x, (float)y, (float)z), r, key, minStay);
	}

	public LocationGoal createLocationGoal(Vector3 pos, double r, string key, float minStay = 0) {
		var g =  new LocationGoal {
			position = pos,
			key = key,
			range = (float)r,
		};
		g.location = g.key;
		g.goalType = Story.GoalType.Story;
		return g;
	}

	public ConditionalLocationGoal createLocationGoal(double x, double y, double z, double r, string key, Predicate<Vector3> condition, float minStay = 0) {
		return createLocationGoal(new Vector3((float)x, (float)y, (float)z), r, key, condition, minStay);
	}

	public ConditionalLocationGoal createLocationGoal(Vector3 pos, double r, string key, Predicate<Vector3> condition, float minStay = 0) {
		var g = new ConditionalLocationGoal {
			position = pos,
			key = key,
			range = (float)r,
			goalType = Story.GoalType.Story,
			condition = condition,
		};
		return g;
	}

	public void tick(Player ep) {
		if (disableStoryHooks || !DIHooks.IsWorldLoaded())
			return;
		foreach (var kvp in triggers) {
			if (kvp.Key.isReady(ep)) {
				var dt = kvp.Value;
				dt.time += Time.deltaTime;
				//if (!dt.isFired())
				//	SNUtil.writeToChat("Trigger "+kvp.Key+" is ready, T="+dt.time.ToString("0.000")+"/"+dt.minDelay.ToString("0.0"));
				if (!dt.isFired() && dt.time >= dt.minDelay && UnityEngine.Random.Range(0, 1F) <= dt.chancePerTick * Time.timeScale) {
					//SNUtil.writeToChat("Firing "+dt);
					dt.fire();
				}
			}
			else {
				//SNUtil.writeToChat("Trigger "+kvp.Key+" condition is not met");
			}
		}
	}

	protected override void add(StoryGoalRecord e) {
		base.add(e);
		unlocks[e.goal] = e;
	}

	public StoryGoalRecord getRecord(string goal) {
		return unlocks.ContainsKey(goal) ? unlocks[goal] : null;
	}

	public float getTimeSince(string goal) {
		var rec = getRecord(goal);
		return rec == null ? -1 : DayNightCycle.main.timePassedAsFloat - (float)rec.eventTime;
	}

	protected override void clear() {
		base.clear();
		unlocks.Clear();
	}

	public void NotifyGoalComplete(string key) {
		SNUtil.Log("Completed Story Goal '" + key + "' @ " + DayNightCycle.main.timePassedAsFloat, SNUtil.DiDLL);
		foreach (var ig in listeners) {
			ig.NotifyGoalComplete(key);
		}
		add(new StoryGoalRecord(key, DayNightCycle.main.timePassedAsFloat));
	}

	private class DelegateGoalListener : IStoryGoalListener {

		private readonly Action<string> callback;

		internal DelegateGoalListener(Action<string> a) {
			callback = a;
		}

		public void NotifyGoalComplete(string key) {
			callback(key);
		}

	}

	private class ConditionalLocationGoalTracker : MonoBehaviour {

		internal readonly List<ConditionalLocationGoal> goals = [];

		private void Start() {
			InvokeRepeating(nameof(TrackLocation), UnityEngine.Random.value, 2);
		}

		private void TrackLocation() {
			var position = Player.main.transform.position;
			var timePassed = DayNightCycle.main.timePassed;
			for (var i = goals.Count - 1; i >= 0; i--) {
				if (goals[i].Trigger(position, (float)timePassed)) {
					goals.RemoveFast(i);
				}
			}
		}
	}

	public class ConditionalLocationGoal : StoryGoal {

		public Vector3 position;

		public float range;

		public float minStayDuration;

		private float timeEntered = -1f;

		public Predicate<Vector3> condition;

		public new bool Trigger(Vector3 pos, float time) {
			if (Vector3.SqrMagnitude(pos - position) > range * range || !condition.Invoke(pos)) {
				timeEntered = -1f;
				return false;
			}
			if (timeEntered < 0f)
				timeEntered = time;
			if (time - timeEntered < minStayDuration)
				return false;
			base.Trigger();
			return true;
		}

	}

	public class StoryGoalRecord : SerializedTrackedEvent {

		public readonly string goal;

		internal StoryGoalRecord(string tt, double time) : base(time) {
			goal = tt;
		}

		public override void saveToXML(XmlElement e) {
			e.AddProperty("goal", goal);
		}

		public override string ToString() {
			return $"[StoryGoal Goal={goal}, Time={eventTime}]";
		}

	}
}