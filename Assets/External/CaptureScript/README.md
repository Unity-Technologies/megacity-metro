# CaptureScript

![image](Readme/image.png)
The CaptureScript is a custom two part script created for this engagement to capture performance metrics on an Android device in a repeatable manner across several APKs.

### Requirement

- [ADPF](https://docs.unity3d.com/Packages/com.unity.adaptiveperformance@5.1/manual/index.html) package to capture
  thermal data if needed.
- Bash compatible console to run bash script.

### Quick Start

#### Setup

- Add CaptureScript MonoBehaviour to a new or existent GameObject
- Have another script to call `public static void OnPassWaypoint()` of the CaptureScript

#### How it works

When the `OnPassWaypoint()` method is called on the Capture Script object, it records a snapshot of performance metrics at that moment. These metrics are then logged in a CSV file. This capturing and logging process repeats until a helper bash script terminates the capture and transfers the CSV file to a designated results folder.

#### Helper bash script

The bash script is designed to automate the process of running multiple Android Packages (APKs) on a connected Android device and fulfills four key objectives:
1. Installs and runs APKs on the connected device
2. Stops the APK from running after a set amount of time
3. Copies the generated CSV of metrics data from the device back to the connected computer
4. If there are more APKs to test, the script monitors the device’s temperature until it reaches a specified threshold before installing and running the next APK


### Configuration

![image](Readme/image.png)

| Settings                      |                                                                                                                     |
|-------------------------------|---------------------------------------------------------------------------------------------------------------------|
| Minimum Time Between Captures | Set a delay after taking a capture before taking another one.                                                       |
| Screen Capture Cadence        | In debug builds, screenshots will be captured when this many stat captures have occurred. Set to 0 to disable. one. |
| Fps Sample Count              | Set the size of the sample when gathering metrics, larger value will give more stable values.                       |

### Data collected in CSV

Time, Waypoint, Fps, Cpu, Gpu, Main thread, Render thread, Draw calls, SetPass, GFX mem, Tex mem, MB, Temperature level,
Thermal status, DR, Target Fps

| Metrics           |                                                                                                                                           |
|-------------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| Time              | The real time in seconds since the game started.                                                                                          |
| Waypoint          | The number provided during the OnChange.                                                                                                  |
| Fps               | Frame per seconds                                                                                                                         |
| Cpu               | This is the total CPU frame time calculated as the time between ends of two frames, which includes all waiting time and overheads, in ms. |
| Gpu               | The GPU time for a given frame, in ms.                                                                                                    |
| Main thread       | Total time between start of the frame and when the main thread finished the job, in ms.                                                   |
| Render thread     | The frame time between start of the work on the render thread and when Present was called, in ms.                                         |
| Draw calls        | The number of draw calls to the graphics API.                                                                                             |
| SetPass           | The number of times Unity switches which shader pass it uses to render GameObjects during a frame.                                        |
| GFX mem           | Gfx Used Memory, only works in debug, it will show 0 on release.                                                                          |
| Tex mem           | Texture Memory, only works in debug, it will show 0 on release.                                                                           |
| MB                | Total Used Memory                                                                                                                         |
| Temperature level | ADPF: The current thermal status of the device, 0 for cold, 100 for overheating.                                                          |
| DR                | Dynamic resolution, a Vulkan only feature.                                                                                                |
| Target Fps        | Device target FPS                                                                                                                         |

