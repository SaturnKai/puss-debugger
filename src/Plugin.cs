using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

using static puss_debugger.Level;
namespace puss_debugger;

[BepInPlugin("dev.saturnkai.pussdebugger", "puss-debugger", "1.0.0")]
[BepInProcess("PUSS!.exe")]
public class Plugin : BaseUnityPlugin
{
    // logger
    internal static ManualLogSource Log;

    // scrolling behavior
    private readonly float initialDelay = 0.5f;
    private readonly float scrollInterval = 0.05f;
    private float timer = 0f;
    private bool isScrolling = false;
    private int scrollDirection = 0;

    // level debug
    public static int selectedLevelIndex = 0;
    private static bool selectedLevelExists = false;

    // fps
	private float deltaTime;

    // config
    private static ConfigEntry<bool> LevelDebuggerEnabled;
    private static ConfigEntry<bool> vSyncEnabled;
    private static ConfigEntry<int> FPSLimit;
    private static ConfigEntry<bool> FPSDisplay;

    private void Awake()
    {
        // load config
        LevelDebuggerEnabled = Config.Bind("General", "Level_Debugger", true, "Enable level debugger.");
        vSyncEnabled = Config.Bind("General", "VSync", true, "Enable VSync.");
        FPSLimit = Config.Bind("General", "FPS_Limit", -1, "Limit the FPS. (no limit = -1)");
        FPSDisplay = Config.Bind("General", "FPS_Display", true, "Display FPS.");

        // set logger
        Log = Logger;

        // load patches
        Harmony.CreateAndPatchAll(typeof(Plugin));
        Harmony.CreateAndPatchAll(typeof(State));
        if (LevelDebuggerEnabled.Value) {
            Harmony.CreateAndPatchAll(typeof(Level));
            Harmony.CreateAndPatchAll(typeof(Splash));
            Logger.LogInfo("Patches successfully loaded.");
        }
    }

    private void OnGUI() {
        // print fps
        if (FPSDisplay.Value) {
            DebugText.PrintFPS(deltaTime);
        }

        if (!LevelDebuggerEnabled.Value)
            return;

        // print loaded level info
        if (loadedLevel != null) {
            PrintLevelInfo(loadedLevel, "Loaded:  ", 1);
        }

        // print selected level info
        if (levelKeys != null) {
            PrintLevelInfo(Core.Instance.levelsDataModel[levelKeys[selectedLevelIndex]], "Selected:", 2);
        }
    }

    private void Update() {

        // print fps
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        if (!LevelDebuggerEnabled.Value)
            return;

        // increment level
        if (Input.GetKeyDown(KeyCode.PageUp)) {
            ChangeSelectedLevel(1);
            isScrolling = true;
            scrollDirection = 1;
            timer = 0f;
        }
        // decrement level
        else if (Input.GetKeyDown(KeyCode.PageDown)) {
            ChangeSelectedLevel(-1);
            isScrolling = true;
            scrollDirection = -1;
            timer = 0f;
        }
        // change level
        else if (Input.GetKeyDown(KeyCode.Insert)) {
            if (selectedLevelExists && State.allowedStates.Contains(State.currentState)) {
                debug_ChangeLevelEnabled = true;
                Core.Instance.GC.FinishLevel();
            } else Log.LogWarning($"Selected level '{levelKeys[selectedLevelIndex]}' scene does not exist.");
        }

        // continuous scroll handler
        if (isScrolling) {
            timer += Time.deltaTime;

            if (timer > initialDelay) {
                float rapidScrollTime = timer - initialDelay;
                int levelChanges = Mathf.FloorToInt(rapidScrollTime / scrollInterval);

                if (levelChanges > 0) {
                    ChangeSelectedLevel(scrollDirection * levelChanges);
                    timer = initialDelay + (rapidScrollTime % scrollInterval);
                }
            }

            // stop scrolling when released
            if ((scrollDirection == 1 && Input.GetKeyUp(KeyCode.PageUp)) ||
                (scrollDirection == -1 && Input.GetKeyUp(KeyCode.PageDown))) {
                isScrolling = false;
            }
        }
    }

    private static bool LevelExists(string sceneName) {
        return SceneUtility.GetBuildIndexByScenePath(sceneName) != -1;
    }

    private static void ChangeSelectedLevel(int delta) {
        // change index
        selectedLevelIndex += delta;

        // set bounds
        if (selectedLevelIndex < 0) {
            selectedLevelIndex = Core.Instance.levelsDataModel.Count - 1;
        } else if (selectedLevelIndex >= Core.Instance.levelsDataModel.Count) {
            selectedLevelIndex = 0;
        }

        // set level exists
        selectedLevelExists = LevelExists(levelKeys[selectedLevelIndex]);
    }

    private void PrintLevelInfo(LevelData levelData, string header, int startIndex) {
        // define world list
        string worldList = levelData.worldsList.Count > 0
                ? string.Join(" ", [.. levelData.worldsList])
                : "<color=#FF0000>Unused</color>";
    
        // check if selected exists
        string levelExists = (!selectedLevelExists && header == "Selected:") 
            ? "<color=#FF0000>(No Scene)</color>" : "";
        
        string music = (header != "Loaded:  ") ? "" :
            $"<color=#FFFF00>{string.Join(" ", [..loadedLevelMusic])}</color>";
        
        DebugText.PrintMessage($"<color=#FFFFFF>{header} {levelData.levelId} {levelData.levelName}</color><color=#949494> {worldList}</color> {music} {levelExists}", startIndex);
    }

    [HarmonyPatch(typeof(Core), "Awake")]
    [HarmonyPostfix]
    static void Core_Awake() {
        // init level debug
        if (LevelDebuggerEnabled.Value) {
            Level.Init();
            ChangeSelectedLevel(0);
        }

        // set vsync
        if (vSyncEnabled.Value) {
            QualitySettings.vSyncCount = 1;
        }
        
        // set fps limit
        Application.targetFrameRate = FPSLimit.Value;
    }
}
