using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System;
using System.Text;
using System.Threading.Tasks;

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
            socket = new StreamSocket();
            var hostName = new Windows.Networking.HostName(hostIPAddress);
            await socket.ConnectAsync(hostName, port);
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
    
    bool lastMessageSent = true;
    public async void SendUINT16Async(ushort[] data1, ushort[] data2)
    {
        if (!lastMessageSent) return;
        lastMessageSent = false;
        try
        {
            // Write header
            dw.WriteString("s"); // header "s" stands for it is ushort array (uint16)

            // Write Length
            dw.WriteInt32(data1.Length + data2.Length);

            // Write actual data
            dw.WriteBytes(UINT16ToBytes(data1));
            dw.WriteBytes(UINT16ToBytes(data2));

            // Send out
            await dw.StoreAsync();
            await dw.FlushAsync();
        }
        catch (Exception ex)
        {
            SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            Debug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
        }
        lastMessageSent = true;
    }

    public async void PingRedis()
    {
        dw.WriteString("ping \"Hello from the Hololens!\"\r\n");
        await dw.StoreAsync();
        await dw.FlushAsync();
        var recv = await dr.LoadAsync(64);
        Debug.Log(dr.ReadString(recv));
    }

    public async void Publish(byte[] data1, byte[] data2)
    {
        byte[] combined = new byte[data1.Length + data2.Length];
        System.Buffer.BlockCopy(data1, 0, combined, 0, data1.Length);
        System.Buffer.BlockCopy(data2, 0, combined, data1.Length, data2.Length);
        dw.WriteBytes(combined);
        await dw.StoreAsync();
        await dw.FlushAsync();
        await dr.LoadAsync(15665);
        var input = dr.ReadString(15665);
        Debug.Log(input);
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
                debugText.text = "Count is 4, read is: " + read; 
                dr.ReadBuffer(read);
                return;
            }
            var size = dr.ReadUInt32();
            if(size == 0)
            {
                debugText.text = "Read a size of 0";
                return;
            }
            read = await dr.LoadAsync(size);
            if(read != size)
            {
                debugText.text = "read is: " + read + "size is: " + size;
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

    byte[] UINT16ToBytes(ushort[] data)
    {
        byte[] ushortInBytes = new byte[data.Length * sizeof(ushort)];
        System.Buffer.BlockCopy(data, 0, ushortInBytes, 0, ushortInBytes.Length);
        return ushortInBytes;
    }
}
