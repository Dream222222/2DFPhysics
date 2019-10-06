using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDFP.Core
{
    public class ManifoldComparer : IComparer<Manifold>
    {
        public int Compare(Manifold x, Manifold y)
        {
            if (x.A.Position.Y < y.A.Position.Y && x.A.Position.Y < y.B.Position.Y
                || x.B.Position.Y < y.A.Position.Y && x.B.Position.Y < y.B.Position.Y)
            {
                return -1;
            }
            else if (x.A.Position.Y > y.A.Position.Y && x.A.Position.Y > y.B.Position.Y
               || x.B.Position.Y > y.A.Position.Y && x.B.Position.Y > y.B.Position.Y)
            {
                return 1;
            }

            if (x.A.Position.X < y.A.Position.X && x.A.Position.X < y.B.Position.X
                || x.B.Position.X < y.A.Position.X && x.B.Position.X < y.B.Position.X)
            {
                return -1;
            }

            if (x.A.Position.X == y.A.Position.X && x.A.Position.X == y.B.Position.X
                || x.B.Position.X == y.A.Position.X && x.B.Position.X == y.B.Position.X)
            {
                return 0;
            }

            return 1;
        }
    }
}   