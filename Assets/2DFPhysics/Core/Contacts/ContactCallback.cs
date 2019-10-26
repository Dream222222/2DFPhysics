using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDFP.Core
{
    public interface ContactCallback
    {
        void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b);
    }
}