using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace ConceptsClient.Controllers.Common
{
    public class State
    {
        public string Url { get; set; }
        public Action Action { get; set; }
    } 

    public class TimeoutController
    {
        public string NormalStateUrl;
        public State TimeoutWarningState;
        public State Timeout;

        System.Threading.Timer StateTimer;
    
        public TimeoutController(string NormalStateUrl, State onTimeout, State TimeoutWarningState, State Timeout, NavigationManager navigationManager)
        {




            this.StateTimer = new System.Threading.Timer((e) =>
            {

                if (this.ElapsedTime > this.TimeoutWarning + this.TimoutExpired)
                {
                    navigationManager.NavigateTo(onTimeout.Url);
                    onTimeout.Action();
                    this.StateTimer.Dispose();
                    //navigate to error
                }
                else if(this.ElapsedTime > this.TimeoutWarning)
                {
                    navigationManager.NavigateTo(onTimeout.Url);
                }
            }
                , null, ElapsedTime, 1000);

        }
      
        public int ElapsedTime { get; set; } = 0;
        public int TimeoutWarning = Program.appSettings.InactivityWarningTime;
        public int TimoutExpired = Program.appSettings.InactivityTime;



    }
}
