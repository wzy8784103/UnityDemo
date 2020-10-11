public abstract class MessageRequestBase
{
    public int id;
    public MessageRequestBase(int id)
    {
        this.id = id;
    }
    public abstract void Encode(ByteBuffer buffer);
}
