using System.Collections.Generic;
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
        [SerializeField] private Vector2 milestoneMarkerSize = new(4f, 18f);
        [SerializeField] private Color milestoneMarkerColor = new(0.525f, 0.73f, 0.86f, 0.9f);

        private readonly List<RectTransform> _milestoneMarkers = new();

        private void Update()
        {
            if (submarineMovement == null || routeTrack == null || routePoint == null)
            {
                return;
            }

            if (fuelFill != null)
            {
                fuelFill.fillAmount = submarineMovement.FuelNormalized;
            }

            RefreshMilestoneMarkers();

            var progress = submarineMovement.RouteProgressNormalized;
            var x = GetTrackPosition(progress, routePoint.rect.width);
            routePoint.anchoredPosition = new Vector2(x, routePoint.anchoredPosition.y);
        }

        private void RefreshMilestoneMarkers()
        {
            var milestoneCount = submarineMovement.MilestoneCount;
            while (_milestoneMarkers.Count < milestoneCount)
            {
                _milestoneMarkers.Add(CreateMilestoneMarker(_milestoneMarkers.Count));
            }

            while (_milestoneMarkers.Count > milestoneCount)
            {
                var marker = _milestoneMarkers[_milestoneMarkers.Count - 1];
                if (marker != null)
                {
                    Destroy(marker.gameObject);
                }

                _milestoneMarkers.RemoveAt(_milestoneMarkers.Count - 1);
            }

            for (var i = 0; i < _milestoneMarkers.Count; i++)
            {
                var marker = _milestoneMarkers[i];
                if (marker == null)
                {
                    continue;
                }

                marker.sizeDelta = milestoneMarkerSize;
                marker.anchorMin = new Vector2(0f, 0.5f);
                marker.anchorMax = new Vector2(0f, 0.5f);
                marker.pivot = new Vector2(0.5f, 0.5f);

                var markerImage = marker.GetComponent<Image>();
                if (markerImage != null)
                {
                    markerImage.color = milestoneMarkerColor;
                    markerImage.raycastTarget = false;
                }

                var progress = submarineMovement.GetMilestoneProgressNormalized(i);
                var x = GetTrackPosition(progress, milestoneMarkerSize.x);
                marker.anchoredPosition = new Vector2(x, 0f);
            }
        }

        private RectTransform CreateMilestoneMarker(int milestoneIndex)
        {
            var markerObject = new GameObject($"Milestone_{milestoneIndex}", typeof(RectTransform), typeof(Image));
            markerObject.transform.SetParent(routeTrack, false);

            var markerTransform = markerObject.GetComponent<RectTransform>();
            markerTransform.anchorMin = new Vector2(0f, 0.5f);
            markerTransform.anchorMax = new Vector2(0f, 0.5f);
            markerTransform.pivot = new Vector2(0.5f, 0.5f);
            markerTransform.sizeDelta = milestoneMarkerSize;

            if (routePoint != null && routePoint.parent == routeTrack)
            {
                markerTransform.SetSiblingIndex(routePoint.GetSiblingIndex());
            }
            else
            {
                markerTransform.SetAsLastSibling();
            }

            return markerTransform;
        }

        private float GetTrackPosition(float progress, float elementWidth)
        {
            var padding = Mathf.Max(0f, elementWidth * 0.5f);
            var maxX = Mathf.Max(padding, routeTrack.rect.width - padding);
            return Mathf.Lerp(padding, maxX, Mathf.Clamp01(progress));
        }
    }
}
