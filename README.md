# KittScanner
Remember Knight Rider? KITT is the 80's 1982 Pontiac Trans Am with flashy red LEDs bouncing across its grill? As many people have done this effect, I have too!

## Intro
* Knight Rider -> Back in the 80's, the Knight Rider TV show featured a car with a scanner light that moved back and forth. Me and my friends  enjoyed the show. 

Now you can have this in your own application. Many people have done it, but this is my version.

Here comes a screenshot to check out:

![KittScanner Screenshot](https://github.com/sorrynofocus/KittScanner/blob/1ddb792a7cc3042f36879350c9fdb49ef439d7e4/img/kitt-scanner.gif)

## Features

- **Animated LED Blocks:**  
  Displays a row of LED segments whose brightness changes based on a moving scanner position.

- **Inner Glowing Circles:**  
  Each LED block has a small glowing circle at its center. The glow is created using a `PathGradientBrush` for a radial gradient effect, an artifact from the base light.

- **Speed Control:**  
  A `TrackBar` at the bottom of the window lets users adjust the scanning speed.

- **Window Size Display:**  
  A label shows the current window width and height, updating as the window is resized.

- **Smooth Animation:**  
  The application uses a custom double-buffered panel to minimize flicker during animation.

## Customization Options

- **Number of LED Blocks:**  
  Modify the `ledCount` variable in the code to change how many LED segments are displayed.

- **Block Size and Spacing:**  
  In the `DisplayPanel_Paint` method, adjust the `margin` and `spacing` values to control the padding around and between the LED blocks.

- **Scanning Speed:**  
  The speed is controlled by the trackbar (dividing its value by 10.0 gives the speed factor) and by the sleep duration (`Thread.Sleep(30)`) in the animation loop. These can be modified to adjust the speed of the animation.


  ## How this works

### Display and Animation

#### Display Panel
The `DoubleBufferedPanel` reduces flicker by drawing to an off-screen buffer before displaying the final image. Original code I ran into flicking. Got a few pointers from some real graphics folks :)

#### Animation Loop
A background thread continuously updates the `scannerPosition` (based on `scannerStep`). When the scanner reaches the boundaries, the direction is reversed. The panel is then repainted using `BeginInvoke` to maintain smooth, asynchronous updates. Due to licensing, I did _not_ include sound effects.

### Drawing the LEDs

#### LED Blocks
The LED blocks are sized and spaced based on the panel's width, a margin, and a fixed spacing. Their brightness is computed using the distance to the current scanner position.

#### Inner Glowing Circles
A small circle is drawn at the center of each LED block using a `PathGradientBrush`. The center of the gradient is a bright white (with intensity-determined transparency) that fades to fully transparent at the edges. This was added because the "base light" effect needed a more dynamic appearance. If you examine some of the images of KITT, 
you'll notice an artifact from the base light that disperses outward, creating a halo effect around the LEDs.

### UI Controls

#### Speed Control
The `TrackBar` adjusts the scanning speed. The value is divided by 10.0 to get a speed factor that affects the `scannerStep`.

#### Window Size Display
A label in the bottom panel displays the current window dimensions and updates when the window is resized.

### Customization Options

#### Number of LED Blocks
Change the `ledCount` variable to display more or fewer LED segments.

#### Block Size and Spacing
Modify the `margin` and `spacing` values in the `DisplayPanel_Paint` method to change the size and space between blocks.

#### Speed Settings
Adjust the trackbar's `Minimum`, `Maximum`, and `Value`, or modify the `Thread.Sleep(30)` duration to control the animation speed.


## Build 

Built with `Visual Studio 2022 Enterprise` (C# Lang.) and help with Copilot. 

Note: _Debugging in VS with Copilot is pretty awesome._


**To build this project,** you need to have `.NET Framework 4.8` installed.
 
 Run:
 
 ```
 "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Auxiliary\Build\vcvars64.bat"
 ```

 This will set up the environment variables for building .NET Framework projects.
 
    - Note: In my case, I'm using VS2022 Enterprise, so the path may vary based on your VS version and installation.

Now, compile when things are setup. There's no dependency on the solution file. 

  ```
  msbuild /p:Configuration=Debug /p:Platform=x64
  ```

Output will be in the `bin\x64\Release` folder.
