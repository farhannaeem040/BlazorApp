using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConceptsClient.Controllers.Common
{
    public class CJSAController
    {
        public string Lang;
        public string MachineID;
        public string PAN;
        public bool ReuseSession;

        public CJSAController()
        {

            this.Lang ="eng";
            this.MachineID = "123456";
            this.ReuseSession = false;
            this.PAN = "999999999999";
        }

       

        public void onCancel()
        {
            if (Program.appSettings.SimulateATM)
                Console.WriteLine("Cancelled Called");

        }
        public void onComplete()
        {
            if (Program.appSettings.SimulateATM)
                Console.WriteLine("onComplete Called");

        }
        public void onTimeOut()
        {
            if (Program.appSettings.SimulateATM)
                Console.WriteLine("Cancelled Called");

        }
    }
}
