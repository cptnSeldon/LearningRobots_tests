--------------------------------------------------------------------------------------------------------------------------------
										Hello World - Intel RealSense C#/WPF project
--------------------------------------------------------------------------------------------------------------------------------

Link : https://software.intel.com/en-us/articles/using-the-intel-realsense-sdk-to-create-hello-world-project

--------------------------------------------------------------------------------------------------------------------------------
1.	Add references to the Intel RealSense SDK libraries :
	-> 2 DLL (dynamic-link libraries) are required for creating IRS apps in C#.
	
	1. libpxcclr.cs.dll 	the managed C# interface DLL	-> directly through References
	2. libpxccpp2c.dll 		the unmanaged C++ P/Invoke DLL	-> from properties window -> always copy
	
	// two approches are available for adding these libraries : 
	//	- create references for DLL located outside
	//  - add the DLLs to the project and creet local references	-> this on will be used
	//		-> reduces chances of broken references
	//		-> downside : DLLs may need to be manually updated (new SDK versions)
	
	-> taken from : C:\Program Files (x86)\Intel\RSSDK\bin\win32
	copy to the project folder : location -> libraries
	
	-> then build
	
--------------------------------------------------------------------------------------------------------------------------------
2.	UI
	-> 3 WPF controls 
		- Image control for hosting the video stream
		- Label for printing [] on the screen when the user waves a hand at the camera
		- Stackpanel container to hold the other controls
		
--------------------------------------------------------------------------------------------------------------------------------
3. Create the Utility Class : for supporting bitmap conversion
	-> needs System.Drawing.dll
	
--------------------------------------------------------------------------------------------------------------------------------
	