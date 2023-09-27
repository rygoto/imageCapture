using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DrawOnARfixed : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Camera arCamera;

    public Material targetMaterial;

    public RenderTexture renderTexture;
    private List<Vector3> linePositions = new List<Vector3>();
    private List<Vector2Int> pixelPositions = new List<Vector2Int>();

    private Renderer planeRenderer;
    private Material planeMaterial;
    private bool isDrawing = false;

    private void Start()
    {
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        arCamera.targetTexture = renderTexture;
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
            isDrawing = true; // 描画が終了したらフラグをセットしてOnPostRenderで処理を行う
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

    private void OnPostRender()
    {
        if (isDrawing)
        {
            Texture2D capturedTexture = new Texture2D(renderTexture.width, renderTexture.height);
            RenderTexture.active = renderTexture;
            capturedTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            capturedTexture.Apply();
            RenderTexture.active = null;

            // 描画終了時に線の中にあるピクセルをキャプチャ
            Texture2D screenshot = new Texture2D(Screen.width, Screen.height);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            List<Color> capturedColors = new List<Color>();
            foreach (Vector2Int pixelPos in pixelPositions)
            {
                capturedColors.Add(screenshot.GetPixel(pixelPos.x, pixelPos.y));
            }

            foreach (Color color in capturedColors)
            {
                Debug.Log("Pixel Color: " + color);
            }
            // capturedColorsには線の中にあるピクセルの色情報が含まれる

            // ここでcapturedColorsを他の用途に使用できます

            Texture2D resultTexture = new Texture2D(Screen.width, Screen.height);
            resultTexture.SetPixels(capturedColors.ToArray());
            resultTexture.Apply();

            if (targetMaterial != null)
            {
                targetMaterial.mainTexture = capturedTexture;
            }

            //planeMaterial.mainTexture = resultTexture;

            isDrawing = false;
        }
    }

    private Vector2Int WorldToPixelCoordinates(Vector3 worldPos)
    {
        Vector3 screenPos = arCamera.WorldToScreenPoint(worldPos);
        return new Vector2Int(Mathf.RoundToInt(screenPos.x), Mathf.RoundToInt(screenPos.y));
    }
}
