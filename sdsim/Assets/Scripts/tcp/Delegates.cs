
namespace tk
{
    public class Delegates
    {
           
        public delegate void OnMsgRecv(JSONObject data);

        public OnMsgRecv onMsgCb;
    }
}