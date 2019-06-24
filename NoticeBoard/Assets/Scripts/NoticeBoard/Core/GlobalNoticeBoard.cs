/********************************************************\\

          Created by Harry Davies ~ 08/06/2019

      Sends static messages to a list of Subscribers
                    Singleton Object

\\********************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TravellerPack.NoticeBoard
{
    public class GlobalNoticeBoard : MonoBehaviour
    {
        public static GlobalNoticeBoard s_instance;

        //Holds reference to a class interested in static message
        [Serializable]
        public class SubscriberList
        {
            public MessageHeader m_messageHeader;
            public SubscriberFunction m_messageFunction;
        }

        //Holds data package linked to static message
        [Serializable]
        public class MessageBuffer
        {
            public MessageHeader m_messageHeader;
            public object m_packageData;
        }

        [SerializeField] private bool m_useRapidMessages = true;
        [SerializeField] private bool m_useDelayedMessages = true;

        private bool m_hasDelayedMessages = false;
        private bool m_hasBypassMessages = false;

        private List<SubscriberList> m_subscriberList = new List<SubscriberList>();
        private List<MessageBuffer> m_delayBuffer = new List<MessageBuffer>();
        private List<MessageBuffer> m_bypassBuffer = new List<MessageBuffer>();

        #region Unity Functions
        void Awake()
        {
            if (CheckInstance())
            {
                Initialise();
            }
        }

        void LateUpdate()
        {
            if (m_useDelayedMessages && m_hasDelayedMessages)
            {
                DelayedBufferHandler();
            }

            if (m_hasBypassMessages)
            {
                BypassBufferHandler();
            }
        }
        #endregion

        #region Initialise/Clear
        /// <summary>Either sets Singleton or Destroy this</summary>
        private bool CheckInstance()
        {
            if (s_instance)
            {
                Destroy(this);
                return false;
            }
            else
            {
                s_instance = this;
                DontDestroyOnLoad(transform.root);
                return true;
            }
        }

        /// <summary>Sets up subscriber lists if using delayed messages</summary>
        private void Initialise()
        {
            //Generates subscriber lists per static message
            for (int i = 0; i < (int)MessageHeader.Count; i++)
            {
                CreateSubscriberList((MessageHeader)i);
            }
        }
        #endregion

        #region Message Handlers
        /// <summary>Processes all delayed messages</summary>
        void DelayedBufferHandler()
        {
            if (m_useDelayedMessages)
            {
                foreach (MessageBuffer message in m_delayBuffer)
                {
                    TriggerMessage(message);
                }

                if (m_delayBuffer != null)
                {
                    m_delayBuffer.Clear();
                }

                m_hasDelayedMessages = false;
            }
        }

        /// <summary>Processes all bypass messages</summary>
        void BypassBufferHandler()
        {
            foreach (MessageBuffer message in m_bypassBuffer)
            {
                TriggerMessage(message);
            }

            if (m_delayBuffer != null)
            {
                m_bypassBuffer.Clear();
            }

            m_hasBypassMessages = false;
        }

        /// <summary>Calls all functions subscribe to this message, using the EventQueue struct </summary>
        void TriggerMessage(MessageBuffer _buffer)
        {
            if (m_subscriberList[(int)_buffer.m_messageHeader].m_messageFunction != null)
            {
                if (_buffer.m_packageData == null)
                {
                    m_subscriberList[(int)_buffer.m_messageHeader].m_messageFunction();
                }
                else if (_buffer.m_packageData != null)
                {
                    m_subscriberList[(int)_buffer.m_messageHeader].m_messageFunction(_buffer.m_packageData);
                }
            }
        }
        #endregion

        #region Add/Remove Functions
        /// <summary>Adds message relative to MessageFormat. </summary>
        public void AddMessage(MessageFormat _format, MessageHeader _messageHeader, object _data = null)
        {
            switch (_format)
            {
                case MessageFormat.Rapid:
                    {
                        AddRapidMessage(_messageHeader, _data);
                        break;
                    }
                case MessageFormat.Delayed:
                    {
                        AddDelayMessage(_messageHeader, _data);
                        break;
                    }
                case MessageFormat.RapidBypass:
                    {
                        AddRapidMessage(_messageHeader, _data, true);
                        break;
                    }
                case MessageFormat.DelayedBypass:
                    {
                        AddDelayMessage(_messageHeader, _data, true);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>Adds message to be triggered immediately</summary>
        private void AddRapidMessage(MessageHeader _messageHeader, object _data = null, bool _bypassFlag = false)
        {
            if (m_useRapidMessages || _bypassFlag)
            {
                if (m_subscriberList[(int)_messageHeader].m_messageFunction != null && _data == null)
                {
                    m_subscriberList[(int)_messageHeader].m_messageFunction();
                }
                else if (m_subscriberList[(int)_messageHeader].m_messageFunction != null && _data != null)
                {
                    m_subscriberList[(int)_messageHeader].m_messageFunction(_data);
                }
            }
        }

        /// <summary>Adds message to delayed list, will be triggered during LateUpdate</summary>
        private void AddDelayMessage(MessageHeader _messageHeader, object _data = null, bool _bypassFlag = false)
        {
            MessageBuffer buffer = new MessageBuffer();
            buffer.m_messageHeader = _messageHeader;
            buffer.m_packageData = _data;

            if (!_bypassFlag)
            {
                m_delayBuffer.Add(buffer);
                m_hasDelayedMessages = true;
            }
            else
            {
                m_bypassBuffer.Add(buffer);
                m_hasBypassMessages = true;
            }
        }

        /// <summary>Creates subscriber List from MessageHeader.</summary>
        private void CreateSubscriberList(MessageHeader _messageHeader)
        {
            SubscriberList tempList = new SubscriberList();
            tempList.m_messageHeader = _messageHeader;
            tempList.m_messageFunction = null;
            m_subscriberList.Add(tempList);
        }

        /// <summary>Adds function that requires a data object to call list when this message is triggered</summary>
        public void SubscribeToMessage(MessageHeader _messageHeader, SubscriberFunction _subscriberFunction)
        {
            SubscriberList subscriberList = m_subscriberList[(int)_messageHeader];
            subscriberList.m_messageFunction += _subscriberFunction;
            m_subscriberList[(int)_messageHeader] = subscriberList;
        }

        /// <summary>Removes function that requires a data object from call list of this message</summary>
        public void UnsubscribeToMessage(MessageHeader _messageHeader, SubscriberFunction _subscriberFunction)
        {
            SubscriberList subscriberList = m_subscriberList[(int)_messageHeader];
            subscriberList.m_messageFunction -= _subscriberFunction;
            m_subscriberList[(int)_messageHeader] = subscriberList;
        }
        #endregion
    }
}