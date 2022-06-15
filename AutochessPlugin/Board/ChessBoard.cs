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

        public static Vector3 boardPosition = new Vector3(0, 100f, 0);
        public static Vector3 cameraPosition = new Vector3(6.75f, 7.285f, 6.16f);
        public static Quaternion cameraRotation = Quaternion.Euler(40f, 180f, 0f);

        public static List<ChessBoard> instancesList;
        public static GameObject gridPrefab;

        private static float scale;
        private static float gap;
        private static int boardWidth;
        private static int boardHeight;

        public GameObject gridInstance;
        public GameObject benchInstance;

        public static NodeGraph groundNodeGraph;
        public static NodeGraph airNodeGraph;

        public Tile[] tiles;

        public static Transform cameraTransform;

        public CharacterMaster owner;
        public List<UnitData> ownerUnitsOnBoard = new List<UnitData>();
        public List<CharacterMaster> enemiesOnBoard = new List<CharacterMaster>();
        private CharacterMaster enemy;


        public bool readyForCombat;
        public bool inCombat;

        public Action onPrepPhase;
        public Action onCombatPhase;


        public static int teleporterNodeIndex = 127;

        // this whole thing is set up horribly. needs to be redone for multiplayer
        public static void Setup(int x, int y, float gap, float scale)
        {
            SurfaceDef d = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SurfaceDef>("RoR2/Base/Common/sdStone.asset").WaitForCompletion();
            AutochessPlugin.assetbundle.LoadAsset<GameObject>("Hexagon1").GetComponent<SurfaceDefProvider>().surfaceDef = d;
            AutochessPlugin.assetbundle.LoadAsset<GameObject>("Hexagon2").GetComponent<SurfaceDefProvider>().surfaceDef = d;
            AutochessPlugin.assetbundle.LoadAsset<GameObject>("Hexagon3").GetComponent<SurfaceDefProvider>().surfaceDef = d;

            

            ChessBoard.scale = scale;
            ChessBoard.gap = gap;
            boardWidth = x;
            boardHeight = y;

            gridPrefab = AutochessPlugin.assetbundle.LoadAsset<GameObject>("Grid");

            instancesList = new List<ChessBoard>();


            SceneManager.sceneLoaded += SetupScene;
        }

        private void OnDestroy()
        {
            ChessBoard.instancesList.Remove(this);
            Stage.onStageStartGlobal -= Stage_onStageStartGlobal;
        }


        private static void SetupScene(Scene scene, LoadSceneMode arg1) // ugh
        {
            if (AutochessRun.instance is AutochessRun)
            {

                SceneInfo info = GameObject.FindObjectOfType<SceneInfo>();

                Renderer[] obj = GameObject.FindObjectsOfType<Renderer>(); // ?
                for(int i = 0; i < obj.Length; i++)
                {
                    if (obj[i].gameObject != info.gameObject)
                        GameObject.Destroy(obj[i].gameObject);
                }
                Collider[] obj2 = GameObject.FindObjectsOfType<Collider>();
                for (int i = 0; i < obj2.Length; i++)
                {
                    if (obj2[i].gameObject != info.gameObject)
                        GameObject.Destroy(obj2[i].gameObject);
                }


                CreateBoard();



                groundNodeGraph = GenerateNodegraph(boardWidth, boardHeight, gap, scale, ChessBoard.boardPosition);
                airNodeGraph = GenerateNodegraph(boardWidth, boardHeight, gap, scale, ChessBoard.boardPosition + Vector3.up * 10f);
                info.groundNodesAsset = groundNodeGraph;
                info.groundNodes = groundNodeGraph;
                info.airNodesAsset = airNodeGraph;
                info.airNodes = airNodeGraph;

                GameObject.Instantiate<GameObject>(Board.RoundController.prefab);

            }



        }

        private static void CreateBoard()
        {
            List<LocalUser> players = LocalUserManager.localUsersList;

            CharacterMaster player = players[0].cachedMaster;
            GameObject obj = new GameObject("Board_" + players[0].userProfile.name);

            GameObject camera = new GameObject("CameraHolder_" + obj.name);    // NEED TO CALCULATE CAMERA POSITION FROM BOARD SIZE !!!!! 
            camera.transform.parent = obj.transform;
            camera.transform.position = ChessBoard.cameraPosition * ChessBoard.scale * 2; ///  2 ? 
            camera.transform.rotation = ChessBoard.cameraRotation;
            ChessBoard.cameraTransform = camera.transform;

            obj.transform.position = ChessBoard.boardPosition;
            ChessBoard board = obj.AddComponent<ChessBoard>();
            board.owner = player;
            board.gridInstance = GameObject.Instantiate<GameObject>(ChessBoard.gridPrefab, obj.transform);
            Board.GridGeneration g = board.gridInstance.transform.Find("Hexagons").gameObject.GetComponent<Board.GridGeneration>(); // cringe
            g.gridWidth = boardWidth;
            g.gridHeight = boardHeight;
            g.gap = gap;
            g.scale = scale;

            board.tiles = board.GenerateTiles(boardWidth, boardHeight, gap, scale, obj.transform.position); // should probably combine tile and hexagon generation ??                                 
            for (int i = 0; i < board.tiles.Length; i++)
            {
                board.tiles[i].LinkTile();
            }
            for (int i = 0; i < board.tiles.Length; i++)
            {
                board.tiles[i].CalculateTileDistances();
            }

            
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
            
            ChessBoard.instancesList.Add(this);
        }

        private void Start()
        {
            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            
        }

        private enum CombatStatus
        {
            InCombat,
            Draw,
            PlayerWin,
            EnemyWin
        }
        private CombatStatus GetCombatStatus()
        {
            bool allEnemiesDead = true;
            bool allAlliesDead = true;
            CombatStatus s = CombatStatus.InCombat;
            foreach(CharacterMaster m in this.enemiesOnBoard)
            {
                if(m && !m.IsDeadAndOutOfLivesServer())
                {
                    allEnemiesDead = false;
                    break;
                }
            }
            foreach(UnitData u in this.ownerUnitsOnBoard)
            {
                if(u && u.master && !u.master.IsDeadAndOutOfLivesServer())
                {
                    allAlliesDead = false;
                    break;
                }
            }

            if (!allAlliesDead && !allEnemiesDead) s = CombatStatus.InCombat; // probably a way to make this look nicer
            if (allAlliesDead && allEnemiesDead) s = CombatStatus.Draw;
            if (!allAlliesDead && allEnemiesDead) s = CombatStatus.PlayerWin;
            if (allAlliesDead && !allEnemiesDead) s = CombatStatus.EnemyWin;

            return s;
        }

        private void FixedUpdate()
        {
            if(this.inCombat)
            {
                CombatStatus s = this.GetCombatStatus();
                if(s > CombatStatus.InCombat)
                {
                    this.SetCombat(false);
                }
            }
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {

            CombatDirector[] obj2 = GameObject.FindObjectsOfType<CombatDirector>(); // make own director
            for (int i = 0; i < obj2.Length; i++)
            {
                GameObject.Destroy(obj2[i]);
            }

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

            GameObject body = this.owner.GetBodyObject();
            ModelLocator model = body.GetComponent<ModelLocator>();
            if (model)
            {
                GameObject.Destroy(model.modelTransform.gameObject);
            }

            if (!player.gameObject.GetComponent<Interactor>())
                player.gameObject.AddComponent<Interactor>();
            if (!player.gameObject.GetComponent<UI.MouseInteractionDriver2>())
                player.gameObject.AddComponent<UI.MouseInteractionDriver2>();
            if (!player.gameObject.GetComponent<Traits.UnitOwnership>())
                player.gameObject.AddComponent<Traits.UnitOwnership>();

            

            ChessBoard.Tile tile = GetLowestUnoccupiedTile(this.tiles);
            DeployUnit(master.GetComponent<CharacterMaster>(), tile);

            this.owner.bodyPrefab = Stuff.playerBodyPrefab;
            this.owner.Respawn(Vector3.zero, Quaternion.identity);
        }

        public void CreatePodShop(Tile tile) // should redo this completely
        {
            if(this.owner) // bad bad bad
            {
                GameObject pod = GameObject.Instantiate(Stuff.podSpawnPointPrefab, tile.worldPosition + Vector3.up * 2, Quaternion.identity);

                UI.AutochessHUDAddon h = UI.AutochessHUDAddon.FindByMaster(this.owner);
                if(h)
                {
                    GameObject hud = h.gameObject;
                    UICamera uicamera = hud.GetComponent<Canvas>().rootCanvas.worldCamera.GetComponent<UICamera>();

                    Camera sceneCam = uicamera.cameraRigController.sceneCam;
                    Camera uiCam = uicamera.camera;

                    Vector3 position = pod.transform.position;
                    position.y += 4.5f;
                    Vector3 vector = sceneCam.WorldToScreenPoint(position);
                    Vector3 position2 = uiCam.ScreenToWorldPoint(vector);

                    Transform t = hud.transform.Find("MainContainer");
                    GameObject shop = GameObject.Instantiate(Stuff.podShopPrefab, t);

                    UI.PodShop ps = shop.GetComponent<UI.PodShop>();
                    ps.source = this.owner;
                    ps.podObject = pod;
                    ps.uiCamera = uicamera;

                    shop.transform.position = position2;
                }
               
            }
            
        }

        public void SetupUnit(CharacterMaster m)
        {
            Units.UnitData unitData = m.gameObject.AddComponent<Units.UnitData>();
            GameObject bodyObject = m.GetBodyObject();
            CharacterBody body = bodyObject.GetComponent<CharacterBody>();
            unitData.master = m;
            unitData.bodyObject = m.bodyPrefab;
            unitData.unitName = body.baseNameToken;
            m.gameObject.AddComponent<AI.TileNavigator>();
            EntityStateMachine mac = m.GetComponent<EntityStateMachine>();
            if (mac)
            {
                mac.initialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AI.Walker.Guard));
                mac.mainStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AI.Walker.Guard));
            }
            RoR2.CharacterAI.BaseAI ai = m.GetComponent<RoR2.CharacterAI.BaseAI>();
            if (ai)
            {
                ai.fullVision = true;
                ai.scanState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AI.Walker.Guard));
            }

            this.SetupBody(bodyObject);
        }

        public void SetupBody(GameObject bodyObject)
        {
            CharacterBody body = bodyObject.GetComponent<CharacterBody>();
            bodyObject.AddComponent<Units.UnitPickupInteraction>();
            body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            if (body.characterMotor) body.characterMotor.mass = 10000f;
            if (body.rigidbody) body.rigidbody.mass = 10000f;
        }

        public CharacterBody DeployUnit(CharacterMaster master, Tile tile)
        {
            if(tile == null) tile = GetLowestUnoccupiedTile(this.tiles);                      
            if (tile != null)
            {
                CharacterMaster m = new MasterSummon
                {
                    masterPrefab = master.gameObject,
                    summonerBodyObject = this.owner.GetBodyObject(), // ?????
                    ignoreTeamMemberLimit = true,
                    inventoryToCopy = null,
                    useAmbientLevel = new bool?(true),
                    position = tile.worldPosition,
                    rotation = Quaternion.identity,

                }.Perform();

                CharacterBody body = m.GetBody();

                if (body.preferredPodPrefab)
                {
                    // maybe do rotation
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(body.preferredPodPrefab, body.transform.position, Quaternion.identity);
                    VehicleSeat component = gameObject.GetComponent<VehicleSeat>();
                    

                    gameObject.AddComponent<Board.AutoOpenPod>();
                    if (component)
                    {
                        component.onPassengerExit += (GameObject bodyObject) =>
                        {
                            HealthComponent h = bodyObject.GetComponent<HealthComponent>();
                            if (h)
                            {
                                foreach (UI.AllyHealthBarViewer combatHealthBarViewer in UI.AllyHealthBarViewer.instancesList)
                                {
                                    if (body.teamComponent.teamIndex == combatHealthBarViewer.viewerTeamIndex)
                                    {
                                        combatHealthBarViewer.AddHealthBarInfo(body.healthComponent);
                                    }
                                }
                            }

                        };
                        
                        component.AssignPassenger(body.gameObject);
                    }
                    NetworkServer.Spawn(gameObject);
                }
                else
                {
                    foreach (UI.AllyHealthBarViewer combatHealthBarViewer in UI.AllyHealthBarViewer.instancesList) ///////
                    {
                        if (body.teamComponent.teamIndex == combatHealthBarViewer.viewerTeamIndex)
                        {
                            combatHealthBarViewer.AddHealthBarInfo(body.healthComponent);
                        }
                    }
                }

                SetupUnit(m);

                m.destroyOnBodyDeath = false;

                AI.TileNavigator t = m.GetComponent<AI.TileNavigator>();

                t.currentBoard = this;
                t.SetCurrentTile(tile);


                return body;
            }
            return null;
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
                    if(data && data.navigator) // shouldnt null check, game will break anyways
                    {
                        data.navigator.inCombat = false;
                        data.tileIndex = data.navigator.currentTile.index;
                        this.ownerUnitsOnBoard.Add(data);
                        
                    }
                    
                }
                              
            }
        }

        public static int testPveRoundEnemies = 5;
        public List<CharacterMaster> CreatePVERound() // giga testing. could do director stuff here?
        {
            var enemyUnits = new List<CharacterMaster>(); 
            var masters = new GameObject[testPveRoundEnemies];

            var choices = new GameObject[]{ MasterCatalog.FindMasterPrefab("LemurianMaster"), MasterCatalog.FindMasterPrefab("BeetleMaster") };

            for (int i = 0; i < masters.Length; i++)
            {
                int z = UnityEngine.Random.RandomRangeInt(0, choices.Length);
                var body = choices[z].GetComponent<CharacterMaster>().bodyPrefab;

                masters[i] = UnityEngine.Object.Instantiate<GameObject>(choices[z]);
                CharacterMaster master = masters[i].GetComponent<CharacterMaster>();                
                NetworkServer.Spawn(masters[i]);
                master.bodyPrefab = body;
                master.teamIndex = TeamIndex.Monster;

                

                enemyUnits.Add(master);
            }
            return enemyUnits;


        }
        public void ResetBoard()
        {

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].occupant = null;
            }

            foreach (CharacterMaster unit in this.enemiesOnBoard)
            {
                if(unit)
                    unit.TrueKill(); // FOR TESTING
            }
            this.enemiesOnBoard.Clear();
            foreach(UnitData unit in this.ownerUnitsOnBoard)
            {
                unit.navigator.inCombat = false;
                RespawnUnitHome(unit);
            }
            this.ownerUnitsOnBoard.Clear();
            this.readyForCombat = false;
        }

        private void RespawnUnitHome(UnitData unit)
        {
            Vector3 location = this.tiles[unit.tileIndex].worldPosition;

            CharacterMaster master = unit.master;

            SetupBody(master.Respawn(location, Quaternion.identity).gameObject);

            AI.TileNavigator t = master.GetComponent<AI.TileNavigator>();
            t.currentBoard = this;
            t.SetCurrentTile(this.tiles[unit.tileIndex]);
        }

        public void CreateEnemyTeam(List<CharacterMaster> enemyUnits)
        {
            this.enemiesOnBoard = new List<CharacterMaster>();
            foreach (CharacterMaster master in enemyUnits)
            {
                int i = UnityEngine.Random.RandomRangeInt(0, (this.tiles.Length / 2) - 1);
                int k = (this.tiles.Length - 1) - i;
                Vector3 location = this.tiles[k].worldPosition;
                master.Respawn(location, Quaternion.identity);

                this.enemiesOnBoard.Add(master);
            }


        }

        public void SetCombat(bool b)
        {
            this.inCombat = b;

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
            foreach (CharacterMaster unit in this.enemiesOnBoard) ///
            {
                if(unit)
                {
                    unit.teamIndex = TeamIndex.Monster;

                    RoR2.CharacterAI.BaseAI ai = unit.GetComponent<RoR2.CharacterAI.BaseAI>();
                    if (ai)
                    {
                        ai.enabled = b;
                    }
                }                           
            }

        }

        private static NodeGraph GenerateNodegraph(int x, int y, float gap, float scale, Vector3 startPos)
        {
            float width = baseTileWidth + baseTileWidth * gap;
            float height = baseTileHeight + baseTileHeight * gap;

            NodeGraph nodeGraph = ScriptableObject.CreateInstance<NodeGraph>();
            nodeGraph.Clear();
            nodeGraph.name = "hexboard" + startPos.ToString() + "NodesNodegraph";
            NodeGraph.Node[] nodes = new NodeGraph.Node[x * y];
            List<NodeGraph.Link> links = new List<NodeGraph.Link>();

            for (int i = 0; i < y; i++)
            {
                for (int k = 0; k < x; k++)
                {
                    int index = (i * x + k);
                    float offset = i % 2 != 0 ? width / 2f : 0f;
                    nodes[index] = new NodeGraph.Node
                    {
                        position = new Vector3((width * k + offset), 0.2f, (-height * i * 0.75f)) * scale + startPos,
                    };

                    if(index == ChessBoard.teleporterNodeIndex) ////////////////// CALCULATE INDEX FOR THESE board size might change
                    {
                        nodes[index].flags = NodeFlags.TeleporterOK;
                    }
                    if(index <= (nodes.Length / 2) - 1) 
                    {
                        nodes[index].flags = NodeFlags.NoCharacterSpawn;
                        nodes[index].flags |= NodeFlags.NoShrineSpawn;
                    }

                    #region NodeGraph
                    NodeGraph.NodeIndex nodeIndex = new NodeGraph.NodeIndex { nodeIndex = index };
                    int debugLinksGenerated = 0;

                    //Log.LogInfo("LINKING " + index);
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

                    //Log.LogDebug("Hex " + k + "|" + i + " generated at position + " + nodes[index].position.ToString() + " with " + debugLinksGenerated + " links.");
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
            //Log.LogDebug(indexA.nodeIndex + " -> " + indexB);
            return new NodeGraph.Link
            {
                nodeIndexA = indexA,
                nodeIndexB = new NodeGraph.NodeIndex { nodeIndex = indexB },
                distanceScore = (baseTileWidth + baseTileWidth * gap) * scale,
                hullMask = 7,
            };
        }


        /*
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
        */
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
            /*
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
            */
            return tile;
        }

        public class Tile
        {
            public Dictionary<Tile, int> tileDistances;
            public ChessBoard board;
            public int index;
            public Vector3 worldPosition;
            public bool occupied
            {
                get
                {
                    return occupant != null;
                }
            }
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
