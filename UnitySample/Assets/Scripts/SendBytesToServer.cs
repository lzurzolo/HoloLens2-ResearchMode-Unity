using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

public class SendBytesToServer : MonoBehaviour
{
    [SerializeField]
    string hostIPAddress, port;

    public string outJson;
    public ConcurrentQueue<string> outJsons;

    private void Start()
    {
        outJsons = new ConcurrentQueue<string>();
    }

#if WINDOWS_UWP
    StreamSocket socket = null;
    StreamSocket inputSocket = null;
    public DataWriter dw;
    public DataReader dr;
    public async void StartConnection()
    {
        if (socket != null) socket.Dispose();

        try
        {
            var hostFromFile = File.ReadAllText(Application.persistentDataPath + @"\host.txt");
            var portFromFile = File.ReadAllText(Application.persistentDataPath + @"\port.txt");

            inputSocket = new StreamSocket();
            var hostName = new Windows.Networking.HostName(hostFromFile);
            await inputSocket.ConnectAsync(hostName, "4002");
            dw = new DataWriter(inputSocket.OutputStream);
            dr = new DataReader(inputSocket.InputStream);
            dr.InputStreamOptions = InputStreamOptions.Partial;
        }
        catch(Exception ex)
        {
            SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            Debug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
        }
    }
    
    private void StopConnection()
    {
        dr?.DetachStream();
        dr?.Dispose();
        dr = null;
    }
    
    public async Task GetLandmarksFromServer()
    {
        try
        {
            // ack server
            dw.WriteInt32(42);

            await dw.StoreAsync();
            await dw.FlushAsync();

            UInt32 count = 4;
            UInt32 read = await dr.LoadAsync(count);
            Debug.Log("Count " + read);
            if(read != count)
            {
                dr.ReadBuffer(read);
                return;
            }
            var size = dr.ReadUInt32();
            Debug.Log("Size " + size);
            if(size == 0)
            {
                return;
            }
            var totalRead = await dr.LoadAsync(size);
            Debug.Log("Read " + totalRead);

            while(totalRead < size)
            {
                var justRead = await dr.LoadAsync(size - totalRead);
                totalRead += justRead;
            }

            byte[] buff = new byte[size];
            dr.ReadBytes(buff);
            
            outJson = Encoding.UTF8.GetString(buff, 0, buff.Length);
            Debug.Log(outJson);
            outJsons.Enqueue(outJson);
        }
        catch (Exception ex)
        {
            Debug.Log("EXCEPTION " + ex.Message);
        }
    }
#endif
}
