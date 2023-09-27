using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class Draw : MonoBehaviour
{
    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Vector2 dragStartPosition;
    private Vector2 dragEndPosition;
    private bool isDragging = false;
    public LineRenderer lineRenderer;

    private Camera mainCamera;

    private Rect viewportRect;


    // Start is called before the first frame update
    private void Start()
    {
        mainCamera = Camera.main;
        raycastManager = GetComponent<ARRaycastManager>();
        lineRenderer.positionCount = 0;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartPosition = Input.mousePosition;
            lineRenderer.positionCount = 0;
        }

        if (isDragging)
        {
            dragEndPosition = Input.mousePosition;

            Rect viewportRect = new Rect(
                Mathf.Min(dragStartPosition.x, dragEndPosition.x) / Screen.width,
                Mathf.Min(dragStartPosition.y, dragEndPosition.y) / Screen.height,
                Mathf.Abs(dragStartPosition.x - dragEndPosition.x) / Screen.width,
                Mathf.Abs(dragStartPosition.y - dragEndPosition.y) / Screen.height
            );

            DrawLine(viewportRect);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            lineRenderer.positionCount = 0;

            CaptureImage(viewportRect);

            //ここでAIモデルに送信
        }

    }

    private void DrawLine(Rect viewportRect)
    {
        lineRenderer.positionCount = 5;
        Vector3[] positions = new Vector3[5];
        positions[0] = Camera.main.ViewportToWorldPoint(new Vector3(viewportRect.x, viewportRect.y, 1));
        positions[1] = Camera.main.ViewportToWorldPoint(new Vector3(viewportRect.x, viewportRect.y + viewportRect.height, 1));
        positions[2] = Camera.main.ViewportToWorldPoint(new Vector3(viewportRect.x + viewportRect.width, viewportRect.y + viewportRect.height, 1));
        positions[3] = Camera.main.ViewportToWorldPoint(new Vector3(viewportRect.x + viewportRect.width, viewportRect.y, 1));
        positions[4] = Camera.main.ViewportToWorldPoint(new Vector3(viewportRect.x, viewportRect.y, 1));

        lineRenderer.SetPositions(positions);
    }

    private void CaptureImage(Rect viewportRect)
    {
        Texture2D capturedTextrue = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        Camera camera = GetComponent<Camera>();

        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;
        capturedTextrue.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        capturedTextrue.Apply();

        int x = (int)(viewportRect.x * Screen.width);
        int y = (int)(viewportRect.y * Screen.height);
        int width = (int)(viewportRect.width * Screen.width);
        int height = (int)(viewportRect.height * Screen.height);
        Color[] pixels = capturedTextrue.GetPixels(x, y, width, height);

    }
}
