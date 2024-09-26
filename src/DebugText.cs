using System;
using UnityEngine;
using UnityEngine.UI;

namespace puss_debugger;

public static class DebugText {

    // constants
    private static readonly string fontFamily = "PxPlus_IBM_MDA";
    private static readonly int fontSize = 24;
    private static readonly float screenHeight = Screen.height;
    private static readonly float labelWidth = 400;
    private static readonly float labelHeight = 20;
    private static readonly float bottomMargin = 0;

    // style
    private static GUIStyle textStyle = null;

    public static void PrintMessage(string message, int index) {
        // check style
        if (textStyle == null) {
            return;
        }

        // define constants
        float x = 20;

        // calculate y position
        float y = screenHeight - labelHeight * (index + 1) - bottomMargin;
        if (index == 0)
        {
            y = screenHeight - labelHeight - bottomMargin;
        }

        // draw label
        GUI.Label(new Rect(x, y, labelWidth, labelHeight), message, textStyle);
    }

    public static void PrintFPS(float deltaTime) {
        // check style
        if (textStyle == null) {
            return;
        }

        // display fps
        int fps = Convert.ToInt32(1.0f / deltaTime);
        GUI.Label(new Rect(20, 20, 200, 200), $"<color=#FFFFFF>FPS: {fps}</color>", textStyle);
    }

    public static void Init() {
        // check init
        if (textStyle == null) {

            // init style
            textStyle = new GUIStyle();

            // load font
            Font font = GetFont();
            if (font == null) {
                Plugin.Log.LogError("Failed to load font.");
            } else {
                textStyle.font = font;
                Plugin.Log.LogInfo("Successfully loaded font.");
            }

            // set font size
            textStyle.fontSize = fontSize;
        }

    }

    private static Font GetFont() {

        // loop through all text components
        Text[] textComponents = GameObject.FindObjectsOfType<Text>();
        foreach (Text textComponent in textComponents) {
            Font font = textComponent.font;
            if (font != null && font.name == fontFamily) {
                return font;
            }
        }

        return null;
    }

}