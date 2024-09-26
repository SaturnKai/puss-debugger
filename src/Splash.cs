using HarmonyLib;

namespace puss_debugger;

public static class Splash {

    [HarmonyPatch(typeof(SplashState), "Enable")]
    [HarmonyPostfix]
    static void SplashState_Enable(SplashState __instance) {
        
        // auto play
        if (Level.debug_ChangeLevelEnabled) {
            __instance.Play();
        }

    }

}