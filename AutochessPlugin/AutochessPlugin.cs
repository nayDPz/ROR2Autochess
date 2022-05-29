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
        internal static AssetBundle scene;
        internal static AssetBundle assets;
        internal static AssetBundle hud;

        public void Awake()
        {
            Log.Init(Logger);
            PInfo = Info;

            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };


            if (scene == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RORAutochess.genericboardscene"))
                {
                    scene = AssetBundle.LoadFromStream(assetStream);
                }           
            }
            if (assets == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RORAutochess.chessassets"))
                {
                    assets = AssetBundle.LoadFromStream(assetStream);
                }
            }
            if (hud == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RORAutochess.hudautochess"))
                {
                    hud = AssetBundle.LoadFromStream(assetStream);
                }
            }

            ChessBoard.Setup(15, 12, 0.1f, 4f);
            //Player.PlayerBodyObject.CreatePrefab();
            AutochessRun.CreatePrefab();
            Board.RoundController.Init();
            UI.InventoryClickDrag.Init();
            Stuff.LoadStuff();


            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
            On.RoR2.CameraRigController.Start += CameraRigController_Start;

            new ContentPacks().Initialize();

            Log.LogInfo(nameof(Awake) + " done.");
        }

        private void CameraRigController_Start(On.RoR2.CameraRigController.orig_Start orig, CameraRigController self) 
        {
            if(ChessBoard.inBoardScene) // gamemode check ?
            {
                if (self.createHud)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(AutochessRun.ui); // should learn to do IL eventually
                    gameObject.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster").Find("ChatBoxRoot").GetComponent<InstantiatePrefabBehavior>().prefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/ChatBox, In Run.prefab").WaitForCompletion();
                    gameObject.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("AutochessRunInfoHudPanel").Find("TimerPanel").Find("TimerText").GetComponent<TimerText>().format = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<TimerStringFormatter>("RoR2/Base/Common/tsfRunStopwatch.asset").WaitForCompletion(); // O_O
                    self.hud = gameObject.GetComponent<HUD>();
                    self.hud.cameraRigController = self;
                    self.hud.GetComponent<Canvas>().worldCamera = self.uiCam;
                    self.hud.GetComponent<CrosshairManager>().cameraRigController = self;
                    self.hud.localUserViewer = self.localUserViewer;
                }
                if (self.uiCam)
                {
                    self.uiCam.transform.parent = null;
                    self.uiCam.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                }
                if (!DamageNumberManager.instance)
                {
                    UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/DamageNumberManager"));
                }
                self.currentCameraState = new CameraState
                {
                    position = base.transform.position,
                    rotation = base.transform.rotation,
                    fov = self.baseFov
                };
                self.cameraMode = RoR2.CameraModes.CameraModePlayerBasic.playerBasic;
            }
            else
            {
                orig(self);
            }
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

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.F2))
            {
                Vector3 location = Vector3.zero;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
                Physics.Raycast(ray, out RaycastHit hit, 1000f);
                location = hit.point;

                var masterprefab = MasterCatalog.FindMasterPrefab("LemurianMaster");
                var body = masterprefab.GetComponent<CharacterMaster>().bodyPrefab;
                var bodyGameObject = UnityEngine.Object.Instantiate<GameObject>(masterprefab, location, Quaternion.identity);
                CharacterMaster master = bodyGameObject.GetComponent<CharacterMaster>();
                NetworkServer.Spawn(bodyGameObject);
                master.bodyPrefab = body;
                master.teamIndex = TeamIndex.Monster;
                master.SpawnBody(location, Quaternion.identity);
                
                LocalUser firstLocalUser = LocalUserManager.GetFirstLocalUser();
                CharacterMaster player = firstLocalUser.cachedMaster;
                AI.TileNavigator t = master.GetComponent<AI.TileNavigator>();
                t.currentBoard = ChessBoard.GetBoardFromMaster(player);
                t.SetCurrentTile(t.currentBoard.GetClosestTile(location));

            }
                

        }

    }
}
