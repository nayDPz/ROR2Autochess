using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using System.Runtime.InteropServices;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.UI;
using System.Collections.ObjectModel;

namespace RORAutochess
{
    public class AutochessRun : Run
    {
        public static GameObject gamemodePrefab;
        public static GameObject ui;
        public static void CreatePrefab()
        {
            GameObject clone = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClassicRun/ClassicRun.prefab").WaitForCompletion();
            Run cr = clone.GetComponent<Run>();

            Log.LogInfo("Prefab");
            gamemodePrefab = AutochessPlugin.assets.LoadAsset<GameObject>("CustomRun");

            SceneCollection sceneCollection = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/sgStage1.asset").WaitForCompletion();
            AutochessRun r = gamemodePrefab.AddComponent<AutochessRun>();
            r.startingSceneGroup = sceneCollection;
            r.nameToken = "GAMEMODE_AUTOCHESS_RUN_NAME";
            r.lobbyBackgroundPrefab = cr.lobbyBackgroundPrefab;
            r.gameOverPrefab = cr.gameOverPrefab;

            ui = AutochessPlugin.assetbundle.LoadAsset<GameObject>("HUDAutochess");
            ui.AddComponent<CursorOpener>();
            r.uiPrefab = null;

            gamemodePrefab.AddComponent<TeamManager>();
            gamemodePrefab.AddComponent<UI.AutochessRunCameraManager>();
            
            R2API.PrefabAPI.RegisterNetworkPrefab(gamemodePrefab);

            ContentPacks.gameModePrefabs.Add(gamemodePrefab);


            
        }



        [SystemInitializer(typeof(GameModeCatalog))]
        private static void E()
        {
            gamemodePrefab.SetActive(true);
        }

        public override bool spawnWithPod
        {
            get
            {
                return false;
            }
        }
    }
}
