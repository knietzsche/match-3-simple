using System;
using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private static readonly float durationAnimation = .5f;

    private Vector3 localPosition;
    private Coroutine coroutine;

    private void Start()
    {
        localPosition = transform.localPosition;
    }

    private void OnMouseDown()
    {
        if (coroutine == null)
        {
            GameManager.mouseDown?.Invoke(this);
        }
    }

    public void SetPosition(Vector3 localPosition)
    {
        if (this.localPosition == localPosition)
        {
            return;
        }

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            CleanUp();
        }

        this.localPosition = localPosition;
        coroutine = StartCoroutine(MoveToPosition());
    }

    private IEnumerator MoveToPosition()
    {
        var localPositionStart = transform.localPosition;
        var localPositionDifference = localPosition - localPositionStart;

        var start = DateTime.Now;
        var progress = 0f;
        while (progress < 1f)
        {
            var duration = DateTime.Now - start;
            progress = (float)duration.TotalSeconds / durationAnimation;
            var value = Mathf.SmoothStep(0f, 1f, progress);

            transform.localPosition = localPositionStart + localPositionDifference * value;

            yield return new WaitForEndOfFrame();
        }

        CleanUp();
    }

    private void CleanUp()
    {
        transform.localPosition = localPosition;
        coroutine = null;
    }
}
