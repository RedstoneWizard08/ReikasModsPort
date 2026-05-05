using System.Collections.Generic;
using Story;

namespace ReikaKalseki.DIAlterra;

public class PDAMessagePrompts {

	public static readonly PDAMessagePrompts instance = new();

	private readonly Dictionary<string, StoryGoal> mappings = new();

	private PDAMessagePrompts() {

	}

	public StoryGoal addPDAMessage(XMLLocale.LocaleEntry e) {
		return addPDAMessage(e.key, e.desc, e.pda);
	}

	public StoryGoal addPDAMessage(string key, string text, string soundFile) {
		SNUtil.log("Constructing PDA message " + key);
		var item = new StoryGoal(key, Story.GoalType.PDA, 0);
		SNUtil.addVOLine(item, text, SoundManager.registerPDASound(SNUtil.tryGetModDLL(), "prompt_" + key, soundFile).asset);
		mappings[key] = item;
		return item;
	}

	public StoryGoal getMessage(string key) {
		return mappings[key];
	}

	public bool isTriggered(string m) {
		return StoryGoalManager.main.completedGoals.Contains(getMessage(m).key);
	}

	public bool trigger(string m) {
		var sg = getMessage(m);
		if (!StoryGoalManager.main.completedGoals.Contains(sg.key)) {
			StoryGoal.Execute(sg.key, sg.goalType);
			return true;
		}
		return false;
	}
}