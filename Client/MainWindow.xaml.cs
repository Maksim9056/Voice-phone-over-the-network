using Microsoft.VisualBasic;
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

namespace Client
{
    public partial class MainWindow : Window
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private WaveInEvent _waveIn;
        private WaveOutEvent _waveOut;
        private BufferedWaveProvider _bufferedWaveProvider;
        static bool microphoneEnabled = true; // Изначально микрофон включен
        WaveOutEvent waveOut = null;
        WaveStream waveStream = null;
        public MainWindow()
        {
            try
            {


                InitializeComponent();
                _waveIn = new WaveInEvent();
                _waveIn.WaveFormat = new WaveFormat(44100, 1); // 44.1kHz, моно
                _waveOut = new WaveOutEvent();
                _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1)); // Установите нужные параметры формата здесь
                _waveOut.Init(_bufferedWaveProvider);
            }
            catch (Exception)
            {

            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(Text.Text, 49153);

                //_networkStream = _tcpClient.GetStream();

                _waveOut.Play();
                _waveIn.DataAvailable += WaveIn_DataAvailable;
                

                // Start recording from microphone
                _waveIn.StartRecording();
            
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к серверу: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


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
        private async void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                if (_tcpClient != null && _tcpClient.Connected)
                {
                    byte[] audioBuffer = microphoneEnabled ? e.Buffer : new byte[e.BytesRecorded];
                    await _tcpClient.GetStream().WriteAsync(audioBuffer, 0, e.BytesRecorded);
                }

                _bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке аудио данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _waveIn.StopRecording();
                _waveOut.Stop();

                _networkStream?.Close();
                _tcpClient?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке аудио данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

   

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                base.OnClosed(e);
                DisconnectButton_Click(null, null); // Закрываем соединение при закрытии окна
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке аудио данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


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
