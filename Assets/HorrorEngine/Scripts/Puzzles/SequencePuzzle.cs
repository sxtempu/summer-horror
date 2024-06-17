using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorEngine
{
    public class SequencePuzzle : PuzzleBase
    {
        public UnityEvent<string> OnEntryAdded;
        public UnityEvent OnIncorrectSolution;
        public UnityEvent OnCleared;
        [SerializeField] private string[] m_Solution;
        [Tooltip("The solution is sorted and the puzzle will only be solved if given in the define order")]
        [SerializeField] private bool m_RespectOrder = true;

        private List<string> m_Entries = new List<string>();

        private int m_EntriesCount;

        public void Clear()
        {
            m_EntriesCount = 0;
            m_Entries.Clear();
            OnCleared?.Invoke();
        }

        public void Add(string entry)
        {
            if (m_Solved)
                Debug.LogWarning("Puzzle is already solved but you're still adding entries");

            m_Entries.Add(entry);
            ++m_EntriesCount;

            OnEntryAdded?.Invoke(entry);

            if (m_Entries.Count > m_Solution.Length)
            {
                m_Entries.RemoveAt(0);
            }

            if (CheckSolution())
            {
                Solve();
            }
            else if (m_EntriesCount % m_Solution.Length == 0)
            {
                OnIncorrectSolution?.Invoke();
            }
        }

        private bool CheckSolution()
        {
            if (m_Entries.Count < m_Solution.Length)
                return false;

            if (m_RespectOrder)
            {
                for (int i = 0; i < m_Solution.Length; ++i)
                {
                    if (m_Entries[i] != m_Solution[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                List<string> remainingSolution = new List<string>(m_Solution);
                for (int i = 0; i < m_Entries.Count; ++i)
                {
                    remainingSolution.Remove(m_Entries[i]);
                }

                return remainingSolution.Count == 0;
            }

            return true;
        }
    }
}