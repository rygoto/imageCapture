using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DrawOnARDraw : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Camera arCamera;
    private List<Vector3> linePositions = new List<Vector3>();
    private List<Vector2Int> pixelPositions = new List<Vector2Int>();

    private Renderer planeRenderer;
    private Material planeMaterial;

    private void Start()
    {
        planeRenderer = GetComponent<Renderer>();
        planeMaterial = planeRenderer.material;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrawing();
        }
        else if (Input.GetMouseButton(0))
        {
            ContinueDrawing();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopDrawing();
        }
    }

    private void StartDrawing()
    {
        // 線を初期化
        linePositions.Clear();
        pixelPositions.Clear();
        lineRenderer.positionCount = 0;

        // 最初のポイントを追加
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10; // 距離を設定
        Vector3 worldPos = arCamera.ScreenToWorldPoint(mousePos);
        linePositions.Add(worldPos);

        // ピクセル座標に変換して追加
        pixelPositions.Add(WorldToPixelCoordinates(worldPos));
    }

    private void ContinueDrawing()
    {
        // マウスがドラッグされたらポイントを追加
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10; // 距離を設定
        Vector3 worldPos = arCamera.ScreenToWorldPoint(mousePos);

        // 一定の距離ごとにポイントを追加
        if (Vector3.Distance(worldPos, linePositions[linePositions.Count - 1]) > 0.1f)
        {
            linePositions.Add(worldPos);
            pixelPositions.Add(WorldToPixelCoordinates(worldPos));
        }

        // LineRendererにポイントを設定
        lineRenderer.positionCount = linePositions.Count;
        lineRenderer.SetPositions(linePositions.ToArray());
    }

    private void StopDrawing()
    {
        // 描画終了時に線の中にあるピクセルをキャプチャ
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        Texture2D resultTexture = new Texture2D(Screen.width, Screen.height);
        List<Color> capturedColors = new List<Color>();
        foreach (Vector2Int pixelPos in pixelPositions)
        {
            if (pixelPos.x >= 0 && pixelPos.x < resultTexture.width && pixelPos.y >= 0 && pixelPos.y < resultTexture.height)
            {
                capturedColors.Add(screenshot.GetPixel(pixelPos.x, pixelPos.y));
            }
        }

        // capturedColorsには線の中にあるピクセルの色情報が含まれる

        // ここでcapturedColorsを他の用途に使用できます


        resultTexture.SetPixels(capturedColors.ToArray());
        resultTexture.Apply();

        planeMaterial.mainTexture = resultTexture;
    }
    private Vector2Int WorldToPixelCoordinates(Vector3 worldPos)
    {
        Vector3 screenPos = arCamera.WorldToScreenPoint(worldPos);
        return new Vector2Int(Mathf.RoundToInt(screenPos.x), Mathf.RoundToInt(screenPos.y));
    }


}