using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AppTCPSocketMuntilChat
{
    public class chatExtension
    {
       
        public Socket socket { get; set; }

        public string ClientID { get; set; }
    }
}
