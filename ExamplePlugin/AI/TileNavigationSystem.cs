using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;

namespace AutochessPlugin.AI
{
    public class TileNavigationSystem
    {
        private static List<TileNavigationSystem> instances = new List<TileNavigationSystem>();

        private static float timeSinceLastUpdate;
        private static float updateInterval = 0.33f;

        static TileNavigationSystem()
        {
            RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
        }

        private static void RoR2Application_onFixedUpdate()
        {
            timeSinceLastUpdate += Time.fixedDeltaTime;
            if(timeSinceLastUpdate >= updateInterval)
            {
                timeSinceLastUpdate -= updateInterval;
                UpdateInstances();
            }
        }

        public static void UpdateInstances()
        {
            foreach (TileNavigationSystem tileNavigationSystem in instances)
            {
                tileNavigationSystem.Update(updateInterval);
            }
        }

        private void Update(float deltaTime)
        {

        }

        protected struct AgentData
        {
            public bool enabled;

            public float localTime;

            public Vector3? currentPosition;

            public Vector3? goalPosition;
        }

        public struct Agent
        {
            TileNavigationSystem system;

            public Agent(TileNavigationSystem system)
            {
                this.system = system;
            }
        }

    }
}
