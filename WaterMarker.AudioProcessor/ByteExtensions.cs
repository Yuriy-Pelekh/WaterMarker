using System;

namespace WaterMarker.AudioProcessor
{
  internal static class ByteExtensions
  {
    internal static byte CheckBit(this byte b, byte position)
    {
      switch (position)
      {
        case 0:
          return (byte)(b & 0x01);
        case 1:
          return (byte)((b & 0x02) >> 1);
        case 2:
          return (byte)((b & 0x04) >> 2);
        case 3:
          return (byte)((b & 0x08) >> 3);
        case 4:
          return (byte)((b & 0x10) >> 4);
        case 5:
          return (byte)((b & 0x20) >> 5);
        case 6:
          return (byte)((b & 0x40) >> 6);
        case 7:
          return (byte)((b & 0x80) >> 7);
        default:
          throw new ArgumentException("Value should be in range [0..7]", "position");
      }
    }

    internal static byte SetBit(this byte b, byte position)
    {
      switch (position)
      {
        case 0:
          return (byte) (b | 0x01);
        case 1:
          return (byte) (b | 0x02);
        case 2:
          return (byte) (b | 0x04);
        case 3:
          return (byte) (b | 0x08);
        case 4:
          return (byte) (b | 0x10);
        case 5:
          return (byte) (b | 0x20);
        case 6:
          return (byte) (b | 0x40);
        case 7:
          return (byte) (b | 0x80);
        default:
          throw new ArgumentException("Value should be in range [0..7]", "position");
      }
    }
  }
}
