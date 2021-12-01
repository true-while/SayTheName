# Accessing Azure Cognitive Services from a Raspberry Pi 3

This is a sample showing how to get connected from Raspberry Pi 3 device to Cognitive Azure Services. *Speech Service* is a service trained to convert speech to text and text to speech. In the tutorial speech will be synthesized by speech service and played from RPi. Another service named *Form Recognizer* will be used for analyzing snapshot of business cards taken by RPi camera and respond back with extract data in JSON. Form Recognizer will use unsupervised algorithms to extract info from business cards. The info from the card will be synthesized as speech and played from IoT Device (RPi).

![schema](/img/iot.png)

## Toolbox.

- RPi 3 or 4 with power adapter
- External Camera or RPi camera.
- Speaker or headphone with audio jack.
- LAN or WiFi access.
- USB Keyboard.
- HDMI cable.
- HDMI Monitor. 
- Business cards for processing.
- Laptop with VS code and docker.
- Azure Subscription (Free trial works). 

## Prepare dev environment.

1. You should have the following prerequisites in place:

    - [Visual Studio Code](https://code.visualstudio.com/) configured with the [Azure IoT Tools](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-tools).
    - SSH client like PuTY and client like [WinSCP](https://winscp.net/eng/download.php) to copy files over SSH.
 

1. To develop an IoT application with the Custom Vision service, install the following additional prerequisites on your development machine:

    - .Net 5.0 (https://dotnet.microsoft.com/download/dotnet/5.0)
    - Git (https://git-scm.com/downloads)
    

## Prepare Azure Resources. 

1. In your web browser, navigate to the [Azure Portal](http://portal.azire.com). Sign in and sign in with your account or start Free trial if you do not have a subscription.

1. Create a new Azure Cognitive Service for your subscription. You can build multi-services resource as provided in tutorial or https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account?tabs=multiservice%2Cwindows 

1. The project set up and train described in the [tutorial](https://docs.microsoft.com/en-us/azure/cognitive-services/custom-vision-service/get-started-build-detector). Alternately you can build every service separate:

    - Form Recognizer (Azure Cognitive Service) [build one](https://portal.azure.com/#create/Microsoft.CognitiveServicesFormRecognizer
    
    - Speech service (Azure Cognitive Service) [tutorial][https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/overview#create-the-azure-resource]

>Note: Free tier is available for form recognizer and speech service but not available for multi-service  resources.

## Prepare the project

Update **appsettings.json** file in the root folder with cognitive service account details used for recognition and speech synthesize. Keep other values for RPi by default.
If you use a multi-service resource the keys and regions settings will be the same. Alternatively you can provide key and region from each speech and form service. Following [tutorial](https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account?tabs=multiservice%2Cwindows#get-the-keys-for-your-resource) will explain the location of keys and regions.


```JSON
{
  "SpeechServiceKey": "<your key>",
  "SpeechServiceRegion": "<region>",
  "FormServiceKey": "<your key>",
  "FormServiceRegion": "<region>",
  "PhotoCommand": "fswebcam",
  "PhotoCommandParam": "-r 1280x720 --no-banner /home/pi/photos/cam.jpg",
  "PhotoPath": "/home/pi/photos/cam.jpg"
}
```


## Prepare RPi to run the project

1. Download and flash the latest Raspbian image from the [official web site](https://www.raspberrypi.org/software/operating-systems/). Lite version without desktop will be a vise choice. For flashing SD cards you can use [Etcher](https://www.balena.io/etcher/).

1. Start RPi and run configuration [raspi-config](https://www.raspberrypi.org/documentation/computers/configuration.html) to allow SSH, camera access and network access. You need to connect the monitor and keyboard to complete configuration.

1. Connect by SSH to RPi (PuTTY) and change the default password.

1. Connect your USB camera to RPi and install the tool `fswebcam` for taking snapshots. You also can test how the camera takes snapshots by following the [tutorial](https://tutorials-raspberrypi.com/raspberry-pi-security-camera-with-webcam/). Then use WinSCP to connect to the RPi and download images to observe.



1. Install .Net 5.0 by follow the [tutorial](https://docs.microsoft.com/en-us/dotnet/iot/deployment#deploying-a-framework-dependent-app) steps

1. Make a folder and copy project files by using WinSCP tool.

1. Build the project.

## Setting up the scene

1. Place the objects for identification on the solid background with about 3 fit distance from the camera.The blurry images will be unrecognizable so try to stabilize the camera.

    ![scene](/img/setup.png)

1. Make sure that there are enough lights in the picture area. 

1. Prepare objects for detection. You can download some examples from the internet, but an original business card will work the best. Following sample cards can be printed and used for detection.

1. Use `fswebcam` as explained above to take test snapshots.

    ![camera test](/img/camtest.png)

## Run applications.

1. From the SSH console you can use commands like dotnet run and monitor output. The application will go in a cycle of taking snapshots from camera and analyzing with form recognizer. If the form recognizer is not able to detect Name on the card it will skip the snapshot and wait for 5 seconds before taking a next snapshot. If the Name is detected (can be followed by title and company name if it can be detected) it will be sent for speech synthesize and played as audio stream on speaker or headphones.

    ![output](/img/result.png)

1. Monitor output for unrecognized or blurry images which lead to message 'no business card detected'. During 5second interval you can change the objects for detection.


# References

1. Use form recognizer studio and supervised algorithms when unsupervised does not help. https://docs.microsoft.com/bg-bg/azure/applied-ai-services/form-recognizer/concept-business-card

2. Form Recognizer SDK and code snippets on C#, Java and Python. https://docs.microsoft.com/en-us/azure/applied-ai-services/form-recognizer/how-to-guides/try-sdk-rest-api?pivots=programming-language-csharp

3. Speech SDk and code snippets on C#, Java and Python. https://docs.microsoft.com/en-us/samples/azure-samples/cognitive-services-speech-sdk/sample-repository-for-the-microsoft-cognitive-services-speech-sdk/
