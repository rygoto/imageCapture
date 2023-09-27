using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DrawOnARUpdate : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Camera arCamera;
    public RenderTexture renderTexture;

    private List<Vector3> linePositions = new List<Vector3>();
    private bool isDrawing = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrawing();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopDrawing();
        }

        if (isDrawing)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;

            Vector3 worldPos = arCamera.ScreenToWorldPoint(mousePos);

            if (linePositions.Count == 0 || Vector3.Distance(worldPos, linePositions[linePositions.Count - 1]) > 0.1f)
            {
                linePositions.Add(worldPos);
                lineRenderer.positionCount = linePositions.Count;
                lineRenderer.SetPositions(linePositions.ToArray());
            }
        }
    }

    private void StartDrawing()
    {
        isDrawing = true;
        linePositions.Clear(); // 清空之前的线段
        lineRenderer.positionCount = 0;
    }

    private void StopDrawing()
    {
        isDrawing = false;
        CaptureImage();
    }

    private void CaptureImage()
    {
        Rect captureRect = CalculateCaptureRect();
        int width = (int)captureRect.width;
        int height = (int)captureRect.height;

        if (width > 0 && height > 0)
        {
            Texture2D screenshot = new Texture2D((int)captureRect.width, (int)captureRect.height, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            screenshot.ReadPixels(captureRect, 0, 0);
            screenshot.Apply();
            RenderTexture.active = null;
        }
        {
            Debug.LogError("CaptureRect has invalid dimensions.");
        }
        // ここでscreenshotを他の用途に使用できます。
    }

    private Rect CalculateCaptureRect()
    {
        // ドラッグした範囲を計算するためのロジックを追加してください。
        // 例えば、ドラッグ開始地点と終了地点から矩形を計算できます。
        // Rectのx, y, width, heightを設定し、戻り値として返します。

        // ここでは仮の値を設定します。
        Vector3 start = lineRenderer.GetPosition(0);
        Vector3 end = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
        float minX = Mathf.Min(start.x, end.x);
        float minY = Mathf.Min(start.y, end.y);
        float maxX = Mathf.Max(start.x, end.x);
        float maxY = Mathf.Max(start.y, end.y);

        Rect captureRect = new Rect(minX, minY, maxX - minX, maxY - minY);
        return captureRect;
    }
}
