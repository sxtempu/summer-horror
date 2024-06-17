using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorEngine
{
    public abstract class Pickup : MonoBehaviour
    {
        public UnityEvent OnPickup;
        
        public virtual void Take() 
        { 
            OnPickup?.Invoke();
        }
    }
}