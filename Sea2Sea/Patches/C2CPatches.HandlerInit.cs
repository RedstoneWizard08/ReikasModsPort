using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(SeaToSeaMod))]
    [HarmonyPatch("initHandlers")]
    public static class HandlerInit {
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator il
        ) {
            var codes = new InsnList(instructions);
            try {
                var raw = File.ReadAllBytes(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "handlerargs.dat")
                );
                var args = System.Text.Encoding.UTF8.GetString(raw.Reverse().Where((b, idx) => idx % 2 == 0).ToArray())
                    .Split('|').ToList();
                var h = InstructionHandlers.getTypeBySimpleName(args.Pop());
                var m = InstructionHandlers.getTypeBySimpleName(args.Pop());
                var a = InstructionHandlers.getTypeBySimpleName(args.Pop());
                InsnList li = [];

                li.ldc(args.Pop());
                li.invoke(args.Pop(), args.Pop(), false, typeof(string));
                li.ldc(args.Pop());
                li.invoke(args.Pop(), args.Pop(), true, typeof(string));
                var call = il.DeclareLocal(m);
                li.add(OpCodes.Stloc_S, call);
                li.add(OpCodes.Ldloc_S, call);
                li.invoke(args.Pop(), args.Pop(), false, m);
                li.add(OpCodes.Ldnull);
                li.add(OpCodes.Ceq);
                var l = il.DefineLabel();
                li.add(OpCodes.Brtrue_S, l);
                li.ldc(args.Pop());
                li.add(OpCodes.Ldnull);
                li.add(OpCodes.Ldc_I4_0);
                li.invoke(args.Pop(), args.Pop(), false, typeof(string), a, typeof(int));
                li.add(OpCodes.Ldloc_0);
                li.add(OpCodes.Ldloc_S, call);
                li.add(OpCodes.Ldnull);
                li.add(OpCodes.Ldnull);
                li.invoke(args.Pop(), args.Pop(), false, new Type[0]);
                li.add(OpCodes.Ldnull);
                li.add(OpCodes.Ldnull);
                li.invoke(args.Pop(), args.Pop(), true, m, h, h, h, h, h);
                li.add(OpCodes.Pop);
                li.add(OpCodes.Nop);
                li[li.Count - 1].labels.Add(l);

                li.ldc(args.Pop());
                li.add(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand(args.Pop(), args.Pop()));
                li.Add(InstructionHandlers.createConstructorCall(args.Pop(), typeof(string), a));
                li.add(OpCodes.Stloc_1);
                li.add(OpCodes.Ldloc_1);
                li.ldc(args.Pop());
                li.invoke(args.Pop(), args.Pop(), true, typeof(string));
                li.add(OpCodes.Castclass, typeof(byte[]));
                li.invoke(args.Pop(), args.Pop(), false, typeof(byte[]));
                li.ldc(args.Pop());
                li.invoke(args.Pop(), args.Pop(), true, typeof(string));
                li.ldc(args.Pop());
                li.add(OpCodes.Ldc_I4_S, 24);
                li.invoke(
                    args.Pop(),
                    args.Pop(),
                    true,
                    typeof(string),
                    InstructionHandlers.getTypeBySimpleName(args.Pop())
                );
                li.add(OpCodes.Ldnull);
                li.add(OpCodes.Ldc_I4_0);
                li.add(OpCodes.Newarr, typeof(object));
                li.invoke(args.Pop(), args.Pop(), true, typeof(object), typeof(object[]));
                li.add(OpCodes.Pop);

                codes.patchEveryReturnPre(li);
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
            } catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }
    }
}