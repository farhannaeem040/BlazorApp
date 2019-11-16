using ANBBusinessServices.TerminalResponses;
using ConceptsClient.AppData;
using Lib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConceptsClient.Controllers.Common
{
    public class SessionController
    {
        public static string MachineID { get; set; } = "";

     
        public Response<AuthorizationTokenResponse> GetRequesterAuthorizer(string CardNum)
        {
            if (CardNum != null)
            {
                var simpleAES = new SimpleAES();

                return new Response<AuthorizationTokenResponse>
                {
                    Success = true,
                    Data = new AuthorizationTokenResponse
                    {
                        requester = getBase64(simpleAES.Encrypt(CardNum)),
                        authorizer = getBase64(simpleAES.Encrypt(DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss:fff")))
                    }
                };
            }
            return null;
        }

        private string getBase64(byte[] dataArray)
        {
            return Convert.ToBase64String(dataArray, 0, dataArray.Length);
        }

        [HttpPost("closed")]
        public void Closed()
        {
            UserSession.CurrentSession = null;
            LogableTask.LogSingleActivity("SessionController", MethodBase.GetCurrentMethod(), TraceLevel.Info, "Session closed");
        }

        public class SetCommonParamsReq
        {
            public bool SetCustNum { get; set; }
            public string CustNum { get; set; }
            public bool SetPAN { get; set; }
            public string PAN { get; set; }
        }

        public void SetCommonParams(SetCommonParamsReq req)
        {
            LogableTask task = LogableTask.NewTask("SetCommonParams");
            try
            {
                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Info, "called");
                if (req.SetCustNum)
                    UserSession.CurrentSession.CustNum = req.CustNum;

                if (req.SetPAN)
                    UserSession.CurrentSession.PAN = req.PAN;
            }
            catch (Exception ex)
            {
                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Error, ex);
            }
            finally { task.EndTask(); }
        }

        public class StartUpData
        {
            public string Lang { get; set; }
            public string MachineID { get; set; }
            public string TSN { get; set; }
            public string PAN { get; set; }
            public bool ReuseSession { get; set; }
        }

        
        public void SetStartUpData(StartUpData req)
        {
            LogableTask task = LogableTask.NewTask("SetStartupData");
            try
            {
                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Info, "Session started, TSN = " + req.TSN);

                if (req.ReuseSession == false)
                {
                    UserSession.CurrentSession = new UserSession();

                    UserSession.CurrentSession.Language = req.Lang == "eng" ? Language.Eng : Language.Ara;
                    task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Info, "language set as " + UserSession.CurrentSession.Language);
                    SessionController.MachineID = UserSession.CurrentSession.MachineID = req.MachineID;
                    UserSession.CurrentSession.PAN = req.PAN;
                    var token = GetRequesterAuthorizer(req.PAN);
                    UserSession.CurrentSession.Requester = token.Data.requester;
                    UserSession.CurrentSession.Authorizer = token.Data.authorizer;
                    UserSession.CurrentSession.SessionData = new List<KeyValuePair<string, string>>();
                }
                else
                    task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Info, "Reusing session");

                UserSession.CurrentSession.TSN = req.TSN;
            }
            catch (Exception ex)
            {
                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Error, ex);
            }
            finally { task.EndTask(); }
        }

        public class SessionDataToSet
        {
            public string key { get; set; }
            public string value { get; set; }

        }
        [HttpPost("SetSessionData")]
        public Response<BasicResponse> SetSessionData([FromBody]SessionDataToSet req)
        {
            try
            {
                lock (UserSession.CurrentSession)
                {
                    LogableTask.LogSingleActivity("SetStartupData", "SetStartupData", TraceLevel.Info, $"going to save session key {req.key} as {MaskingUtil.MasKPANInString(req.value)}");
                    UserSession.CurrentSession.SessionData.Add(new KeyValuePair<string, string>(req.key, req.value));

                    LogableTask.LogSingleActivity("New SessionData", MethodBase.GetCurrentMethod(), TraceLevel.Info, JsonConvert.SerializeObject(UserSession.CurrentSession.SessionData));

                }

                return new Response<BasicResponse>
                {
                    Success = true
                };

            }
            catch (Exception ex)
            {
                LogableTask.LogSingleActivity("SetSessionData", MethodBase.GetCurrentMethod(), TraceLevel.Error, ex);
                return new Response<BasicResponse>
                {
                    Success = false
                };
            }

        }
        public string GetSessionVar(string key)
        {
            return UserSession.CurrentSession.SessionData.SingleOrDefault(a => a.Key == key).Value;
        }

        public MemoryStream GetAllSessionData(string key)
        {
            return new MemoryStream(UTF8Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(UserSession.CurrentSession.SessionData)));
        }
    }
}
