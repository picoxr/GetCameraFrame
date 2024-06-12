using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.PICO.TOBSupport;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GetCameraFrame : MonoBehaviour
{
    public RawImage rawImageAntiDistortion;
    public TMPro.TextMeshProUGUI UIText;

    private Texture2D texture2D { get; set; }

    // Maximum Width/Height is 2328x1748

    private int ImageWidth = 1164;
    private int ImageHeight = 874;

    public InputActionReference captureAction;

    private void Start()
    {
        PXR_Boundary.EnableSeeThroughManual(true);          // Activate Seethrough

        PXR_Enterprise.InitEnterpriseService();         
        PXR_Enterprise.BindEnterpriseService();         

        texture2D = new Texture2D(ImageWidth, ImageHeight, TextureFormat.RGB24, false, false);

        OpenVSTCamera();

        captureAction.action.Enable();
        captureAction.action.performed += AcquireVSTCameraFrameAntiDistortionIntPtr;
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            PXR_Boundary.EnableSeeThroughManual(true);
        }
    }

    private void OnApplicationQuit()
    {
        CloseVSTCamera();
    }

    private void OnDisable()
    {
        texture2D = null;
        PXR_Enterprise.CloseVSTCamera();
    }

    public void OpenVSTCamera()
    {
        bool result = PXR_Enterprise.OpenVSTCamera();
        Debug.Log("Open VST Camera" + result);
    }

    public void CloseVSTCamera()
    {
        bool result = PXR_Enterprise.CloseVSTCamera();
        Debug.Log("Close VST Camera" + result);
    }

    public void AcquireVSTCameraFrameAntiDistortionIntPtr(InputAction.CallbackContext ctxt)
    {
        try
        {
            PXR_Enterprise.AcquireVSTCameraFrameAntiDistortion(ImageWidth, ImageHeight, out Frame frame);

            texture2D.LoadRawTextureData(frame.data, (int)frame.datasize);
            texture2D.Apply();
            rawImageAntiDistortion.texture = texture2D;

            byte[] data = texture2D.EncodeToJPG();

            StartCoroutine(SendToAzureVisionAPI(data));

        }
        catch (Exception e)
        {
            Debug.LogFormat("e={0}", e);
            throw;
        }
    }

    #region Azure Vision API

    private IEnumerator SendToAzureVisionAPI(byte[] data)
    {

        string visionURL = "YOUR_ENDPOINT_URL";
        string subscriptionKey = "YOUR_API_KEY";

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(visionURL, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(data);
            www.SetRequestHeader("Content-Type", "image/jpeg");
            www.SetRequestHeader("Ocp-Apim-Subscription-Key", subscriptionKey);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Image upload complete!");
                string responseText = www.downloadHandler.text;

                try
                {
                    RootObject rootObject = JsonUtility.FromJson<RootObject>(responseText);

                    Debug.Log("Model Version: " + rootObject.modelVersion);
                    Debug.Log("Caption Text: " + rootObject.captionResult.text);
                    Debug.Log("Caption Confidence: " + rootObject.captionResult.confidence);
                    Debug.Log("Image Width: " + rootObject.metadata.width);
                    Debug.Log("Image Height: " + rootObject.metadata.height);

                    foreach (var tag in rootObject.tagsResult.values)
                    {
                        Debug.Log("Tag Name: " + tag.name + ", Confidence: " + tag.confidence);
                    }
                    UIText.text = rootObject.captionResult.text;
                }
                catch (Exception ex)
                {
                    Debug.LogError("JSON Parsing Error: " + ex.Message);
                }
            }
        }
    }

    [Serializable]
    public class CaptionResult
    {
        public string text;
        public float confidence;
    }

    [Serializable]
    public class Metadata
    {
        public int width;
        public int height;
    }

    [Serializable]
    public class Tag
    {
        public string name;
        public float confidence;
    }

    [Serializable]
    public class TagsResult
    {
        public List<Tag> values;
    }

    [Serializable]
    public class RootObject
    {
        public string modelVersion;
        public CaptionResult captionResult;
        public Metadata metadata;
        public TagsResult tagsResult;
    }

    #endregion

}
