using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;


namespace tank_game.util
{
    /// <summary>
    /// Is responsible for communication handling
    /// </summary>
    class Communicator
    {
        #region "Variables"
        private NetworkStream sendStream; //Stream - outgoing
        private TcpClient server; //talk to the server
        private BinaryWriter writer; //To write to the servers

        private NetworkStream readStream; //Stream - incoming        
        private TcpListener listener; //To listen to the server        
        public string readMsg = ""; //reading msg

        private String IP = "127.0.0.1";
        private int SENDING_PORT = 6000;
        private int RECEIVING_PORT = 7000;
        
        private static Communicator com = new Communicator();
        private Map map=new Map();
        #endregion

        private Communicator()
        {
            
        }

        public static Communicator GetInstance()
        {
            return com;
        }
        public void StartListening()
        {
            Thread t = new Thread(ReceiveData);
            t.Start();
        }


        public void ReceiveData()
        {
            bool errorOcurred = false;
            Socket connection = null; //The socket that is listened to       
            try
            {
                //Creating listening Socket
                this.listener = new TcpListener(IPAddress.Parse(IP), RECEIVING_PORT);
                //Starts listening
                this.listener.Start();
                //Establish connection upon server request
                
                while (true)
                {
                    //connection is connected socket
                    connection = listener.AcceptSocket();
                    if (connection.Connected)
                    {
                        //To read from socket create NetworkStream object associated with socket
                        this.readStream = new NetworkStream(connection);

                        SocketAddress sockAdd = connection.RemoteEndPoint.Serialize();
                        string s = connection.RemoteEndPoint.ToString();
                        List<Byte> inputStr = new List<byte>();

                        int asw = 0;
                        while (asw != -1)
                        {
                            asw = this.readStream.ReadByte();
                            inputStr.Add((Byte)asw);
                        }

                        readMsg = Encoding.UTF8.GetString(inputStr.ToArray());
                        //Console.WriteLine("\n" + readMsg);                          //read input
                        this.readStream.Close();

                        map.read(readMsg);
                       
                      
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Communication (RECEIVING) Failed! \n " + e.Message);
                errorOcurred = true;
            }
            finally
            {
                if(connection != null)
                    if(connection.Connected)
                        connection.Close();
                if (errorOcurred)
                    this.ReceiveData();
            }
        }
        public void SendData(String msg)
        {
            
            this.server = new TcpClient();

            try
            {
                {
                    
                    this.server.Connect(IP, SENDING_PORT);
                    if (this.server.Connected)
                    {
                        //To write to the socket
                        this.sendStream = server.GetStream();

                        //Create objects for writing across stream
                        this.writer = new BinaryWriter(sendStream);
                        Byte[] tempStr = Encoding.ASCII.GetBytes(msg);

                        //writing to the port                
                        this.writer.Write(tempStr);
                        Console.WriteLine("\t Data: " + msg + " is written to " + IP);
                        this.writer.Close();
                        this.sendStream.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Communication (WRITING) to " + IP+" Failed! \n " + e.Message);
            }
            finally
            {
                this.server.Close();
            }
        }

    }
}
