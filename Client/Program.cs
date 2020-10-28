using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SynchronousSocketClient
{

    public static void StartClient(string name)
    {
        // Data buffer for incoming data.  
        byte[] bytes = new byte[1024];

        // Connect to a remote device.  
        try
        {
            // Establish the remote endpoint for the socket.  
            // This example uses port 11000 on the local computer.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 5000);

            // Create a TCP/IP  socket.  
            Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            Socket sender2 = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket sender3 = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                sender.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}",
                    sender.RemoteEndPoint.ToString());

                // Encode the data string into a byte array.  
                byte[] msg = Encoding.ASCII.GetBytes(name + "<EOF>");

                // Send the data through the socket.  
                int bytesSent = sender.Send(msg);

                // Receive the response from the remote device.  
                int bytesRec = sender.Receive(bytes);
                string returnMsg = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine("Echoed test = {0}",
                    Encoding.ASCII.GetString(bytes, 0, bytesRec));

                // If we get a return message directing us to another server, forward request there
                if(returnMsg.Contains("server1")) {
                    // Shutdown the first connection
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Disconnect(true);
                    sender.Close();

                    // Open up connection to the new server
                    Console.WriteLine("Forwarding response to next server");
                    sender2.Connect(new IPEndPoint(ipAddress, Int32.Parse(returnMsg.Split(',')[1])));
                    Console.WriteLine("Socket connected to {0}", sender2.RemoteEndPoint.ToString());

                    // Encode and send the request
                    msg = Encoding.ASCII.GetBytes(returnMsg.Split(',')[0].Substring(returnMsg.Split(',')[0].Length - 8) + "<EOF>");
                    sender2.Send(msg);
                }

                // Receive the response from the remote device.  
                bytesRec = sender2.Receive(bytes);
                returnMsg = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

            // If we get a return message directing us to another server, forward request there
            if (returnMsg.Contains("server2"))
            {
               // Shutdown the first connection
               sender2.Shutdown(SocketShutdown.Both);
               sender2.Disconnect(true);
               sender2.Close();

               // Open up connection to the new server
               Console.WriteLine("Forwarding response to next server");
               sender3.Connect(new IPEndPoint(ipAddress, Int32.Parse(returnMsg.Split(',')[1])));
               Console.WriteLine("Socket connected to {0}", sender3.RemoteEndPoint.ToString());

               // Encode and send the request
               msg = Encoding.ASCII.GetBytes("address?<EOF>");
               sender3.Send(msg);
            }

            // Receive the response from the remote device.  
            bytesRec = sender3.Receive(bytes);
            returnMsg = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

            if (returnMsg.Contains("address"))
            {
               // Shutdown the first connection
               sender3.Shutdown(SocketShutdown.Both);
               sender3.Disconnect(true);
               sender3.Close();

               // Open up connection to the new server
               Console.WriteLine(returnMsg);
            }

         }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static int Main(String[] args)
    {
        Console.WriteLine("Enter name to send to server: ");
        string name = Console.ReadLine();

        StartClient(name);
        return 0;
    }
}