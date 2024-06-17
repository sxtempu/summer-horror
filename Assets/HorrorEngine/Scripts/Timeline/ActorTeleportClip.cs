using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HorrorEngine
{
   
    [System.Serializable]
    public class ActorTeleportClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField] ExposedReference<Transform> m_TeleportPoint;
        

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ActorTeleportClipBehaviour>.Create(graph);

            ActorTeleportClipBehaviour playableBehaviour = playable.GetBehaviour();
            playableBehaviour.TeleportPoint = m_TeleportPoint.Resolve(graph.GetResolver());
            
            return playable;
        }
    }

    [System.Serializable]
    public class ActorTeleportClipBehaviour : ActorClipBehaviour
    {
        public Transform TeleportPoint;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);

            if (!Application.isPlaying)
            {
                return;
            }

            m_Actor.transform.position = TeleportPoint.position;
            m_Actor.transform.rotation = TeleportPoint.rotation;
        }
    }
}