using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FixedPointy;

namespace TDFP.Core.Editor
{
    public class DynamicTreeViewerEditor : EditorWindow
    {
        static TDFPPhysicsScene scene;
        static DynamicTree tree;

        public static void OpenWindow(TDFPPhysicsScene pScene)
        {
            scene = pScene;
            tree = scene.dynamicTree;
            DynamicTreeViewerEditor window = GetWindow<DynamicTreeViewerEditor>();
            window.titleContent = new GUIContent("Dynamic Tree");
        }

        private void OnGUI()
        {
            Rect aRect = new Rect(Screen.width/2.0f, 20, 50, 50);
            GUI.Box(aRect, "Root");
            DrawNodes(tree.rootIndex, aRect, 1);

            if (GUI.changed)
            {
                Repaint();
            }
        }

        private void DrawNodes(int nodeIndex, Rect pos, int height)
        {
            if(nodeIndex == -1)
            {
                return;
            }

            DTNode dt = tree.nodes[nodeIndex];
            Rect leftChildRect = new Rect(pos.x - (150.0f/(float)height), pos.y + 100, pos.width, pos.height);
            Rect rightChildRect = new Rect(pos.x + (150.0f/(float)height), pos.y + 100, pos.width, pos.height);

            if (dt.leftChildIndex != -1)
            {
                if (tree.nodes[dt.leftChildIndex].IsLeaf())
                {
                    if (GUI.Button(leftChildRect, ""))
                    {
                        Selection.activeGameObject = scene.bodies[tree.nodes[dt.leftChildIndex].bodyIndex].gameObject;
                    }
                }
                GUI.Box(leftChildRect, tree.nodes[dt.leftChildIndex].IsLeaf() ? "L" : "N");
            }
            if(dt.rightChildIndex != -1)
            {
                if (tree.nodes[dt.rightChildIndex].IsLeaf())
                {
                    if (GUI.Button(rightChildRect, ""))
                    {
                        Selection.activeGameObject = scene.bodies[tree.nodes[dt.rightChildIndex].bodyIndex].gameObject;
                    }
                }
                GUI.Box(rightChildRect, tree.nodes[dt.rightChildIndex].IsLeaf() ? "R" : "N");
            }

            if (!dt.IsLeaf())
            {
                DrawNodes(dt.leftChildIndex, leftChildRect, height+1);
                DrawNodes(dt.rightChildIndex, rightChildRect, height+1);
            }
        }
    }
}