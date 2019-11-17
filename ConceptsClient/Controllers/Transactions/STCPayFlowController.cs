using ANBBusinessServices.TerminalRequests;
using ANBBusinessServices.TerminalResponses;
using ConceptsClient.AppData;
using ConceptsClient.Pages.STCPay;
using DTOs.TerminalResponses;
using Lib;
using Microsoft.AspNetCore.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ConceptsClient.Controllers.Transactions
{
    public class STCPayUrls
    {
        public static string ShowQrCodePageUrl = "/STCPay/ShowQRCode";
        public static string ShowTrxDetailsPageUrl = "/STCPay/ShowTrxDetails";
        public static string TimeoutPageUrl = "/TimeoutPage";
        public static string CancelPageUrl = "/CancelPage";
        public static string LoadingPageUrl = "/LoadingPage";
        public static string CompletePageUrl = "/CompletePage";

    }



    public class STCPayFlowController
    {

        public ConnectionsController connectionsController { get; set; }
        public STCPayLayout stcPayLayout { get; set; }
        public bool ShowTimeOut { get; set; } = true;
        public QRCodeInfoResponse qRCodeInfoResponse { get; set; }
        public QRCodeInfoResponse GetQRCode()
        {
            qRCodeInfoResponse = GetNextQRCodeInfo();
            return qRCodeInfoResponse;
        }

        public bool QRImageLoaded(SingleFieldRequest<string> request)
        {
            var task = LogableTask.NewTask("QRImageLoaded");
            try
            {

                ShowTimeOut = false;
            
                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Info, "received request");

                ServerHelper.GetResponse<string>("MobileCash/" + MethodBase.GetCurrentMethod().Name, request, false);
                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromSeconds(3);

                this.FindTrx = new System.Threading.Timer((e) =>
                {

                    if (this.ElapsedDisplayTime > this.TotalQRCodeDisplayTime)
                    {
                        this.FindTrx.Dispose();
                        //navigate to error
                    }
                    else
                    {
                        StagedTrxDetails getQRTrxDetails = this.GetQRTrxDetails(new SingleFieldRequest<string>
                        { Data = request.Data });

                        if (getQRTrxDetails.TrxFound)
                        {
                            Console.WriteLine(getQRTrxDetails.TrxFound);
                            this.Amount = getQRTrxDetails.Amount;
                            this.RefNo = getQRTrxDetails.refNo;


                            navigationManager.NavigateTo(STCPayUrls.ShowTrxDetailsPageUrl);

                            this.FindTrx.Dispose();

                        }

                        else
                            this.ElapsedDisplayTime = this.ElapsedDisplayTime + 3;
                    }
                }
                 , null, startTimeSpan, periodTimeSpan);

                return true;
            }
            catch (Exception ex)
            {
                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Error, ex);
                return false;
            }
            finally
            { task.EndTask(); }
        }

        public QRCodeInfoResponse GetNextQRCodeInfo()
        {
            ElapsedDisplayTime = 0;
            TotalQRCodeDisplayTime = Program.appSettings.AdditionalVariables.QRCodeDisplayTime;

            var task = Lib.LogableTask.NewTask("GetNextQRCodeInfo");
            try
            {
                this.connectionsController = new ConnectionsController();
                this.connectionsController.CreateNewSession();



                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Info, "received request");
                QRCodeGenerator qrGenerator = new QRCodeGenerator();

                var qrCodeDetails = ServerHelper.GetResponse<QRCodeInfoResponse>("MobileCash/" + MethodBase.GetCurrentMethod().Name, new BasicRequest(), false);

                if (qrCodeDetails.Success)
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeDetails.Data.QRCodeData, QRCodeGenerator.ECCLevel.Q);
                    QRCode qrCode = new QRCode(qrCodeData);
                    Bitmap qrCodeImage = qrCode.GetGraphic(10);
                    qrCodeImage.Save(Startup.hostingEnvironment.WebRootPath + "\\QrCode.png", ImageFormat.Png);
                    return new QRCodeInfoResponse
                    {
                        QRCodeSeqNo = qrCodeDetails.Data.QRCodeSeqNo,
                        QRCodeData = "QrCode.png?" + qrCodeDetails.Data.QRCodeSeqNo

                    };
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Error, ex);
                return null;
            }
            finally
            { task.EndTask(); }
        }

        public StagedTrxDetails GetQRTrxDetails(SingleFieldRequest<string> qrCodeSeqNoReq)
        {
            var task = LogableTask.NewTask("GetQRTrxDetails");
            try
            {
                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Info, "received request");

                var trxDetails = ServerHelper.GetResponse<StagedTrxDetails>("MobileCash/" + MethodBase.GetCurrentMethod().Name,
                    new SingleFieldRequest<string> { Data = qrCodeSeqNoReq.Data }, false);

                if (trxDetails.Success && trxDetails.Data.HasError == false)
                {
                    if (trxDetails.Data.TrxFound)
                        UserSession.CurrentSession.QRCodeSeqNo = trxDetails.Data.SeqNo;

                    return trxDetails.Data;
                }
                else
                    return new StagedTrxDetails
                    {
                        TrxFound = false
                    };
            }
            catch (Exception ex)
            {
                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Error, ex);
                return new StagedTrxDetails
                {
                    TrxFound = false
                };
            }

            finally
            { task.EndTask(); }
        }

        public bool SendingDispenseReq([FromBody]SingleFieldRequest<string> request)
        {
            var task = LogableTask.NewTask("SendingDispenseReq");
            try
            {
                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Info, "received request");

                ServerHelper.GetResponse<string>("MobileCash/" + MethodBase.GetCurrentMethod().Name, request, false);
                return true;
            }
            catch (Exception ex)
            {
                task.Log(MethodBase.GetCurrentMethod(), TraceLevel.Error, ex);
                return false;
            }
            finally
            { task.EndTask(); }
        }









        //Data Exchange Varibales
        public int TotalQRCodeDisplayTime { get; set; }
        public int ElapsedDisplayTime { get; set; }
        public decimal Amount { get; set; }
        public string RefNo { get; set; }

        public NavigationManager navigationManager { get; set; }

        Timer FindTrx;


    }
}
