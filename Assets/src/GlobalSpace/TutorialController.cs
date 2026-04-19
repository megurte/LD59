using System;
using UnityEngine;

namespace GlobalSpace
{
    public class TutorialController : MonoBehaviour
    {
        private void Awake()
        {
            Global.TutorialController = this;
        }
    }
}