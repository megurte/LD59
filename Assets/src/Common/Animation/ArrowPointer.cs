using System;
using DG.Tweening;
using UnityEngine;

namespace Common.Animation
{
    public class ArrowPointer : MonoBehaviour
    {
        [Serializable]
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
        
        [SerializeField] private float moveDistance = 0.2f; 
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Direction direction;

        private Tween _tween;
        
        private void OnEnable()
        {
           var startPos = transform.localPosition;
           var dir = GetDirection();
           var targetPos = startPos + (Vector3)(dir * moveDistance);

            _tween = transform.DOLocalMove(targetPos, duration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private Vector2 GetDirection()
        {
            return direction switch
            {
                Direction.Up => Vector2.up,
                Direction.Down => Vector2.down,
                Direction.Left => Vector2.left,
                Direction.Right => Vector2.right,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private void OnDisable()
        {
            _tween.Kill();
            _tween = null;
        }
    }
}