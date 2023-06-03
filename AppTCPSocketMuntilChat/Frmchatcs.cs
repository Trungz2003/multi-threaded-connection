using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppTCPSocketMuntilChat
{
    public partial class Frmchatcs : Form
    {
        public  chatExtension _client;
        public Frmchatcs(chatExtension Client)
        {
            this._client = Client;
            InitializeComponent();
        }

        private void Frmchatcs_Load(object sender, EventArgs e)
        {
            this.Text = "Chat with Client ID: " + _client.ClientID;

            //bat dau cho chat
            send(_client.socket, "server-bat dau-true");
        }

        

        private void btnSend_Click(object sender, EventArgs e)
        {
            send(_client.socket, "Server - "+txtMessager.Text+"- true");

            //add to listview
            AddTolistView("Server - " + txtMessager.Text);

        }
        private void AddTolistView(string packet)
        {
            string username = packet.Split('-')[0];
            string message = packet.Split('-')[1];
            string ip = "Local";

            ListViewItem lvi = new ListViewItem(new string[] { username, message, ip });
            lsvMesseger.Items.Add(lvi);
        }
        void send(Socket client,string packet)
        {
            try
            {
                if (client != null && !String.IsNullOrEmpty(packet))
                {
                    string data = packet;

                    client.Send(Serialize(data));
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }
        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter Formatter = new BinaryFormatter();
            Formatter.Serialize(stream, obj);

            return stream.ToArray();
        }

        
        private void Frmchatcs_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(Settings.frmchatcs.Count==0)
            {
                return;
            }
            Settings.frmchatcs.Remove(this);

            send(_client.socket, "server-ket thuc-false");
        }
    }
}
