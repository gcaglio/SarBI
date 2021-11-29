/*
 * Copyright 2016 Caglio Giuliano
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */
using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;



namespace ProxyBI
{

    public class AsynchronousSocketListener
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public AsynchronousSocketListener()
        {
        }

        public static void StartListening(int port, string address)
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            IPAddress ipAddress = null;
            IPAddress.TryParse(address, out ipAddress);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {

                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    
                    Console.Out.WriteLine("INFO : Listening on local  " + address + ":" + port);
                    Console.Out.WriteLine("INFO : Waiting for a connection...");

                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ERROR : error during openinig local socket -" + e.ToString());
            }

      
            

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();

                int get_index = content.IndexOf("GET");
                if (get_index > -1)
                {
                    if (content.IndexOf("\n") > -1)
                    {
                        string req_content = content.Substring(get_index+2, content.IndexOf("\n", get_index + 1) - get_index);
                        req_content = req_content.Substring(req_content.IndexOf("//") + 2);

                        Console.WriteLine("INFO : received request string = " + req_content);

                        // formato : http://127.0.0.1:18080/ssh::host::username::password/getFile/_filepath__


                        string payload = req_content.Substring(req_content.IndexOf("/")+1);

                        string conn_details = payload.Substring(0, payload.IndexOf("/"));
                        string operation_details = payload.Substring(payload.IndexOf("/")+1);

                        int protocol_end = conn_details.IndexOf("::");
                        int hostname_end = conn_details.IndexOf("::", protocol_end + 3);
                        int username_end = conn_details.IndexOf("::", hostname_end + 3);

                        string protocol = conn_details.Substring(0, protocol_end);
                        string hostname = conn_details.Substring(protocol_end+2, hostname_end - protocol_end -2);
                        string username = conn_details.Substring(hostname_end+2, username_end - hostname_end  - 2);
                        string pwd = conn_details.Substring(username_end + 2);


                        // test if "password" is an URI with a path-to-the-key or is a password
                        bool pwd_is_a_uri_to_key = true;
                        string decodedUrl = Uri.UnescapeDataString(pwd);
                        try
                        {

                            Uri outUri = null;
                            bool test_uri_absolute = Uri.TryCreate(decodedUrl, UriKind.Absolute, out outUri);
                            bool test_uri_relative= Uri.TryCreate(decodedUrl, UriKind.Relative, out outUri);
                            bool test_uri_both = Uri.TryCreate(decodedUrl, UriKind.RelativeOrAbsolute, out outUri);

                            pwd_is_a_uri_to_key = test_uri_both || test_uri_absolute || test_uri_relative ;
                        }
                        catch(Exception e)
                        {
                            pwd_is_a_uri_to_key = false;
                        }

                
                        if (pwd_is_a_uri_to_key)
                        {
                            Console.Out.WriteLine("INFO : password field ("+pwd+") seems to contain a path to a private key.");
                            Console.WriteLine("INFO : connecting to " + hostname + " as " + username + " with key " + decodedUrl);
                        }
                        else
                        {
                            Console.Out.WriteLine("INFO : password field seems to contain an 'interactive' password (i'll not show it).");
                            Console.WriteLine("INFO : connecting to " + hostname + " as " + username + " with password.");
                        }
                        

                        string action = operation_details.Substring(0, operation_details.IndexOf("/"));
                        if (action == "getFile")
                        {
                            string get_filename = operation_details.Substring(operation_details.IndexOf("/"));
                            if (get_filename.IndexOf("HTTP") > -1)
                                get_filename = get_filename.Substring(0, get_filename.IndexOf("HTTP"));

                            Console.Out.WriteLine("INFO : invoked action '" + action + "' for file " + get_filename);

                            String s = "";
                            if (!pwd_is_a_uri_to_key)
                            {
                                SshGetter.getFileContent(hostname, username, pwd, get_filename);
                            }else
                            {
                                SshGetter.getFileContentWithKey(hostname, 22, username, decodedUrl, get_filename);
                            }


                            if (s.Trim().Length==0)
                            {
                                Console.Out.WriteLine("WARNING : remote command returned 0 lines output!");
                            }else
                            {
                                Send(handler, s);
                                Console.Out.WriteLine("DEBUG : command output follows.\r\n" + s);
                            }

                            Console.Out.WriteLine();

                        }
                        else
                        {
                            Console.Error.WriteLine("ERROR : " + action + " is an unrecognized action for ProxyBI.");
                        }


                    }
                    else
                    {
                        string req_content = content.Substring(get_index);
                        Console.WriteLine("INFO : received request string = " + req_content);
                    }
                    //Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }



                /*            if (content.IndexOf("<EOF>") > -1)
                            {
                                // All the data has been read from the 
                                // client. Display it on the console.
                                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                                // Echo the data back to the client.
                                Send(handler, content);
                            }
                            else
                            {
                                // Not all data received. Get more.
                                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                                new AsyncCallback(ReadCallback), state);
                            }
                */
            }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}