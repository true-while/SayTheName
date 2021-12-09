# Accessing Azure Cognitive Services from a Raspberry Pi

if the following tutorial you will found how to build AI solution on the IoT devices for connected scenario and upgrade it for containers and host with disconnected scenario.

# Part 1: Connected Application Scenario.  

This is a chapter showing how to build connected scenario on Raspberry Pi 3 device and communicate with Cognitive Azure Services. *Speech Service* is a service trained to convert speech to text and text to speech. In the tutorial speech will be synthesized by speech service and played from RPi. Another service named *Form Recognizer* will be used for analyzing snapshot of business cards taken by RPi camera and respond back with extract data in JSON. Form Recognizer will use unsupervised algorithms to extract info from business cards. The info from the card will be synthesized as speech and played from IoT Device (RPi).

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

1. The project source located in `/src` folder.

1. Update **appsettings.json** file in the root folder with cognitive service account details used for recognition and speech synthesize. Keep other values for RPi by default.
If you use a multi-service resource the keys and regions settings will be the same. Alternatively you can provide key and region from each speech and form service. Following [tutorial](https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account?tabs=multiservice%2Cwindows#get-the-keys-for-your-resource) will explain the location of keys and regions.


```JSON
{
  "SpeechServiceKey": "<your key>",
  "SpeechServiceRegion": "<region>",
  "FormServiceKey": "<your key>",
  "FormServiceRegion": "<region>",
  "PhotoCommand": "fswebcam",
  "PhotoCommandParam": "-r 1280x720 --no-banner cam.jpg",
  "PhotoPath": "cam.jpg"
}
```


## Prepare RPi to run the project

1. Download and flash the latest Raspbian image from the [official web site](https://www.raspberrypi.org/software/operating-systems/). Lite version without desktop will be a vise choice. For flashing SD cards you can use [Etcher](https://www.balena.io/etcher/).

1. Start RPi and run configuration [raspi-config](https://www.raspberrypi.org/documentation/computers/configuration.html) to allow SSH, camera access and network access. You need to connect the monitor and keyboard to complete configuration.

1. Connect by SSH to RPi (PuTTY) and change the default password.

1. Install .Net 5.0 by following  the [tutorial](https://docs.microsoft.com/en-us/dotnet/iot/deployment#deploying-a-framework-dependent-app) steps.

1. Install ALSA packages by following command:

    ```bash
    sudo apt-get install gcc libasound2 libasound2-dev alsa-utils
    ```

1. You can check if the audio and speaker works on your RPi by running the following command. It should produce a noise for your default audio device connected to `**`audio jack`. *USB or Bluetooth speakers are not supported* by the code. 

    ```bash
        speaker-test -c2
    ```

1. Connect your USB camera to RPi and install the tool `fswebcam` for taking snapshots. You also can test how the camera takes snapshots by following the [tutorial](https://tutorials-raspberrypi.com/raspberry-pi-security-camera-with-webcam/). Then use WinSCP to connect to the RPi and download images to observe.

1. Copy files including updated `appsettings.json` to RPi by using WinSCP tool.

1. Build dotnet the project.

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



# Part 2: Connected Containers Scenario.  

In this chapter you will upgrade the code for hosting on the docker container running on RPi with IoT Edge. The same code you have above now will be pushed to the container registry in Azure from your development environment. Later IoT edge service installed on RPi will pull the container and start the process in the same way as it works before. We can update our schema with IoT Hub and Azure container registry:

![](/img/schema-2.png)


## Prepare dev environment.

1. The tutorial supposed you have already completed the steps above and has Cognitive Services deployed in your Azure subscription.

1. Install extension for [Visual Studio Code](https://code.visualstudio.com/) to configured with the [Azure IoT Tools](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-tools).

1. To build docker images you also need to install [Docker CE](https://docs.docker.com/install/) and configure it to run Linux containers.

## Prepare Azure Resources. 

1. In your web browser, navigate to the [Azure Portal](http://portal.azire.com) and create a new free or standard-tier [IoT Hub](https://docs.microsoft.com/en-us/azure/iot-hub/).

1. On the Azure portal create a new [Azure Container Registry](https://docs.microsoft.com/en-us/azure/container-registry/?view=iotedge-2020-11). 



## Build project on development environment

1. You can find the application for docker in `/docker/` folder.

1. Make sure in VS Code you select `arm32v7` as the target platform.

1. In the `deployment.template.json` update registryCredentials with the `full` name of your ACR.

    ```JSON

     "alexacrdemo": {
        "username": "<your username>",
        "password": "<your password>",
        "address": "<your arc name>.azurecr.io"
    }
    ```

1. From terminal window in VS Code run command to sign in `az acr login -n <short name of your ACR>`.

1. From terminal run command `py version.py` to generate new versions of the `module.json` files.

1. Select file `deployment.template.json` in project view and from context menu choice `Build and Push IoT Edge Solution`. Then docker builds images and pushes them in ACR. 

1. Make sure the build ends up successfully and your Azure Container Registry has required container images.

![acr](/img/acr.png)


## Prepare RPi to Host Iot Edge

1. Before install IoT Edge you first need to install the container engine by executing the following commands in the [tutorial](https://phoenixnap.com/kb/docker-on-raspberry-pi). 

1. You also need to install [IoT Edge](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge?view=iotedge-2020-11#install-iot-edge) on your RPi device. You also need to update the security [demon](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-update-iot-edge?view=iotedge-2020-11&tabs=linux#update-the-security-daemon).

1. To properly configure `/etc/aziot/config.toml` you need to register your IoT device in Iot Hub. The following [tutorial](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-register-device?view=iotedge-2020-11&tabs=azure-portal) explained the registration process with a symmetric key. From the Azure portal create an IoT Hub and add a new Iot Edge device. Provide a unique name and keep all default settings and generated keys. Copy primary key into `config.toml`. 

1. Restart your device and wait for status update of device on the IoT Hub. You also can execute command "iotedge list" to monitor process. Please pay attention for the version of `azureiotedge-hub` and `azureiotedge-agent`you use. Version 1.0.0 has an issue and you can update that to version 1.2.3. You can use runtime settings as explained in following [tutorial](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-update-iot-edge?view=iotedge-2020-11&tabs=windows#update-a-specific-tag-image)   

1. Finlay you should have your IoTEdge modules running on RPi without errors. 
    
    ![output](/img/iothub-screen.png)



## Deploy modules on RPi

1. Make sure your build is successful and ACR contains the version you set up in configuration.

1. From the VS code context menu select command `Build and Publish Iot Edge Solution`. Then you can use the command palette `Azure IoT Edge: Create deployment for a single device` and choose your device and file `config\deployment.arm32v7.json`.

    >If you lost with deployment follow the steps in [tutorial](https://docs.microsoft.com/en-us/azure/iot-edge/tutorial-deploy-custom-vision?view=iotedge-2020-11#deploy-modules-to-device)

1. As result of deployment your RPi should contain 3 modules (`azureiotedge-agent`, `azureiotedge-hub`, `SayTheName`). You can monitor modules from IoT Hub or from the SSH console by command `iotedge list`. You also can retrieve the logs of the container run by command `iotedge log <your container name>`

    ![service list](/img/iotedgecontainers.png)


## Setting up the scene

The settings should not be changed from the previous run. You still need to connect your speaker or headphones to the audio jack on RPi and fix the camera steady to take clear shots of the business card. For example take a look at the setup above in Part 1.


## Monitor and diagnose applications.

1. From the SSH console you can use commands like `iotedge logs SayTheName`  to monitor errors and issues. Correct output of the detector should looks as following:

    ![output](/img/container-results.png)

    > Note that the error from ALSA library is coming only for the first and does not prevent speech synthesize and output on speaker.

    > [troubleshot iotedge service](https://docs.microsoft.com/en-us/azure/iot-edge/troubleshoot?view=iotedge-2020-11)


1. To trace and diagnose an application you can modify the docker file and run some diagnostic app as the entry point of your container. For example, replace `ENTRYPOINT ["dotnet", "SayTheName.dll"]` with `ENTRYPOINT ["speaker-test", "-c2"]` to test access to your speaker running from a container.


# References

1. Use form recognizer studio and supervised algorithms when unsupervised does not help. https://docs.microsoft.com/bg-bg/azure/applied-ai-services/form-recognizer/concept-business-card

1. Form Recognizer SDK and code snippets on C#, Java and Python. https://docs.microsoft.com/en-us/azure/applied-ai-services/form-recognizer/how-to-guides/try-sdk-rest-api?pivots=programming-language-csharp

1. Speech SDk and code snippets on C#, Java and Python. https://docs.microsoft.com/en-us/samples/azure-samples/cognitive-services-speech-sdk/sample-repository-for-the-microsoft-cognitive-services-speech-sdk/

1. [Build RPi solution from Ch9](https://github.com/Azure-Samples/Custom-vision-service-iot-edge-raspberry-pi)

1. [How To Access the Raspberry Pi Camera Inside Docker and OpenCV](https://spltech.co.uk/how-to-access-the-raspberry-pi-camera-inside-docker-and-opencv/)

1. [Fixing Docker build issue](https://dev.to/kenakamu/export-custom-vision-model-to-raspberry-pi-3-issue-and-fix-29bg)

1. [Uncompleted MS tutorial: Perform image classification at the edge with Custom Vision Service](https://docs.microsoft.com/en-us/azure/iot-edge/tutorial-deploy-custom-vision?view=iotedge-2020-11)

1. [Connection PI camera to RPi](https://www.teachmemicro.com/uploading-camera-images-raspberry-pi-website/)