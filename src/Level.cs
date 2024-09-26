using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace puss_debugger;

public static class Level {

    // loaded level
    public static LevelData loadedLevel = null;
    public static List<string> loadedLevelMusic = new List<string>();

    // level data model
    public static List<string> levelKeys = null;

    // level debug
    public static bool debug_ChangeLevelEnabled = false;
    static readonly string levelPattern = @"^level_\d{3}$";

    public static void Init() {

        // get all scenes
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++) {
            string name = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            if (Regex.IsMatch(name, levelPattern) && !Core.Instance.levelsDataModel.ContainsKey(name) && i > 2) {
                
                // init level data
                LevelData levelData = new LevelData(name);
                levelData.levelName = "Unknown";
                levelData.AddLevelStep(LevelDeathMecanics.ResetPlayerPosition, false, false);

                // add to data model
                Core.Instance.levelsDataModel.Add(name, levelData);
            }
        }

        // set level keys
        levelKeys = [.. Core.Instance.levelsDataModel.Keys];
    }

    [HarmonyPatch(typeof(LevelRandomizator), "GetLevel")]
    [HarmonyPrefix]
    static bool LR_GetLevel(int world_level_id, ref string __result) {

        if (debug_ChangeLevelEnabled) {
            __result = levelKeys[Plugin.selectedLevelIndex];
            debug_ChangeLevelEnabled = false;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(LevelManager), "OnLoaded")]
    [HarmonyPostfix]
    static void LM_OnLoaded() {
        // update loaded level
        loadedLevel = Core.Instance.LM.CurrentLevel;

        // load music
        loadedLevelMusic.Clear();
        foreach (AudioSource source in AudioSource.FindObjectsOfType<AudioSource>()) {
            if (source.gameObject.name.ToLower().Contains("music"))
                loadedLevelMusic.Add(source.clip.name);
        }
    }

}