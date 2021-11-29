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


namespace ProxyBI
{
    class Program
    {
        static void Main(string[] args)
        {
            // command line parameters
            // -p=<port>            optional      port to listen on
            // -ip=<localaddress>   optional      local ip to bind for socket

            //  INVOKE URL http://127.0.0.1:18080/ssh::host::username::password/getFile/_filepath_

            int sck_port = 18181;
            string sck_bind_addr = "127.0.0.1";
            foreach (String s in args)
            {
                if (s.StartsWith("-h")|| s.StartsWith("-?") || s.StartsWith("-help") || s.StartsWith("--help")) {
                    printUsage();
                    return;
                }else if (s.StartsWith("-p="))
                {
                    try
                    {
                        int sck_port_temp = Int32.Parse(s.Substring(s.IndexOf("=") + 1));
                        if (!(sck_port_temp<=65535 && sck_port_temp>0))
                        {
                            Console.Error.WriteLine("ERROR : parameter -p does not contain a VALID tcp port. It should be >0 and <65535");
                        }else
                        {
                            sck_port = sck_port_temp;
                        }
                    }catch(Exception e)
                    {
                        Console.Error.WriteLine("ERROR : parameter -p does not contain an integer value.");
                    }
                }else if (s.StartsWith("-ip="))
                {
                    try
                    {
                        string ip_temp = s.Substring(s.IndexOf("=")+1);
                        System.Text.RegularExpressions.Regex is_ip = new System.Text.RegularExpressions.Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                        
                        if ( is_ip.Match(ip_temp).Success )
                        {
                            Console.Out.WriteLine("INFO : ProxyBI will bind on local ip address " + ip_temp );
                            sck_bind_addr = ip_temp;
                        }
                        else
                        {
                            Console.Error.WriteLine("ERROR : ip address provided on commandline ("+ip_temp+") is invalid. ProxyBI will bind on local ip address " + sck_bind_addr);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("ERROR : parameter -p does not contain an integer value.");
                    }
                }else
                {
                    printUsage();
                    return;
                }


            }

            Console.Out.WriteLine("## ProxyBI - http>to>ssh file access proxy.                       ##");
            Console.Out.WriteLine("##           Copyright Giuliano Caglio 2016                       ##");
            Console.Out.WriteLine("##                                                                ##");
            Console.Out.WriteLine("## This software is released open source under Apache License 2.0 ##");
            Console.Out.WriteLine("## More details at http://www.apache.org/licenses/                ##");
            Console.Out.WriteLine();

            Console.Out.WriteLine("INFO : ProxyBI started.");
            AsynchronousSocketListener.StartListening(sck_port,sck_bind_addr);
            Console.WriteLine("INFO : ProxyBI ended. Goodbye.");
        }

        /// <summary>
        /// Print usage information
        /// </summary>
        private static void printUsage()
        {
            Console.Out.WriteLine(" USAGE : ProxyBI.exe  [-ip=<ip address>] [-p=<port>] ");
            Console.Out.WriteLine();
            Console.Out.WriteLine("   -ip=<ip address>      specify the local IPv4 address to bind on.");
            Console.Out.WriteLine("                         Default is localhost 127.0.0.1");
            Console.Out.WriteLine();
            Console.Out.WriteLine("   -p=<port>             specify the local tcp port to listen on.");
            Console.Out.WriteLine("                         Default is tcp port 18181");
            Console.Out.WriteLine();
            Console.Out.WriteLine();
            Console.Out.WriteLine(" ProxyBI listen for HTTP requests on the specified ip/port and perform a ");
            Console.Out.WriteLine(" 'cat' on the file specified, connecting to the remote system with the ");
            Console.Out.WriteLine(" supplied protocol/host/credentials. ");
            Console.Out.WriteLine();
            Console.Out.WriteLine(" EXAMPLE :");
            Console.Out.WriteLine("  >  This will get the content of the file   /home/canti/myData.csv");
            Console.Out.WriteLine("     from the remote host                    192.168.111.4");
            Console.Out.WriteLine("     with username                           canti");
            Console.Out.WriteLine("     with ssh key                            c:\\temp\\mykey.rsa.key");
            Console.Out.WriteLine("     http://127.0.0.1:18181/ssh::192.168.111.4::canti::c:%5Ctemp%5Cmykey.rsa.key/getFile/etc/passwd");
            Console.Out.WriteLine();
            Console.Out.WriteLine("  >  This will get the content of the file   /home/canti/myData.csv");
            Console.Out.WriteLine("     from the remote host                    192.168.111.4");
            Console.Out.WriteLine("     with username                           canti");
            Console.Out.WriteLine("     with password                           myPassw0rd");
            Console.Out.WriteLine("     http://127.0.0.1:18181/ssh::192.168.111.4::canti::myPassw0rd/getFile/etc/passwd");
            Console.Out.WriteLine();
            Console.Out.WriteLine();

        }
    }
}
