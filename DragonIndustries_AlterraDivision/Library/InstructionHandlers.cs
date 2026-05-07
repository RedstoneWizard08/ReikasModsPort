using System;
//Working with Lists and Collections
using System.Collections.Generic; //Working with Lists and Collections
using System.IO; //For data read/write methods
using System.Linq; //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

public static class InstructionHandlers {
    public static bool DumpIL = false;

    public static long GetIntFromOpcode(CodeInstruction ci) {
        switch (ci.opcode.Name) {
            case "ldc.i4.m1":
                return -1;
            case "ldc.i4.0":
                return 0;
            case "ldc.i4.1":
                return 1;
            case "ldc.i4.2":
                return 2;
            case "ldc.i4.3":
                return 3;
            case "ldc.i4.4":
                return 4;
            case "ldc.i4.5":
                return 5;
            case "ldc.i4.6":
                return 6;
            case "ldc.i4.7":
                return 7;
            case "ldc.i4.8":
                return 8;
            case "ldc.i4.s":
                return (sbyte)ci.operand;
            case "ldc.i4":
                return (int)ci.operand;
            case "ldc.i8":
                return (long)ci.operand;
            default:
                return long.MaxValue;
        }
    }

    public static void NullInstructions(InsnList li, int begin, int end) {
        for (var i = begin; i <= end; i++) {
            var insn = li[i];
            insn.opcode = OpCodes.Nop;
            insn.operand = null;
        }
    }

    public static CodeInstruction CreateMethodCall(string owner, string name, bool instance, params string[] args) {
        return new CodeInstruction(OpCodes.Call, ConvertMethodOperand(owner, name, instance, args));
    }

    public static CodeInstruction CreateMethodCall(string owner, string name, bool instance, params Type[] args) {
        return new CodeInstruction(OpCodes.Call, ConvertMethodOperand(owner, name, instance, args));
    }

    public static CodeInstruction CreateConstructorCall(string owner, params Type[] args) {
        return new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(AccessTools.TypeByName(owner), args));
    }

    public static MethodInfo ConvertMethodOperand(string owner, string name, bool instance, params string[] args) {
        var types = new Type[args.Length];
        for (var i = 0; i < args.Length; i++) {
            types[i] = AccessTools.TypeByName(args[i]);
        }

        return ConvertMethodOperand(owner, name, instance, types);
    }

    public static MethodInfo ConvertMethodOperand(string owner, string name, bool instance, params Type[] args) {
        var container = AccessTools.TypeByName(owner);
        if (container == null) {
            throw new Exception("Could not find a type matching name '" + owner + "'!");
        }

        MethodInfo ret;
        try {
            ret = AccessTools.Method(container, name, args);
        } catch (Exception e) {
            throw new Exception(
                "[Harmony " + typeof(AccessTools).AssemblyQualifiedName + "] Failed to perform search for " + owner +
                "::" + name,
                e
            );
        }

        //ret.IsStatic = !instance;
        if (ret == null) {
            var info = "Harmony version:" + typeof(AccessTools).FullClearName() + "\n";
            info += "Methods:\n" + container.ListMethods() + "\nComparison:\n";
            foreach (var mi in GetMethods(container)) {
                if (mi.Name == name) {
                    info += "Name Match: " + mi.Name;
                    var pp = mi.GetParameters();

                    if (pp.Length == args.Length)
                        info += "; Arg Len Match: " + pp.Length;
                    else
                        info += "; Arg Len mismatch: " + args.Length + " vs " + pp.Length;

                    if (args.SequenceEqual(pp.Select(p => p.ParameterType))) {
                        info += "; Arg Match: " + string.Join(", ", pp.Select(ClearString).ToArray());
                    } else {
                        info += "; Arg mismatch:\n";
                        for (var i = 0; i < pp.Length; i++) {
                            var pt = pp[i].ParameterType;
                            info += i + ": " + args[i].FullClearName() + " vs " + pt.FullClearName() + " (" +
                                    (args[i] == pt) + ")\n";
                        }
                    }

                    info += "\n";
                }
            }

            throw new Exception(
                "Could not find a method named '" + name + "' with args " + args.ToDebugString() + " in type '" +
                owner + "'!\n" + info
            );
        }

        return ret;
    }

    public static FieldInfo ConvertFieldOperand(string owner, string name) {
        var container = AccessTools.TypeByName(owner);
        var ret = AccessTools.Field(container, name);
        return ret == null
            ? throw new Exception("Could not find a method named '" + name + "' in type '" + owner + "'!")
            : ret;
    }

    extension(InsnList li) {
        public int GetInstruction(int start, int index, OpCode opcode, params object[] args) {
            var count = 0;
            if (index < 0) {
                index = -index - 1;
                for (var i = li.Count - 1; i >= 0; i--) {
                    var insn = li[i];
                    if (insn.opcode == opcode) {
                        if (Match(insn, args)) {
                            if (count == index)
                                return i;
                            count++;
                        }
                    }
                }
            } else {
                for (var i = start; i < li.Count; i++) {
                    var insn = li[i];
                    if (insn.opcode == opcode) {
                        if (Match(insn, args)) {
                            if (count == index)
                                return i;
                            count++;
                        }
                    }
                }
            }

            throw new Exception(
                "Instruction not found: " + opcode + " #" + string.Join(",", args) + "\nInstruction list:\n" +
                li.ClearString()
            );
        }

        public int GetMethodCallByName(int start, int index, string owner, string name) {
            var count = 0;
            if (index < 0) {
                index = -index - 1;
                for (var i = li.Count - 1; i >= 0; i--) {
                    var insn = li[i];
                    if (IsMethodCall(insn, owner, name)) {
                        if (count == index)
                            return i;
                        count++;
                    }
                }
            } else {
                for (var i = start; i < li.Count; i++) {
                    var insn = li[i];
                    if (IsMethodCall(insn, owner, name)) {
                        if (count == index)
                            return i;
                        count++;
                    }
                }
            }

            throw new Exception(
                "Method call not found: " + owner + "::" + name + "\nInstruction list:\n" + li.ClearString()
            );
        }
    }

    public static int GetFirstOpcode(InsnList li, int after, OpCode opcode) {
        for (var i = after; i < li.Count; i++) {
            var insn = li[i];
            if (insn.opcode == opcode) {
                return i;
            }
        }

        throw new Exception("Instruction not found: " + opcode + "\nInstruction list:\n" + li.ClearString());
    }

    extension(InsnList li) {
        public int GetLastOpcodeBefore(int before, OpCode opcode) {
            if (before > li.Count)
                before = li.Count;
            for (var i = before - 1; i >= 0; i--) {
                var insn = li[i];
                if (insn.opcode == opcode) {
                    return i;
                }
            }

            throw new Exception("Instruction not found: " + opcode + "\nInstruction list:\n" + li.ClearString());
        }

        public int GetLastInstructionBefore(int before, OpCode opcode, params object[] args) {
            for (var i = before - 1; i >= 0; i--) {
                var insn = li[i];
                if (insn.opcode == opcode) {
                    if (Match(insn, args)) {
                        return i;
                    }
                }
            }

            throw new Exception(
                "Instruction not found: " + opcode + " #" + string.Join(",", args) + "\nInstruction list:\n" +
                li.ClearString()
            );
        }
    }

    public static bool Match(CodeInstruction a, CodeInstruction b) {
        return a.opcode == b.opcode && MatchOperands(a.operand, b.operand);
    }

    public static bool MatchOperands(object o1, object o2) {
        return o1 == o2 || (o1 != null && o2 != null && (o1 is LocalBuilder builder && o2 is LocalBuilder localBuilder
            ? builder.LocalIndex == localBuilder.LocalIndex
            : o1.Equals(o2)));
    }

    public static bool IsMethodCall(CodeInstruction insn, string owner, string name) {
        if (insn.opcode != OpCodes.Call && insn.opcode != OpCodes.Callvirt && insn.opcode != OpCodes.Calli)
            return false;
        var mi = (MethodInfo)insn.operand;
        //FileLog.Log("Comparing "+mi.Name+" in "+mi.DeclaringType.FullName+" to "+owner+" & "+name);
        return mi.Name == name && mi.DeclaringType!.FullName == owner;
    }

    public static bool Match(CodeInstruction insn, params object[] args) {
        //FileLog.Log("Comparing "+insn.operand.GetType()+" "+insn.operand.ToString()+" against seek of "+String.Join(",", args.Select(p=>p.ToString()).ToArray()));
        if (insn.opcode == OpCodes.Call || insn.opcode == OpCodes.Callvirt) {
            //string class, string name, bool instance, Type[] args
            var info = ConvertMethodOperand((string)args[0], (string)args[1], (bool)args[2], (Type[])args[3]);
            return (MethodInfo)insn.operand == info;
        }

        if (insn.opcode == OpCodes.Isinst || insn.opcode == OpCodes.Newobj) { //string class
            return (Type)insn.operand == AccessTools.TypeByName((string)args[0]);
        }

        if (insn.opcode == OpCodes.Ldfld || insn.opcode == OpCodes.Stfld || insn.opcode == OpCodes.Ldsfld ||
            insn.opcode == OpCodes.Stsfld) { //string class, string name
            var info = ConvertFieldOperand((string)args[0], (string)args[1]);
            return (FieldInfo)insn.operand == info;
        }

        if (insn.opcode == OpCodes.Ldarg) { //int pos
            return insn.operand == args[0];
        }

        if (insn.opcode == OpCodes.Ldc_I4) { //ldc
            return insn.LoadsConstant(Convert.ToInt32(args[0]));
        }

        if (insn.opcode == OpCodes.Ldc_R4) { //ldc
            return insn.LoadsConstant(Convert.ToSingle(args[0]));
        }

        if (insn.opcode == OpCodes.Ldc_I8) { //ldc
            return insn.LoadsConstant(Convert.ToInt64(args[0]));
        }

        if (insn.opcode == OpCodes.Ldc_R8) { //ldc
            return insn.LoadsConstant(Convert.ToDouble(args[0]));
        }

        if (insn.opcode == OpCodes.Ldloc_S || insn.opcode == OpCodes.Stloc_S) {
            //LocalBuilder contains a pos and type
            var loc = (LocalBuilder)insn.operand;
            return args[0] is int && loc.LocalIndex == (int)args[0] /* && loc.LocalType == args[1]*/;
        }

        if (insn.opcode == OpCodes.Ldstr) { //string var
            return (string)insn.operand == (string)args[0];
        }

        return true;
    }

    public static string ClearString(this CodeInstruction ci) {
        return ci.opcode.Name + " " + ToOperandString(ci.opcode, ci.operand);
    }

    private static string ToOperandString(OpCode code, object operand) {
        if (operand is MethodInfo info) {
            return info.DeclaringType + "." + info.Name + " (" + string.Join(
                ", ",
                info.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)
            ) + ") [static=" + info.IsStatic + "]";
        }

        if (operand is FieldInfo fieldInfo) {
            return fieldInfo.DeclaringType + "." + fieldInfo.Name + " [static=" + fieldInfo.IsStatic + "]";
        }

        return operand is LocalBuilder builder
                ? "localvar " + builder.LocalIndex
                : code == OpCodes.Ldarg_S || code == OpCodes.Ldarg
                    ? "arg " + operand
                    : operand is Type type
                        ? "type " + type.Name
                        : operand != null
                            ? operand + " [" + operand.GetType() + "]"
                            : "<null>"; /*
                        if (code == OpCodes.Ldloc_S || code == OpCodes.Stloc_S) {
                            return "localvar "+((LocalBuilder)operand).LocalIndex;
                        }*/
    }

    public static Type GetTypeBySimpleName(string name) {
        if (string.IsNullOrEmpty(name))
            throw new Exception("You cannot get a type of no name!");
        return (from a in AppDomain.CurrentDomain.GetAssemblies().Reverse()
            let an = a.GetName().Name
            where !an.StartsWith("0Harmony") || an == "0Harmony"
            select a.GetType(name)).FirstOrDefault(tt => tt != null);
    }

    public static HarmonyMethod Clear() {
        Func<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>> dele = _ =>
            new InsnList { new CodeInstruction(OpCodes.Ret) };
        return new HarmonyMethod(dele.Method);
    }

    public static void RunPatchesIn(Harmony h, Type parent) {
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(parent.Assembly.Location)!, "harmony-log.txt");
        var msg = "Running harmony patches in " + parent.Assembly.GetName().Name + "::" + parent.Name;
        SNUtil.Log(msg);
        FileLog.Log(msg);
        foreach (var t in parent.GetNestedTypes(
                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static
                 )) {
            FileLog.Log("Running harmony patches in " + t.Name);
            h.PatchAll(t);
        }
    }

    public static void PatchMethod(Harmony h, Type methodHolder, string name, Type patchHolder, string patchName) {
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(patchHolder.Assembly.Location)!, "harmony-log.txt");
        FileLog.Log(
            "Running harmony patch in " + patchHolder.FullName + "::" + patchName + " on " + methodHolder.FullName +
            "::" + name
        );
        var m = methodHolder.GetMethod(
            name,
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
        );
        if (m == null)
            throw new Exception("Method " + name + " not found in " + methodHolder.AssemblyQualifiedName);
        PatchMethod(
            h,
            m,
            new HarmonyMethod(
                AccessTools.Method(
                    patchHolder,
                    patchName,
                    [
                        typeof(IEnumerable<CodeInstruction>),
                    ]
                )
            )
        );
    }

    public static void PatchMethod(HarmonySystem h, Type methodHolder, string name, Action<InsnList> patch) {
        PatchMethod(h.harmonyInstance, methodHolder, name, h.owner, patch);
    }

    public static void PatchMethod(
        Harmony h,
        Type methodHolder,
        string name,
        Assembly patchHolder,
        Action<InsnList> patch
    ) {
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(patchHolder.Location)!, "harmony-log.txt");
        FileLog.Log(
            "Running harmony patch from " + patchHolder.GetName().Name + " on " + methodHolder.FullName + "::" + name
        );
        var m = methodHolder.GetMethod(
            name,
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
        );
        if (m == null)
            throw new Exception("Method " + name + " not found in " + methodHolder.FullName);
        _currentPatch = patch;
        PatchMethod(
            h,
            m,
            new HarmonyMethod(
                AccessTools.Method(
                    MethodBase.GetCurrentMethod()!.DeclaringType,
                    "patchHook",
                    [typeof(IEnumerable<CodeInstruction>)]
                )
            )
        );
        _currentPatch = null;
    }

    public static void PatchMethod(Harmony h, MethodInfo m, Assembly patchHolder, Action<InsnList> patch) {
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(patchHolder.Location)!, "harmony-log.txt");
        FileLog.Log(
            "Running harmony patch from " + patchHolder.GetName().Name + " on " + m.DeclaringType!.FullName + "::" +
            m.Name
        );
        _currentPatch = patch;
        PatchMethod(
            h,
            m,
            new HarmonyMethod(
                AccessTools.Method(
                    MethodBase.GetCurrentMethod()!.DeclaringType,
                    "patchHook",
                    [typeof(IEnumerable<CodeInstruction>)]
                )
            )
        );
        _currentPatch = null;
    }

    private static Action<InsnList> _currentPatch;

    private static IEnumerable<CodeInstruction> PatchHook(IEnumerable<CodeInstruction> instructions) {
        var codes = new InsnList(instructions);
        _currentPatch.Invoke(codes);
        //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
        return codes.AsEnumerable();
    }

    private static void PatchMethod(Harmony h, MethodInfo m, HarmonyMethod patch) {
        try {
            h.Patch(m, null, null, patch);
            FileLog.Log("Done patch");
        } catch (Exception e) {
            FileLog.Log("Caught exception when running patch!");
            FileLog.Log(e.Message);
            FileLog.Log(e.StackTrace);
            FileLog.Log(e.ToString());
        }
    }

    public static string GetILDumpFolder() {
        return Path.Combine(Path.GetDirectoryName(SNUtil.DiDLL.Location)!, "original-il");
    }

    public static void DumpMethodIL(IEnumerable<CodeInstruction> li, string id) {
        var file = Path.Combine(GetILDumpFolder(), id + ".txt");
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, li.ToList().ClearString());
    }

    extension(Type t) {
        public IEnumerable<MethodInfo> GetMethods() {
            return t.GetMethods(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.FlattenHierarchy
            );
        }

        public string ListMethods() {
            return string.Join("\n", GetMethods(t).Select(ClearString).ToArray());
        }

        public MethodInfo GetAnyMethod(string name) { //do not remove, useful for debug!
            try {
                return string.IsNullOrEmpty(name)
                    ? throw new Exception("You cannot get a method of no name!")
                    : t.GetMethod(
                        name,
                        BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
                    );
            } catch (Exception ex) {
                throw new Exception("Failed to find '" + t.Name + "::" + name + "'", ex);
            }
        }
    }

    public static string ClearString(this MethodInfo m) {
        return m.Name + "(" + string.Join(", ", m.GetParameters().Select(ClearString).ToArray()) + ") -> " +
               m.ReturnType.FullName;
    }

    public static string ClearString(this ParameterInfo m) {
        return m.ParameterType.FullName + " " + m.Name;
    }

    public static string FullClearName(this Type t) {
        return t == null ? "NULL" : t.FullName + " in " + t.Assembly.GetName().Name + " @ " + t.Assembly.Location;
    }

    public static void LogPatchStart(MethodBase patch, IEnumerable<CodeInstruction> orig) {
        if (DumpIL) {
            var attrs = Attribute.GetCustomAttributes(patch.DeclaringType!);
            try {
                Type t = null;
                string name = null;
                foreach (var a in attrs) {
                    if (a is HarmonyPatch ha) {
                        if (ha.info.declaringType != null)
                            t = ha.info.declaringType;
                        if (ha.info.methodName != null)
                            name = ha.info.methodName;
                    }
                }

                if (t == null)
                    throw new Exception("No target type");
                if (string.IsNullOrEmpty(name))
                    throw new Exception("No target name");
                var id = t.Name + "--" + name;
                DumpMethodIL(orig, id);
            } catch (Exception e) {
                SNUtil.Log(
                    "Threw exception dumping patch '" + patch.DeclaringType.Name + "' [" + attrs.ToDebugString("\n") +
                    "] IL: " + e
                );
            }
        }

        FileLog.Log("Starting patch " + patch.DeclaringType);
    }

    public static void LogCompletedPatch(MethodBase patch, IEnumerable<CodeInstruction> orig) { /*
        if (dumpIL) {
            Attribute[] attrs = System.Attribute.GetCustomAttributes(patch.DeclaringType);
            try {
                Type t = null;
                string name = null;
                foreach (Attribute a in attrs) {
                    if (a is HarmonyPatch ha) {
                        if (ha.info.declaringType != null)
                            t = ha.info.declaringType;
                        if (ha.info.methodName != null)
                            name = ha.info.methodName;
                    }
                }
                if (t == null)
                    throw new Exception("No target type");
                if (string.IsNullOrEmpty(name))
                    throw new Exception("No target name");
                string id = t.Name+"--"+name;
                dumpMethodIL(orig, id);
            }
            catch (Exception e) {
                SNUtil.log("Threw exception dumping patch '"+patch.DeclaringType.Name+"' ["+ attrs.toDebugString("\n") + "] IL: "+e);
            }
        }*/
        FileLog.Log("Done patch " + patch.DeclaringType);
    }

    public static void LogErroredPatch(MethodBase patch) {
        FileLog.Log("Caught exception when running patch " + patch.DeclaringType + "!");
    }

    extension(List<CodeInstruction> li) {
        public string ClearString() {
            return "\n" + string.Join("\n", li.Select(p => p.ClearString()).ToArray());
        }

        public string ClearString(int idx) {
            return idx < 0 || idx >= li.Count
                ? "ERROR: OOB " + idx + "/" + li.Count
                : "#" + Convert.ToString(idx, 16) + " = " + li[idx].ClearString();
        }
    }
}

public class InsnList : List<CodeInstruction> {
    public InsnList() {
    }

    public InsnList(IEnumerable<CodeInstruction> li) : base(li) {
    }

    public InsnList Add(OpCode opcode, object operand = null) {
        Add(new CodeInstruction(opcode, operand));
        return this;
    }

    public InsnList Invoke(string owner, string name, bool instance, params Type[] args) {
        Add(InstructionHandlers.CreateMethodCall(owner, name, instance, args));
        return this;
    }

    public InsnList Field(OpCode opcode, string owner, string name) {
        return Add(opcode, InstructionHandlers.ConvertFieldOperand(owner, name));
    }

    public InsnList Field(string owner, string name, bool inst, bool put) {
        OpCode code;
        if (inst)
            code = put ? OpCodes.Stfld : OpCodes.Ldfld;
        else
            code = put ? OpCodes.Stsfld : OpCodes.Ldsfld;
        return Add(code, InstructionHandlers.ConvertFieldOperand(owner, name));
    }

    public InsnList Ldc(string val) {
        return Add(OpCodes.Ldstr, val);
    }

    public InsnList Ldc(int val) {
        var code = OpCodes.Ldc_I4;
        switch (val) {
            case -1:
                code = OpCodes.Ldc_I4_M1;
                break;
            case 0:
                code = OpCodes.Ldc_I4_0;
                break;
            case 1:
                code = OpCodes.Ldc_I4_1;
                break;
            case 2:
                code = OpCodes.Ldc_I4_2;
                break;
            case 3:
                code = OpCodes.Ldc_I4_3;
                break;
            case 4:
                code = OpCodes.Ldc_I4_4;
                break;
            case 5:
                code = OpCodes.Ldc_I4_5;
                break;
            case 6:
                code = OpCodes.Ldc_I4_6;
                break;
            case 7:
                code = OpCodes.Ldc_I4_7;
                break;
            case 8:
                code = OpCodes.Ldc_I4_8;
                break;
        }

        return code == OpCodes.Ldc_I4 ? Add(code, val) : Add(code);
    }

    public InsnList Ldc(long val) {
        return Add(OpCodes.Ldc_I8, val);
    }

    public InsnList Ldc(float val) {
        return Add(OpCodes.Ldc_R4, val);
    }

    public InsnList Ldc(double val) {
        return Add(OpCodes.Ldc_R8, val);
    }

    public int PatchEveryReturnPre(params CodeInstruction[] insert) {
        return PatchEveryReturnPre(insert.ToList());
    }

    public int PatchEveryReturnPre(IEnumerable<CodeInstruction> insert) {
        var times = PatchEveryReturnPre((li, idx) => li.InsertRange(idx, insert));
        //FileLog.Log("Injected "+times+" times, codes are now: " + InstructionHandlers.toString(codes));
        return times;
    }

    public int PatchEveryReturnPre(Action<InsnList, int> injectHook) {
        var times = 0;
        for (var i = Count - 1; i >= 0; i--) {
            if (this[i].opcode != OpCodes.Ret) continue;
            //FileLog.Log("Injected @ "+i+", codes are now: "+InstructionHandlers.toString(codes));
            injectHook(this, i);
            times++;
        }

        return times;
    }

    public InsnList PatchInitialHook(params CodeInstruction[] insert) {
        InsnList li = [];
        foreach (var c in insert) {
            li.Add(c);
        }

        return PatchInitialHook(li);
    }

    public InsnList PatchInitialHook(InsnList insert) {
        for (var i = insert.Count - 1; i >= 0; i--) {
            Insert(0, insert[i]);
        }

        return this;
    }

    public InsnList Extract(int from, int to) {
        InsnList li = [];
        for (var i = from; i <= to; i++) {
            li.Add(this[i]);
        }

        RemoveRange(from, to - from + 1);
        return li;
    }

    public void ReplaceConstantWithMethodCall(int val, InsnList put) {
        ReplaceConstantWithMethodCall(
            val,
            c => c.opcode == OpCodes.Ldc_I4 && c.LoadsConstant(Convert.ToInt32(val)),
            put
        );
    }

    public void ReplaceConstantWithMethodCall(long val, InsnList put) {
        ReplaceConstantWithMethodCall(
            val,
            c => c.opcode == OpCodes.Ldc_I8 && c.LoadsConstant(Convert.ToInt64(val)),
            put
        );
    }

    public void ReplaceConstantWithMethodCall(float val, InsnList put) {
        ReplaceConstantWithMethodCall(
            val,
            c => c.opcode == OpCodes.Ldc_R4 && c.LoadsConstant(Convert.ToSingle(val)),
            put
        );
    }

    public void ReplaceConstantWithMethodCall(double val, InsnList put) {
        ReplaceConstantWithMethodCall(
            val,
            c => c.opcode == OpCodes.Ldc_R8 && c.LoadsConstant(Convert.ToDouble(val)),
            put
        );
    }

    private void ReplaceConstantWithMethodCall(double val, Func<CodeInstruction, bool> f, InsnList put) {
        for (var i = Count - 1; i >= 0; i--) {
            var c = this[i];
            if (!f(c)) continue;
            RemoveAt(i);
            InsertRange(i, put);
        }
    }
}