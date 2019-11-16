using ANBBusinessServices.TerminalRequests;
using ANBBusinessServices.TerminalResponses;
using ConceptsClient.AppData;
using ConceptsClient.Controllers;
using Lib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ConceptsClient.Controllers.Common
{
    public class DataSynchronizer
    {
        public static DateTime? SADADDataSynchedAt { get; private set; }
        public static DateTime? SADADDataCheckedAt { get; private set; }
        public static int LocalSADADDataVersion { get; private set; }
        public static DTOs.Sadad.BillersData SADADBillersData { get; private set; }

        public static TerminalDetailsResponse TerminalDetails { get; private set; }

        public static void OnSynchTimerTick(object state)
        {
            LogableTask task = LogableTask.NewTask("Update terminal Details");
            try
            {
                UpdateTerminalDetails(task);

            }
            catch (Exception ex)
            {
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Error, ex);
            }
            finally { task.EndTask(); }

            task = LogableTask.NewTask("Synch data");
            try
            {
                UpdateSadadaData(task);
            }
            catch (Exception ex)
            {
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Error, ex);
            }
            finally { task.EndTask(); }
        }

        public static void UpdateTerminalDetails(LogableTaskWithoutEnd task)
        {
            task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "Called");
            task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "going to synch terminal info");

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "TerminalDetails.json"))
            {
                var terminalDetailsStr = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "TerminalDetails.json", System.Text.Encoding.UTF8);
                DataSynchronizer.TerminalDetails = JsonConvert.DeserializeObject<TerminalDetailsResponse>(terminalDetailsStr);
            }

            var reply = ServerHelper.GetResponse<TerminalDetailsResponse>("Terminal/GetTerminalDetails", new BaseTerminalRequest
            {
                CommonParams = new CommonParameters { MachineID = SessionController.MachineID }
            }, task);

            if (reply.Success)
            {
                DataSynchronizer.TerminalDetails = reply.Data;
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "TerminalDetails.json",
                    JsonConvert.SerializeObject(DataSynchronizer.TerminalDetails), System.Text.Encoding.UTF8);
            }
            else
            {
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Error, reply.ErrorDesc);
                throw new Exception(reply.ErrorDesc);
            }
        }

        public static void UpdateSadadaData(LogableTaskWithoutEnd task)
        {

            task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "going to synch sadad data");
            if (DataSynchronizer.SADADBillersData == null)
                try
                {
                    DataSynchronizer.LocalSADADDataVersion = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "SadadData", "*.json", System.IO.SearchOption.TopDirectoryOnly)
                    .Select(f => int.Parse(Path.GetFileNameWithoutExtension(f))).Max();

                    var sadadDataStr = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "SadadData/" + LocalSADADDataVersion.ToString() + ".json", System.Text.Encoding.UTF8);
                    DataSynchronizer.SADADBillersData = Newtonsoft.Json.JsonConvert.DeserializeObject<DTOs.Sadad.BillersData>(sadadDataStr);

                }
                catch { LocalSADADDataVersion = -1; }

            var versionReply = ServerHelper.GetResponse<int>("/sadad/GetSadadDataVersion", new BaseTerminalRequest
            {
                CommonParams = new CommonParameters { MachineID = SessionController.MachineID }
            }, task);

            if (versionReply.Success == false)
                throw new Exception(versionReply.ErrorDesc);

            var sadadDataVersionAtServer = versionReply.Data;
            if (sadadDataVersionAtServer > LocalSADADDataVersion)
            {
                var SadadDataReply = ServerHelper.GetResponse<byte[]>("/sadad/GetSadadData",
                    new BaseTerminalRequest { CommonParams = new CommonParameters { MachineID = SessionController.MachineID } }, task);

                if (SadadDataReply.Success == false)
                    throw new Exception(SadadDataReply.ErrorDesc);

                var sadadDataStr = Lib.GZipper.Unzip(SadadDataReply.Data);

                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "SadadData") == false)
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "SadadData");

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "SadadData\\" + sadadDataVersionAtServer.ToString() + ".json", sadadDataStr, System.Text.Encoding.UTF8);

                SADADBillersData = Newtonsoft.Json.JsonConvert.DeserializeObject<DTOs.Sadad.BillersData>(sadadDataStr);
                LocalSADADDataVersion = sadadDataVersionAtServer;
                SADADDataSynchedAt = DateTime.Now;
            }

            DataSynchronizer.SADADDataCheckedAt = DateTime.Now;

        }
    }
}
