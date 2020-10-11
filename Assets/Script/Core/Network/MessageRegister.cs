using System.Collections.Generic;
public class MessageRegister : SingletonBase<MessageRegister>
{
    private Dictionary<int, MessageResponseBase> dict = new Dictionary<int, MessageResponseBase>();
    public MessageResponseBase GetMessageRes(int id)
    {
        if(!dict.ContainsKey(id))
        {
            UnityEngine.Debug.LogError("未注册message id==" + id);
            return null;
        }
        return dict[id];
    }
    public void Register()
    {

    }
}
