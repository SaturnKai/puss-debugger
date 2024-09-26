using System.Linq;
using HarmonyLib;

namespace puss_debugger;

public static class State {

    public static string currentState;
    public static string[] allowedStates = [
        "TUTORIAL", "SEVEN_SEALS", "GAME"
    ];

    [HarmonyPatch(typeof(StatesManager), "SwitchState")]
    [HarmonyPostfix]
    static void SM_SwitchState(States stateId) {
        // set current state
        currentState = stateId.ToString();

        // init debug font
        if (allowedStates.Contains(currentState)) {
            DebugText.Init();
        }
    }

}