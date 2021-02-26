using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyServer
{
    public partial class Form1 : Form
    {
        private delegate void AddListItem();
        public static string selectedIp;
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();
            selectedIp = "";
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and
            // listen for incoming connections.  


            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.  
            Thread thread = new Thread(() => SynchronousSocketListener.HandleClients(listener));
            thread.Start();
        }

        private void AddItems()
        {
            bool updated = false;
            foreach (string ip in SynchronousSocketListener.ips)
            {
                if (!this.Clients.Items.Contains(ip))
                {
                    updated = true;
                    this.Clients.Items.Add(ip);
                }
            }
            string[] ips = new string[this.Clients.Items.Count];
            this.Clients.Items.CopyTo(ips, 0);
            foreach (string ip in ips)
            {
                if (!SynchronousSocketListener.ips.Contains(ip))
                {
                    updated = true;
                    this.Clients.Items.Remove(ip);
                }
            }
            if (updated)
                this.Clients.Update();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string txt = textBox1.Text;
            string answer = SynchronousSocketListener.HandleSingleClient(txt);
            MessageBox.Show(answer);

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "";
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedIp = (string) this.Clients.SelectedItem;
        }
 
        public static int Main(String[] args)
        {
            //fuck ari stein
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form = new Form1();
            myTimer.Tick += new EventHandler(form.Refresh_Click);
            myTimer.Interval = 2000;
            myTimer.Start();
            form.ShowDialog();
            return 0;
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            this.Invoke(new AddListItem(AddItems));
        }
    }
}
