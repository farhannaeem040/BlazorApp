using ANBBusinessServices.TerminalRequests;
using ANBBusinessServices.TerminalResponses;
using ConceptsClient.AppData;
using DTOs.TerminalResponses;
using Lib;
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
    public class STCPayController : ConnectionsController
    {
        public int TotalQRCodeDisplayTime { get; set; }
        public int ElapsedDisplayTime { get; set; }
        Timer FindTrx;
        public QRCodeInfoResponse qRCodeInfoResponse { get; set; }



        public QRCodeInfoResponse Start()
        {
            qRCodeInfoResponse = GetNextQRCodeInfo();
            return qRCodeInfoResponse;
        }
        public void onLoaded(string QRCodeSeqNo)
        {



        }
        public bool QRImageLoaded([FromBody]SingleFieldRequest<string> request)
        {
            var task = LogableTask.NewTask("QRImageLoaded");
            try
            {
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
                        StagedTrxDetails getQRTrxDetails = this.GetQRTrxDetails(new SingleFieldRequest<string> { Data = request.Data });

                        if (getQRTrxDetails.TrxFound)
                        {
                            Console.WriteLine(getQRTrxDetails.TrxFound);
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
                CreateNewSession();
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

    }
}
