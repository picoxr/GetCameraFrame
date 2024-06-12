# GetCameraFrame

## DISCLAIMER: This API is reserved for Enterprise developers who sign up for our testing program. If you're interested, reach out to pico-business-techsupport@bytedance.com


## Description
This demo acquires an anti-distorted frame from a VST camera, processes it into a texture, updates a UI element to display the texture, encodes the texture to a JPG, and sends the encoded image to Azure Vision API for further analysis.
## Demo requirements
- Unity 2022.3.f1
- PICO XR SDK 2.5.0
- OPTIONAL: An Azure subscription with a computer vision resource
## Dependencies (UPM)
- XR Interaction Toolkit 2.5.2 + Starter Assets
- Android Logcat
## About AcquireVSTCameraFrameAntiDistortion

> Acquires RGB camera frame (the image after anti-distortion).

## Parameters

| Parameter  | Description |
| ------------- | ------------- |
| width  | Desired frame width, should not exceed 2328.|   |
| height | Desired frame height, should not exceed 1748. |
| out frame | Frame info. |
