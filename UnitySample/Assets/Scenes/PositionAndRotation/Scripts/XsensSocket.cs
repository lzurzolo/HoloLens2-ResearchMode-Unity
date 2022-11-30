using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif
using System.IO;


public class XsensSocket : MonoBehaviour
{
    private const string SEND_SOCKET_PORT_NUMBER = "6004";
    private const string RECEIVE_PORT_NUMBER = "1524";
    private const string SEND_IP_ADDRESS = "192.168.1.2";
    public Director director;
#if WINDOWS_UWP
    private DatagramSocket _receiveSocket;
    private DatagramSocket _sendSocket;
#endif

    private void Start()
    {
#if WINDOWS_UWP
    _receiveSocket = new DatagramSocket();
    _receiveSocket.MessageReceived += MessageReceived;
    _receiveSocket.BindServiceNameAsync(RECEIVE_PORT_NUMBER);
    _sendSocket = new DatagramSocket();
#endif
    }

#if WINDOWS_UWP
    void MessageReceived(DatagramSocket socket, DatagramSocketMessageReceivedEventArgs args)
    {
        DataReader reader = args.GetDataReader();
        uint len = reader.UnconsumedBufferLength;
        string msg = reader.ReadString(len);

        if(msg.Contains("CaptureStart")) director.StartXsensRecording();

        string remoteHost = args.RemoteAddress.DisplayName;
        reader.Dispose();
    }

    public async void StartXsensRecording()
    {
        string request = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n<StartRecordingReq><StartTime VALUE=\"\"/>\n</StartRecordingReq>";

        _sendSocket.BindServiceNameAsync("9861");
        var host = new HostName(SEND_IP_ADDRESS);

        using (_sendSocket = new Windows.Networking.Sockets.DatagramSocket())
        {
            using (Stream outputStream = (await _sendSocket.GetOutputStreamAsync(host, SEND_SOCKET_PORT_NUMBER)).AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }
        }

        //_sendSocket.Dispose();
    }

    public async void StopXsensRecording()
    {
        string request = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n<StopRecordingReq><StopTime VALUE=\"\"/>\n</StopRecordingReq>";

        //_sendSocket.BindServiceNameAsync("9861");
        var host = new HostName(SEND_IP_ADDRESS);

        using (_sendSocket = new Windows.Networking.Sockets.DatagramSocket())
        {
            using (Stream outputStream = (await _sendSocket.GetOutputStreamAsync(host, SEND_SOCKET_PORT_NUMBER)).AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }
        }

        _sendSocket.Dispose();
    }
#endif
}
