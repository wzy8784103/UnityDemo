using System;
using System.Text;

/// <summary>
/// 整理一个读写byte的类
/// 后来发现个BinaryReader BinaryWriter类,貌似写起来比较简单
/// 另外大小端也可以用IPAddress.HostToNetworkOrder 来做
/// 目前够用了，而且可以随便扩充，就先这样吧
/// </summary>
public class ByteBuffer
{
    private byte[] buffer;
    private int readIndex = 0;
    private int writeIndex = 0;
    //这里单列一个变量，读正常数据的时候就不翻转了
    private bool isReverse = true;

    public ByteBuffer(bool isReverse = false)
    {
        buffer = new byte[256];
        this.isReverse = isReverse;
    }
    public ByteBuffer(int size, bool isReverse = false)
    {
        buffer = new byte[size];
        this.isReverse = isReverse;
    }
    public ByteBuffer(byte[] bytes, bool isReverse = false)
    {
        buffer = bytes;
        this.isReverse = isReverse;
    }

    public byte ReadByte()
    {
        return buffer[readIndex++];
    }
    public short ReadShort()
    {
        if(IsOutRange(2))
        {
            return 0;
        }
        return BitConverter.ToInt16(GetReverseBytes(2, false), 0);
    }
    public int ReadInt()
    {
        if (IsOutRange(4))
        {
            return 0;
        }
        return BitConverter.ToInt32(GetReverseBytes(4, false), 0);
    }
    public int PeekShort()
    {
        if (IsOutRange(2))
        {
            return 0;
        }
        return BitConverter.ToInt16(GetReverseBytes(2, true), 0);
    }
    public int PeekInt()
    {
        if (IsOutRange(4))
        {
            return 0;
        }
        return BitConverter.ToInt32(GetReverseBytes(4, true), 0);
    }
    public long PeekLong()
    {
        if (IsOutRange(8))
        {
            return 0;
        }
        return BitConverter.ToInt64(GetReverseBytes(8, true), 0);
    }
    public float PeekFloat()
    {
        if (IsOutRange(4))
        {
            return 0;
        }
        return BitConverter.ToSingle(GetReverseBytes(4, true), 0);
    }
    public string PeekString()
    {
        int len = PeekShort();
        //必须得用Encoding.UTF8，刚开始忘了
        //这里别忘了+2，因为上面的也是peek的
        return Encoding.UTF8.GetString(buffer, readIndex + 2, len);
    }
    /// <summary>
    /// string特殊处理的，开头拼一个长度，然后根据长度取,注意不需要拼大小头
    /// </summary>
    /// <returns></returns>
    public string ReadString()
    {
        int len = ReadShort();
        //必须得用Encoding.UTF8，刚开始忘了
        string result = Encoding.UTF8.GetString(buffer, readIndex, len);
        readIndex += len;
        return result;
    }
    public long ReadLong()
    {
        if (IsOutRange(8))
        {
            return 0;
        }
        return BitConverter.ToInt64(GetReverseBytes(8, false), 0);
    }
    public float ReadFloat()
    {
        if (IsOutRange(4))
        {
            return 0;
        }
        return BitConverter.ToSingle(GetReverseBytes(4, false), 0);
    }
    public double ReadDouble()
    {
        if (IsOutRange(8))
        {
            return 0;
        }
        return BitConverter.ToDouble(GetReverseBytes(8, false), 0);
    }

    public byte[] ReadBytes(int len)
    {
        byte[] bytes = new byte[len];
        for (int i = 0; i < len; i++)
        {
            bytes[i] = ReadByte();
        }
        return bytes;
    }

    private byte[] GetReverseBytes(int len, bool isPeek)
    {
        byte[] bytes = new byte[len];
        Array.Copy(buffer, readIndex, bytes, 0, len);
        Reverse(bytes);
        if(!isPeek)
        {
            readIndex += len;
        }
        return bytes;
    }

    private bool IsOutRange(int len)
    {
        return readIndex + len > buffer.Length;
    }

    public void WriteShort(short value)
    {
        WriteBytes(Reverse(BitConverter.GetBytes(value)));
    }
    public void WriteInt(int value)
    {
        WriteBytes(Reverse(BitConverter.GetBytes(value)));
    }
    /// <summary>
    /// string特殊处理，先拼入长度
    /// </summary>
    /// <param name="value"></param>
    public void WriteString(string value)
    {
        //utf8单字节解码，没有大小端问题
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        WriteShort((short)bytes.Length);
        WriteBytes(bytes);
    }
    public void WriteLong(long value)
    {
        WriteBytes(Reverse(BitConverter.GetBytes(value)));
    }
    public void WriteFloat(float value)
    {
        WriteBytes(Reverse(BitConverter.GetBytes(value)));
    }
    public void WriteDouble(long value)
    {
        WriteBytes(Reverse(BitConverter.GetBytes(value)));
    }

    public void WriteBytes(byte[] bytes)
    {
        WriteBytes(bytes, bytes.Length);
    }
    public void WriteBytes(byte[] bytes, int len)
    {
        int bufferLen = buffer.Length;
        //容量不够就乘2
        if (writeIndex + len > bufferLen)
        {
            while (writeIndex + len > bufferLen)
            {
                bufferLen *= 2;
            }
            byte[] newBytes = new byte[bufferLen];
            Array.Copy(buffer, newBytes, buffer.Length);
            buffer = newBytes;
        }
        //这里得根据writeIndex加，因为原buffer后面有很多空位
        for(int i = 0, j = writeIndex; i < len; i++,j++)
        {
            buffer[j] = bytes[i];
        }
        writeIndex += len;
    }

    //大小端转换
    public byte[] Reverse(byte[] bytes)
    {
        if (isReverse)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    public int GetCanRead()
    {
        return writeIndex - readIndex;
    }
    public void Reset()
    {
        int len = buffer.Length - readIndex;
        writeIndex -= readIndex;
        
        byte[] newbuf = new byte[len];
        Array.Copy(buffer, readIndex, newbuf, 0, len);
        buffer = newbuf;
        readIndex = 0;
    }

    public void Clear()
    {
        readIndex = 0;
        writeIndex = 0;
    }

    public byte[] ToArray()
    {
        byte[] bytes = new byte[writeIndex];
        Array.Copy(buffer, 0, bytes, 0, bytes.Length);
        return bytes;
    }
}
