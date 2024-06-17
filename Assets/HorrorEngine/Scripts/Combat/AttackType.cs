using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
    [System.Serializable]
    public class AttackImpact
    {
        public float Damage;
        public DamageableType[] Damageable;
        public AttackFilter[] Filters;
        public AttackEffect[] PreDamageEffects;
        public AttackEffect[] PostDamageEffects;
    }

    [CreateAssetMenu(menuName = "Horror Engine/Combat/AttackType")]
    public class AttackType : ScriptableObject
    {
        public AttackImpact[] Impacts;

        [SerializeField, HideInInspector] Dictionary<DamageableType, AttackImpact> m_HashedImpacts = new Dictionary<DamageableType, AttackImpact>();

        private void OnEnable()
        {
            m_HashedImpacts.Clear();
            foreach (var impact in Impacts)
            {
                foreach (var type in impact.Damageable)
                {
                    if (type)
                    {
                        Debug.Assert(!m_HashedImpacts.ContainsKey(type), $"AttackType has a duplicated Damageable entry for {type.name}");
                        m_HashedImpacts.Add(type, impact);
                    }
                }
            }
        }

        public AttackImpact GetImpact(DamageableType damageable)
        {
            if (m_HashedImpacts.ContainsKey(damageable))
                return m_HashedImpacts[damageable];
            else
                return null;
        }
    }
}