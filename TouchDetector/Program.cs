using System.Runtime.InteropServices;
using Linearstar.Windows.RawInput;
using TouchDetector.InputDevices;

class TouchDetectorMain
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    static void Main()
    {
        #region DEVICE INFO (WHICH CONNECTED)
        var devices = RawInputDevice.GetDevices();
        var touches = devices.OfType<RawInputDigitizer>();
        var mouses = devices.OfType<RawInputMouse>();
        Console.WriteLine("------------------------------------------------------DEVICE LIST------------------------------------------------------");
        foreach (var device in touches)
        {
           Console.WriteLine($"{device.DeviceType} {device.VendorId:X4}:{device.ProductId:X4} {device.ProductName}, {device.ManufacturerName}");
        }
        foreach (var device in mouses)
        Console.WriteLine($"{device.DeviceType} {device.VendorId:X4}:{device.ProductId:X4} {device.ProductName}, {device.ManufacturerName}");
        #endregion

        #region INPUT
        //Console.WriteLine("---------------------------------------------------------INPUT---------------------------------------------------------");
        //window.Input += (sender, e) =>
        //{
        //var data = e.Data;
        //Console.WriteLine(e.Data);


        //};

        #endregion

        IntPtr consoleWindow = GetConsoleWindow();
        ShowWindow(consoleWindow, SW_HIDE);

        var window = new RawInputReceiverWindow();
        try
        {
            RawInputDevice.RegisterDevice(HidUsageAndPage.TouchScreen, RawInputDeviceFlags.ExInputSink, window.Handle);
            RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.ExInputSink | RawInputDeviceFlags.NoLegacy, window.Handle);
            Application.Run();

        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.ReadLine();
        }
        finally
        {
            RawInputDevice.UnregisterDevice(HidUsageAndPage.TouchScreen);
            RawInputDevice.UnregisterDevice(HidUsageAndPage.Mouse);
        }
    }
}


