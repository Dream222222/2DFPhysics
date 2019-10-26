using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDFP.Core
{
    public interface CollisionCallback
    {
        void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b);
    }
}