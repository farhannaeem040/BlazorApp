using System.Collections.Generic;

namespace ConceptsClient
{
    public class appSettingsDTO
    {
        public string Server { get; set; }
        public string ServerUrl { get; set; }
        //public string LogsFolderPath { get; set; }
        public int LogsFlushWaitSec { get; set; }
        public string LogLevel { get; set; }
        public bool SimulateServer { get; set; }
        public bool SimulateATM { get; set; }
        public int InactivityTime { get; set; }
        public int InactivityWarningTime { get; set; }
        public int RequestTimeOut { get; set; }
        public string TerminalType { get; set; }
        public string EngErrorMessage { get; set; }
        public string AraErrorMessage { get; set; }
        public string EngServerTimeoutMessage { get; set; }
        public string AraServerTimeoutMessage { get; set; }
        public string[] LabelFiles { get; set; }
        public int ClickVolume { get; set; }
        public int ErrorVolume { get; set; }
        public int HeatBeatMinutes { get; set; }
        public AdditionalVariables AdditionalVariables { get; set; }
        public List<RedirectionInfo> Urls { get; set; }
        public string FingerPrintServiceUrl { get; set; }
        public bool RunAsService { get; set; }
        public string LocalUrl { get; set; }
    }

    public class AdditionalVariables
    {
        public int QRCodeDisplayTime { get; set; }
    }

    public class RedirectionInfo
    {
        public string ServiceName { get; set; }
        public string Url { get; set; }
    }
}
