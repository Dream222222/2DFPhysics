using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using System;

namespace TF.Core
{
    public class ManifoldComparer : Comparer<Manifold>
    {
        public override int Compare(Manifold x, Manifold y)
        {
            if ((x.A.ProxyID * 2) + (x.B.ProxyID * 3)
                == (y.B.ProxyID * 2) + (y.A.ProxyID * 3))
            {
                return 0; // These are the same but flipped.
            }

            if ((x.A.ProxyID * 2) + (x.B.ProxyID * 3)
                > (y.A.ProxyID * 2) + (y.B.ProxyID * 3))
            {
                return 1; // x is more than y.
            }

            return -1; // x is less than y.
        }
    }
}
