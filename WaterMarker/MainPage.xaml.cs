using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Audio;
using WaterMarker.AudioProcessor;

namespace WaterMarker
{
  public partial class MainPage : PhoneApplicationPage
  {
    private readonly MicrophoneHelper microphoneHelper;
    private readonly Stopwatch stopwatch = new Stopwatch();
    private readonly DispatcherTimer stopwatchTimer = new DispatcherTimer();

    public MainPage()
    {
      InitializeComponent();

      stopwatchTimer.Interval = TimeSpan.FromMilliseconds(33);
      stopwatchTimer.Tick += delegate
                               {
                                 textBlockTime.Text = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                                                    stopwatch.Elapsed.Hours,
                                                                    stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds,
                                                                    stopwatch.Elapsed.Milliseconds/10);
                               };

      microphoneHelper = new MicrophoneHelper();
    }

    private void MainPageOrientationChanged(object sender, OrientationChangedEventArgs e)
    {
      Orientation orientation;

      switch (e.Orientation)
      {
        case PageOrientation.Landscape:
        case PageOrientation.LandscapeLeft:
        case PageOrientation.LandscapeRight:
          orientation = System.Windows.Controls.Orientation.Horizontal;
          break;
        case PageOrientation.Portrait:
        case PageOrientation.PortraitDown:
        case PageOrientation.PortraitUp:
          orientation = System.Windows.Controls.Orientation.Vertical;
          break;
        default:
          orientation = System.Windows.Controls.Orientation.Vertical;
          break;
      }

      buttonPanel.Orientation = orientation;

      switch (orientation)
      {
        case System.Windows.Controls.Orientation.Horizontal:
          ContentPanel.VerticalAlignment = VerticalAlignment.Top;
          break;
        case System.Windows.Controls.Orientation.Vertical:
          ContentPanel.VerticalAlignment = VerticalAlignment.Center;
          break;
      }
    }

    private void ButtonRecordClick(object sender, RoutedEventArgs e)
    {
      if (microphoneHelper.StartRecording())
      {
        stopwatch.Start();
        stopwatchTimer.Start();
      }
    }

    private void ButtonStopClick(object sender, RoutedEventArgs e)
    {
      if (microphoneHelper.StopRecording())
      {
        stopwatch.Stop();
        stopwatchTimer.Stop();
      }
    }

    private void ButtonPlayClick(object sender, RoutedEventArgs e)
    {
      if (String.IsNullOrEmpty(textBoxWaterMark.Text))
      {
        ApplicationTitle.Text = "Invalid watermark text.";
        return;
      }

      var bytes = microphoneHelper.GetRecord();

      if (bytes.Any())
      {
        var processor = new Processor(bytes);

        var markedBytes = processor.GetWaterMarkedBytes(textBoxWaterMark.Text);

        string waterMark;
        var unWaterMarkedBytes = processor.ExtractWaterMark(markedBytes, out waterMark);

        if (toggleSwitchPlaySound.IsChecked == true)
        {
          var sound = new SoundEffect(bytes, microphoneHelper.SampleRate, AudioChannels.Mono);
          sound.Play();
          Thread.Sleep(TimeSpan.FromMilliseconds(sound.Duration.TotalMilliseconds + 1000));

          sound = new SoundEffect(markedBytes, microphoneHelper.SampleRate, AudioChannels.Mono);
          sound.Play();
          Thread.Sleep(TimeSpan.FromMilliseconds(sound.Duration.TotalMilliseconds + 1000));

          sound = new SoundEffect(unWaterMarkedBytes, microphoneHelper.SampleRate, AudioChannels.Mono);
          sound.Play();
        }

        var originalWaterMarkBits = processor.OriginalWaterMarkBits;
        var extractedWaterMarkBits = processor.ExtractedWaterMarkBits;

        var totalBitsCount = Math.Max(originalWaterMarkBits.Length, extractedWaterMarkBits.Length);

        var persentage = 0.0;
        
        for (var i = 0; i < totalBitsCount; i++)
        {
          if (extractedWaterMarkBits[i % extractedWaterMarkBits.Length] == originalWaterMarkBits[i % originalWaterMarkBits.Length])
          {
            persentage++;
          }
        }

        persentage /= totalBitsCount;

        ApplicationTitle.Text = String.Format("Identity: {0}% -> {1}", (persentage*100).ToString("F0"), WaterMark.FromBitArray(extractedWaterMarkBits));
      }
    }
  }
}
