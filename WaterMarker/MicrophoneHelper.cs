using System;
using System.IO;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using WaterMarker.AudioProcessor;

namespace WaterMarker
{
  public class MicrophoneHelper : IDisposable
  {
    private readonly Microphone microphone = Microphone.Default;
    private DispatcherTimer xnaTimer;
    private byte[] buffer;
    private readonly MemoryStream stream = new MemoryStream();

    public MicrophoneState State
    {
      get { return microphone.State; }
    }

    public int SampleRate
    {
      get { return microphone.SampleRate; }
    }

    public MicrophoneHelper()
    {
      SimulateXNAEnvironment();
      microphone.BufferReady += MicrophoneBufferReady;
    }

    public bool StartRecording()
    {
      if (microphone.State != MicrophoneState.Started)
      {
        microphone.BufferDuration = TimeSpan.FromMilliseconds(500);
        buffer = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)];

        // + ?
        stream.SetLength(0);
        WAV.WriteWavHeader(stream, microphone.SampleRate);
        // -

        microphone.Start();
        
        return true;
      }
     
      return false;
    }

    public bool StopRecording()
    {
      if (microphone.State == MicrophoneState.Started)
      {
        microphone.Stop();
        
        // +
        WAV.UpdateWavHeader(stream);
        // -

        return true;
      }

      return false;
    }

    public byte[] GetRecord()
    {
      if (microphone.State == MicrophoneState.Stopped && stream != null && stream.Length > 0)
      {
        return stream.ToArray();
      }

      return new byte[0];
    }

    private void MicrophoneBufferReady(object sender, EventArgs e)
    {
      microphone.GetData(buffer);
      stream.Write(buffer, 0, buffer.Length);
    }

    private void SimulateXNAEnvironment()
    {
      // Timer to simulate the XNA Game Studio game loop (Microphone is from XNA Game Studio)
      xnaTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(33)};
      xnaTimer.Tick += delegate {try {FrameworkDispatcher.Update();} catch { }};
      xnaTimer.Start();
    }

    public void Dispose()
    {
      xnaTimer.Stop();
      microphone.BufferReady -= MicrophoneBufferReady;
    }
  }
}
