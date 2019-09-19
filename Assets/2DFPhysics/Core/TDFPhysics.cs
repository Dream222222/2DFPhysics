using UnityEngine;
using FixedPointy;
using System.Collections.Generic;

namespace TDFP.Core
{
    public struct BroadPhasePair
    {
        FPRigidbody A;
        FPRigidbody B;

        public BroadPhasePair(FPRigidbody a, FPRigidbody b)
        {
            A = a;
            B = b;
        }
    };

    public class TDFPhysics : MonoBehaviour
    {
        public static TDFPhysics instance;

        public TDFPSettings settings;

        public List<FPRigidbody> bodies = new List<FPRigidbody>();

        //Broad Phase
        private List<BroadPhasePair> broadPhasePairs = new List<BroadPhasePair>();

        private void Awake()
        {
            instance = this;
        }

        private void FixedUpdate()
        {
            if (settings.AutoSimulation)
            {
                TimeStep();
            }
        }

        void TimeStep()
        {

        }

        public void UpdatePhysics(Fix dt)
        {
            BroadPhase();
        }

        #region Broad Phase
        void BroadPhase()
        {
            GenerateBPairs();
        }

        void GenerateBPairs()
        {
            broadPhasePairs.Clear();
            for (int i = 0; i < bodies.Count; i++)
            {
                for (int w = 0; w < bodies.Count; w++)
                {
                    if (bodies[i] == bodies[w])
                    {
                        return;
                    }

                    if(CollisionChecks.AABBvsAABB(new Manifold(bodies[i], bodies[w])))
                    {
                        Debug.Log("Got pair.");
                        broadPhasePairs.Add(new BroadPhasePair(bodies[i], bodies[w]));
                    }
                }
            }
        }
        #endregion
    }
}