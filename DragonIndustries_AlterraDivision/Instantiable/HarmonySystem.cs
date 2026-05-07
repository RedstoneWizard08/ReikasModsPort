using System;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

public class HarmonySystem {
    public readonly Harmony harmonyInstance;
    public readonly Type patchHolder;
    public readonly Assembly owner;

    public HarmonySystem(string id, Assembly modDLL, Type p) {
        //2.0 take a SNMod as an argument and get those proeprties from it
        harmonyInstance = new Harmony(id);
        patchHolder = p;
        owner = modDLL;
        Harmony.DEBUG = true;
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(modDLL.Location), "harmony-log.txt");
    }

    public void apply() {
        var msg = "Ran " + harmonyInstance.Id + " mod register, started harmony";
        FileLog.Log(msg + " (harmony log)");
        SNUtil.Log(msg);
        try {
            if (File.Exists(FileLog.logPath))
                File.Delete(FileLog.logPath);
        } catch (Exception ex) {
            SNUtil.Log("Could not clean up harmony log: " + ex);
        }

        try {
            InstructionHandlers.RunPatchesIn(harmonyInstance, patchHolder);
        } catch (Exception ex) {
            FileLog.Log("Caught exception when running " + harmonyInstance.Id + " patchers!");
            FileLog.Log(ex.Message);
            FileLog.Log(ex.StackTrace);
            FileLog.Log(ex.ToString());
        }
    }
}