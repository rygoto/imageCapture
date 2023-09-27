using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using UnityEngine.XR.ARCore;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class ARCapture : MonoBehaviour
{
    public ARCameraBackground arCamBg;
    public RawImage displayImage;
    public RectTransform selectionRect;
    public RenderTexture capturedTex;
    public Camera arCamera;

    private Vector2 startDragPosition;
    private bool isDragging = false;

    private TextureCropper textureCropper;

    public Texture2D imageToSend;
    public TMP_Text responseText;


    private void Start()
    {
        capturedTex = new RenderTexture(Screen.width, Screen.height, 24);
        //Camera.main.targetTexture = capturedTex;
        textureCropper = new TextureCropper();
    }


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
            StartCapture();
            SaveTex();
            EndCapture();
            Texture2D croppedTexture = textureCropper.CropTexture(capturedTex, GetScreenSpaceSelectionRect());
            displayImage.texture = croppedTexture;
            StartCoroutine(UploadTexture(croppedTexture));
        }
    }

    void StartCapture()
    {
        arCamera.targetTexture = capturedTex;
    }

    void EndCapture()
    {
        arCamera.targetTexture = null;
    }

    void UpdateSelectionRect(Vector2 start, Vector2 end)
    {
        selectionRect.pivot = new Vector2(0, 0);
        Vector2 min = Vector2.Min(start, end);
        Vector2 max = Vector2.Max(start, end);
        selectionRect.position = min;
        selectionRect.sizeDelta = max - min;


    }

    public void SaveTex()
    {
        if (arCamBg.material != null)
        {
            Graphics.Blit(null, capturedTex, arCamBg.material);
        }
    }

    Rect GetScreenSpaceSelectionRect()
    {
        return new Rect(
            selectionRect.anchoredPosition.x, //- selectionRect.sizeDelta.x * 0.5f,
            selectionRect.anchoredPosition.y, //- selectionRect.sizeDelta.y * 0.5f,
            selectionRect.sizeDelta.x,
            selectionRect.sizeDelta.y
        );
    }

    public class TextureCropper
    {
        public Texture2D CropTexture(RenderTexture source, Rect screenSpaceCropRect)
        {
            Rect textureRect = new Rect(
                screenSpaceCropRect.x / Screen.width * source.width,
                screenSpaceCropRect.y / Screen.height * source.height,
                screenSpaceCropRect.width / Screen.width * source.width,
                screenSpaceCropRect.height / Screen.height * source.height
            );

            Texture2D result = new Texture2D((int)textureRect.width, (int)textureRect.height, TextureFormat.RGBA32, false);
            RenderTexture.active = source;
            result.ReadPixels(textureRect, 0, 0);
            result.Apply();
            RenderTexture.active = null;
            return result;
        }
    }

    IEnumerator UploadTexture(Texture2D texture)
    {
        byte[] textureBytes = texture.EncodeToPNG();
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", textureBytes, "image.png", "image/png");

        using (UnityWebRequest www = UnityWebRequest.Post("https://us-central1-text2imagear.cloudfunctions.net/function-1", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                responseText.text = www.downloadHandler.text;
                Debug.Log("Upload Successful:" + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Upload Failed:" + www.error);
            }
        }
    }
}

