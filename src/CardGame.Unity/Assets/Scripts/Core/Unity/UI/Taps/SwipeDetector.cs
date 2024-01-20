﻿using Core.Unity.Scripts;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Unity.UI.Taps
{
    public class SwipeDetector : UnityScript
    {
        [SerializeField] private float _maxSwipeTime = 1f;
        [SerializeField] private float _minSwipeDistance = 50f;

        private long _tapStartTime;
        private Vector2 _drag;

        public event Action OnDetected;

        private Vector2 _lastMousePos;
        private RectTransform _rt;

        private void Awake()
        {
            _rt = this.RT();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && Input.mousePosition.IsInRect(_rt))
                OnPointerDown(null);

            if (_tapStartTime == 0)
                return;

            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                _drag += Input.GetTouch(0).deltaPosition;
            }
            else
            {
                _drag += (Vector2) Input.mousePosition - _lastMousePos;
                _lastMousePos = Input.mousePosition;
            }

            if (DateTime.UtcNow.Ticks <= (_tapStartTime + (long) (_maxSwipeTime * TimeSpan.TicksPerSecond)) &&
                _drag.magnitude >= _minSwipeDistance)
            {
                _tapStartTime = 0;
                OnDetected?.Invoke();
            }

            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended ||
                Input.GetMouseButtonUp(0))
            {
                _tapStartTime = 0;
                _drag = Vector2.zero;
                _lastMousePos = Vector2.zero;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _tapStartTime = DateTime.UtcNow.Ticks;
            _lastMousePos = Input.mousePosition;
            _drag = Vector2.zero;
        }
    }
}
