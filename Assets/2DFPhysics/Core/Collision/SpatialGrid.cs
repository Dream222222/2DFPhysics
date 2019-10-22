using UnityEngine;
using FixedPointy;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System;

//http://buildnewgames.com/broad-phase-collision-detection/
namespace TDFP.Core
{
    public class SpatialGrid
    {
        public FixVec2 min;
        public FixVec2 max;
        public FixVec2 cellSize;
        public int columns;
        public int rows;
        public List<FPRigidbody>[][] grid;

        public SpatialGrid(FixVec2 min, FixVec2 max, FixVec2 cellSize)
        {
            this.min = min;
            this.max = max;
            this.cellSize = cellSize;
            this.columns = (int)((max.X - min.X) / cellSize.X);
            this.rows = (int)((max.Y - min.Y) / cellSize.Y);
            grid = new List<FPRigidbody>[columns][];
        }

        void ResetGrid()
        {
            for(int i = 0; i < columns; i++)
            {
                grid[i] = null;
            }
        }

        public void Update()
        {
            //Reset our cells.
            ResetGrid();

            // insert all entities into grid
            for (int i = 0; i < TDFPhysics.bodies.Count; i++)
            {
                FPRigidbody rigid = TDFPhysics.bodies[i];

                // If the rigidbody isn't part of the simulation, ignore it.
                if (!rigid.simulated)
                {
                    return;
                }

                // If entity is outside the grid extends, ignore it.
                if (rigid.bounds.min.X < min.X || rigid.bounds.max.X > max.X
                    || rigid.bounds.min.Y < min.Y || rigid.bounds.max.Y > max.Y)
                {
                    continue;
                }

                // find extremes of cells that entity overlaps
                // subtract min to shift grid to avoid negative numbers
                int cXEntityMin = Mathf.FloorToInt((int)((rigid.bounds.min.X - min.X) / cellSize.X));
                int cXEntityMax = Mathf.FloorToInt((int)((rigid.bounds.max.X - min.X) / cellSize.X));

                int cYEntityMin = Mathf.FloorToInt((int)((rigid.bounds.min.Y - min.Y) / cellSize.Y));
                int cYEntityMax = Mathf.FloorToInt((int)((rigid.bounds.max.Y - min.Y) / cellSize.Y));

                // insert entity into each cell it overlaps
                // we're looping to make sure that all cells between extremes are found
                for (int cX = cXEntityMin; cX <= cXEntityMax; cX++)
                {
                    // make sure a column exists, initialize if not to grid height length
                    if (grid[cX] == null) {
                        grid[cX] = new List<FPRigidbody>[rows];
                    }

                    // loop through each cell in this column
                    for (int cY = (int)cYEntityMin; cY <= cYEntityMax; cY++)
                    {
                        // ensure we have a bucket to put entities into for this cell
                        if (grid[cX][cY] == null)
                        {
                            grid[cX][cY] = new List<FPRigidbody>();
                        }

                        grid[cX][cY].Add(rigid);
                    }
                }
            }

            //Get broad phase pairs.
            GetPairs();
        }

        private void GetPairs()
        {
            List<int> checkList = new List<int>();

            for(int i = 0; i < grid.GetLength(0); i++)
            {
                // ignore columns that have no cells
                if(grid[i] == null)
                {
                    continue;
                }

                for(int w = 0; w < grid[i].Length; w++)
                {
                    // ignore cells that have no objects
                    if (grid[i][w] == null)
                    {
                        continue;
                    }

                    // for every object in a cell...
                    for(int g = 0; g < grid[i][w].Count; g++)
                    {
                        FPRigidbody rigidA = grid[i][w][g];

                        // for every other object in a cell..
                        for (int k = g + 1; k < grid[i][w].Count; k++)
                        {
                            FPRigidbody rigidB = grid[i][w][k];

                            // create a unique key to mark this pair.
                            // use both combinations to ensure linear time
                            int hashA = rigidA.GetHashCode() + rigidB.GetHashCode() + 1;
                            int hashB = hashA + 1;

                            if (!checkList.Contains(hashA) && !checkList.Contains(hashB)){
                                checkList.Add(hashA);
                                checkList.Add(hashB);

                                // If both bodies are static, ignore them.
                                if (rigidA.invMass == 0 && rigidB.invMass == 0)
                                {
                                    continue;
                                }

                                if (CollisionChecks.AABBvsAABB(rigidA.coll, rigidB.coll))
                                {
                                    TDFPhysics.broadPhasePairs.Add(new Manifold(rigidA, rigidB));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}