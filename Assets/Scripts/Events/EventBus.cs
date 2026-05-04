using System;
using System.Reflection;
using UnityEngine;

namespace Events
{
    [CreateAssetMenu(fileName = "EventBus", menuName = "Events/EventBus", order = 0)]
    public class EventBus : ScriptableObject
    {
        public readonly GenericEventChannel BubblePopup = new(nameof(BubblePopup));
        public readonly GenericEventChannel<string> BubbleShow = new(nameof(BubbleShow));
        
        public void SubscribeAll(EventChannel.EventRaisedRawHandler callback)
        {
            foreach (FieldInfo gameEvent in GetType().GetFields())
            {
                EventChannel eventObj = (EventChannel)gameEvent.GetValue(this);
                eventObj.EventRaisedRaw += callback;
            }
        }
        
        public void UnsubscribeAll(EventChannel.EventRaisedRawHandler callback)
        {
            foreach (FieldInfo gameEvent in GetType().GetFields())
            {
                EventChannel eventObj = (EventChannel)gameEvent.GetValue(this);
                eventObj.EventRaisedRaw -= callback;
            }
        }
    }
}