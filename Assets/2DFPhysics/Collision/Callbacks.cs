using UnityEngine;
using FixedPointy;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System;

namespace TF.Core
{
    public interface ITreeQueryCallback
    {
        bool QueryCallback(int proxyID);
    }

    public interface ITreeRaycastCallback
    {
        /// <summary>
        /// Returns a value that determines what happens after a raycast hits a body.
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="maxFraction"></param>
        /// <param name="proxyID"></param>
        /// <returns>0 terminates the raycast, the fraction of a ray to clip it (0-1), or just 1 to continue.</returns>
        Fix RayCastCallback(FixVec2 pointA, FixVec2 pointB, Fix maxFraction, int proxyID);
    }
}