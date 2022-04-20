using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using System.Runtime.InteropServices;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.UI;

namespace RORAutochess
{
    public class AutochessRun : Run
    {
        public static GameObject gamemodePrefab;
        public static void CreatePrefab()
        {

            // gamemodePrefab = R2API.PrefabAPI.InstantiateClone(UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClassicRun/ClassicRun.prefab").WaitForCompletion(), "AutochessRun");


            GameObject clone = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClassicRun/ClassicRun.prefab").WaitForCompletion();
            Run cr = clone.GetComponent<Run>();

            Log.LogInfo("Prefab");
            gamemodePrefab = AutochessPlugin.assets.LoadAsset<GameObject>("CustomRun");

            SceneCollection sceneCollection = ScriptableObject.CreateInstance<SceneCollection>();
            sceneCollection.name = "sgAutochess";
            sceneCollection._sceneEntries = (new SceneCollection.SceneEntry[] { new SceneCollection.SceneEntry { sceneDef = GenericBoard.sceneDef } });
            AutochessRun r = gamemodePrefab.AddComponent<AutochessRun>();
            r.startingSceneGroup = sceneCollection;
            r.nameToken = "GAMEMODE_CHESS_RUN_NAME";
            r.lobbyBackgroundPrefab = cr.lobbyBackgroundPrefab;
            r.gameOverPrefab = cr.gameOverPrefab;
            GameObject ui = R2API.PrefabAPI.InstantiateClone(cr.uiPrefab, "AutochessUI");
            ui.AddComponent<CursorOpener>();

            GameObject.Destroy(ui.GetComponent<CrosshairManager>());

            r.uiPrefab = ui;

            gamemodePrefab.AddComponent<TeamManager>();
            gamemodePrefab.AddComponent<RunCameraManager>();
            
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
