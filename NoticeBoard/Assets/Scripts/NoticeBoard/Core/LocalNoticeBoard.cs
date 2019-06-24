/********************************************************\\

          Created by Harry Davies ~ 08/06/2019

     Sends dynamic messages to a list of Subscribers

\\********************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace TravellerPack.NoticeBoard
{
    public class LocalNoticeBoard : MonoBehaviour
    {
        //Holds reference to a class interested in dynamic message
        [Serializable]
        public class SubscriberList
        {
            public string m_messageHeader;
            public SubscriberFunction m_messageFunction;
            public SubscriberFunction m_removeFunction;
        }

        //Holds data package linked to dynamic message
        [Serializable]
        public class MessageBuffer
        {
            public MessageBuffer(string _messageHeader, object _packageData)
            {
                m_messageHeader = _messageHeader;
                m_packageData = _packageData;
            }

            public string m_messageHeader;
            public object m_packageData;
        }

        [SerializeField] private bool m_useRapidMessages = true;
        [SerializeField] private bool m_useDelayedMessages = true;

        private bool m_hasDelayedMessages = false;
        private bool m_hasBypassMessages = false;

        [SerializeField] private List<string> m_messageHeaders = new List<string>();

        private List<SubscriberList> m_subscriberList = new List<SubscriberList>();
        private List<MessageBuffer> m_delayBuffer = new List<MessageBuffer>();
        private List<MessageBuffer> m_bypassBuffer = new List<MessageBuffer>();

        private Dictionary<string, int> m_listReferences = new Dictionary<string, int>();

        #region Unity Functions
        private void Awake()
        {
            Initialise();
        }

        private void LateUpdate()
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
        /// <summary>Sets up intial subscriber lists</summary>
        public void Initialise()
        {
            foreach (string header in m_messageHeaders)
            {
                CreateSubscriberList(header);
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
        private void TriggerMessage(MessageBuffer _buffer)
        {
            int messageIndex = GetMessageIndex(_buffer.m_messageHeader);
            if (messageIndex != -1)
            {
                if (m_subscriberList[messageIndex].m_messageFunction != null)
                {
                    if (_buffer.m_packageData == null)
                    {
                        m_subscriberList[messageIndex].m_messageFunction();
                    }
                    else if (_buffer.m_packageData != null)
                    {
                        m_subscriberList[messageIndex].m_messageFunction(_buffer.m_packageData);
                    }
                }
            }
        }

        /// <summary>Calls all remove functions subscribe to this message, using the EventQueue struct </summary>
        private void TriggerRemove(MessageBuffer _buffer)
        {
            int messageIndex = GetMessageIndex(_buffer.m_messageHeader);
            if (messageIndex != -1)
            {
                if (m_subscriberList[messageIndex].m_removeFunction != null)
                {
                    if (_buffer.m_packageData == null)
                    {
                        m_subscriberList[messageIndex].m_removeFunction();
                    }
                    else if (_buffer.m_packageData != null)
                    {
                        m_subscriberList[messageIndex].m_removeFunction(_buffer.m_packageData);
                    }
                }
            }
        }
        #endregion

        #region Add/Remove Functions
        /// <summary>Adds message relative to MessageFormat. </summary>
        public void AddMessage(MessageFormat _format, string _messageHeader, object _data = null)
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

        /// <summary>Adds message to be triggered immediately.</summary>
        private void AddRapidMessage(string _messageHeader, object _data = null, bool _bypassFlags = false)
        {
            if (m_useRapidMessages || _bypassFlags)
            {
                int messageIndex = GetMessageIndex(_messageHeader);
                if (messageIndex != -1)
                {
                    if (m_subscriberList[messageIndex].m_messageFunction != null && _data == null)
                    {
                        m_subscriberList[messageIndex].m_messageFunction();
                    }
                    else if (m_subscriberList[messageIndex].m_messageFunction != null && _data != null)
                    {
                        m_subscriberList[messageIndex].m_messageFunction(_data);
                    }
                }
            }
        }

        /// <summary>Adds message to delayed list, will be triggered during LateUpdate.</summary>
        private void AddDelayMessage(string _messageHeader, object _data = null, bool _bypassFlags = false)
        {
            MessageBuffer buffer = new MessageBuffer(_messageHeader, _data);

            if (!_bypassFlags)
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
        public void CreateSubscriberList(string _messageHeader)
        {
            //Adds reference to index in message buffer
            m_listReferences.Add(_messageHeader, m_subscriberList.Count);

            SubscriberList list = new SubscriberList();
            list.m_messageHeader = _messageHeader;
            list.m_messageFunction = null;
            list.m_removeFunction = null;
            m_subscriberList.Add(list);

            //Informs global that a new Message has been created
            MPac_DynamicMessageCreated package = new MPac_DynamicMessageCreated(this, MessageHelper.GetPrefix(_messageHeader), _messageHeader);
            GlobalNoticeBoard.s_instance.AddMessage(MessageFormat.RapidBypass, MessageHeader.GLO_DynamicMessageCreated, package);
        }

        /// <summary>Creates subscriber List from MessageHeader. Adds message immediately, triggered relative to MessageFormat.</summary>
        public void CreateListAndAddMessage(MessageFormat _format, string _messageHeader, object _data = null)
        {
            CreateSubscriberList(_messageHeader);
            AddMessage(_format, _messageHeader, _data);
        }

        /// <summary>Removes subscriber list.</summary>
        public void RemoveSubscriberList(string _messageHeader, MPac_SubscriberListRemoved _package)
        {
            if(m_listReferences.ContainsKey(_messageHeader))
            {
                TriggerRemove(new MessageBuffer(_messageHeader, _package));

                int index = GetMessageIndex(_messageHeader);
                m_subscriberList.RemoveAt(index);

                RemoveReferences(_messageHeader);
            }
        }

        /// <summary>Removes reference and adjusts other reference's index.</summary>
        private void RemoveReferences(string _messageHeader)
        {
            int index = GetMessageIndex(_messageHeader);
            List<string> indexToChange = new List<string>();
            
            foreach (KeyValuePair<string, int> pair in m_listReferences)
            {
                if(pair.Value > index)
                {
                    indexToChange.Add(pair.Key);
                }
            }

            foreach (string key in indexToChange)
            {
                m_listReferences[key]--;
            }

            m_listReferences.Remove(_messageHeader);
        }

        /// <summary>Adds function that requires a data object to call list when this message is triggered.</summary>
        public void SubscribeToMessage(string _messageHeader, SubscriberFunction _subscriberFunction,
            SubscriberFunction _removeFunction = null)
        {
            int messageIndex = GetMessageIndex(_messageHeader);
            if (messageIndex != -1)
            {
                SubscriberList subscriberList = m_subscriberList[messageIndex];
                subscriberList.m_messageFunction += _subscriberFunction;
                subscriberList.m_removeFunction += _removeFunction;
                m_subscriberList[messageIndex] = subscriberList;
            }
        }

        /// <summary>Removes function that requires a data object from call list of this message.</summary>
        public void UnsubscribeToMessage(string _messageHeader, SubscriberFunction _subscriberFunction,
            SubscriberFunction _removeFunction = null)
        {
            int messageIndex = GetMessageIndex(_messageHeader);
            if (messageIndex != -1)
            {
                SubscriberList subscriberList = m_subscriberList[messageIndex];
                subscriberList.m_messageFunction -= _subscriberFunction;
                subscriberList.m_removeFunction -= _removeFunction;
                m_subscriberList[messageIndex] = subscriberList;
            }
        }
        #endregion

        #region Getters/Setters
        /// <summary>Finds message buffer index from message handler</summary>
        private int GetMessageIndex(string _messageHeader)
        {
            if (m_listReferences.ContainsKey(_messageHeader))
            {
                return m_listReferences[_messageHeader];
            }

            return -1;
        }
        #endregion
    }
}