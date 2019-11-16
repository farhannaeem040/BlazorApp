using ConceptsClient.AppData;
using ConceptsClient.Controllers.Common;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConceptsClient.Controllers
{
    public delegate void Component(ComponentBase ComponentView);

    public class ConnectionsController : LayoutComponentBase
    {
       public SessionController sessionController { get; set; }
       public void CreateNewSession()
       {
            sessionController = new SessionController();
            sessionController.SetStartUpData( new SessionController.StartUpData { 
                Lang = "eng",
                MachineID = "123456",
                ReuseSession = false,
                PAN = "999999999999"
            });
       }
        public void onTimeoutWarning(Component TimeoutView)
        {

        }
        public void onTimoutComplete(Component TimoutCompletetView)
        {

        }
        public void onError(Component ErrorView)
        {

        }
        public void onCancel(Component CancelView)
        {

        }
        
    }
}
