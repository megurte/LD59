using System;
using UnityEngine;

namespace GlobalSpace
{
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField] private float secondsToDie;
        private bool _destroyScheduled;

        private void OnEnable()
        {
            TryScheduleDestroy();
        }

        public void SetLifetime(float seconds)
        {
            secondsToDie = seconds;
            TryScheduleDestroy();
        }

        private void TryScheduleDestroy()
        {
            if (_destroyScheduled || !isActiveAndEnabled || secondsToDie <= 0f)
            {
                return;
            }

            _destroyScheduled = true;
            Destroy(gameObject, secondsToDie);
        }
    }
}
