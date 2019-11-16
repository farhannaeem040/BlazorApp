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
        public ConnectionsController()
        {

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
