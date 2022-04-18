using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using RoR2.Navigation;
using UnityEngine.SceneManagement;

namespace RORAutochess
{
    class GenericBoard
    {
        public const float baseWidth = 1.732f;
        public const float baseHeight = 2.0f;

        private static float scale;
        private static float gap;

        public static SceneDef sceneDef;
        public static NodeGraph nodeGraph;
        public static Tile[] tiles;

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
            nodeGraph = GenerateNodegraph(x, y, gap, scale);

            ContentPacks.sceneDefs.Add(sceneDef);

            SceneManager.sceneLoaded += SetupBoard;     
        }

        private static void SetupBoard(Scene scene, LoadSceneMode arg1)
        {
            if (scene.name == "genericboardscene")
            {
                GameObject.Find("Grid").transform.localScale *= GenericBoard.scale * 2; // i fucked something up with scale idk
                GameObject si = new GameObject("SceneInfo");
                SceneInfo info = si.AddComponent<SceneInfo>();
                info.groundNodesAsset = nodeGraph;
                info.groundNodes = nodeGraph;

                ClassicStageInfo csInfo = si.AddComponent<ClassicStageInfo>();
                SceneManager.MoveGameObjectToScene(si, scene);

                GameObject ev = new GameObject("GameManager");
                GlobalEventManager manager = ev.AddComponent<GlobalEventManager>();
                SceneManager.MoveGameObjectToScene(ev, scene);

                GameObject di = new GameObject("Director");
                //di.AddComponent<DirectorCore>();
                //di.AddComponent<SceneDirector>();
                //di.AddComponent<CombatDirector>();
            }




        }

        private static NodeGraph GenerateNodegraph(int x, int y, float gap, float scale)
        {
            float width = baseWidth + baseWidth * gap;
            float height = baseHeight + baseHeight * gap;

            tiles = new Tile[x * y];

            NodeGraph nodeGraph = ScriptableObject.CreateInstance<NodeGraph>();
            nodeGraph.Clear();
            nodeGraph.name = "hexboardGroundNodesNodegraph";
            NodeGraph.Node[] nodes = new NodeGraph.Node[x * y];
            List<NodeGraph.Link> links = new List<NodeGraph.Link>();

            for(int i = 0; i < y; i++)
            {
                for(int k = 0; k < x; k++)
                {
                    int index = i * x + k;
                    float offset = i % 2 != 0 ? width / 2f : 0f;
                    nodes[index] = new NodeGraph.Node {
                        position = new Vector3((width * k + offset) * scale, 0.1f, (-height * i * 0.75f) * scale),
                        

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
                            if(k == 0)
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
                            if(k == x - 1)
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
                distanceScore = baseWidth + baseWidth * GenericBoard.gap,
                hullMask = 7,
            };
        }

        public struct Tile
        {
            int index;
            Vector3 worldPosition;
            bool occupied;
            int[] connectedTileIndices;
            Tile[] connectedTiles;
        }
    }
}
