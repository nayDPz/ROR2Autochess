using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using RoR2.Navigation;
using UnityEngine.SceneManagement;
using RORAutochess.AI;

namespace RORAutochess
{
    public class GenericBoard : MonoBehaviour
    {
        public const float baseTileWidth = 1.732f;
        public const float baseTileHeight = 2.0f;

        public static List<GenericBoard> instancesList;
        public static GameObject gridPrefab;
        public static GameObject benchPrefab;
        private static float scale;
        private static float gap;
        private static int boardWidth;
        private static int boardHeight;
        public static SceneDef sceneDef;

        public static bool inBoardScene; // SHOULD REPLACE WITH GAMEMODE CHECK

        public GameObject gridInstance;
        public GameObject benchInstance;
        public Tile[] tiles;
        public Tile[] benchTiles;
        public Vector3 benchPos = new Vector3(0.5f, 0, 3.75f);

        public CharacterMaster boardOwner;

        public List<CharacterBody> ownerUnitBodies = new List<CharacterBody>();
        public List<CharacterBody> enemyUnitBodies = new List<CharacterBody>();
        

        
        public static void Setup(int x, int y, float gap, float scale)
        {
            sceneDef = ScriptableObject.CreateInstance<SceneDef>();
            sceneDef.cachedName = "genericboardscene";
            sceneDef.nameToken = "GridStage";
            sceneDef.loreToken = "GridStage";
            sceneDef.stageOrder = 0;
            sceneDef.sceneType = SceneType.Stage;

            SurfaceDef d = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SurfaceDef>("RoR2/Base/Common/sdStone.asset").WaitForCompletion();
            AutochessPlugin.hud.LoadAsset<GameObject>("Hexagon1").GetComponent<SurfaceDefProvider>().surfaceDef = d;
            AutochessPlugin.hud.LoadAsset<GameObject>("Hexagon2").GetComponent<SurfaceDefProvider>().surfaceDef = d;
            AutochessPlugin.hud.LoadAsset<GameObject>("Hexagon3").GetComponent<SurfaceDefProvider>().surfaceDef = d; 

            GenericBoard.scale = scale;
            GenericBoard.gap = gap;
            boardWidth = x;
            boardHeight = y;

            gridPrefab = AutochessPlugin.hud.LoadAsset<GameObject>("Grid");
            benchPrefab = AutochessPlugin.hud.LoadAsset<GameObject>("Bench");

            instancesList = new List<GenericBoard>();

            ContentPacks.sceneDefs.Add(sceneDef);

            SceneManager.sceneLoaded += SetupBoard;
            RoR2.Stage.onStageStartGlobal += SetupPlayers;
        }

        private static void SetupPlayers(Stage stage)
        {
            if (stage.sceneDef == GenericBoard.sceneDef)
            {
                List<LocalUser> players = LocalUserManager.localUsersList;
                Vector3 startPos = Vector3.zero;
                foreach (LocalUser l in players)
                {
                    CharacterMaster player = l.cachedMaster;
                    GameObject obj = new GameObject("Board_" + l.userProfile.name); //???????
                    GenericBoard board = obj.AddComponent<GenericBoard>();
                    board.boardOwner = player;
                    player.gameObject.AddComponent<Traits.UnitOwnership>();

                    //GenericBoard board = new GenericBoard(startPos, GenericBoard.boardWidth, GenericBoard.boardHeight, l.cachedMaster);


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

                master.gameObject.AddComponent<AI.TileNavigator>();

                if (master.GetComponent<RoR2.CharacterAI.BaseAI>())
                    master.GetComponent<RoR2.CharacterAI.BaseAI>().fullVision = true;

                if (master.GetComponent<EntityStateMachine>())
                {
                    master.GetComponent<EntityStateMachine>().initialStateType = new EntityStates.SerializableEntityStateType(typeof(RORAutochess.AI.DoNothing));
                    master.GetComponent<EntityStateMachine>().mainStateType = new EntityStates.SerializableEntityStateType(typeof(RORAutochess.AI.DoNothing));
                }


                if (bodyObject && !bodyObject.GetComponent<Units.UnitPickupInteraction>())
                {
                    bodyObject.AddComponent<Units.UnitPickupInteraction>();
                    body.baseAcceleration = 999;
                    body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                }
            }
        }
        private static void SetupBoard(Scene scene, LoadSceneMode arg1) // this should probably not exist
        {
            if (scene.name == "genericboardscene")
            {
                inBoardScene = true;
                GameObject camera = GameObject.Find("CameraHolder"); // needs to be per board
                camera.transform.position *= GenericBoard.scale * 2;

                GameObject si = new GameObject("SceneInfo");
                SceneInfo info = si.AddComponent<SceneInfo>();

                ClassicStageInfo csInfo = si.AddComponent<ClassicStageInfo>();
                SceneManager.MoveGameObjectToScene(si, scene);

                GameObject ev = new GameObject("GameManager");
                GlobalEventManager manager = ev.AddComponent<GlobalEventManager>();
                SceneManager.MoveGameObjectToScene(ev, scene);

                //GameObject stage = GameObject.Find("Grid(Clone)");
                //stage.transform.localScale *= GenericBoard.scale * 2;
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
                inBoardScene = false;



        }

        private void Awake()
        {
            gridInstance = GameObject.Instantiate<GameObject>(GenericBoard.gridPrefab, base.transform);
            Board.GridGeneration g = gridInstance.transform.Find("Hexagons").gameObject.GetComponent<Board.GridGeneration>();
            g.gridWidth = GenericBoard.boardWidth;
            g.gridHeight = GenericBoard.boardHeight;
            g.gap = gap;
            g.scale = GenericBoard.scale;
            tiles = GenerateTiles(boardWidth, boardHeight, gap, scale, base.transform.position); // should probably combine tile and hexagon generation ?

            benchTiles = GenerateBenchTiles(scale, base.transform.position);
            benchInstance = GameObject.Instantiate<GameObject>(GenericBoard.benchPrefab, base.transform);
            benchInstance.transform.localPosition = this.benchPos * scale;
            benchInstance.transform.localScale *= GenericBoard.scale * 2; // WHY 2 WHY 2 WHYYYYYYYYYY
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].LinkTile();
            }
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].CalculateTileDistances();
            }
            GenericBoard.instancesList.Add(this);
        }

        private void OnDestroy()
        {
            GenericBoard.instancesList.Remove(this);
        }



        private Tile[] GenerateBenchTiles(float scale, Vector3 startPos)
        {
            Tile[] tiles = new Tile[9];


            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new Tile
                {
                    worldPosition = new Vector3(i * 1.05f * scale * 3f , 0, 0) + (benchPos * scale) + startPos, // OH MY GOD SCALE IS SO FUCKED AHHHHHHHH WHY IS IT 3
                    index = i,
                    board = this,
                };
            }

            return tiles;
        }


        

        private Tile[] GenerateTiles(int x, int y, float gap, float scale, Vector3 startPos) 
        {


            float width = baseTileWidth + baseTileWidth * gap;
            float height = baseTileHeight + baseTileHeight * gap;

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

        public static GenericBoard GetBoardFromMaster(CharacterMaster m)
        {
            foreach (GenericBoard board in instancesList)
            {
                if (board.boardOwner == m)
                    return board;
            }
            return null;
        }
        public Tile GetLowestUnoccupiedTile(Tile[] tiles)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                if (!tiles[i].occupied)
                    return tiles[i];
            }
            return null;
        }
        public Tile GetClosestTile(Vector3 worldPosition)
        {
            return GetClosestTile(worldPosition, false);
        }        
        public Tile GetClosestTile(Vector3 worldPosition, bool includeBench)
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
            if(includeBench)
            {
                for(int i = 0; i < this.benchTiles.Length; i++)
                {
                    if(this.benchInstance)
                    {
                        float distance2 = ( this.benchInstance.transform.Find("Slot" + (i + 1)).position - worldPosition).magnitude; // should do properly (later)
                        if (distance2 < lowestDistance)
                        {
                            tile = this.benchTiles[i];
                            lowestDistance = distance2;
                        }
                    }
                    
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
            public Dictionary<Tile, int> tileDistances;
            public GenericBoard board;
            public int index;
            public Vector3 worldPosition;
            public bool occupied;
            public TileNavigator occupant; // who even uses the word occupant lol
            public int[] connectedTileIndices;
            public Tile[] connectedTiles;

            public void LinkTile()
            {
                tileDistances = new Dictionary<Tile, int>();

                connectedTiles = new Tile[connectedTileIndices.Length];
                for(int i = 0; i < connectedTileIndices.Length; i++)
                {
                    connectedTiles[i] = board.GetTileFromIndex(connectedTileIndices[i]);
                }
                
            }
            public void CalculateTileDistances()
            {
                foreach (Tile tile in board.tiles)
                {
                    if (tile == this)
                        tileDistances.Add(tile, 0);
                    else
                    {
                        int d = 1;
                        Tile step = this;
                        while (tile != step.GetClosestConnectedTile(tile))
                        {
                            step = step.GetClosestConnectedTile(tile);
                            d++;
                        }

                        tileDistances.Add(tile, d);
                    }
                    
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
