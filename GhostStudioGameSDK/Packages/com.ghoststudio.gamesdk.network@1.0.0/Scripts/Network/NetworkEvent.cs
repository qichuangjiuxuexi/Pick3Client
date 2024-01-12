using API.V1.Game;
using AppBase.Event;

namespace AppBase.Network
{
    public struct UploadRequestFailedEvent : IEvent
    {
        public ErrorReason errorReason;

        public UploadRequestFailedEvent(ErrorReason reason)
        {
            errorReason = reason;
        }
    }
}