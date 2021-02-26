using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;



namespace MyServer
{


    public class SynchronousSocketListener
    {
        static List<Socket> handlers = new List<Socket>();
        public static List<string> ips = new List<string>();
        //public static Dictionary<string, string> ipsAndNames = new

        public static void HandleClients(Socket listener)
        {

            bool serverDone = false;
            while (!serverDone)
            {
                //Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.  
                Socket handler = listener.Accept();
                handlers.Add(handler);
                IPEndPoint ip = handler.RemoteEndPoint as IPEndPoint;
                ips.Add(ip.Address.ToString());
                Console.WriteLine("conection added");
            }
        }


        public static string HandleSingleClient(string commanFromClient)
        {
            int clientPosition = 0;
            bool clientDone = false;
            bool serverDone = false;
       
            Console.WriteLine("Please enter command: ");
            Console.WriteLine("commanFromClient: " + commanFromClient);

            if (Form1.selectedIp.Equals(""))
                return "No client selected, please choose one from the list.";

            clientPosition = ips.IndexOf(Form1.selectedIp);
            if (clientPosition == -1)
                return "Error with selected client, please refresh the list.";

            string msgLen = commanFromClient.Length.ToString().PadLeft(10, '0');

            // Echo the data back to the client.  
            byte[] msg = Encoding.ASCII.GetBytes(msgLen + commanFromClient);

            handlers[clientPosition].Send(msg);

            String command = commanFromClient.Split(' ')[0];
            if (command.Equals("done"))
            {
                clientDone = true;
            }
            else if (command.Equals("quit"))
            {
                clientDone = true;
                serverDone = true;
            }

            if (command.Equals("send_file"))
            {
                String filePath = @"C:\Users\shay\Desktop\server\" + commanFromClient.Split(' ')[1].Split('\\').Last();
                Console.WriteLine(filePath);
                // Create a temporary file, and put some data into it.
                using (FileStream fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    byte[] bytes = new Byte[1024];
                    byte[] msgLenBytes = new byte[10];
                    handlers[clientPosition].Receive(msgLenBytes);
                    int sizeToRead = Int32.Parse(Encoding.ASCII.GetString(msgLenBytes));

                    Console.WriteLine("ab");
                    int bytesRec;
                    // An incoming connection needs to be processed.  
                    while (sizeToRead > 0)
                    {
                        if (sizeToRead > 1024)
                        {
                            bytesRec = handlers[clientPosition].Receive(bytes);
                        }
                        else
                        {
                            bytesRec = handlers[clientPosition].Receive(bytes, sizeToRead, 0);
                        }
                        // Add some information to the file.
                        fs.Write(bytes, 0, bytesRec);
                        sizeToRead -= bytesRec;
                    }
                }
            }

            String clientResponse = ReciveMessageFromClient(handlers[clientPosition]);

            Console.WriteLine("response"+clientResponse);
            if (commanFromClient.Equals("done")){
                handlers[clientPosition].Shutdown(SocketShutdown.Both);
                handlers[clientPosition].Close();
            }
            if (clientDone)
            {
                ips.Remove(Form1.selectedIp);
                handlers.RemoveAt(clientPosition);
            }
            return clientResponse;

        }

        public static String ReciveMessageFromClient(Socket sender)
        {
            byte[] size = new byte[10];

            // Receive the response from the remote device.  
            int sizeLen = sender.Receive(size);

            int rcvSize = Int32.Parse(Encoding.ASCII.GetString(size, 0, sizeLen));

            byte[] bytes = new byte[rcvSize];
            int bytesRec = sender.Receive(bytes);

            String msg = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            return msg;
        }
    }
}
