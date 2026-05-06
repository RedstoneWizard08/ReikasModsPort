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
    [HarmonyPatch(typeof(WorldgenIntegrityChecks))]
    [HarmonyPatch("checkWorldgenIntegrity")]
    public static class WorldLoadCheck {
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator il
        ) {
            var codes = new InsnList(instructions);
            try {
                var raw = File.ReadAllBytes(
                    Path.Combine(Path.GetDirectoryName(SeaToSeaMod.ModDLL.Location), "worldhash.dat")
                );
                var data = System.Text.Encoding.UTF8.GetString(raw.Reverse().Where((b, idx) => idx % 3 == 1).ToArray())
                    .PolySplit('%', '|');
                var args = data.Pop().ToList();

                var loc0 = il.DeclareLocal(typeof(string[]));
                var loc1 = il.DeclareLocal(typeof(int));
                var loc2 = il.DeclareLocal(typeof(string));

                var ref1 = new CodeInstruction(OpCodes.Ldloc_S, loc1);
                var l1 = il.DefineLabel();
                ref1.labels.Add(l1);

                var ref2 = new CodeInstruction(OpCodes.Nop);
                var l2 = il.DefineLabel();
                ref2.labels.Add(l2);

                var ref3 = new CodeInstruction(OpCodes.Nop);
                var l3 = il.DefineLabel();
                ref3.labels.Add(l3);


                InsnList li = [];
                li.ldc(data.Pop()[0]);
                li.add(OpCodes.Ldc_I4_S, 58);
                li.invoke(args.Pop(), args.Pop(), false, typeof(string), typeof(char));
                li.add(OpCodes.Stloc_S, loc0);
                li.add(OpCodes.Ldc_I4_0);
                li.add(OpCodes.Stloc_S, loc1);
                li.add(OpCodes.Br_S, l1);
                li.Add(ref2);
                li.add(OpCodes.Ldarg_0);
                li.add(OpCodes.Ldloc_S, loc0);
                li.add(OpCodes.Ldloc_S, loc1);
                li.add(OpCodes.Ldelem_Ref);
                li.invoke(args.Pop(), args.Pop(), false, typeof(string));
                li.add(OpCodes.Ldloc_S, loc0);
                li.add(OpCodes.Ldloc_S, loc1);
                li.add(OpCodes.Ldc_I4_1);
                li.add(OpCodes.Add);
                li.add(OpCodes.Ldelem_Ref);

                args = data.Pop().ToList();
                li.invoke(args.Pop(), args.Pop(), false, typeof(Type), typeof(string));
                li.invoke(args.Pop(), args.Pop(), false, typeof(MethodBase));
                li.add(OpCodes.Ldnull);
                li.add(OpCodes.Ceq);
                li.add(OpCodes.Ldc_I4_0);
                li.add(OpCodes.Ceq);
                li.add(OpCodes.Or);
                li.add(OpCodes.Starg_S, 0);
                li.add(OpCodes.Nop);
                li.add(OpCodes.Ldloc_S, loc1);
                li.add(OpCodes.Ldc_I4_2);
                li.add(OpCodes.Add);
                li.add(OpCodes.Stloc_S, loc1);
                li.Add(ref1);
                li.add(OpCodes.Ldloc_S, loc0);
                li.add(OpCodes.Ldlen);
                li.add(OpCodes.Conv_I4);
                li.add(OpCodes.Clt);
                li.add(OpCodes.Brtrue_S, l2);
                li.add(OpCodes.Ldarg_0);
                li.add(OpCodes.Ldc_I4_0);
                li.add(OpCodes.Ceq);
                li.add(OpCodes.Brtrue_S, l3);
                li.add(OpCodes.Nop);

                args = data.Pop().ToList();
                li.add(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand(args.Pop(), args.Pop()));
                li.invoke(args.Pop(), args.Pop(), true, new Type[0]);
                li.invoke(args.Pop(), args.Pop(), false, typeof(string));
                li.add(OpCodes.Stloc_S, loc2);
                li.add(OpCodes.Ldloc_S, loc2);

                args = data.Pop().ToList();
                li.ldc(args.Pop());
                li.invoke(args.Pop(), args.Pop(), false, typeof(string), typeof(string));
                li.invoke(args.Pop(), args.Pop(), false, typeof(string));
                li.add(OpCodes.Nop);
                li.add(OpCodes.Ldloc_S, loc2);
                li.ldc(args.Pop());
                li.invoke(args.Pop(), args.Pop(), false, typeof(string), typeof(string));
                li.invoke(args.Pop(), args.Pop(), false, typeof(string));
                li.add(OpCodes.Nop);
                li.add(OpCodes.Ldloc_S, loc2);
                li.ldc(args.Pop());
                li.invoke(args.Pop(), args.Pop(), false, typeof(string), typeof(string));
                li.invoke(args.Pop(), args.Pop(), false, typeof(string));
                li.Add(ref3);

                codes.patchInitialHook(li.ToArray());
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