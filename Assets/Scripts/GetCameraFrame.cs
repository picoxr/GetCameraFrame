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
    public RawImage rawImageAntiDistortion; // UI element to display the camera frame
    public TMPro.TextMeshProUGUI UIText; // UI element for displaying text

    private Texture2D texture2D { get; set; }

    // Maximum Width/Height is 2328x1748. Reduce for more FPS

    private int ImageWidth = 1164;
    private int ImageHeight = 874;

    public InputActionReference captureAction; // Input action for capturing frames

    private float lastAzureCallTime = -3f; // Initialize to allow the first call immediately
    private const float azureCallInterval = 3f; // 3 seconds interval for Azure calls

    private void Start()
    {
        PXR_Boundary.EnableSeeThroughManual(true);          // Activate Seethrough

        PXR_Enterprise.InitEnterpriseService(); // Initialize the enterprise service
        PXR_Enterprise.BindEnterpriseService(); // Bind the enterprise service     

        texture2D = new Texture2D(ImageWidth, ImageHeight, TextureFormat.RGB24, false, false);

        PXR_Enterprise.OpenVSTCamera(); // Activate the VST camera frame acquisiton feature

        captureAction.action.Enable(); // Enable the capture action
        captureAction.action.performed += AcquireVSTCameraFrameAntiDistortionIntPtr; // Subscribe to the capture action event
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            PXR_Boundary.EnableSeeThroughManual(true); // Re-enable seethrough when resuming
        }
    }

    // Clean up when the application quits
    private void OnApplicationQuit()
    {
        PXR_Enterprise.CloseVSTCamera(); // Deavtivate the frame capture feature
    }

    // Clean up when the component is disabled
    private void OnDisable()
    {
        texture2D = null; // Clear texture
        PXR_Enterprise.CloseVSTCamera(); // Close the VST camera
    }

    // Acquire the camera frame and process it for Azure Vision API
    public void AcquireVSTCameraFrameAntiDistortionIntPtr(InputAction.CallbackContext ctxt)
    {
        float currentTime = Time.time;

        // Check if the interval for Azure API call has passed
        if (currentTime - lastAzureCallTime >= azureCallInterval)
        {
            lastAzureCallTime = currentTime; // Update the last call time

            try
            {
                GetComponent<AudioSource>().Play(); // Play audio on capture

                // Acquire the camera frame
                PXR_Enterprise.AcquireVSTCameraFrameAntiDistortion(ImageWidth, ImageHeight, out Frame frame);

                // Load raw texture data into texture2D and apply it
                texture2D.LoadRawTextureData(frame.data, (int)frame.datasize);
                texture2D.Apply();
                rawImageAntiDistortion.texture = texture2D; // Update UI texture

                byte[] data = texture2D.EncodeToJPG(); // Encode texture to JPG format

                // Start coroutine to send data to Azure Vision API
                StartCoroutine(SendToAzureVisionAPI(data));
            }
            catch (Exception e)
            {
                Debug.LogFormat("Exception: {0}", e);
                throw; // Rethrow exception for further handling
            }
        }
        else
        {
            Debug.Log("Azure Vision API call is being throttled. Please wait for the interval to pass.");
            UIText.text = "Reloading camera..."; // Update UI with message
        }
    }

    #region Azure Vision API

    // Coroutine to send image data to Azure Vision API
    private IEnumerator SendToAzureVisionAPI(byte[] data)
    {
        string visionURL = "YOUR_ENDPOINT_URL"; // Azure Vision API endpoint
        string subscriptionKey = "YOUR_API_KEY"; // Azure subscription key

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(visionURL, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(data); // Set up the upload handler
            www.SetRequestHeader("Content-Type", "image/jpeg"); // Set content type
            www.SetRequestHeader("Ocp-Apim-Subscription-Key", subscriptionKey); // Set subscription key

            yield return www.SendWebRequest(); // Send the request and wait for the response

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error); // Log any errors
            }
            else
            {
                Debug.Log("Image upload complete!");
                string responseText = www.downloadHandler.text; // Get response text

                try
                {
                    // Parse JSON response
                    RootObject rootObject = JsonUtility.FromJson<RootObject>(responseText);
                    Debug.Log("Model Version: " + rootObject.modelVersion);
                    Debug.Log("Caption Text: " + rootObject.captionResult.text);
                    Debug.Log("Caption Confidence: " + rootObject.captionResult.confidence);
                    Debug.Log("Image Width: " + rootObject.metadata.width);
                    Debug.Log("Image Height: " + rootObject.metadata.height);

                    // Log each tag's name and confidence
                    foreach (Tag tag in rootObject.tagsResult.values)
                    {
                        Debug.Log("Tag Name: " + tag.name + ", Confidence: " + tag.confidence);
                    }

                    UIText.text = rootObject.captionResult.text; // Update UI with caption text
                }
                catch (Exception ex)
                {
                    Debug.LogError("JSON Parsing Error: " + ex.Message);
                }
            }
        }
    }

    // Classes for parsing JSON response from Azure Vision API
    [Serializable]
    public class CaptionResult
    {
        public string text; // Caption text
        public float confidence; // Caption confidence score
    }

    [Serializable]
    public class Metadata
    {
        public int width; // Image width
        public int height; // Image height
    }

    [Serializable]
    public class Tag
    {
        public string name; // Tag name
        public float confidence; // Tag confidence score
    }

    [Serializable]
    public class TagsResult
    {
        public List<Tag> values; // List of tags
    }

    [Serializable]
    public class RootObject
    {
        public string modelVersion; // Version of the model
        public CaptionResult captionResult; // Caption result data
        public Metadata metadata; // Metadata about the image
        public TagsResult tagsResult; // Tags result data
    }

    #endregion

}
