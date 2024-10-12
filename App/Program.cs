using System;
using System.Management;

namespace ProcessCreationWatcher
{
    class Program
    {
        /*
         * 
         * Require the nuget package System.Management
         * and can't use AOT since system.management appear to be a COM object
         * 
         */


        static void Main(string[] args)
        {
            try
            {
                string wmiQuery = "SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'";

                ManagementScope scope = new ManagementScope("\\\\.\\root\\cimv2");

                scope.Connect();

                WqlEventQuery query = new WqlEventQuery(wmiQuery);

                using (ManagementEventWatcher watcher = new ManagementEventWatcher(scope, query))
                {
                    watcher.EventArrived += new EventArrivedEventHandler(ProcessCreated);

                    watcher.Start();

                    Console.WriteLine("Listening for process creation events. Press Enter to exit.");
                    Console.ReadLine();

                    watcher.Stop();
                }
            }
            catch (ManagementException me)
            {
                Console.WriteLine("A WMI error occurred: " + me.Message);
                if (me.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + me.InnerException.Message);
                }
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine("Insufficient permissions to execute WMI query: " + uae.Message);
                if (uae.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + uae.InnerException.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
            }
        }

        private static void ProcessCreated(object sender, EventArrivedEventArgs e)
        {
            try
            {
                ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

                string processName = targetInstance["Name"]?.ToString();
                uint processId = (uint)targetInstance["ProcessId"];
                string commandLine = targetInstance["CommandLine"]?.ToString();
                string executablePath = targetInstance["ExecutablePath"]?.ToString();

                // Display information
                Console.WriteLine($"New process created:");
                Console.WriteLine($"  Name: {processName}");
                Console.WriteLine($"  Process ID: {processId}");
                Console.WriteLine($"  Executable Path: {executablePath}");
                Console.WriteLine($"  Command Line: {commandLine}");
                Console.WriteLine($"  Timestamp: {DateTime.Now}");
                Console.WriteLine(new string('-', 50));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing event: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
            }
        }
    }
}
