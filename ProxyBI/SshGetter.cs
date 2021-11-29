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
using System.Collections;
using Renci.SshNet;

namespace ProxyBI
{

    public class SshGetter
    {
        public static string getFileContent(string hostname_or_ip, int ssh_port, string username, string password, string filepath)
        {
            try
            {
                SshClient cSSH = new SshClient(hostname_or_ip, ssh_port, username, password);
                cSSH.Connect();
                SshCommand x = cSSH.RunCommand("cat " + filepath);

                cSSH.Disconnect();
                cSSH.Dispose();
                return x.Result;
            }
            catch (Renci.SshNet.Common.SshAuthenticationException) {
                Console.Error.Write("ERROR : authentication error. Please check connectivity and credentials.");
                return "";
            }
            catch (Renci.SshNet.Common.SshConnectionException) {
                Console.Error.Write("ERROR : error in ssh connection. Please check connectivity and credentials.");
                return "";
            }
            catch (Exception e)
            {
                Console.Error.Write("ERROR : generic error in remote connection handling. Please check connectivity and credentials.");
                return "";
            }
        }


        public static string getFileContentWithKey(string hostname_or_ip, int ssh_port, string username, string private_key_path, string filepath)
        {
            try
            {

                SshClient cSSH = new SshClient(hostname_or_ip, ssh_port, username, new PrivateKeyFile[] { new PrivateKeyFile(private_key_path) });
                cSSH.Connect();
                SshCommand x = cSSH.RunCommand("cat " + filepath);

                cSSH.Disconnect();
                cSSH.Dispose();
                return x.Result;
            }
            catch (Renci.SshNet.Common.SshAuthenticationException)
            {
                Console.Error.Write("ERROR : authentication error. Please check connectivity and credentials.");
                return "";
            }
            catch (Renci.SshNet.Common.SshConnectionException)
            {
                Console.Error.Write("ERROR : error in ssh connection. Please check connectivity and credentials.");
                return "";
            }
            catch (Exception e)
            {
                Console.Error.Write("ERROR : generic error in remote connection handling. Please check connectivity and credentials.");
                return "";
            }
        }


        public static string getFileContent(string hostname_or_ip, string username, string password, string filepath)
        {
            return getFileContent(hostname_or_ip, 22, username, password, filepath);
        }
    }
}
