using ANBBusinessServices.TerminalRequests;
using ANBBusinessServices.TerminalResponses;
using Lib;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Reflection;

namespace ConceptsClient.AppData
{
    public class ServerHelper
    {
        static int CSN = int.Parse(DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + "000");
        /// <summary>
        /// throws (WebException e) when (e.Status == WebExceptionStatus.Timeout)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subUrl"></param>
        /// <param name="request"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Response<T> GetResponse<T>(string subUrl, BaseTerminalRequest request, bool waitForCustomerDetails = true)
        {
            LogableTask task = LogableTask.NewTask("Posting to " + subUrl);
            try
            {
                if (waitForCustomerDetails)
                {
                    if (UserSession.CurrentSession.CustomerDetailsStatus.WaitOne(1000 * 150) == false)
                        task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Error, "wait of 150 sec expired for customer details ");

                    if (UserSession.CurrentSession.CustomerDetailsResponse == null)
                    {
                        task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Error, "CustomerDetailsResponse is null");
                        return new Response<T>
                        {
                            Success = false,
                            ErrorCode = "Exception",
                            ErrorDesc = "UserSession.CurrentSession.CustomerDetailsResponse is null"
                        };
                    }

                    if (UserSession.CurrentSession.CustomerDetailsResponse.Success == false)
                    {
                        task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Error,
                            "customer details was failed. " + UserSession.CurrentSession.CustomerDetailsResponse.ErrorDesc);
                        return new Response<T>
                        {
                            Success = false,
                            ErrorCode = UserSession.CurrentSession.CustomerDetailsResponse.ErrorCode,
                            ErrorDesc = UserSession.CurrentSession.CustomerDetailsResponse.ErrorDesc
                        };
                    }
                }
                request.CommonParams = ServerHelper.GetCommonParameters();
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, "sending req to " + subUrl);

                return GetResponse<T>(subUrl, request, task);
            }
            catch (Exception ex)
            {
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Error, ex);
                throw ex;
            }
            finally { task.EndTask(); }
        }

        public static Response<T> GetResponse<T>(string subUrl, BaseTerminalRequest request, LogableTaskWithoutEnd task)
        {
            try
            {
                var url = Program.appSettings.ServerUrl + subUrl;
                string req = JsonConvert.SerializeObject(request);
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, Lib.MaskingUtil.MasKPANInString(req));

                string str = Lib.HttpUtil.PostToServerStr(url, req, Program.appSettings.RequestTimeOut);
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Info, Lib.MaskingUtil.MasKPANInString(str));
                var reply = JsonConvert.DeserializeObject<Response<T>>(str);
                if (reply.Success == false)
                    task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Error, reply.ErrorDesc);
                else if (UserSession.CurrentSession != null)
                    UserSession.CurrentSession.SessionID = reply.SessionID;
                return reply;

            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout)
            {
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Warning, ex.Message);
                return new Response<T> { ErrorDesc = UserSession.getServerTimeoutDesc(), Success = false };
            }
            catch (Exception ex)
            {
                task.Log(MethodBase.GetCurrentMethod(), System.Diagnostics.TraceLevel.Warning, ex.Message);
                return new Response<T> { ErrorDesc = ex.Message, Success = false };
            }
        }

        public static string GetNextCSN()
        {
            return (++CSN).ToString();
        }

        public static CommonParameters GetCommonParameters()
        {
            return new CommonParameters
            {
                CSN = GetNextCSN(),
                CustomerID = UserSession.CurrentSession.CustNum,
                Language = UserSession.CurrentSession.Language == Language.Ara ? "ara" : "eng",
                MachineID = UserSession.CurrentSession.MachineID,
                PAN = UserSession.CurrentSession.PAN,
                TerminalType = Program.appSettings.TerminalType,
                TSN = UserSession.CurrentSession.TSN,
                SessionID = UserSession.CurrentSession.SessionID,
                Authorizer = UserSession.CurrentSession.Authorizer,
                Requester = UserSession.CurrentSession.Requester,
                TimeStamp = DateTime.Now.ToString("yyyy-M-d H:m:s")

            };
        }


    }
}
