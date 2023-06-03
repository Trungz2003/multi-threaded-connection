using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace AppTCPSocketMuntilChat
{
    public partial class server : Form
    {
        private static int onClient;

     
        public server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            connect();
        }


        IPEndPoint IP;
        Socket socketServer;
        List<Socket> clientList;

        // kết nối
        void connect()
        {
            clientList = new List<Socket>();
            IP = new IPEndPoint(IPAddress.Any, 2023);
            socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            socketServer.Bind(IP);
            Thread listen = new Thread(
                () =>
                {
                   try
                    {
                        while (true)
                        {
                            socketServer.Listen(100);
                            Socket client = socketServer.Accept();
                            clientList.Add(client);
                            //add client to list clients
                            onClient++;
                            UpdateWindowTitle(onClient.ToString());
                            
                            Thread receive = new Thread(Receive);
                            receive.IsBackground = true;
                            receive.Start(client);

                        }
                    }
                    catch
                    {
                        IP = new IPEndPoint(IPAddress.Any, 2023);
                        socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    }
                });
            listen.IsBackground = true;
            listen.Start();
        }
        private void AddClientTolistView(Socket client,string id)
        {
            LV_ListClients.Invoke((MethodInvoker)delegate () {


                ListViewItem item = new ListViewItem(new string[] { client.RemoteEndPoint.ToString().Split(':')[0],id }) { Tag = client,Name = id};
                LV_ListClients.Items.Add(item);
            });
        }
        private void ClearClientInListView(Socket client)
        {
            LV_ListClients.Invoke((MethodInvoker)delegate ()
            {
                foreach (ListViewItem lvi in LV_ListClients.Items.Cast<ListViewItem>().Where(lvi => lvi != null && client.Equals(lvi.Tag)))
                {
                    lvi.Remove();
                    break;
                }


            });

        }
        // đóng kết nối
        void close()
        {
            socketServer.Close();
        }


        // nhận tin
        void Receive(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);
                    string message = (string)Deserialize(data);
                    if (message.Contains("indentification"))
                    {
                        //add id to listview
                        AddClientTolistView(client, message.Split('-')[0]);

                    }
                    else
                    {
                        SendMessageToFrom(client, message);
                        //addMessage(message, client, false);

                    }

                    foreach (Socket item in clientList)
                    {
                        if(item != null && item != client)
                        item.Send(Serialize(message));
                    }

                }
            }
            catch
            {
                clientList.Remove(client);
                client.Close();

                //clear client in listview
                ClearClientInListView(client);
                onClient--;
                UpdateWindowTitle(onClient.ToString());
                CloseFormChat(client);
                //close form chat


            }
        }
        private void CloseFormChat(Socket client)
        {
            try
            {
                foreach (var frm in Settings.frmchatcs)
                {
                    if (frm.IsDisposed)
                    {
                        continue;
                    }
                    if (frm._client.socket.Equals(client))
                    {
                        frm.Invoke((MethodInvoker)delegate () {

                            frm.Close();
                        });
                    }
                }


            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
        private void SendMessageToFrom(Socket client,string packet)
        {
            foreach( var frm in Settings.frmchatcs)
            {
                if (frm.IsDisposed)
                {
                    continue;
                }
                if (frm._client.socket.Equals(client))
                {
                    string username = packet.Split('-')[0];
                    string message = packet.Split('-')[1];
                    string ip = client.LocalEndPoint.ToString().Split(':')[0];

                    //add to form
                    ListViewItem lvi = new ListViewItem(new string[] { username, message, ip });

                    frm.Invoke((MethodInvoker)delegate ()
                    {
                        frm.lsvMesseger.Items.Add(lvi);
                    });
                }
            }

        }

        // phân mảnh
        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter Formatter = new BinaryFormatter();
            Formatter.Serialize(stream, obj);

            return stream.ToArray();
        }

        // gom mảnh
        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter Formatter = new BinaryFormatter();
            return Formatter.Deserialize(stream);


        }

        // đóng kết nối
        private void server_FormClosed(object sender, FormClosedEventArgs e)
        {
            close();
        }

        private void UpdateWindowTitle(string title)
        {
            this.Text="Client Online: " + title;

        }
        private chatExtension[] GetSelectedClients()
        {
            
            List<chatExtension> clients = new List<chatExtension>();
            this.Invoke((MethodInvoker)delegate
            {

                foreach(ListViewItem lvi in LV_ListClients.SelectedItems)
                {
                    Socket client = (Socket)lvi.Tag;
                    string clientID = lvi.SubItems[1].Text;

                    clients.Add(new chatExtension() { socket = client,ClientID=clientID });
                   
                }
            });

           
            return clients.ToArray();
        }

        private void chatToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            chatExtension[] sockets = GetSelectedClients();

            foreach(var socket in sockets)
            {
                Frmchatcs frm = new Frmchatcs(socket);
                Settings.frmchatcs.Add(frm);
                frm.Show();
            }


        }
    }

    public static class Settings
    {
        public static List<Frmchatcs> frmchatcs = new List<Frmchatcs>();
    }
}
