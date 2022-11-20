using log4net;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ViewModels.Communication
{
    public class TCP
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private TcpClient client;
        private NetworkStream clientStream;

        public bool IsConnected
        {
            get { return client != null && client.Connected; }
        }

        /// <summary>
        /// Initialized a new instance of the client and connect it to the remote host
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public Status Connect(string ip, Int32 port)
        {
            try
            {
                log.Debug(String.Format("On Connect"));
                client = new TcpClient()
                {
                    NoDelay = true,
                    ReceiveBufferSize = 1000,
                    SendBufferSize = 1000,
                    SendTimeout = 1000,
                    ReceiveTimeout = 1000
                };
                client.ConnectAsync(IPAddress.Parse(ip), port).Wait(1000);

                if (client.Connected)
                {
                    clientStream = client.GetStream();
                    log.Debug(String.Format("Connected to: {0}@{1}", ip, port));
                    return Status.OK;
                }
                else
                {
                    log.Debug(String.Format("Connection Error: {0}@{1}", ip, port));
                    return Status.ConnectionError;
                }
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
#if (DEBUG)
                log.Error(String.Format("NO!!! Exception"), ex);
#else
                log.Error(String.Format("An unexpected error occurred while processing the connect request!"));
#endif
                return Status.InitializationError;
            }
        }

        /// <summary>
        /// Close the stream and dispose the client
        /// </summary>
        /// <returns></returns>
        public Status Disconnect()
        {
            try
            {
                log.Debug(String.Format("On Disconnect"));
                if (client != null && client.Connected)
                {
                    clientStream.Close();
                    client.Close();
                    log.Info(String.Format("The client disconnected"));
                    return Status.OK;
                }
                else
                {
                    log.Error(String.Format("Disconnection Error"));
                    return Status.ConnectionError;
                }
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
#if (DEBUG)
                log.Error(String.Format("NO!!! Exception"), ex);
#else
                log.Error(String.Format("An unexpected error occurred while processing the disconnect request!"));
#endif
                return Status.UnexpectedError;
            }
        }

        /// <summary>
        /// Send String Command
        /// </summary>
        /// <param name="command">string command</param>
        /// <param name="sleep">sleep time in ms before reading feedback</param>
        /// <param name="symbol">ASCII symbol</param>
        /// <returns>Feedback from the System</returns>
        ///
        public string SendCommand(string command, int sleep = 400, Symbol symbol = Symbol.EOT)
        {
            string feedback = string.Empty;
            try
            {
#if (DEBUG)
                log.Debug(String.Format("Send Command {0}", command));
#endif
                if (clientStream.CanWrite && clientStream.CanRead)
                {
                    ASCIIEncoding encoder = new ASCIIEncoding();
                    byte[] buffer = new byte[1000];
                    byte[] sendBytes;

                    switch (symbol)
                    {
                        case Symbol.EOL:
                            sendBytes = encoder.GetBytes(command + (char)0x0D + (char)0x0A);
                            break;

                        case Symbol.EOT:
                            sendBytes = encoder.GetBytes(command + (char)0x04);
                            break;

                        default:
                            sendBytes = encoder.GetBytes(command);
                            break;
                    }

                    // Clear the data from previews commands
                    while (clientStream.DataAvailable)
                        clientStream.Read(buffer, 0, buffer.Length);

                    clientStream.Write(sendBytes, 0, sendBytes.Length);
                    Thread.Sleep(sleep);

                    while (clientStream.DataAvailable)
                    {
                        int bytes = clientStream.Read(buffer, 0, buffer.Length);
                        feedback += encoder.GetString(buffer, 0, bytes);
                    }
#if (DEBUG)
                    log.Debug(String.Format("Read Feedback {0}", feedback));
#endif
                }
                return feedback;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
#if (DEBUG)
                log.Error(String.Format("NO!!! Exception"), ex);
#else
                log.Error(String.Format("An unexpected error occurred while processing the request!"));
#endif
                return feedback;
            }
        }
    }
}
