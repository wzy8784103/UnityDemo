public abstract class MessageResponseBase
{
    public int result;
    public int id;
    public MessageResponseBase(int id)
    {
        this.id = id;
    }
    public abstract void Decode(ByteBuffer buffer);
}
