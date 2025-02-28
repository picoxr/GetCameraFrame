# GetCameraFrame

## DISCLAIMER: This API is reserved for Enterprise developers who sign up for our testing program. If you're interested in joining, reach out to pico-business-techsupport@bytedance.com


## Description
This demo acquires an anti-distorted frame from a VST camera, processes it into a texture, updates a UI element to display the texture, encodes the texture to a JPG, and sends the encoded image to Azure Vision API for further analysis.

## Setup
- Once you have a PICO Enterprise authorization (by reaching out to the address above), you must change the package name to the one you provided for the authorization procedure.
- The **package name** is bunlded with the **SN code** of your authorized device. Running the apk with another package name, or on an different device, will result in the API not providing any camera data.
- **[OPTIONAL]** You can also set up an Azure Computer Vision resource. You can then set your resource endpoint and API key in the code. [API reference](https://learn.microsoft.com/en-us/azure/ai-services/computer-vision/how-to/call-analyze-image-40?tabs=csharp&pivots=programming-language-rest-api)

## Demo requirements
- Unity 2022.3.f1
- PICO XR SDK 2.5.0
- OPTIONAL: An Azure subscription with a computer vision resource
## Dependencies (UPM)
- XR Interaction Toolkit 2.5.2 + Starter Assets
- Android Logcat
### Third party assets
- Plastic materials from: https://freepbr.com/materials/worn-scuffed-plastic-pbr-material/
## About AcquireVSTCameraFrameAntiDistortion

> Acquires RGB camera frame (the image after anti-distortion).

## Parameters

| Parameter  | Description |
| ------------- | ------------- |
| width  | Desired frame width, should not exceed 2328.|   |
| height | Desired frame height, should not exceed 1748. |
| out frame | Frame info. |

API Reference: https://developer.picoxr.com/reference/unity/latest/PXR_Enterprise/#05920ad3

## Demo


https://github.com/picoxr/GetCameraFrame/assets/15983798/2b3eb5c3-30d0-4c85-9745-0f3588daca95

