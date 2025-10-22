using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewportSizeNotifier : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Vector2 _lastSize;

    public event Action<Vector2> OnViewportChanged;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (_rectTransform == null)
        {
            Awake();
        }

        Vector2 curSize = _rectTransform.rect.size;

        if (_lastSize == curSize)
        {
            return;
        }

        _lastSize = curSize;

        OnViewportChanged?.Invoke(curSize);
    }

    private void OnEnable()
    {
        if (_rectTransform != null)
        {
            _lastSize = _rectTransform.rect.size;
        }
    }

    public void NotifyCurrentSize()
    {  
        if (_rectTransform == null)
        {
            Awake();
        }

        _lastSize = _rectTransform.rect.size;

        OnViewportChanged?.Invoke(_lastSize);
    }
}
