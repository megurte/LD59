using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public class RaycastInverter : Mask
    {
        public override bool IsRaycastLocationValid(Vector2 sp, UnityEngine.Camera eventCamera)
        {
            return !base.IsRaycastLocationValid(sp, eventCamera);
        }
    }
}