using Lib;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ConceptsClient
{
    public class Program
    {
        public const string AppVersion = "v1.5, Build Date 2019-01-27, +core V2.2, +Card printing, +MOI, +STC Pay, +SADAD";
        public static appSettingsDTO appSettings { get; set; }

        public static TextLogWriter textLogWriter;
        public static void Main(string[] args)
        {
            DebugLog("Starting");
            if (Debugger.IsAttached == false)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
                DebugLog("Set Current Dir as " + pathToContentRoot);
            }

            Program.textLogWriter = new TextLogWriter();
            DebugLog("Creating logs at " + Environment.CurrentDirectory + "\\Logs");

            Program.textLogWriter.InitializeLogWriter(System.Environment.CurrentDirectory + "\\Logs", "ConceptsClient", false, true, true, true);
            LogableTask task = LogableTask.NewTask("Main");
            try
            {
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "Starting app");
                appSettings = JsonConvert.DeserializeObject<appSettingsDTO>(System.IO.File.ReadAllText(System.Environment.CurrentDirectory + "\\ConceptsClient.json"));

                appSettings.ServerUrl = string.Format(appSettings.ServerUrl, appSettings.Server);
                if (Program.appSettings.ServerUrl.EndsWith('/') == false && Program.appSettings.ServerUrl.EndsWith('\\') == false)
                    appSettings.ServerUrl += "/";

                //if (appSettings.LogsFolderPath == null || appSettings.LogsFolderPath == "")
                //    appSettings.LogsFolderPath = Startup.hostingEnvironment.ContentRootPath + "\\Logs";

                //textLogWriter.InitializeLogWriter(appSettings.LogsFolderPath, "ConceptsClient", false, true, true, true);


                var builder = CreateWebHostBuilder(args);
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "web host builder created");

                builder.UseKestrel().UseIISIntegration();
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "going to use URL " + appSettings.LocalUrl);

                //    builder.UseUrls("http://0.0.0.0:82");


                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "useUrls set");

                //   if (Debugger.IsAttached == false)// that is running as windows service
                //   builder.UseContentRoot(System.Environment.CurrentDirectory);


                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "Content root set as " + Environment.CurrentDirectory);

                var host = builder.Build();
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "Host built now. Run as Service =" + appSettings.RunAsService);

                if (appSettings.RunAsService)
                {
                    task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "Host starting as windows service");
                    task.EndTask();
                    // host.RunAsCustomService();// host.RunAsService();
                }
                else
                {
                    task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "Host starting now as process");
                    task.EndTask();
                    host.Run();
                }
            }
            catch (Exception ex)
            {
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Error, ex);
            }
            finally
            {
                if (task.ended == false)
                    task.EndTask();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
        }

        static void DebugLog(string msg)
        {
            File.AppendAllLines("C:\\DebugLog.txt", new string[] { DateTime.Now.ToLongTimeString(), msg });
        }
    }
}
