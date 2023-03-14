using Linearstar.Windows.RawInput;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using UIAutomationClient;

namespace TouchDetector.InputDevices
{
    class RawInputReceiverWindow : NativeWindow
    {
        private const int WM_INPUT = 0x00FF;

        private string json;

        private CUIAutomation8 Automation;

        private IUIAutomationElement element, elementByTouch;

        private bool tnBrowserWorking;

        private bool EDITFIELD_FOUND;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public RawInputReceiverWindow()
        {
            CreateHandle(new CreateParams
            {
                X = 0,
                Y = 0,
                Width = 0,
                Height = 0,
                Style = 0x800000,
        });
            Automation = new CUIAutomation8();
            /*Thread tnBrowserThread = new Thread(() =>
            {
                while (true)
                {
                    if (Automation.ElementFromPoint(new tagPOINT { x = System.Windows.Forms.Cursor.Position.X, y = System.Windows.Forms.Cursor.Position.Y }) != null)
                    {
                        if (Process.GetProcessById(Automation.ElementFromPoint(new tagPOINT { x = System.Windows.Forms.Cursor.Position.X, y = System.Windows.Forms.Cursor.Position.Y }).CurrentProcessId).ProcessName == "tn-browser")
                        {
                            if (!tnBrowserWorking)
                            {
                                tnBrowserWorking = true;
                                ProcessStartInfo processStartInfo = new ProcessStartInfo(Environment.CurrentDirectory + @"\inspectorPanel\Inspect.exe");
                                processStartInfo.CreateNoWindow = true;
                                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                var TOUCHINPUT_PROCID = Process.Start(processStartInfo).Id;
                                Thread.Sleep(3000);
                                Process.GetProcessById(TOUCHINPUT_PROCID).Kill();
                            }
                        }
                        else if (Process.GetProcessesByName("tn-browser").Length <= 0)
                        {
                            tnBrowserWorking = false;
                        }
                    }
                    Thread.Sleep(5000);
                }
                
            });
            tnBrowserThread.IsBackground = true;
            tnBrowserThread.Start();*/
            /*Thread healthKeyboard = new Thread(() =>
            {
                while(true)
                {
                    Thread.Sleep(2000);
                    var httpRequest = (HttpWebRequest)WebRequest.Create("http://localhost:7000/keyboardhealth");
                    httpRequest.Method = "POST";
                    httpRequest.ContentType = "application/json";
                    try
                    {
                        using (var requestStream = httpRequest.GetRequestStream())
                        using (var writer = new StreamWriter(requestStream))
                        {
                            writer.Write("HEALTH");
                        }
                        using (var httpResponse = httpRequest.GetResponse())
                        using (var responseStream = httpResponse.GetResponseStream())
                        using (var reader = new StreamReader(responseStream))
                        {
                            string response = reader.ReadToEnd();
                        }
                    }
                    catch (Exception e)
                    {
                        Process.GetCurrentProcess().Kill();
                        Console.WriteLine(e.Message);
                    }
                    GC.Collect();
                }
            });
            healthKeyboard.IsBackground = true;
            healthKeyboard.Start();*/
        }

        [DllImport("oleacc.dll")]
        public static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint dwObjectID, byte[] refID, out IntPtr pcObtained);
        [ComImport]
        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IServiceProvider
        {
            void QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject);
        }

        // Here is my new function
        public static void GetIAccessible2(int pid)
        {
            Console.WriteLine("PID = " + pid);
            //Guid guid = new Guid("6C496175-9160-5C3D-93EE-7DDB1E2D1CB0");
            Guid guid = new Guid("618736e0-3c3d-11cf-810c-00aa00389b71");
            Process proc = Process.GetProcessById(pid);
            IntPtr hwnd = proc.MainWindowHandle;

            Console.WriteLine(proc.MainWindowTitle);

            IntPtr ptrToObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Guid)));

            uint OBJECT_ID = 0xFFFFFFFC; // client: 0xFFFFFFFC window: 0x00000000

            IntPtr ptrAccObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(IntPtr)));
            int retAcc = AccessibleObjectFromWindow(hwnd, OBJECT_ID, guid.ToByteArray(), out ptrAccObj);
            if (retAcc != -2147467259)
            {
                Console.WriteLine(retAcc);
                ptrToObj = Marshal.ReadIntPtr(ptrAccObj);
            }
            else
                Console.WriteLine("ПРИПЛЫЛИ");

                Guid iAccessibleGuid = new Guid(0x618736e0, 0x3c3d, 0x11cf, 0x81, 0xc, 0x0, 0xaa, 0x0, 0x38, 0x9b, 0x71);
            IntPtr iAccessiblePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Guid)));

            Guid iAccServiceProvider = new Guid("6d5140c1-7436-11ce-8034-00aa006009fa");
            int retQuery = Marshal.QueryInterface(ptrAccObj, ref iAccServiceProvider, out iAccessiblePtr);

            Accessibility.IAccessible acc = (Accessibility.IAccessible)Marshal.GetTypedObjectForIUnknown(iAccessiblePtr, typeof(Accessibility.IAccessible));

            IntPtr ptrAcc2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(IntPtr)));

            var serviceProvider = (IServiceProvider)acc;
            Guid IID_IAccessible2 = new Guid(0xE89F726E, 0xC4F4, 0x4c19, 0xbb, 0x19, 0xb6, 0x47, 0xd7, 0xfa, 0x84, 0x78);
            try
            {
                serviceProvider.QueryService(ref IID_IAccessible2, ref IID_IAccessible2, out ptrAcc2);
            }
            catch
            {
                // ignore - don't care!!!
            }
            //Marshal.FinalReleaseComObject(serviceProvider);
            //Marshal.FreeCoTaskMem(ptrAcc2);
            //Marshal.FreeCoTaskMem(ptrAccObj);
            //Marshal.FreeCoTaskMem(ptrToObj); Resulted in a corrupted heap error, because it is not a marshalled intptr
            //Marshal.FinalReleaseComObject(acc);
            proc = null;
            //ParseChildren(new MSAAUIItem(acc));
            acc = null;
        }

        private void SendData(int X, int Y)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("http://localhost:7000/coordinates");
            json = X + "," + Y;
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/json";
            try
            {
                using (var requestStream = httpRequest.GetRequestStream())
                using (var writer = new StreamWriter(requestStream))
                {
                    writer.Write(json);
                }
                using (var httpResponse = httpRequest.GetResponse())
                using (var responseStream = httpResponse.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    string response = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Process.GetCurrentProcess().Kill();
                Console.WriteLine(e.Message);
            }
            GC.Collect();
        }

        private void SendData(string message)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("http://localhost:7000/coordinates");
            json = message;
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/json";
            try
            {
                using (var requestStream = httpRequest.GetRequestStream())
                using (var writer = new StreamWriter(requestStream))
                {
                    writer.Write(json);
                }
                using (var httpResponse = httpRequest.GetResponse())
                using (var responseStream = httpResponse.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    string response = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Process.GetCurrentProcess().Kill();
                Console.WriteLine(e.Message);
            }
            GC.Collect();
        }

        protected override void WndProc(ref Message m)
        { 
            if (m.Msg == WM_INPUT)
            {
                try
                {
                    switch (RawInputData.FromHandle(m.LParam))
                    {
                        case RawInputDigitizerData hid:
                            TouchPosition.SetTouchPosition(hid.Contacts[0].Y, hid.Contacts[0].X, hid.Contacts[0].MaxY, hid.Contacts[0].MaxX);
                            CheckElement("Touch");
                            //SendData(Convert.ToInt32(TouchPosition.GetTouch().X), Convert.ToInt32(TouchPosition.GetTouch().Y));
                            break;
                        case RawInputMouseData mouse:
                            if (mouse.Mouse.Buttons.ToString() == "LeftButtonDown")
                            {
                                CheckElement("Mouse");
                                //SendData(0, 0);
                            }   
                            break;
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            base.WndProc(ref m);
        }
        
        private void CheckElement(string inputType)
        {
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    element = Automation.GetFocusedElement();
                    switch(inputType)
                    {
                        case "Mouse":
                            elementByTouch = Automation.ElementFromPoint(new tagPOINT { x = System.Windows.Forms.Cursor.Position.X, y = System.Windows.Forms.Cursor.Position.Y });
                            break;
                        case "Touch":
                            elementByTouch = Automation.ElementFromPoint(new tagPOINT { x = Convert.ToInt32(TouchPosition.GetTouch().X), y = Convert.ToInt32(TouchPosition.GetTouch().Y) });
                            break;
                    }
                    if (element != null)
                    {
                        try
                        {
                            if (elementByTouch != null)
                            {
                                if (Process.GetCurrentProcess().Id != elementByTouch.CurrentProcessId)
                                {
                                    for (int j = 0; j < 5; j++)
                                    {
                                        GetIAccessible2(element.CurrentProcessId);
                                    }
                                    
                                    Console.WriteLine(Process.GetProcessById(element.CurrentProcessId).ProcessName + " ID: " + element.CurrentProcessId + " WndName: " + Process.GetProcessById(element.CurrentProcessId).MainWindowTitle + " Control: " + element.CurrentLocalizedControlType + " Control type: " + element.CurrentAutomationId + " Control type: " + element.CurrentControlType);
                                    if (i >= 2)
                                    {
                                        switch (element.CurrentControlType)
                                        {
                                            case 50016:
                                                SendData("OPEN_NUMBERKEYBOARD");
                                                EDITFIELD_FOUND = true;
                                                break;
                                            case 50004:
                                                SendData("OPEN_TEXTKEYBOARD");
                                                EDITFIELD_FOUND = true;
                                                break;
                                            case 50003:
                                                SendData("OPEN_TEXTKEYBOARD");
                                                EDITFIELD_FOUND = true;
                                                break;
                                            default:
                                                EDITFIELD_FOUND = false;
                                                break;
                                        }
                                        if(!EDITFIELD_FOUND)
                                        {
                                            switch (elementByTouch.CurrentControlType)
                                            {
                                                case 50016:
                                                    SendData("OPEN_NUMBERKEYBOARD");
                                                    EDITFIELD_FOUND = true;
                                                    break;
                                                case 50004:
                                                    SendData("OPEN_TEXTKEYBOARD");
                                                    EDITFIELD_FOUND = true;
                                                    break;
                                                case 50003:
                                                    SendData("OPEN_TEXTKEYBOARD");
                                                    EDITFIELD_FOUND = true;
                                                    break;
                                                default:
                                                    EDITFIELD_FOUND = false;
                                                    break;
                                            }
                                        }
                                    }
                                    /*switch (element.CurrentLocalizedControlType)
                                    {
                                        case "поле":
                                            if (i >= 2)
                                            {
                                                if (element.CurrentControlType == 50016 || elementByTouch.CurrentControlType == 50016)
                                                {
                                                    SendData("OPEN_NUMBERKEYBOARD");
                                                }
                                                else
                                                {
                                                    SendData("OPEN_TEXTKEYBOARD");
                                                }
                                                EDITFIELD_FOUND = true;
                                            }
                                            break;
                                        case "edit":
                                            if (i >= 2)
                                            {
                                                if (element.CurrentControlType == 50016)
                                                {
                                                    SendData("OPEN_NUMBERKEYBOARD");
                                                }
                                                else
                                                {
                                                    SendData("OPEN_TEXTKEYBOARD");
                                                }
                                                EDITFIELD_FOUND = true;
                                            }
                                            break;
                                        case "вертушка":
                                            if (i >= 2)
                                            {
                                                if (element.CurrentControlType == 50016)
                                                {
                                                    SendData("OPEN_NUMBERKEYBOARD");
                                                }
                                                else
                                                {
                                                    SendData("OPEN_TEXTKEYBOARD");
                                                }
                                                EDITFIELD_FOUND = true;
                                            }
                                            break;
                                        case "pinwheel":
                                            if (i >= 2)
                                            {
                                                if (element.CurrentControlType == 50016)
                                                {
                                                    SendData("OPEN_NUMBERKEYBOARD");
                                                }
                                                else
                                                {
                                                    SendData("OPEN_TEXTKEYBOARD");
                                                }
                                                EDITFIELD_FOUND = true;
                                            }
                                            break;
                                        case "spinner":
                                            if (i >= 2)
                                            {
                                                if (element.CurrentControlType == 50016)
                                                {
                                                    SendData("OPEN_NUMBERKEYBOARD");
                                                }
                                                else
                                                {
                                                    SendData("OPEN_TEXTKEYBOARD");
                                                }
                                                EDITFIELD_FOUND = true;
                                            }
                                            break;
                                        default:
                                            break;

                                    }*/
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    }
                }
                if (EDITFIELD_FOUND)
                    EDITFIELD_FOUND = false;
                else
                    SendData("HIDE");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
