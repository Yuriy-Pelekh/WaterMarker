using System;
using System.Collections.Generic;

namespace WaterMarker.AudioProcessor
{
  public class Processor
  {
    private const int SampleLength = 2;
    private const int ChunkLength = 512;
    private const int Shift = SampleLength*ChunkLength;
    private readonly byte[] originalBytes;

    #region TestData

    public byte[] OriginalWaterMarkBits { get; private set; }
    public byte[] ExtractedWaterMarkBits { get; private set; }

    #endregion

    public Processor(byte[] bytes)
    {
      originalBytes = new byte[bytes.Length];
      bytes.CopyTo(originalBytes, 0);
    }

    public byte[] GetWaterMarkedBytes(string waterMark)
    {
      var waterMarkedBytes = new byte[originalBytes.Length];
      originalBytes.CopyTo(waterMarkedBytes, 0);

      var waterMarkBits = WaterMark.FromString(waterMark);
      OriginalWaterMarkBits = waterMarkBits;
      var waterMarkBitIndex = 0;

      for (var index = 44 + 4; index + Shift < waterMarkedBytes.Length; index += Shift)
      {
        var startIndex = index;
        var endIndex = index + Shift;

        var maxAmplitudeIndex = FindMaxAmplitudeIndex(waterMarkedBytes, startIndex, endIndex);

        SetMark(waterMarkedBytes, waterMarkBits[waterMarkBitIndex], maxAmplitudeIndex);

        waterMarkBitIndex = waterMarkBitIndex < waterMarkBits.Length - 1
                              ? waterMarkBitIndex + 1
                              : 0;
      }

      return waterMarkedBytes;
    }

    public string ExtractWaterMark(byte[] waterMarkedBytes)
    {
      string waterMark;
      ExtractWaterMark(waterMarkedBytes, out waterMark);
      return waterMark;
    }

    public byte[] ExtractWaterMark(byte[] waterMarkedBytes, out string waterMark)
    {
      var unwaterMarkedBytes = new byte[waterMarkedBytes.Length];
      waterMarkedBytes.CopyTo(unwaterMarkedBytes, 0);

      var waterMarkBits = new List<byte>();

      for (var index = 44 + 4; index + Shift < unwaterMarkedBytes.Length; index += Shift)
      {
        var startIndex = index;
        var endIndex = index + Shift;

        var maxAmplitudeIndex = FindMaxAmplitudeIndex(unwaterMarkedBytes, startIndex, endIndex);

        waterMarkBits.Add(GetMark(unwaterMarkedBytes, maxAmplitudeIndex));
      }

      ExtractedWaterMarkBits = waterMarkBits.ToArray();

      waterMark = WaterMark.FromBitArray(waterMarkBits.ToArray());

      return unwaterMarkedBytes;
    }

    private int FindMaxAmplitudeIndex(byte[] bytes, int startIndex, int endIndex)
    {
      var maxAmplitude = Math.Abs(bytes[3*SampleLength] - bytes[4*SampleLength]);
      var maxAmplitudeIndex = 0;

      for (var i = startIndex + 3*SampleLength; i < endIndex - 3*SampleLength; i += SampleLength)
      {
        var currentAmplitude = Math.Abs(bytes[i] - bytes[i + SampleLength]);

        if (currentAmplitude > maxAmplitude)
        {
          maxAmplitude = currentAmplitude;
          maxAmplitudeIndex = i;
        }
      }

      return maxAmplitudeIndex;
    }

    private void SetMark(byte[] bytes, byte mark, int markIndex)
    {
      if (mark > 1)
      {
        throw new ArgumentException("Mark byte should be in a range [0, 1]");
      }

      bytes[markIndex] ^= mark;

      if (mark == 0)
      {
        bytes[markIndex - 1*SampleLength] &= 0xFE;
        bytes[markIndex - 2*SampleLength] &= 0xFE;
        bytes[markIndex - 3*SampleLength] &= 0xFE;
      }
      else
      {
        bytes[markIndex + 1*SampleLength] &= 0xFE;
        bytes[markIndex + 2*SampleLength] &= 0xFE;
        bytes[markIndex + 3*SampleLength] &= 0xFE;
      }
    }

    private byte GetMark(byte[] bytes, int markIndex)
    {
      var leftSum = (bytes[markIndex - 1*SampleLength] & 0x01) +
                    (bytes[markIndex - 2*SampleLength] & 0x01) +
                    (bytes[markIndex - 3*SampleLength] & 0x01);
      var rightSum = (bytes[markIndex + 1*SampleLength] & 0x01) +
                     (bytes[markIndex + 2*SampleLength] & 0x01) +
                     (bytes[markIndex + 3*SampleLength] & 0x01);

      var markBit = (byte) (rightSum > leftSum ? 0 : 1);

      bytes[markIndex] ^= markBit;

      return markBit;
    }
  }
}
