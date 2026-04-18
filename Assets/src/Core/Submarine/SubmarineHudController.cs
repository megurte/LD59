using UnityEngine;
using UnityEngine.UI;

namespace Core.Submarine
{
    public class SubmarineHudController : MonoBehaviour
    {
        [SerializeField] private SubmarineMovementController submarineMovement;
        [SerializeField] private Image fuelFill;
        [SerializeField] private RectTransform routeTrack;
        [SerializeField] private RectTransform routePoint;

        private void Update()
        {
            fuelFill.fillAmount = submarineMovement.FuelNormalized;

            var progress = submarineMovement.RouteProgressNormalized;
            var padding = routePoint.rect.width * 0.5f;
            var x = Mathf.Lerp(padding, routeTrack.rect.width - padding, progress);
            routePoint.anchoredPosition = new Vector2(x, routePoint.anchoredPosition.y);
        }
    }
}
