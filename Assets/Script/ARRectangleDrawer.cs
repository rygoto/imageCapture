using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public class ARRectangleDrawer : MonoBehaviour
{
    public ARCameraManager arCameraManager;
    public RawImage displayImage; // This is for displaying the captured texture
    public RectTransform selectionRect;

    private Texture2D cameraTexture;
    private Vector2 startDragPosition;
    private bool isDragging = false;

    void Update()
    {
        // Start Dragging
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            startDragPosition = Input.mousePosition;
        }

        // While Dragging, update the UI rectangle
        if (isDragging)
        {
            UpdateSelectionRect(startDragPosition, Input.mousePosition);
        }

        // End Dragging and Capture
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            CaptureSelectedArea();
        }
    }

    void UpdateSelectionRect(Vector2 start, Vector2 end)
    {
        Vector2 min = Vector2.Min(start, end);
        Vector2 max = Vector2.Max(start, end);
        selectionRect.position = min;
        selectionRect.sizeDelta = max - min;

        selectionRect.pivot = new Vector2(0, 0);
        // TODO: Set the position and size of your selectionRect based on the start and end positions
    }

    void CaptureSelectedArea()
    {
        Debug.Log("CaptureSelectedArea called!");

        try
        {
            XRCpuImage cpuImage;
            if (arCameraManager.TryAcquireLatestCpuImage(out cpuImage))
            {
                Debug.Log($"CPU Image size: {cpuImage.width} x {cpuImage.height}");

                cameraTexture = new Texture2D(cpuImage.width, cpuImage.height, TextureFormat.RGBA32, false);
                cpuImage.Convert(new XRCpuImage.ConversionParams(cpuImage, TextureFormat.RGBA32, XRCpuImage.Transformation.None), cameraTexture.GetRawTextureData<byte>());


                //cpuImage.Convert(new XRCpuImage.ConversionParams(cpuImage.rect, TextureFormat.RGBA32, XRCpuImage.Transformation.None), cameraTexture.GetRawTextureData<byte>());
                cameraTexture.Apply();

                Debug.Log($"Converted Texture size: {cameraTexture.width} x {cameraTexture.height}");

                Rect textureRect = new Rect(
                    (selectionRect.position.x / Screen.width) * cameraTexture.width,
                    (selectionRect.position.y / Screen.height) * cameraTexture.height,
                    (selectionRect.sizeDelta.x / Screen.width) * cameraTexture.width,
                    (selectionRect.sizeDelta.y / Screen.height) * cameraTexture.height
                );

                Debug.Log($"Selected Rect position: {textureRect.position} - size: {textureRect.size}");

                // Now crop the texture based on your selectionRect
                Texture2D croppedTexture = CropTexture(cameraTexture, textureRect);/* TODO: your cropping parameters based on selectionRect */

                displayImage.texture = croppedTexture; // Display or use the cropped texture as needed

                Debug.Log("Cropped texture set to displayImage.");

                cpuImage.Dispose();
            }
            else
            {
                Debug.Log("Failed to acquire CPU Image.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in CaptureSelectedArea: {e.Message}\n{e.StackTrace}");
        }
    }

    Texture2D CropTexture(Texture2D source, Rect cropRect)
    {
        Color[] c = source.GetPixels((int)cropRect.x, (int)cropRect.y, (int)cropRect.width, (int)cropRect.height);
        Texture2D result = new Texture2D((int)cropRect.width, (int)cropRect.height);
        result.SetPixels(c);
        result.Apply();

        Debug.Log($"Cropped Texture size: {result.width} x {result.height}");

        return result;
        // TODO: Implement the cropping logic to extract the section of the source texture defined by cropRect
    }
}
