using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioPlayer
{
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer;
        private List<string> audioFilePaths;
        private int currentAudioIndex;
        private bool isPlaying = false;
        private bool isRepeatEnabled = false;
        private bool isShuffleEnabled = false;
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();

            mediaPlayer = new MediaPlayer();
            audioFilePaths = new List<string>();
            currentAudioIndex = 0;

            mediaPlayer.MediaEnded += new EventHandler(Player_MediaEnded);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                sliderPosition.Maximum = mediaPlayer.NaturalDuration.HasTimeSpan ? mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds : 0;

                TimeSpan currentTime = mediaPlayer.Position;
                TimeSpan totalTime;
                try
                {
                    totalTime = mediaPlayer.NaturalDuration.TimeSpan;
                }
                catch (InvalidOperationException) 
                {
                    Thread.Sleep(5000); //Это всё нужно чтобы mp3 файлы не вызвали ошибку
                    totalTime = mediaPlayer.NaturalDuration.TimeSpan;
                    lblCurrentTime.Content = string.Format("{0:mm\\:ss}", totalTime);
                    lblRemainingTime.Content = string.Format("{0:mm\\:ss}", totalTime - currentTime);
                    sliderPosition.Value = mediaPlayer.Position.TotalSeconds;
                }
                lblCurrentTime.Content = string.Format("{0:mm\\:ss}", totalTime);
                lblRemainingTime.Content = string.Format("{0:mm\\:ss}", totalTime - currentTime);
                sliderPosition.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog browser = new CommonOpenFileDialog { IsFolderPicker = true };
            CommonFileDialogResult result = browser.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                string[] validExtensions = { ".mp3", ".m4a", ".wav" };
                audioFilePaths = Directory.GetFiles(browser.FileName).Where(file => validExtensions.Contains(Path.GetExtension(file))).ToList();

                if (audioFilePaths.Count > 0)
                {
                    OpenAudioFile(0);
                }
                else
                {
                    MessageBox.Show("В выбранной папке нет аудио файлов.");
                }
            }
        }

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            if (isRepeatEnabled)
            {
                TimeSpan newPosition = TimeSpan.FromSeconds(0);
                mediaPlayer.Position = newPosition;
            }
            else if (currentAudioIndex < audioFilePaths.Count - 1)
            {
                BtnNext_Click(null, null);
            }
            else
            {
                currentAudioIndex = 0;
                OpenAudioFile(0);
            }
        }

        private void OpenAudioFile(int index)
        {
            if (audioFilePaths.Count > index)
            {
                currentAudioIndex = index;
                string audioFilePath = audioFilePaths[currentAudioIndex];

                mediaPlayer.Open(new Uri(audioFilePath));
                mediaPlayer.Play();

                isPlaying = true;
                btnPlayPause.Content = "Возпроизвести";

                lblCurrentAudioFile.Content = Path.GetFileName(audioFilePath);

                timer.Start();
            }
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                mediaPlayer.Pause();
                isPlaying = false;
                btnPlayPause.Content = "Воспроизвести";
            }
            else
            {
                mediaPlayer.Play();
                isPlaying = true;
                btnPlayPause.Content = "Пауза";
            }
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (isShuffleEnabled)
            {
                ShuffleAudioFiles();
            }
            else
            {
                if (currentAudioIndex > 0)
                {
                    OpenAudioFile(currentAudioIndex - 1);
                }
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (isShuffleEnabled)
            {
                ShuffleAudioFiles();
            }
            else
            {
                if (currentAudioIndex < audioFilePaths.Count - 1)
                {
                    OpenAudioFile(currentAudioIndex + 1);
                }
            }
        }
        private void BtnShuffle_Click(object sender, RoutedEventArgs e)
        {
            ShuffleAudioFiles();

            if (isShuffleEnabled)
            {
                btnShuffle.Content = "Перемешка ON";
            }
            else
            {
                btnShuffle.Content = "Перемешка OFF";
            }
        }
        private void ShuffleAudioFiles()
        {
            isShuffleEnabled = !isShuffleEnabled;

            if (isShuffleEnabled)
            {
                List<string> shuffledAudioFilePaths = new List<string>(audioFilePaths);
                shuffledAudioFilePaths.Shuffle();

                audioFilePaths = shuffledAudioFilePaths;
                currentAudioIndex = 0;
            }
            else
            {
                audioFilePaths.Sort();
                currentAudioIndex = audioFilePaths.IndexOf(mediaPlayer.Source?.LocalPath);
            }

            OpenAudioFile(currentAudioIndex);
        }

        private void BtnRepeat_Click(object sender, RoutedEventArgs e)
        {
            Repeat();

            if (isRepeatEnabled)
            {
                btnRepeat.Content = "Повтор ON";
            }
            else
            {
                btnRepeat.Content = "Повтор OFF";
            }
        }

        private void Repeat()
        {
            isRepeatEnabled = !isRepeatEnabled;
        }
        private void SliderPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                TimeSpan newPosition = TimeSpan.FromSeconds(sliderPosition.Value);
                mediaPlayer.Position = newPosition;
            }
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = sliderVolume.Value;
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(audioFilePaths, currentAudioIndex);
            historyWindow.ShowDialog();

            if (historyWindow.DialogResult == true)
            {
                int selectedAudioIndex = historyWindow.SelectedAudioIndex;
                OpenAudioFile(selectedAudioIndex);
            }
        }
    }

    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            Random random = new Random();

            for (int i = list.Count - 1; i > 0; i--)
            {
                int newIndex = random.Next(i + 1);
                T value = list[i];

                list[i] = list[newIndex];
                list[newIndex] = value;
            }
        }
    }
}

