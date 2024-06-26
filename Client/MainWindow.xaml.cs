﻿using Microsoft.VisualBasic;
using System.Net.WebSockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.Wave;
using System.Net.Sockets;
using System.Net.Http;
using System.IO;
using System;

namespace Client
{
    public partial class MainWindow : Window
    {
        private TcpClient _tcpClient;
        private WaveInEvent _waveIn;
        private WaveOutEvent _waveOut;
        private BufferedWaveProvider _bufferedWaveProvider;

        bool Conect = false;
        public MainWindow()
        {
            InitializeComponent();

            _waveIn = new WaveInEvent();
            _waveIn.WaveFormat = new WaveFormat(44100, 1); // 44.1kHz, моно

            _waveOut = new WaveOutEvent();
            _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1));
            _waveOut.Init(_bufferedWaveProvider);
        }

        private async Task ReceiveAudioData()
        {
            try
            {
                while (Conect)
                {
                    //if (!_tcpClient.Connected)
                    //{
                    //    // Если соединение разорвано, переподключаемся.
                    //    await Reconnect();
                    //}
                    while (_tcpClient.Connected)
                    {
                        // Читаем длину сообщения
                        //byte[] lengthBuffer = new byte[sizeof(int)];
                        //int lengthBytesRead = await _tcpClient.GetStream().ReadAsync(lengthBuffer, 0, lengthBuffer.Length);

                        ////if (lengthBytesRead != sizeof(int))
                        ////{
                        ////    // Обработка ситуации, когда не удалось прочитать длину сообщения
                        ////    continue;
                        ////}

                        //int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                        byte[] buffer = new byte[_tcpClient.SendBufferSize];

                        int bytesRead = await _tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            _bufferedWaveProvider.AddSamples(buffer, 0, bytesRead);
                            if (_waveOut.PlaybackState != PlaybackState.Playing)
                            {
                                _waveOut.Play();
                            }
                        }
                        else
                        {
                            // Если не было прочитано данных, возможно, соединение разорвано.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving audio data: {ex.Message}");
                // Дополнительная обработка ошибок при приеме данных.
            }
        }

        //private async Task ReceiveAudioData()
        //{
        //    try
        //    {
        //        while (Conect)
        //        {
        //            while (_tcpClient.Connected)
        //            {


        //                byte[] buffer = new byte[8080];

        //                int bytesRead = await _tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
        //                if (bytesRead > 0)
        //                {
        //                    _bufferedWaveProvider.AddSamples(buffer, 0, bytesRead);
        //                    if (!_waveOut.PlaybackState.Equals(PlaybackState.Playing))
        //                    {
        //                        _waveOut.Play();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error receiving audio data: {ex.Message}");
        //        // Дополнительная обработка ошибок при приеме данных.
        //    }
        //}
        private async Task AudioData()
        {
            try
            {
             
                    await ReceiveAudioData();
                    //_ = Task.Run(() => ReceiveAudioData());

                
            }
            catch (Exception ex)
            {
            }
        }
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tcpClient = new TcpClient();
                //await _tcpClient.ConnectAsync("127.0.0.1", 5090); // Измените на IP и порт сервера

                await _tcpClient.ConnectAsync(Text.Text, 49153); // Измените на IP и порт сервера
                Conect = true;

                Task.Run(() => AudioData());

                //Task.Run(() => );
                _waveIn.DataAvailable += WaveIn_DataAvailable;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к серверу: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendAudioDataAsync(byte[] data, int length)
        {
            try
            {
                //Оставляем так как таски
                //_bufferedWaveProvider.AddSamples(data, 0, length);
                     _tcpClient.GetStream().Write(data, 0, length);

                //if (_tcpClient != null && _tcpClient.Connected)
                //{
                //    //_bufferedWaveProvider.AddSamples(data, 0, length);

                //}
                //else
                //{
                //    Console.WriteLine("TCP client is not connected.");
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending audio data: {ex.Message}");
                // Обработка ошибок, если необходимо.
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
             
                //while (Conect)
                //{
                     SendAudioDataAsync(e.Buffer, e.BytesRecorded);

                //}

                //_ = Task.Run(() => SendAudioDataAsync(e.Buffer, e.BytesRecorded));




            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка при отправке аудио данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _waveIn.StopRecording();
                _waveOut.Stop();

                _tcpClient?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отключении от сервера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            DisconnectButton_Click(null, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _waveIn.StopRecording();
            }
            catch (Exception ex)
            {

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                _waveIn.StartRecording();

            }
            catch (Exception ex)
            {

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                _waveOut.Stop();
            }
            catch (Exception ex)
            {

            }
        }
    }
}

//if (_tcpClient != null && _tcpClient.Connected)
//{
//    await _tcpClient.GetStream().WriteAsync(e.Buffer, 0, e.BytesRecorded);

//    _bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
//    //using (MemoryStream ms = new MemoryStream())
//    //{
//    //    int bytesToSend = e.BytesRecorded;
//    //    int offset = 0;

//    //    // Создаем копию буфера для отправки
//    //    byte[] bufferToSend = new byte[bytesToSend];
//    //    Array.Copy(e.Buffer, bufferToSend, bytesToSend);

//    //    //while (bytesToSend > 0)
//    //    //{
//    //    //    using (MemoryStream msы = new MemoryStream())
//    //    //    {
//    //    //        int blockSize = Math.Min(bytesToSend, 1024); // Отправляем блоками по 1024 байта
//    //    //        await _tcpClient.GetStream().WriteAsync(bufferToSend, offset, blockSize);

//    //    //        offset += blockSize;
//    //    //        bytesToSend -= blockSize;
//    //    //    }

//    //}
//    //_bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
//    //}


//}

//else
//{
//    // Логирование ошибки отсутствия соединения
//    Console.WriteLine("TCP client is not connected.");
//}
//s
//private async void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
//{
//    try
//    {
//        byte[] audioBuffer = microphoneEnabled ? e.Buffer : new byte[e.BytesRecorded]; // Если микрофон выключен, отправляем тишину

//        if (_networkStream != null && _tcpClient.Connected)
//        {
//            // Отправляем аудио данные на сервер
//            await _networkStream.WriteAsync(e.Buffer, 0, e.BytesRecorded);
//        }

//        // Добавляем записанные данные в буфер для воспроизведения
//        _bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show($"Ошибка при отправке аудио данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//    }
//}
//_waveIn.DataAvailable += async (sender, e) =>
//{
//    byte[] audioBuffer = microphoneEnabled ? e.Buffer : new byte[e.BytesRecorded]; // Если микрофон выключен, отправляем тишину
//    await _tcpClient.GetStream().WriteAsync(e.Buffer, 0, e.BytesRecorded);


//};
// Start playback
//_waveOut.Play();
//_waveIn.DataAvailable += async (sender, e) =>
//{
//    byte[] audioBuffer = microphoneEnabled ? e.Buffer : new byte[e.BytesRecorded]; // Если микрофон выключен, отправляем тишину
//    await _tcpClient.GetStream().WriteAsync(e.Buffer, 0, e.BytesRecorded);


//};
//waveStream = new RawSourceWaveStream(_tcpClient.GetStream(), new WaveFormat(44100, 1));
//waveOut.Init(waveStream);
//_waveIn.DataAvailable += async (sender, e) =>
//{
//    byte[] audioBuffer = microphoneEnabled ? e.Buffer : new byte[e.BytesRecorded]; // Если микрофон выключен, отправляем тишину
//    //foreach (var stream in clientStreams)
//    //{
//    //    await stream.WriteAsync(audioBuffer, 0, audioBuffer.Length);
//    //}
//};
//waveOut.Init(_waveIn);
//waveOut.Play();

//foreach (var stream in clientStreams)
//await _networkStream.WriteAsync(e.Buffer, 0, e.BytesRecorded);
//_bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
//_clientWebSocket = new ClientWebSocket();
//_waveIn = new WaveInEvent();
//_waveOut = new WaveOutEvent();

//_waveIn.DataAvailable += WaveIn_DataAvailable;
////await   Button_Click(sender, e);    
//waveIn = new WaveInEvent();
//waveIn.WaveFormat = new WaveFormat(44100, 1); // 44.1kHz, моно
//waveIn.DataAvailable += async (sender, e) =>
//{
//    byte[] audioBuffer = microphoneEnabled ? e.Buffer : new byte[e.BytesRecorded]; // Если микрофон выключен, отправляем тишину
//    foreach (var stream in clientStreams)
//    {
//        await stream.WriteAsync(audioBuffer, 0, audioBuffer.Length);
//    }
//};
//private WaveInEvent waveIn;
