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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace MuntiChatClient
{
    public partial class Client : Form
    {
        private static bool isconnect;
        private string ID;
        public Client()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;          
        }
        
        // gửi tin đi
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!isconnect)
            {
                MessageBox.Show("Chua ket noi");
                return;
            }
            send();
            addMessage2(TB_Name.Text+"-"+txtMessager.Text);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            
            
        }


        IPEndPoint IP;
        Socket socketClient;

        // kết nối
        void connect()
        {
            Task ts = new Task(() => {

                while (true)
                {
                    IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2023);
                    socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    try
                    {
                        if (string.IsNullOrEmpty(TB_Name.Text))
                        {
                            MessageBox.Show("Bạn cần nhập tên để kết nối !", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show("Kết nối thành công!", "Thông báo", MessageBoxButtons.OK);
                            socketClient.Connect(IP);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        continue;
                        //Thread.Sleep(5000);
                    }

                    break;
                }
                isconnect = true;

                //Send packet indentification
                socketClient.Send(Serialize(ID + "-" + "indentification"));
                Thread listen = new Thread(receive);
                listen.IsBackground = true;
                listen.Start();


            });
            ts.Start();

        }

        // đóng kết nối
        void close()
        {
            socketClient.Close();
        }

        // gửi tin
        void send()
        {
            if (txtMessager.Text != string.Empty)
            {
                string sendata = TB_Name.Text+"-" + txtMessager.Text;
                try
                {
                    socketClient.Send(Serialize(sendata));

                }
                catch (Exception ex)
                {
                    MessageBox.Show("not connect please connect to server - try again!");
                }
            }
        }

        // nhận tin
        void receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    socketClient.Receive(data);
                    string message = (string)Deserialize(data);
                    addMessage(message);
                }
            }
            catch
            {
                close();
            }
        }

        // add message vào khung chat
        void addMessage(string s)
        {
            string name = s.Split('-')[0];
            string message = s.Split('-')[1];
            bool ischat = Convert.ToBoolean(s.Split('-')[2]);
            if (ischat)
            {
                lsvMesseger.Items.Add(new ListViewItem(new string[] { name, message }) { });
                txtMessager.Clear();
                txtMessager.Enabled = true;
                btnSend.Enabled = true;

            }
            else
            {
                txtMessager.Enabled = false;
                btnSend.Enabled = false;
            }
        }
        void addMessage2(string s)
        {
            string name = s.Split('-')[0];
            string message = s.Split('-')[1];
         
            
             lsvMesseger.Items.Add(new ListViewItem(new string[] { name, message }) { });
              txtMessager.Clear();

           
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

        // đóng kết nối khi đóng form
        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            close();
        }

        
        

        private void mnuThoat_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát ứng dụng không ?", "exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void Client_Load(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Properties.Settings.Default.ID))
            {
                Random rd = new Random();
                Properties.Settings.Default.ID = rd.Next(10000, 99999).ToString();
                Properties.Settings.Default.Save();
            }
            ID = Properties.Settings.Default.ID;
            this.Text = "Client ID: " + ID;

            txtMessager.Enabled = false;
            btnSend.Enabled = false;
        }

        private void btnConnect_Click_1(object sender, EventArgs e)
        {
            connect();

        }

        // abc
    }
}
