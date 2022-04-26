using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using RoR2.Navigation;
using UnityEngine.SceneManagement;

namespace RORAutochess
{
    public class GenericBoard : MonoBehaviour
    {
        public const float baseWidth = 1.732f;
        public const float baseHeight = 2.0f;

        public static List<GenericBoard> instancesList;
        public static GameObject gridPrefab;
        private static float scale;
        private static float gap;
        private static int boardWidth;
        private static int boardHeight;

        public static SceneDef sceneDef;
        public GameObject gridInstance;
        public Tile[] tiles;


        public static bool onBoard;
        public static void Setup(int x, int y, float gap, float scale)
        {
            sceneDef = ScriptableObject.CreateInstance<SceneDef>();
            sceneDef.cachedName = "genericboardscene";
            sceneDef.nameToken = "GridStage";
            sceneDef.loreToken = "GridStage";
            sceneDef.stageOrder = 0;
            sceneDef.sceneType = SceneType.Stage;

            GenericBoard.scale = scale;
            GenericBoard.gap = gap;
            boardWidth = x;
            boardHeight = y;

            gridPrefab = AutochessPlugin.hud.LoadAsset<GameObject>("Grid");

            instancesList = new List<GenericBoard>();

            ContentPacks.sceneDefs.Add(sceneDef);

            SceneManager.sceneLoaded += SetupBoard;
            RoR2.Stage.onStageStartGlobal += SetupPlayers;
        }

        public GenericBoard(Vector3 startPos, int width, int height)
        {
            gridInstance = GameObject.Instantiate<GameObject>(GenericBoard.gridPrefab, startPos, Quaternion.identity);
            Board.GridGeneration g = gridInstance.transform.Find("Hexagons").gameObject.GetComponent<Board.GridGeneration>();
            g.gridWidth = width;
            g.gridHeight = height;
            g.gap = gap;
            g.scale = GenericBoard.scale;
            tiles = GenerateTiles(boardWidth, boardHeight, gap, scale, startPos); // should probably combine tile generation and grid generation
            

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].LinkTile();
            }

            GenericBoard.instancesList.Add(this);
        }

        private void OnDestroy()
        {
            GenericBoard.instancesList.Remove(this);
        }

        private static void SetupPlayers(Stage stage)
        {
            if(stage.sceneDef == GenericBoard.sceneDef)
            {
                List<LocalUser> players = LocalUserManager.localUsersList;
                Vector3 startPos = Vector3.zero;
                foreach(LocalUser l in players)
                {
                    GenericBoard board = new GenericBoard(startPos, GenericBoard.boardWidth, GenericBoard.boardHeight);
                    CharacterMaster player = l.cachedMaster;
                    player.gameObject.AddComponent<Traits.UnitOwnership>();
                    //player.bodyPrefab = Player.PlayerBodyObject.bodyPrefab;
                    startPos.x += 300f;
                }
                
            }
        }

        [SystemInitializer(typeof(GameModeCatalog))]
        private static void SetupTestUnits()
        {
            foreach (CharacterMaster master in MasterCatalog.allMasters)
            {

                Units.UnitData unitData = master.gameObject.AddComponent<Units.UnitData>();
                GameObject bodyObject = master.bodyPrefab;
                CharacterBody body = bodyObject.GetComponent<CharacterBody>();

                unitData.master = master;
                unitData.bodyObject = master.bodyPrefab;
                unitData.unitName = body.baseNameToken;
                Debug.Log(unitData.bodyObject.name);
                

                if(master.GetComponent<RoR2.CharacterAI.BaseAI>())
                    master.GetComponent<RoR2.CharacterAI.BaseAI>().fullVision = true;

                if (master.GetComponent<EntityStateMachine>())
                {
                    master.GetComponent<EntityStateMachine>().initialStateType = new EntityStates.SerializableEntityStateType(typeof(RORAutochess.AI.BaseTileAIState));
                    master.GetComponent<EntityStateMachine>().mainStateType = new EntityStates.SerializableEntityStateType(typeof(RORAutochess.AI.BaseTileAIState));
                }


                if (bodyObject && !bodyObject.GetComponent<Units.UnitPickupInteraction>())
                {
                    bodyObject.AddComponent<Units.UnitPickupInteraction>();
                    body.baseAcceleration = 999;
                    body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                }
            }
        }
        private static void SetupBoard(Scene scene, LoadSceneMode arg1)
        {
            if (scene.name == "genericboardscene")
            {
                onBoard = true;
                GameObject camera = GameObject.Find("CameraHolder");
                camera.transform.position *= GenericBoard.scale * 2;

                GameObject si = new GameObject("SceneInfo");
                SceneInfo info = si.AddComponent<SceneInfo>();

                ClassicStageInfo csInfo = si.AddComponent<ClassicStageInfo>();
                SceneManager.MoveGameObjectToScene(si, scene);

                GameObject ev = new GameObject("GameManager");
                GlobalEventManager manager = ev.AddComponent<GlobalEventManager>();
                SceneManager.MoveGameObjectToScene(ev, scene);

                //GameObject stage = GameObject.Find("Grid(Clone)");
                //stage.transform.localScale *= GenericBoard.scale * 2; // i fucked something up with scale idk
                //GameObject floor = stage.transform.Find("Floor").gameObject;
                //floor.layer = LayerIndex.world.intVal;
                //floor.AddComponent<SurfaceDefProvider>().surfaceDef = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SurfaceDef>("RoR2/Base/Common/sdStone.asset").WaitForCompletion();





                
                //GameObject lemtest = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/LemurianMaster.prefab").WaitForCompletion();
                //lemtest.GetComponent<EntityStateMachine>().initialStateType = new EntityStates.SerializableEntityStateType(typeof(RORAutochess.AI.BaseTileAIState));
                //lemtest.GetComponent<EntityStateMachine>().mainStateType = new EntityStates.SerializableEntityStateType(typeof(RORAutochess.AI.BaseTileAIState));
                //GameObject body = lemtest.GetComponent<CharacterMaster>().bodyPrefab;
                //if(!body.GetComponent<Units.UnitPickupInteraction>())
                //{
                //    body.AddComponent<Units.UnitPickupInteraction>();                 
                //    body.GetComponent<CharacterBody>().baseAcceleration = 999;
                //}

            }
            else
                onBoard = false;
            


        }

        private Tile[] GenerateTiles(int x, int y, float gap, float scale, Vector3 startPos)
        {


            float width = baseWidth + baseWidth * gap;
            float height = baseHeight + baseHeight * gap;

            Tile[] tiles = new Tile[x * y];

            for(int i = 0; i < y; i++)
            {
                for(int k = 0; k < x; k++)
                {
                    int index = i * x + k;
                    float offset = i % 2 != 0 ? width / 2f : 0f;
                    tiles[index] = new Tile
                    {
                        worldPosition = new Vector3((width * k + offset) * scale, 0.1f, (-height * i * 0.75f) * scale) + startPos,
                        index = index,
                        board = this,
                    };

                    List<int> links = new List<int>();

                    int debugLinksGenerated = 0;

                    //Log.LogInfo("LINKING " + index);
                    if (k > 0) 
                    {
                        links.Add(index - 1);
                        debugLinksGenerated++;

                    } // behind
                    if (k < x - 1) 
                    {
                        links.Add(index + 1);
                        debugLinksGenerated++;
                    } // in front
                    if (i > 0) 
                    {
                        if (i % 2 == 0)
                        {
                            if(k == 0)
                            {
                                links.Add(index - x);
                                debugLinksGenerated++;
                            }
                            else
                            {
                                links.Add(index - x - 1);
                                links.Add(index - x);
                                debugLinksGenerated += 2;
                            }
                        }
                        else
                        {
                            if (k == x - 1)
                            {
                                links.Add(index - x);
                                debugLinksGenerated++;
                            }
                            else
                            {
                                links.Add(index - x);
                                links.Add(index - x + 1);
                                debugLinksGenerated += 2;
                            }                           
                        }

                    } // below
                    if (i < y - 1) 
                    {
                        if (i % 2 == 0)
                        {
                            if (k == 0)
                            {
                                links.Add(index + x);
                                debugLinksGenerated++;
                            }     
                            else
                            {
                                links.Add(index + x - 1);
                                links.Add(index + x);
                                debugLinksGenerated += 2;
                            }
                        }
                        else
                        {
                            if(k == x - 1)
                            {
                                links.Add(index + x);
                                debugLinksGenerated++;
                            }
                            else
                            {
                                links.Add(index + x);
                                links.Add(index + x + 1);
                                debugLinksGenerated += 2;
                            }
                            
                        }
                    } // above

                    tiles[index].connectedTileIndices = links.ToArray();

                    //Log.LogDebug("Hex " + k + "|" + i + " generated at position + " + tiles[index].worldPosition.ToString() + " with " + debugLinksGenerated + " links.");
                }
            }

            return tiles;
        }

        public Tile GetClosestTile(Vector3 worldPosition)
        {
            Tile tile = tiles[0];
            float lowestDistance = Mathf.Infinity;
            for (int i = 0; i < tiles.Length; i++)
            {
                float distance = (tiles[i].worldPosition - worldPosition).magnitude;
                if (distance < lowestDistance && !tiles[i].occupied)
                {
                    tile = tiles[i];
                    lowestDistance = distance;
                }
            }
            return tile;
        }

        public Tile GetTileFromIndex(int index)
        {
            return tiles[index];
        }
        public class Tile
        {
            public GenericBoard board;
            public bool enabled = false;
            public int index;
            public Vector3 worldPosition;
            public bool occupied;
            public int[] connectedTileIndices;
            public Tile[] connectedTiles;

            public void LinkTile()
            {
                connectedTiles = new Tile[connectedTileIndices.Length];
                for(int i = 0; i < connectedTileIndices.Length; i++)
                {
                    connectedTiles[i] = board.GetTileFromIndex(connectedTileIndices[i]);
                    //Log.LogDebug("Tile " + index + " connected to Tile " + connectedTiles[i].index);
                }
            }

            

            public Tile GetClosestConnectedTile(int targetTileIndex)
            {
                return GetClosestConnectedTile(board.GetTileFromIndex(targetTileIndex));
            }
            public Tile GetClosestConnectedTile(Tile targetTile)
            {
                return (GetClosestConnectedTile(targetTile.worldPosition));                
            }
            public Tile GetClosestConnectedTile(Vector3 worldPosition)
            {
                float lowestDistance = Mathf.Infinity;
                Tile tile = connectedTiles[0];
                for (int i = 0; i < connectedTiles.Length; i++)
                {
                    float distance = (connectedTiles[i].worldPosition - worldPosition).magnitude;
                    if (distance < lowestDistance && !connectedTiles[i].occupied)
                    {
                        tile = connectedTiles[i];
                        lowestDistance = distance;
                    }
                }
                if (lowestDistance == Mathf.Infinity)
                    tile = this;
                return tile;
            }


           
        }
    }
}
