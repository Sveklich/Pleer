using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AudioPlayer
{
    public partial class HistoryWindow : Window
    {
        public int SelectedAudioIndex { get; private set; }

        public HistoryWindow(List<string> audioFilePaths, int currentAudioIndex)
        {
            InitializeComponent();

            for (int i = 0; i < audioFilePaths.Count; i++)
            {
                string audioFileName = System.IO.Path.GetFileName(audioFilePaths[i]);
                Button button = new Button() { Content = audioFileName, Tag = i };
                button.Click += Button_Click;
                stackPanel.Children.Add(button);

                if (i == currentAudioIndex)
                {
                    button.IsEnabled = false;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedAudioIndex = (int)(sender as Button).Tag;
            DialogResult = true;
        }
    }
}