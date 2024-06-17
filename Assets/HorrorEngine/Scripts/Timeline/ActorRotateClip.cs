using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HorrorEngine
{
    [System.Serializable]
    public class ActorRotateClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField] ExposedReference<Transform> m_Target;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ActorRotateClipBehaviour>.Create(graph);

            ActorRotateClipBehaviour playableBehaviour = playable.GetBehaviour();
            playableBehaviour.Target = m_Target.Resolve(graph.GetResolver());

            return playable;
        }
    }

    [System.Serializable]
    public class ActorRotateClipBehaviour : ActorClipBehaviour
    {

        public Transform Target;

        private float m_Angle;
        private float m_RotationRate;
        private float m_Rotated;
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);

            if (!Application.isPlaying)
            {
                return;
            }

            double duration = playable.GetDuration();
            m_Angle = Vector3.SignedAngle(m_Actor.transform.forward, Target.forward, Vector3.up);
            m_RotationRate = m_Angle / (float)duration;
            m_Angle = Mathf.Abs(m_Angle);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);

            if (!Application.isPlaying)
            {
                return;
            }

            if (m_Rotated < m_Angle) 
            {
                float timedRate = m_RotationRate * Time.deltaTime;
                m_Actor.transform.Rotate(Vector3.up, timedRate, Space.World);
                m_Rotated += Mathf.Abs(timedRate);
                if (m_Rotated > m_Angle)
                {
                    m_Actor.transform.rotation = Target.rotation;
                }
            }
            
        }
    }
}