using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nanolite_agent.EventSession
{
    public interface IEventSession
    {
        void StartSession();
        void StopSession();
        void WaitSession();
    }
}
