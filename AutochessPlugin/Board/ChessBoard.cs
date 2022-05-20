using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using RoR2.Navigation;
using UnityEngine.SceneManagement;
using RORAutochess.AI;
using RORAutochess.Units;
using UnityEngine.Networking;
using System.Collections.ObjectModel;

namespace RORAutochess
{
    public class ChessBoard : MonoBehaviour
    {
        public const float baseTileWidth = 1.732f;
        public const float baseTileHeight = 2.0f;

        public static List<ChessBoard> instancesList;
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

        public static NodeGraph nodeGraph;

        public Tile[] tiles;
        public Tile[] benchTiles;
        public Vector3 benchPos = new Vector3(0.5f, 0, 3.75f);

        public CharacterMaster owner;
        public List<UnitData> ownerUnitsOnBoard = new List<UnitData>();
        public List<UnitData> enemyUnitsOnBoard = new List<UnitData>();
        private CharacterMaster enemy;
        
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

            

            ChessBoard.scale = scale;
            ChessBoard.gap = gap;
            boardWidth = x;
            boardHeight = y;

            gridPrefab = AutochessPlugin.hud.LoadAsset<GameObject>("Grid");
            benchPrefab = AutochessPlugin.hud.LoadAsset<GameObject>("Bench");

            instancesList = new List<ChessBoard>();

            ContentPacks.sceneDefs.Add(sceneDef);

            SceneManager.sceneLoaded += SetupBoard;
            RoR2.Stage.onStageStartGlobal += SetupPlayers;
        }
        private static void SetupPlayers(Stage stage)
        {
            if (stage.sceneDef == ChessBoard.sceneDef)
            {
                List<LocalUser> players = LocalUserManager.localUsersList;
                Vector3 startPos = Vector3.zero;
                foreach (LocalUser l in players)
                {
                    CharacterMaster player = l.cachedMaster;
                    GameObject obj = new GameObject("Board_" + l.userProfile.name); //???????
                    ChessBoard board = obj.AddComponent<ChessBoard>();
                    board.owner = player;
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
                if (bodyObject && !bodyObject.GetComponent<Units.UnitPickupInteraction>())
                {
                    bodyObject.AddComponent<Units.UnitPickupInteraction>();
                    body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

                    if (body.characterMotor) body.characterMotor.mass = 10000f;
                    if (body.rigidbody) body.rigidbody.mass = 10000f;
                }

                EntityStateMachine m = master.GetComponent<EntityStateMachine>();
                if (m)
                {
                    m.initialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AI.Walker.Guard));
                    m.mainStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AI.Walker.Guard));
                }

                RoR2.CharacterAI.BaseAI ai = master.GetComponent<RoR2.CharacterAI.BaseAI>();
                if (ai)
                {
                    ai.fullVision = true;
                    ai.scanState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AI.Walker.Guard));
                }

            }
        }
        private static void SetupBoard(Scene scene, LoadSceneMode arg1) // this should probably not exist
        {
            if (scene.name == "genericboardscene")
            {
                inBoardScene = true;
                GameObject camera = GameObject.Find("CameraHolder"); // needs to be per board
                camera.transform.position *= ChessBoard.scale * 2;

                GameObject si = new GameObject("SceneInfo");
                SceneInfo info = si.AddComponent<SceneInfo>();

                nodeGraph = GenerateNodegraph(boardWidth, boardHeight, gap, scale);
                info.groundNodesAsset = nodeGraph;
                info.groundNodes = nodeGraph;
                info.airNodesAsset = nodeGraph;
                info.airNodes = nodeGraph;

                ClassicStageInfo csInfo = si.AddComponent<ClassicStageInfo>();
                SceneManager.MoveGameObjectToScene(si, scene);

                GameObject ev = new GameObject("GameManager");
                GlobalEventManager manager = ev.AddComponent<GlobalEventManager>();
                SceneManager.MoveGameObjectToScene(ev, scene);

                GameObject.Instantiate<GameObject>(Board.RoundController.prefab);


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

        public static ChessBoard GetBoardFromMaster(CharacterMaster m)
        {
            foreach (ChessBoard board in instancesList)
            {
                if (board.owner == m)
                    return board;
            }
            return null;
        }

        private void Awake()
        {
            gridInstance = GameObject.Instantiate<GameObject>(ChessBoard.gridPrefab, base.transform);
            Board.GridGeneration g = gridInstance.transform.Find("Hexagons").gameObject.GetComponent<Board.GridGeneration>();
            g.gridWidth = ChessBoard.boardWidth;
            g.gridHeight = ChessBoard.boardHeight;
            g.gap = gap;
            g.scale = ChessBoard.scale;
            tiles = GenerateTiles(boardWidth, boardHeight, gap, scale, base.transform.position); // should probably combine tile and hexagon generation ??

            benchTiles = GenerateBenchTiles(scale, base.transform.position);
            benchInstance = GameObject.Instantiate<GameObject>(ChessBoard.benchPrefab, base.transform);
            benchInstance.transform.localPosition = this.benchPos * scale;
            benchInstance.transform.localScale *= ChessBoard.scale * 2; // WHY 2 WHY 2 WHYYYYYYYYYY

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].LinkTile();
            }
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].CalculateTileDistances();
            }
            ChessBoard.instancesList.Add(this);
        }

        private void Start()
        {
            ReadOnlyCollection<NetworkUser> readOnlyInstancesList = NetworkUser.readOnlyInstancesList;
            BodyIndex bodyIndex = readOnlyInstancesList[0].bodyIndexPreference;

            MasterCatalog.MasterIndex index = MasterCatalog.FindAiMasterIndexForBody(bodyIndex);
            GameObject master;
            if (index != MasterCatalog.MasterIndex.none)
            {
                master = MasterCatalog.GetMasterPrefab(index);
            }
            else
            {
                Log.LogError("Survivor preference not found!");
                master = MasterCatalog.FindMasterPrefab("CommandoBody");
            }



            CharacterMaster player = readOnlyInstancesList[0].master;

            GameObject body = player.GetBodyObject();
            ModelLocator model = body.GetComponent<ModelLocator>();
            if(model)
            {
                model.modelTransform.gameObject.SetActive(false);
            }
            player.gameObject.AddComponent<UI.MouseInteractionDriver2>();
            //body.AddComponent<UI.MouseInteractionDriver>();
            body.SetActive(false);

            ChessBoard board = this;
            if (board != null)
            {
                ChessBoard.Tile tile = board.GetLowestUnoccupiedTile(board.tiles);
                if (tile != null)
                {
                    CharacterMaster m = new MasterSummon
                    {
                        masterPrefab = master,
                        summonerBodyObject = player.GetBodyObject(),
                        ignoreTeamMemberLimit = true,
                        inventoryToCopy = null,
                        useAmbientLevel = new bool?(true),
                        position = tile.worldPosition + Vector3.up,
                        rotation = Quaternion.identity,

                    }.Perform();

                    m.destroyOnBodyDeath = false;

                    m.GetComponent<RoR2.CharacterAI.BaseAI>().enabled = false;
                    AI.TileNavigator t = m.GetComponent<AI.TileNavigator>();

                    if (!t) // probably shouldnt do this
                    {
                        t = m.gameObject.AddComponent<AI.TileNavigator>();
                    }

                    t.currentBoard = board;
                    t.SetCurrentTile(tile);
                }
            }
        }
        public void SetUnitPositions()
        {
            this.ownerUnitsOnBoard = new List<UnitData>();

            MinionOwnership.MinionGroup unitGroup = MinionOwnership.MinionGroup.FindGroup(this.owner.netId);
            foreach(MinionOwnership o in unitGroup.members)
            {
                if(o)
                {
                    UnitData data = o.gameObject.GetComponent<UnitData>();
                    data.navigator.inCombat = false;
                    if (!data.navigator.benched)
                    {
                        data.tileIndex = data.navigator.currentTile.index;
                        this.ownerUnitsOnBoard.Add(data);
                    }
                }
                              
            }
        }

        public static int testPveRoundEnemies = 9;
        public List<UnitData> CreatePVERound() // giga testing. could do director stuff here?
        {
            var enemyUnits = new List<UnitData>(); 
            var masters = new GameObject[testPveRoundEnemies];

            var choices = new GameObject[]{ MasterCatalog.FindMasterPrefab("LemurianMaster"), MasterCatalog.FindMasterPrefab("GolemMaster"), MasterCatalog.FindMasterPrefab("BeetleMaster") };

            for (int i = 0; i < masters.Length; i++)
            {
                int z = UnityEngine.Random.RandomRangeInt(0, choices.Length);
                var body = choices[z].GetComponent<CharacterMaster>().bodyPrefab;

                masters[i] = UnityEngine.Object.Instantiate<GameObject>(choices[z]);
                CharacterMaster master = masters[i].GetComponent<CharacterMaster>();                
                NetworkServer.Spawn(masters[i]);
                master.bodyPrefab = body;
                master.teamIndex = TeamIndex.Monster;

                enemyUnits.Add(masters[i].GetComponent<UnitData>());
            }

            foreach(UnitData data in enemyUnits)
            {
                data.tileIndex = UnityEngine.Random.RandomRangeInt(0, (this.tiles.Length / 2)-1);
            }
            return enemyUnits;


        }
        public void ResetBoard()
        {

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].occupied = false;
            }

            foreach (UnitData unit in this.enemyUnitsOnBoard)
            {
                if(unit && unit.master)
                    unit.master.TrueKill(); // FOR TESTING
            }
            this.enemyUnitsOnBoard.Clear();
            foreach(UnitData unit in this.ownerUnitsOnBoard)
            {
                unit.navigator.inCombat = false;
                RespawnUnitHome(unit);
            }
            this.ownerUnitsOnBoard.Clear();
        }

        private void RespawnUnitHome(UnitData unit)
        {
            Vector3 location = this.tiles[unit.tileIndex].worldPosition;

            CharacterMaster master = unit.master;

            master.Respawn(location, Quaternion.identity);
            master.GetComponent<RoR2.CharacterAI.BaseAI>().enabled = false;

            AI.TileNavigator t = master.GetComponent<AI.TileNavigator>();
            t.currentBoard = this;
            t.SetCurrentTile(this.tiles[unit.tileIndex]);
        }

        public void CreateEnemyTeam(List<UnitData> enemyUnits, CharacterMaster enemyPlayer)
        {
            this.enemyUnitsOnBoard = new List<UnitData>();
            foreach (UnitData unit in enemyUnits)
            {

                int i = (this.tiles.Length - 1) - unit.tileIndex;
                Vector3 location = this.tiles[i].worldPosition;

                CharacterMaster master = unit.master;

                master.Respawn(location, Quaternion.identity);

                AI.TileNavigator t = master.GetComponent<AI.TileNavigator>();
                t.currentBoard = this;
                t.SetCurrentTile(this.tiles[i]);

                this.enemyUnitsOnBoard.Add(unit);
            }


        }

        public void SetCombat(bool b)
        {
            foreach (UnitData unit in this.ownerUnitsOnBoard)
            {
                unit.navigator.inCombat = b;
                if(unit.master)
                {
                    unit.master.teamIndex = TeamIndex.Player;

                    RoR2.CharacterAI.BaseAI ai = unit.master.GetComponent<RoR2.CharacterAI.BaseAI>();
                    if(ai)
                    {
                        ai.enabled = b;
                    }
                }
                
            }
            foreach (UnitData unit in this.enemyUnitsOnBoard)
            {
                unit.navigator.inCombat = b;
                if (unit.master)
                {
                    unit.master.teamIndex = TeamIndex.Monster;

                    RoR2.CharacterAI.BaseAI ai = unit.master.GetComponent<RoR2.CharacterAI.BaseAI>();
                    if (ai)
                    {
                        ai.enabled = b;
                    }
                }
            }

        }
        private void OnDestroy()
        {
            ChessBoard.instancesList.Remove(this);
        }

        private static NodeGraph GenerateNodegraph(int x, int y, float gap, float scale)
        {
            float width = baseTileWidth + baseTileWidth * gap;
            float height = baseTileHeight + baseTileHeight * gap;

            NodeGraph nodeGraph = ScriptableObject.CreateInstance<NodeGraph>();
            nodeGraph.Clear();
            nodeGraph.name = "hexboardGroundNodesNodegraph";
            NodeGraph.Node[] nodes = new NodeGraph.Node[x * y];
            List<NodeGraph.Link> links = new List<NodeGraph.Link>();

            for (int i = 0; i < y; i++)
            {
                for (int k = 0; k < x; k++)
                {
                    int index = i * x + k;
                    float offset = i % 2 != 0 ? width / 2f : 0f;
                    nodes[index] = new NodeGraph.Node
                    {
                        position = new Vector3((width * k + offset), 0.2f, (-height * i * 0.75f)) * scale,
                    };

                    #region NodeGraph
                    NodeGraph.NodeIndex nodeIndex = new NodeGraph.NodeIndex { nodeIndex = index };
                    int debugLinksGenerated = 0;

                    Log.LogInfo("LINKING " + index);
                    if (k > 0)
                    {
                        links.Add(CreateLink(nodeIndex, index - 1));

                    } // behind
                    if (k < x - 1)
                    {
                        links.Add(CreateLink(nodeIndex, index + 1));
                    } // in front
                    if (i > 0)
                    {
                        if (i % 2 == 0)
                        {
                            if (k == 0)
                            {
                                links.Add(CreateLink(nodeIndex, index - x));
                                debugLinksGenerated++;
                            }
                            else
                            {
                                links.Add(CreateLink(nodeIndex, index - x - 1));
                                links.Add(CreateLink(nodeIndex, index - x));
                                debugLinksGenerated += 2;
                            }
                        }
                        else
                        {
                            if (k == x - 1)
                            {
                                links.Add(CreateLink(nodeIndex, index - x));
                                debugLinksGenerated++;
                            }
                            else
                            {
                                links.Add(CreateLink(nodeIndex, index - x));
                                links.Add(CreateLink(nodeIndex, index - x + 1));
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
                                links.Add(CreateLink(nodeIndex, index + x));
                                debugLinksGenerated++;
                            }
                            else
                            {
                                links.Add(CreateLink(nodeIndex, index + x - 1));
                                links.Add(CreateLink(nodeIndex, index + x));
                                debugLinksGenerated += 2;
                            }
                        }
                        else
                        {
                            if (k == x - 1)
                            {
                                links.Add(CreateLink(nodeIndex, index + x));
                                debugLinksGenerated++;
                            }
                            else
                            {
                                links.Add(CreateLink(nodeIndex, index + x));
                                links.Add(CreateLink(nodeIndex, index + x + 1));
                                debugLinksGenerated += 2;
                            }

                        }
                    } // above

                    /*
                    nodes[index].linkListIndex = new NodeGraph.LinkListIndex
                    {
                        index = links.Count,
                        size = (uint)debugLinksGenerated
                    };   
                    */
                    Log.LogDebug("Hex " + k + "|" + i + " generated at position + " + nodes[index].position.ToString() + " with " + debugLinksGenerated + " links.");
                    #endregion
                }
            }

            nodeGraph.nodes = nodes;
            nodeGraph.links = links.ToArray();

            nodeGraph.OnNodeCountChanged();
            nodeGraph.RebuildBlockMap();

            return nodeGraph;
        }
        private static NodeGraph.Link CreateLink(NodeGraph.NodeIndex indexA, int indexB)
        {
            Log.LogDebug(indexA.nodeIndex + " -> " + indexB);
            return new NodeGraph.Link
            {
                nodeIndexA = indexA,
                nodeIndexB = new NodeGraph.NodeIndex { nodeIndex = indexB },
                distanceScore = (baseTileWidth + baseTileWidth * ChessBoard.gap) * scale,
                hullMask = 7,
            };
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
        private Tile[] GenerateTiles(int x, int y, float gap, float scale, Vector3 startPos) // can be static if i decide all boards are same size
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

        public class Tile
        {
            public Dictionary<Tile, int> tileDistances;
            public ChessBoard board;
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
                    connectedTiles[i] = board.tiles[connectedTileIndices[i]];
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
                return GetClosestConnectedTile(board.tiles[targetTileIndex]);
            }
            public Tile GetClosestConnectedTile(Tile targetTile)
            {
                return (GetClosestConnectedTile(targetTile.worldPosition));                
            }
            public Tile GetClosestConnectedTile(Vector3 worldPosition)
            {
                float lowestDistance = Mathf.Infinity;
                Tile tile = this;
                if (connectedTiles.Length > 0)
                {                   
                    for (int i = 0; i < connectedTiles.Length; i++)
                    {
                        float distance = (connectedTiles[i].worldPosition - worldPosition).magnitude;
                        if (distance < lowestDistance && !connectedTiles[i].occupied)
                        {
                            tile = connectedTiles[i];
                            lowestDistance = distance;
                        }
                    }
                }               
                return tile;
            }

            public Tile GetTileIgnore(Vector3 worldPosition, Tile ignoredTile) // this feels ugly
            {
                float lowestDistance = Mathf.Infinity;
                Tile tile = this;
                if (connectedTiles.Length > 0)
                {
                    for (int i = 0; i < connectedTiles.Length; i++)
                    {
                        if (connectedTiles[i] != ignoredTile)
                        {
                            float distance = (connectedTiles[i].worldPosition - worldPosition).magnitude;
                            if (distance < lowestDistance && !connectedTiles[i].occupied)
                            {
                                tile = connectedTiles[i];
                                lowestDistance = distance;
                            }
                        }                      
                    }
                }
                return tile;
            }

            // should probably do actual pathing
            //public List<Tile> CreatePath(Vector3 worldPosition, int steps, List<Tile> ignoredTiles)
            //{
            //    steps--;
            //    float lowestDistance = Mathf.Infinity;
            //    Tile tile = this;
            //    if (connectedTiles.Length > 0)
            //    {
            //        for (int i = 0; i<connectedTiles.Length; i++)
            //        {
            //            if (!ignoredTiles.Contains(connectedTiles[i]))
            //            {
            //                float distance = (connectedTiles[i].worldPosition - worldPosition).magnitude;
            //                if (distance<lowestDistance && !connectedTiles[i].occupied)
            //                {
            //                    tile = connectedTiles[i];
            //                    lowestDistance = distance;
            //                }
            //            }
            //        }
            //    }
            //    ignoredTiles
            //}

           
        }
    }
}
