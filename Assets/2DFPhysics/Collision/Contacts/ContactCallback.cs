using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TF.Core
{
    public interface ContactCallback
    {
        void HandleCollision(Manifold m, TFRigidbody a, TFRigidbody b);
    }
}