using System.Collections;
using UnityEngine;

namespace HorrorEngine
{
    public class UILetterBox : MonoBehaviour
    {
        [SerializeField] Transform Top;
        [SerializeField] Transform Bottom;
        [SerializeField] Transform TopGoal;
        [SerializeField] Transform BottomGoal;
        [SerializeField] AnimationCurve m_ShowInterpolation;
        [SerializeField] AnimationCurve m_HideInterpolation;

        private Vector3 m_InitTopPos;
        private Vector3 m_InitBottomPos;
        private Vector3 m_GoalTopPos;
        private Vector3 m_GoalBottomPos;

        private void Awake()
        {
            m_InitTopPos = Top.position;
            m_InitBottomPos = Bottom.position;

            m_GoalTopPos = TopGoal.position;
            m_GoalBottomPos = BottomGoal.position;
        }

        public void SetProgress(float progress,  bool visible)
        {
            float t = visible ? m_ShowInterpolation.Evaluate(progress) : m_HideInterpolation.Evaluate(progress);

            Vector3 topPos = Vector3.Lerp(m_InitTopPos, m_GoalTopPos, t);
            Vector3 bottomPos = Vector3.Lerp(m_InitBottomPos, m_GoalBottomPos, t);

            Top.transform.position = topPos;
            Bottom.transform.position = bottomPos;
        }

        public void SetVisible(bool visible, float duration  =1)
        {
            StartCoroutine(Interpolate(visible, duration));
        }

        IEnumerator Interpolate(bool visible, float duration)
        {
            float from = visible ? 0f : 1f;
            float to = visible ? 1f : 0f;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                SetProgress(Mathf.Lerp(from, to, t / duration), visible);
                yield return Yielders.EndOfFrame;
            }

            SetProgress(to, visible);
        }
        
    }
}