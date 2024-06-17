using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HorrorEngine
{
   
    [System.Serializable]
    public class ActorMoveClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField] ExposedReference<Shape> m_Path;
        [SerializeField] bool m_TeleportToStart;
        [SerializeField] float m_RotationRate = 500;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ActorMoveClipBehaviour>.Create(graph);

            ActorMoveClipBehaviour playableBehaviour = playable.GetBehaviour();
            playableBehaviour.Path = m_Path.Resolve(graph.GetResolver());
            playableBehaviour.TeleportToStart = m_TeleportToStart;
            playableBehaviour.RotationRate = m_RotationRate;
            return playable;
        }
    }

    [System.Serializable]
    public class ActorMoveClipBehaviour : ActorClipBehaviour
    {
        public Shape Path;
        public bool TeleportToStart;
        public float RotationRate;

        private int m_CurrentIndex;
        private float m_Speed;
        private bool m_ReachedEnd;
        private float m_LastDist;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);

            if (!Application.isPlaying)
            {
                return;
            }

            double duration = playable.GetDuration();
            float shapeLength = Path.GetLength();
            float distToPoint0 = Vector3.Distance(m_Actor.transform.position, Path.GetWorldPoint(0));

            m_CurrentIndex = 0;
            Vector3 initPoint = Path.GetWorldPoint(m_CurrentIndex);
            m_ReachedEnd = false;
            m_LastDist = Vector3.Distance(m_Actor.transform.position, initPoint);

            if (TeleportToStart)
            {
                m_Actor.transform.position = initPoint;
                distToPoint0 = 0;
            }

            m_Speed = (shapeLength + distToPoint0) / Mathf.Max((float)duration, Mathf.Epsilon);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);

            if (!Application.isPlaying)
            {
                return;
            }

            if (m_ReachedEnd)
            {
                return;
            }

            float stopDistance = 0.5f;
            
            Vector3 point = Path.GetWorldPoint(m_CurrentIndex);
            Vector3 dirToPoint = (point - m_Actor.transform.position).normalized;

            if (dirToPoint != Vector3.zero)
                m_Actor.transform.rotation = Quaternion.RotateTowards(m_Actor.transform.rotation, Quaternion.LookRotation(dirToPoint, Vector3.up), RotationRate * Time.deltaTime);

            m_Actor.transform.position += dirToPoint * Time.deltaTime * m_Speed;
            
            float dist = Vector3.Distance(m_Actor.transform.position, point);
            if (dist < stopDistance || dist > m_LastDist)
            {
                if (m_CurrentIndex == Path.Points.Count - 1)
                {
                    m_Actor.transform.position = Path.GetWorldPoint(m_CurrentIndex);
                    m_ReachedEnd = true;
                }
                else
                {
                    m_CurrentIndex = Mathf.Clamp(m_CurrentIndex + 1, 0, Path.Points.Count - 1);
                    m_LastDist = Vector3.Distance(m_Actor.transform.position, Path.GetWorldPoint(m_CurrentIndex));
                }
            }
            else
            {
                m_LastDist = dist;
            }
        }
    }
}