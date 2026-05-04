using System;

namespace Events
{
    public class EventChannel
    {
        private string _tag;
        
        public delegate void EventRaisedRawHandler(string tag, object payload, Type payloadType);
        public event EventRaisedRawHandler EventRaisedRaw;

        public EventChannel(string tag)
        {
            _tag = tag;
        }
        
        protected void RaiseWrapped<T>(object payload) => EventRaisedRaw?.Invoke(_tag, payload, typeof(T));
    }
    
    public class GenericEventChannel<T> : EventChannel
    {
        public delegate void EventRaisedHandler(T payload);

        public event EventRaisedHandler EventRaised;

        public void RaiseEvent(T payload)
        {
            EventRaised?.Invoke(payload);
            RaiseWrapped<T>(payload);
        }
        
        public GenericEventChannel(string tag) : base(tag) {}
    }
    
    public class GenericEventChannel : EventChannel
    {
        public event Action EventRaised;
        
        public void RaiseEvent()
        {
            EventRaised?.Invoke();
            RaiseWrapped<object>(null);
        }
        
        public GenericEventChannel(string tag) : base(tag) {}
    }
}
