using ConceptsClient.AppData;
using ConceptsClient.Controllers.Common;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConceptsClient.Controllers
{
    public class ConnectionsController 
    {
       public SessionController sessionController { get; set; }
       public CJSAController cJSAController { get; set; }
    
       public void CreateNewSession()
       {
            sessionController = new SessionController();
            cJSAController = new CJSAController();


            sessionController.SetStartUpData( new SessionController.StartUpData { 
                Lang = cJSAController.Lang ,
                MachineID = cJSAController.MachineID ,
                ReuseSession = cJSAController.ReuseSession,
                PAN = cJSAController.PAN
            });

       }


    }
}
