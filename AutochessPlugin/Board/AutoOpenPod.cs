using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using EntityStates.SurvivorPod;

namespace RORAutochess.Board
{
    public class AutoOpenPod : MonoBehaviour
    {
        public static float destroyTime = 2.5f; // make it explode or smth idk
        public static float delay = 0.5f;
        private float stopwatch;
        bool b;

        private EntityStateMachine m;

        private void Awake()
        {
            m = base.GetComponent<EntityStateMachine>();

            if (!m || !base.GetComponent<SurvivorPodController>())
                Destroy(this);

        }

        private void FixedUpdate()
        {
            if(m.state is Landed || b)
            {
               
                stopwatch += Time.fixedDeltaTime;
               
            }
            if (stopwatch >= delay && !b)
            {
                b = true;
                m.SetNextState(new PreRelease());
                stopwatch = 0f;
            }

            if (stopwatch >= destroyTime)
                Destroy(base.gameObject);
        }

    }
}
