# KittScanner
Remember Knight Rider? KITT is the 80's 1982 Pontiac Trans Am with flashy red LEDs bouncing across its grill? As many people have done this effect, I have too!

## Intro
* Knight Rider -> Back in the 80's, the Knight Rider TV show featured a car with a scanner light that moved back and forth. Me and my friends  enjoyed the show. 

Now you can have this in your own application. Many people have done it, but this is my version.

Here comes a screenshot to check out:

![KittScanner Screenshot](https://github.com/sorrynofocus/KittScanner/blob/1ddb792a7cc3042f36879350c9fdb49ef439d7e4/img/kitt-scanner.gif)

### Build 

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
