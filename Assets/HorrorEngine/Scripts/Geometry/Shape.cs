using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HorrorEngine
{
    public class Shape : MonoBehaviour
    {
        public Color GizmoColor = Color.white;
        [SerializeField] private bool m_DrawGizmo = true;

        public List<Vector3> Points = new List<Vector3>(){
            new Vector3(-1,0,-1),
            new Vector3(-1, 0,1),
            new Vector3(1, 0, 1),
            new Vector3(1, 0, -1)
            };

        public bool CloseShape = true;

        public Vector3 GetWorldPoint(int index)
        {
            return transform.TransformPoint(Points[index]);
        }

        public float GetLength()
        {
            float l = 0f;
            if (Points.Count > 1)
            {
                for (int i = 1; i < Points.Count; ++i)
                {
                    l += Vector3.Distance(GetWorldPoint(i-1), GetWorldPoint(i));
                }
            }
            return l;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (m_DrawGizmo)
            {
                Handles.color = GizmoColor;
                for (int i = 0; i < Points.Count; i++)
                {
                    Vector3 position = transform.TransformPoint(Points[i]);

                    if (i > 0)
                    {
                        Vector3 prevPosition = transform.TransformPoint(Points[i - 1]);
                        Handles.DrawLine(position, prevPosition);
                    }
                }

                if (Points.Count >= 3 && CloseShape)
                {
                    Vector3 p1 = transform.TransformPoint(Points[Points.Count - 1]);
                    Vector3 p2 = transform.TransformPoint(Points[0]);
                    Handles.DrawLine(p1, p2);
                }
            }
        }
#endif

    }
}