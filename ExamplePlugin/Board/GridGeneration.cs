using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RORAutochess.Board
{
    public class GridGeneration : MonoBehaviour
    {
        public Transform hexPrefab1;
        public Transform hexPrefab2;
        public Transform hexPrefab3;
        public int gridWidth = 11;
        public int gridHeight = 11;

        float hexWidth = 1.732f;
        float hexHeight = 2.0f;
        public float gap = 0.0f;
        public float scale;

        Vector3 startPos;
        

        void Start()
        {
            AddGap();
            CalcStartPos();
            CreateGrid();
        }

        void AddGap()
        {
            hexWidth += hexWidth * gap;
            hexHeight += hexHeight * gap;
        }

        void CalcStartPos()
        {
            float offset = 0;
            if (gridHeight / 2 % 2 != 0)
                offset = hexWidth / 2;

            float x = -hexWidth * (gridWidth / 2) - offset;
            float z = hexHeight * 0.75f * (gridHeight / 2);

            startPos = new Vector3(0, 0, 0);// Vector3(x, 0, z);
        }

        Vector3 CalcWorldPos(Vector2 gridPos)
        {
            float offset = 0;
            if (gridPos.y % 2 != 0)
                offset = hexWidth / 2;

            float x = startPos.x + gridPos.x * hexWidth + offset;
            float z = startPos.z - gridPos.y * hexHeight * 0.75f;

            return new Vector3(x, 0, z);
        }

        void CreateGrid()
        {
            int i = 1;
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Transform t = hexPrefab1;
                    if (i == 1) t = hexPrefab1;
                    else if (i == 2) t = hexPrefab2;
                    else if (i == 3) t = hexPrefab3;
                    Transform hex = Instantiate(t) as Transform;
                    Vector2 gridPos = new Vector2(x, y);
                    hex.position = CalcWorldPos(gridPos);
                    hex.parent = this.transform;
                    hex.name = "Hexagon" + x + "|" + y;
                    i++;
                    if (i > 3) i = 1;

                }
            }
            base.transform.parent.localScale *= scale; // this is bad
        }
    }
}
