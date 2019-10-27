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
}