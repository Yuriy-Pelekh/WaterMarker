using System.Linq;
using System.Text;

namespace WaterMarker.AudioProcessor
{
  public static class WaterMark
  {
    public static string FromBitArray(byte[] bitArray)
    {
      return bitArray.ToWaterMarkString();
    }

    public static byte[] FromString(string waterMarkText)
    {
      return waterMarkText.ToWaterMarkBits();
    }

    private static string ToWaterMarkString(this byte[] bitArray)
    {
      const byte byteSize = 8;
      var waterMarkBuilder = new StringBuilder();

      for (var i = 0; i < bitArray.Length - byteSize; i += byteSize)
      {
        byte b = 0x00;

        for (byte j = 0; j < byteSize; j++)
        {
          if (bitArray[i + j] == 1)
          {
            b = b.SetBit(j);
          }
        }

        waterMarkBuilder.Append((char) b);
      }

      return waterMarkBuilder.ToString();
    }

    private static byte[] ToWaterMarkBits(this string watermark)
    {
      var bytes = watermark.ToCharArray().Select(c => (byte) c).ToArray();

      var bitArray = new byte[bytes.Length*8];

      for (var i = 0; i < bytes.Length; i++)
      {
        for (byte j = 0; j < 8; j++)
        {
          bitArray[i*8 + j] = bytes[i].CheckBit(j);
        }
      }

      return bitArray;
    }
  }
}
