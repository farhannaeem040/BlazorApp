using System;
using System.Collections.Generic;
using System.Threading;
using ANBBusinessServices.TerminalResponses;


namespace ConceptsClient.AppData
{
    public class UserSession
    {
        public static UserSession CurrentSession { get; set; }

        public Language Language { get; set; }
        private DateTime sessionStartTime;

        public UserSession()
        {
            this.Language = Language.NotSet;
            sessionStartTime = DateTime.Now;
        }

        public string MachineID { get; set; } = "123456";
        public string PAN { get; set; } = "212121121221";
        public string TSN { get; set; } = "3232";
        public string Authorizer { get; set; } = "222";
        public string Requester { get; set; } = "221";
        public string QRCodeSeqNo { get; set; } = "212";

        public List<KeyValuePair<string, string>> SessionData { get; set; }

        //TODO: set when first time received from server
        public string CustNum { get; set; } = "222222";

        //TODO: set when first time received from server
        public int SessionID { get; set; }

        public static string getDefaultErrorDesc()
        {
            if (CurrentSession.Language == Language.Ara)
                return Program.appSettings.AraErrorMessage;
            else
                return Program.appSettings.EngErrorMessage;
        }

        public ManualResetEvent CustomerDetailsStatus { get; internal set; }
        public Response<CustomerDetails> CustomerDetailsResponse { get; internal set; }

        public static string getServerTimeoutDesc()
        {
            try
            {
                if (CurrentSession != null && CurrentSession.Language == Language.Ara)
                    return Program.appSettings.AraServerTimeoutMessage;
                else
                    return Program.appSettings.EngServerTimeoutMessage;
            }
            catch
            {
                return Program.appSettings.EngServerTimeoutMessage;
            }
        }
    }

    public class CardPrintingData
    {
        public string CardProgram { get; set; }
        public string CardNum { get; set; }
        public string NameOnCard { get; set; }
        public string EncryptedPin { get; set; }
        public string MagTrack2Data { get; set; }
        public string ExpDt { get; set; }
        public string FeeRefNumber { get; set; }

        public string SeqNo { get; set; }
    }

    public enum SessionVariables
    {
        NotSet,
        CardNoToPrint
    }
    public enum Language
    {
        NotSet,
        Eng,
        Ara

    }
}
