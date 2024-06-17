using UnityEngine;

namespace HorrorEngine
{
    public class SequencePuzzleText : MonoBehaviour
    {
        [SerializeField] SequencePuzzle m_Puzzle;
        [SerializeField] TMPro.TextMeshPro m_Text;

        private void Start()
        {
            m_Text.text = "";

            m_Puzzle.OnEntryAdded.AddListener((entry) =>
            {
                m_Text.text += entry;
            });

            m_Puzzle.OnCleared.AddListener(() =>
            {
                m_Text.text = "";
            });
        }
    }
}