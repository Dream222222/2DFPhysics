using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using TDFP.Core;

namespace TDFP.Colliders
{
    public class TDFPolygonCollider : TDFPCollider
    {
        private List<FixVec2> normals = new List<FixVec2>();

        [SerializeField] private List<FixVec2> vertices = new List<FixVec2>();
    }
}
