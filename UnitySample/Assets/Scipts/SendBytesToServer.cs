using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;

#if WINDOWS_UWP
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

public class SendBytesToServer : MonoBehaviour
{
    [SerializeField]
    string hostIPAddress, port;

    public string outJson;
    public ConcurrentQueue<string> outJsons;
    public TMPro.TMP_Text debugText;

    private void Start()
    {
        outJsons = new ConcurrentQueue<string>();
    }

    private bool _connected = false;

#if WINDOWS_UWP
    StreamSocket socket = null;
    public DataWriter dw;
    public DataReader dr;
    public async void StartConnection()
    {
        if (socket != null) socket.Dispose();

        try
        {
            var hostFromFile = File.ReadAllText(Application.persistentDataPath + @"\host.txt");
            var portFromFile = File.ReadAllText(Application.persistentDataPath + @"\port.txt");

            socket = new StreamSocket();
            var hostName = new Windows.Networking.HostName(hostFromFile);
            await socket.ConnectAsync(hostName, portFromFile);
            dw = new DataWriter(socket.OutputStream);
            dr = new DataReader(socket.InputStream);
            dr.InputStreamOptions = InputStreamOptions.Partial;
            _connected = true;
        }
        catch(Exception ex)
        {
            SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            Debug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
        }
    }
    
    private void StopConnection()
    {
        dw?.DetachStream();
        dw?.Dispose();
        dw = null;

        dr?.DetachStream();
        dr?.Dispose();
        dr = null;

        socket?.Dispose();
        _connected = false;
    }

    public async void Publish(int length, byte[] data)
    {
        try
        {
            dw.WriteInt32(length);
            dw.WriteBytes(data);
            await dw.StoreAsync();
            await dw.FlushAsync();
            UInt32 count = 4;
            UInt32 read = await dr.LoadAsync(count);
            if(read != count)
            {
                //debugText.text = "Count is 4, read is: " + read; 
                dr.ReadBuffer(read);
                return;
            }
            var size = dr.ReadUInt32();
            if(size == 0)
            {
                //debugText.text = "Read a size of 0";
                return;
            }
            read = await dr.LoadAsync(size);
            if(read != size)
            {
                //debugText.text = "read is: " + read + "size is: " + size;
                dr.ReadString(read);
                return;
            }
            outJson = dr.ReadString(size);
            outJsons.Enqueue(outJson);
        }
        catch (Exception ex)
        {

        }
    } 
#endif
}
