using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SynchronousSocketListener
{

    // Incoming data from the client.  
    public static string data = null;

    public static void StartListening(string name, int port)
    {
        // Data buffer for incoming data.  
        byte[] bytes = new Byte[1024];

        // Establish the local endpoint for the socket.  
        // Dns.GetHostName returns the name of the
        // host running the application.  
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and
        // listen for incoming connections.  
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.  
            while (true)
            {
                Console.WriteLine("Server " + name + " waiting for a connection on port " + port);
                // Program is suspended while waiting for an incoming connection.  
                Socket handler = listener.Accept();
                data = null;

                // An incoming connection needs to be processed.  
                while (true)
                {
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                string returnMessage = "";

                // Show the data on the console.  
                Console.WriteLine("Text received : {0}", data);

                // If we are root, respond
                if (name == "root")
                {
                    returnMessage = "server1:" + data.Substring(0, data.Length - 9);

                    if (data.Contains(".com"))
                    {
                        returnMessage += ",5001";
                    }
                    else if (data.Contains(".edu"))
                    {
                        returnMessage += ",5002";
                    }
                }

                // Echo the data back to the client.  
                byte[] msg = Encoding.ASCII.GetBytes(returnMessage);

                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();

    }

    public static int Main(String[] args)
    {
        Console.WriteLine("Enter server name: ");
        string serverName = Console.ReadLine();
        int port = 11000;

        switch (serverName)
        {
            case "root":
                port = 5000;
                break;
            case "com":
                port = 5001;
                break;
            case "edu":
                port = 5002;
                break;
            case "fred":
                port = 5003;
                break;
            case "wilma":
                port = 5004;
                break;
            case "sensei":
                port = 5005;
                break;
            case "senpai":
                port = 5006;
                break;
        }

        StartListening(serverName, port);
        return 0;
    }
}