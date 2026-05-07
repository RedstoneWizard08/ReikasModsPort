using System;
using System.Reflection;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

public static class DebugExec {

	public static void run(string opcode, string type, string member) {
		run(opcode, type, member, "main");
	}

	public static void run(string opcode, string type, string member, string instField) {
		try {
			var t = InstructionHandlers.GetTypeBySimpleName(type);
			object inst = null;
			var main = t.GetField(instField);
			if (main != null && main.IsStatic && main.FieldType == t)
				inst = main.GetValue(null);
			switch (opcode) {
				case "call": {
					var call = AccessTools.Method(t, member);
					var ret = call.Invoke(inst, []);
					SNUtil.WriteToChat("Invoking " + type + "." + member + " returned: " + toString(ret));
				}
					break;
				case "field": {
					var field = AccessTools.Field(t, member);
					var ret = field.GetValue(inst);
					SNUtil.WriteToChat("Field " + type + "." + member + " contains: " + toString(ret));
				}
					break;
			}
		}
		catch (Exception e) {
			SNUtil.WriteToChat("Exec threw exception: " + e.ToString());
		}
	}

	public static string toString(object o) { //TO DO NOT IMPLEMENTED
		if (o == null) {
			return "null";
		}

		if (o.IsDictionary()) {
			return o.ToString();//((IDictionary)o).toDebugString();
		}

		if (o.IsEnumerable()) {
			return o.ToString();//((IEnumerable)o).toDebugString();
		}

		return o.ToString();
	}

	public static void tempCode() {
		//SNUtil.showPDANotification("I am pda text", "event:/player/story/Goal_BiomeSparseReef");

	}

}