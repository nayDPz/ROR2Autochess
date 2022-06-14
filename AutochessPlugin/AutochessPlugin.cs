using System.Reflection;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using System.Collections;
using RoR2.UI;
using UnityEngine.UI;
using RoR2.Projectile;
using System.Linq;
using BepInEx;
using R2API.Utils;
using BepInEx.Logging;
using System;

namespace RORAutochess
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(PrefabAPI))]
    

    public class AutochessPlugin : BaseUnityPlugin
	{
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "nayDPz";
        public const string PluginName = "Autochess";
        public const string PluginVersion = "1.0.0";

        
        public static string AssetBundlePath
        {
            get
            {
                //This returns the path to your assetbundle assuming said bundle is on the same folder as your DLL. If you have your bundle in a folder, you can uncomment the statement below this one.
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(AutochessPlugin.PInfo.Location), "genericboardscene");
                //return Path.Combine(Path.GetDirectoryName(MainClass.PInfo.Location), assetBundleFolder, myBundle);
            }
        }



        public static PluginInfo PInfo { get; private set; }
        internal static AssetBundle assets;
        internal static AssetBundle assetbundle;

        public void Awake()
        {
            Log.Init(Logger);
            PInfo = Info;

            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };

            if (assets == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RORAutochess.chessassets"))
                {
                    assets = AssetBundle.LoadFromStream(assetStream);
                }
            }
            if (assetbundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RORAutochess.hudautochess"))
                {
                    assetbundle = AssetBundle.LoadFromStream(assetStream);
                }
            }

            ChessBoard.Setup(15, 12, 0.1f, 4f);
            //Player.PlayerBodyObject.CreatePrefab();
            AutochessRun.CreatePrefab();
            Board.RoundController.Init();
            UI.InventoryClickDrag.Init();
            Stuff.LoadStuff();


            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;

            new ContentPacks().Initialize();

            Log.LogInfo(nameof(Awake) + " done.");
        }


        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            //twiner
            try
            {
                Logger.LogDebug("Adding GameModes to ExtraGameModeMenu menu");

                var mainMenu = GameObject.Find("MainMenu")?.transform;
                var weeklyButton = mainMenu.Find("MENU: Extra Game Mode/ExtraGameModeMenu/Main Panel/GenericMenuButtonPanel/JuicePanel/GenericMenuButton (Weekly)");
                Logger.LogDebug($"Found: {weeklyButton.name}");

                var juicedPanel = weeklyButton.transform.parent;
                string[] skip = new[] { "Classic", "ClassicRun" };
                var gameMode = AutochessRun.gamemodePrefab;
                
                var copied = Transform.Instantiate(weeklyButton);
                copied.name = $"GenericMenuButton ({gameMode})";
                GameObject.DestroyImmediate(copied.GetComponent<DisableIfGameModded>());

                var tmc = copied.GetComponent<LanguageTextMeshController>();
                tmc.token = gameMode.GetComponent<Run>().nameToken;

                var consoleFunctions = copied.GetComponent<ConsoleFunctions>();

                var hgbutton = copied.gameObject.GetComponent<HGButton>();
                hgbutton.onClick = new Button.ButtonClickedEvent();

                hgbutton.onClick.AddListener(() => consoleFunctions.SubmitCmd($"transition_command \"gamemode {gameMode}; host 0;\""));

                copied.SetParent(juicedPanel);
                copied.localScale = Vector3.one;
                copied.gameObject.SetActive(true);
                
            }
            catch (Exception e)
            {
                Logger.LogError("Error Adding GameModes to ExtraGameModeMenu menu");
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
            }
            finally
            {
                Logger.LogInfo("Finished Main Menu Modifications");
                orig(self);
            }
        }

    }
}
