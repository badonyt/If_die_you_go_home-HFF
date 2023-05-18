using BepInEx;
using System;
using System.Collections;
using HarmonyLib;
using HumanAPI;
using I2.Loc;
using UnityEngine;
using UnityEngine.Rendering;
using BepInEx.Logging;
using Multiplayer;

namespace MyFirstPlugin
{
    [BepInPlugin("org.bepinex.plugins.If_die_you_go_home", "If_die_you_go_home", "1.0.0.0")]
    [BepInProcess("Human.exe")]
    public class Plugin : BaseUnityPlugin
    {
       
        private static ManualLogSource logger;
        public enum GUIType
        {
            // Token: 0x04000011 RID: 17
            None,
            // Token: 0x04000012 RID: 18
            Timer,
            
        }
        public Rect windowRect;
        public bool guiOpened;
        public static GUIType gui;
        public static Color32 color;
        public static string curLevel;
        public static WorkshopItemSource levelType;
        public static bool WaterBug = false;
        private void Awake()
        {
            // Plugin startup logic
            logger = base.Logger;
            logger.LogInfo($"Plugin is loaded!");
            Harmony harmony = new Harmony("org.bepinex.plugins.If_die_you_go_home");
            harmony.PatchAll();
            this.windowRect = new Rect(10f, 300f, 200f, 160f);
            levelType = WorkshopItemSource.BuiltIn;
        }

       

        // Method to retrieve the value of passedLevel
        private bool GetPassedLevel()
        {
            return Game.instance.passedLevel;
        }
        public void Update()
        {
            bool flag = Game.instance == null;
            if (!flag)
            {
                bool keyDown = Input.GetKeyDown(KeyCode.L);
                if (keyDown)
                {
                    this.guiOpened = !this.guiOpened;
                }
            }
        }
        public void OnGUI()
        {
            bool flag = this.guiOpened;
            if (flag)
            {
                this.windowRect = GUI.Window(99, this.windowRect, new GUI.WindowFunction(this.WindowFunction), "If_die_you_go_home");
            }
        }
        public void WindowFunction(int windowID)
        {
            this.SingleSelector(30f, GUIType.Timer, "Menu when dying");
            GUI.Label(new Rect(10f, 60f, 100f, 30f), "Retry Level: "); 
            curLevel = GUI.TextField(new Rect(110f, 60f, 50f, 20f), curLevel);
            bool flag = GUI.Button(new Rect(10f, 90f, 150f, 20f), "Type: " + ((levelType == WorkshopItemSource.BuiltIn) ? "Main Dream" : "Extra Dream"));
            GUI.Label(new Rect(10f, 60f, 100f, 30f), "Dont use this for checkpoint%!!");
            if (flag)
            {
                levelType = ((levelType == WorkshopItemSource.BuiltIn) ? WorkshopItemSource.EditorPick : WorkshopItemSource.BuiltIn);
            }
            

            GUI.DragWindow(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height));
        }
        public void SingleSelector(float y, GUIType type, string text)
        {
            bool flag = GUI.Toggle(new Rect(10f, y, 150f, 30f), Plugin.gui == type, text) != (Plugin.gui == type);
            if (flag)
            {
                bool flag2 = Plugin.gui == type;
                if (flag2)
                {
                    Plugin.gui = GUIType.None;
                }
                else
                {
                    Plugin.gui = type;
                }
            }
        }

        // Method to handle button click
        public bool OnButtonClick()
        {
            bool passedLevel = GetPassedLevel();
            return passedLevel;
            // Do something with the passedLevel value here
        }

        // Harmony patch for the Game.Fall() method
        [HarmonyPatch(typeof(Game), "Fall")]
        public static class FallPatch
        {
            public static void Prefix(Game __instance)
            {
                bool flag = Plugin.gui == GUIType.Timer;
                if(__instance.passedLevel == true) {
                    logger.LogInfo(Game.instance.currentLevelType);
                    if(Game.instance.currentLevelType == WorkshopItemSource.BuiltIn)
                    {
                        logger.LogInfo("shut up");
                        if (Game.instance.currentLevelNumber == 6)
                        {
                            logger.LogInfo("WATER");
                            WaterBug = true;

                        }
                    }
                }
                logger.LogInfo("Game.Fall() is being called!");
                if (__instance.passedLevel == false && flag == true && WaterBug == false)
                {
                    logger.LogInfo("He died");
                    GoHome(__instance);
                }else if(WaterBug == true)
                {
                    WaterBug = false;
                }
            }

            private static void GoHome(Game game)
            {
                ulong level;
                bool flag3 = ulong.TryParse(curLevel, out level);
                if (flag3)
                {
                    game.StartCoroutine(Restart(level));
                    return;
                }
                game.state = GameState.Paused;
                game.StartCoroutine(WaitAndPause(game));
                
            }
            public static IEnumerator Restart(ulong level)
            {
                Game.instance.state = GameState.Paused;
                yield return new WaitForFixedUpdate();
                App.instance.PauseLeave(false);
                yield return new WaitForFixedUpdate();
                //WorkshopItemSource.EditorPick 
                App.instance.LaunchSinglePlayer(level, Plugin.levelType, 0, 0);
                yield break;
            }   
            private static IEnumerator WaitAndPause(Game game)
            {
                yield return new WaitForFixedUpdate();
                App.instance.PauseLeave(false);
            }
        }
    }
}
