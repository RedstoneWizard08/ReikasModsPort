using System.Xml;

namespace ReikaKalseki.DIAlterra;

public class CommandTracker : SerializedTracker<CommandTracker.CommandEvent> {

	public static readonly CommandTracker instance = new();

	private CommandTracker() : base("Commands.dat", true, parse, null) {

	}

	public void onCommand(string cmd) {
		add(new CommandEvent(cmd, DayNightCycle.main.timePassedAsFloat));
	}

	private static CommandEvent parse(XmlElement s) {
		return new CommandEvent(CommandEvent.buildCommand(s), s.GetFloat("eventTime", -1));
	}

	public class CommandEvent : SerializedTrackedEvent {

		public readonly string command;

		internal CommandEvent(string c, double time) : base(time) {
			command = c;
		}

		public override void saveToXML(XmlElement e) {
			splitCommand(command, e);
		}

		internal static string buildCommand(XmlElement e) {
			var cmd = e.GetProperty("command");
			foreach (var e2 in e.GetDirectElementsByTagName("arg")) {
				cmd += " " + e2.InnerText;
			}
			return cmd;
		}

		private static void splitCommand(string cmd, XmlElement e) {
			var parts = cmd.Split(' ');
			e.AddProperty("command", parts[0]);
			for (var i = 1; i < parts.Length; i++) {
				e.AddProperty("arg", parts[i]);
			}
		}

	}

}