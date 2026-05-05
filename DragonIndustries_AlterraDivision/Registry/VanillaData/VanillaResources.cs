using System.Collections.Generic;
using System.Reflection;

namespace ReikaKalseki.DIAlterra;

public class VanillaResources : PrefabReference {

	private static readonly Dictionary<string, VanillaResources> lookup = new();

	public static readonly VanillaResources SCRAP1 = new("7cd86cbf-0708-41dc-84d7-58c648e25b06"); //"WorldEntities/Natural/metal3",
	public static readonly VanillaResources SCRAP2 = new("7e507655-9fbf-42e0-8422-163ddd668747"); //"WorldEntities/Natural/metal2",
	public static readonly VanillaResources SCRAP3 = new("947f2823-c42a-45ef-94e4-52a9f1d3459c"); //"WorldEntities/Natural/metal1",
	public static readonly VanillaResources SCRAP4 = new("b2d10d9b-878e-4ff8-b71f-cd578e0d2038"); //"WorldEntities/Natural/metal4",

	public static readonly VanillaResources LIMESTONE = new("7e07fce9-0ad6-4c54-9da7-e43eb1e38cea");
	public static readonly VanillaResources SANDSTONE = new("5b702ef7-7403-49ee-99c5-1f67ab04954a");
	public static readonly VanillaResources SHALE = new("814fa303-8697-48ef-b126-cf22e703cefd");

	public static readonly VanillaResources TITANIUM = new("c66b5dfa-7fe9-4688-b165-d2e2f4caa8d9");
	public static readonly VanillaResources COPPER = new("63e251a6-fb65-454b-84b0-4493e19f73cd");
	public static readonly VanillaResources QUARTZ = new("8ef17c52-2aa8-46b6-ada3-c3e3c4a78dd6");
	public static readonly VanillaResources GOLD = new("3c5bd4db-953d-4d23-92be-f5a3b76b2e25");
	public static readonly VanillaResources SILVER = new("439b4b17-2f86-4706-8abd-8d2f68df782b");
	public static readonly VanillaResources SALT = new("f654e870-3101-4ff3-8bb4-528dceef43a5");
	public static readonly VanillaResources LITHIUM = new("f65beedb-2d76-466b-abc8-37c474228157");
	public static readonly VanillaResources LEAD = new("b334fbb1-224b-4082-bb69-d4a39051aaca");
	public static readonly VanillaResources MAGNETITE = new("5462a145-fdc1-464d-ad61-ec81920ec7e3");
	public static readonly VanillaResources DIAMOND = new("ee7ef0cf-21ab-4c0c-871d-e477c5dfa1ce");
	public static readonly VanillaResources RUBY = new("87293f19-cca3-46e6-bb3d-6e8dc579e27b");
	public static readonly VanillaResources SULFUR = new("f08a4013-5852-4b33-bd7d-466aae6b6969");
	public static readonly VanillaResources URANIUM = new("3b52098a-4b58-467c-a29a-1d1b6d92ec3e");
	public static readonly VanillaResources NICKEL = new("7815b1b7-2830-418b-9b5d-19949b0ae9ec");
	public static readonly VanillaResources KYANITE = new("6e7f3d62-7e76-4415-af64-5dcd88fc3fe4");
	public static readonly VanillaResources MERCURY = new("779ef413-44b0-4eab-b94c-dfaadb1d2df0");
	public static readonly VanillaResources ION = new("38ebd2e5-9dcc-4d7a-ada4-86a22e01191a");

	public static readonly VanillaResources LARGE_SILVER = new("026d91e2-430b-4c6d-8bd4-b51e270d5eed");
	public static readonly VanillaResources LARGE_MERCURY = new("06ada673-7d2b-454f-ae11-951d628e64a7");
	public static readonly VanillaResources LARGE_RUBY = new("109bbd29-c445-4ad8-a4bf-be7bc6d421d6");
	public static readonly VanillaResources LARGE_LEAD = new("1efa1a20-3a39-4f56-ace0-154211d6af12");
	public static readonly VanillaResources LARGE_ION = new("41406e76-4b4c-4072-86f8-f5b8e6523b73");
	public static readonly VanillaResources LARGE_KYANITE = new("4f441e53-7a9a-44dc-83a4-b1791dc88ffd");
	public static readonly VanillaResources LARGE_COPPER = new("601ee500-1744-4697-8279-59ef35160edb");
	public static readonly VanillaResources LARGE_SULFUR = new("697beac5-e39a-4809-854d-9163da9f997e");
	public static readonly VanillaResources LARGE_SALT = new("793b4079-ef3b-43da-9fc7-3ec5cbc3ae19");
	public static readonly VanillaResources LARGE_LITHIUM = new("846c3df6-ffbf-4206-b591-72f5ba11ed40");
	public static readonly VanillaResources LARGE_KYANITE_BIG = new("853a9c5b-aba3-4d6b-a547-34553aa73fa9");
	public static readonly VanillaResources LARGE_NICKEL = new("9c8f56e6-3380-42e4-a758-e8d733b5ddec");
	public static readonly VanillaResources LARGE_TITANIUM = new("9f855246-76c4-438b-8e4d-9cd6d7ce4224");
	public static readonly VanillaResources LARGE_GOLD = new("a05fe1c9-ae0d-43db-a12c-865992808cb2");
	public static readonly VanillaResources LARGE_QUARTZ = new("b3db72b6-f0cf-4234-be74-d98bd4c49797");
	public static readonly VanillaResources LARGE_DIAMOND = new("e7c097ac-e7be-4808-aaaa-70178d96f68b");
	public static readonly VanillaResources LARGE_MAGNETITE = new("f67c158c-3b83-473c-ad52-93fd2eeef66b");
	public static readonly VanillaResources LARGE_URANIUM = new("fb5de2b6-1fe8-44fc-a555-dc0a09dc292a");

	private static readonly Dictionary<string, VanillaResources> names = new();

	public readonly string prefab;
	public readonly string pathname;

	private string name;

	private VanillaResources(string id) {
		prefab = id;
		pathname = PrefabData.getPrefab(id);
		lookup[prefab] = this;
	}

	public static VanillaResources getByName(string n) {
		populateNames();
		return names.ContainsKey(n) ? names[n] : null;
	}

	public static List<VanillaResources> getAll() {
		populateNames();
		return [..names.Values];
	}

	private static void populateNames() {
		if (names.Count == 0) {
			foreach (var f in typeof(VanillaResources).GetFields()) {
				if (f.IsStatic && f.FieldType == typeof(VanillaResources)) {
					var vf = (VanillaResources)f.GetValue(null);
					names[f.Name] = vf;
					vf.name = f.Name;
				}
			}
		}
	}

	public string getPrefabID() {
		return prefab;
	}

	public static VanillaResources getFromID(string pfb) {
		return lookup.ContainsKey(pfb) ? lookup[pfb] : null;
	}

	public override string ToString() {
		return name;
	}

}