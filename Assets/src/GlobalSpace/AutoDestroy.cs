using System;
using UnityEngine;

namespace GlobalSpace
{
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField] private float secondsToDie;

        private void OnEnable()
        {
            Destroy(gameObject, secondsToDie);
        }
    }
}