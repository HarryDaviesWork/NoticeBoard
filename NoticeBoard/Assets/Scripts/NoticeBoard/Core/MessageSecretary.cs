/********************************************************\\

           Created by Harry Davies ~ 09/06/2019

           Listens for new dynamic messages and
            subscribes if owner is interested

\\********************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TravellerPack.NoticeBoard
{
    public class MessageSecretary : SerializedMonoBehaviour
    {
        //Determines when subscription ends
        public enum SubscriptionPersistence
        {
            Endless,
            SingleUse,
            SoloFrame
        }

        //Holds subscription interest data
        public class Interest
        {
            public Interest(SubscriptionPersistence _persistence,
                SubscriberFunction _messageFunction,
                SubscriberFunction _removeFunction = null,
                SubscriberFunction _unsubscribeFunction = null)
            {
                m_persistence = _persistence;
                m_messageFunction = _messageFunction;
                m_removeFunction = _removeFunction;
                m_unsubscribeFunction = _unsubscribeFunction;
            }

            public SubscriptionPersistence m_persistence;
            public SubscriberFunction m_messageFunction;
            public SubscriberFunction m_removeFunction;
            public SubscriberFunction m_unsubscribeFunction;
        }

        //Keeps track of subscriptions that must be deallocated OnDestroy
        [Serializable]
        public class ActiveSubscription
        {
            public ActiveSubscription(LocalNoticeBoard _noticeBoard,
                string _messageHeader, SubscriberFunction _subscriberFunction,
                SubscriberFunction _unsubscribeFunction = null)
            {
                m_noticeBoard = _noticeBoard;
                m_dynamicMessageHeader = _messageHeader;
                m_staticMessageHeader = MessageHeader.Count;
                m_subscriberFunction = _subscriberFunction;
                m_unsubscribeFunction = _unsubscribeFunction;
            }

            public ActiveSubscription(MessageHeader _messageHeader,
                SubscriberFunction _subscriberFunction,
                SubscriberFunction _unsubscribeFunction = null)
            {
                m_noticeBoard = null;
                m_dynamicMessageHeader = "Global";
                m_staticMessageHeader = _messageHeader;
                m_subscriberFunction = _subscriberFunction;
                m_unsubscribeFunction = _unsubscribeFunction;
            }

            public LocalNoticeBoard m_noticeBoard;
            public string m_dynamicMessageHeader;
            public MessageHeader m_staticMessageHeader;
            public SubscriberFunction m_subscriberFunction;
            public SubscriberFunction m_unsubscribeFunction;
        }

        #region Active Subscriptions
        private Dictionary<string, ActiveSubscription> m_endlessActiveSubscriptions
            = new Dictionary<string, ActiveSubscription>();
        private Dictionary<string, ActiveSubscription> m_soloActiveSubscriptions
            = new Dictionary<string, ActiveSubscription>();
        private Dictionary<string, ActiveSubscription> m_prevSoloActiveSubscriptions
            = new Dictionary<string, ActiveSubscription>();
        private Dictionary<string, ActiveSubscription> m_singleActiveSubscriptions
            = new Dictionary<string, ActiveSubscription>();

        [SerializeField] private bool m_useEndless = true;
        [SerializeField] private bool m_useSolo = true;
        [SerializeField] private bool m_useSingle = true;

        private bool m_hasSolo = false;
        private bool m_hasSingle = false;
        #endregion

        #region Interest Settings        
        [Header("Subscriptions Interest Settings")]
        [SerializeField] private Dictionary<string, List<Interest>>
            m_prefixesOfInterest = new Dictionary<string, List<Interest>>();

        [SerializeField]
        private Dictionary<string, List<Interest>>
            m_dynamicMessagesOfInterest = new Dictionary<string, List<Interest>>();

        [SerializeField]
        private Dictionary<MessageHeader, List<Interest>>
            m_staticMessagesOfInterest = new Dictionary<MessageHeader, List<Interest>>();
        #endregion

        #region Unity Functions
        private void Awake()
        {
            SetupPreBuiltMessages();
            SetupRunTimeStaticMessages();
        }

        private void OnDestroy()
        {
            ClearPreBuiltMessages();
            ClearActiveSubscriptions();
        }

        private void Update()
        {
            UnsubscribeSoloSubscriptions();
        }
        #endregion

        #region Initialise/Clear
        /// <summary>Subscribes to hard-coded Messages.</summary>
        private void SetupPreBuiltMessages()
        {
            GlobalNoticeBoard.s_instance.SubscribeToMessage(
                MessageHeader.GLO_DynamicMessageCreated, OnDynamicMessageCreated);
        }

        /// <summary>Subscribes to intial static Messages.</summary>
        private void SetupRunTimeStaticMessages()
        {
            foreach (KeyValuePair<MessageHeader, List<Interest>> staticMessage in m_staticMessagesOfInterest)
            {
                foreach (Interest interest in staticMessage.Value)
                {
                    SubscribeGlobalMessage(staticMessage.Key, interest);
                }
            }
        }

        /// <summary>Clears subscriptions to pre-build Messages.</summary>
        private void ClearPreBuiltMessages()
        {
            GlobalNoticeBoard.s_instance.UnsubscribeToMessage(
                MessageHeader.GLO_DynamicMessageCreated, OnDynamicMessageCreated);
        }

        /// <summary>Clears subscription generated from specific interest.</summary>
        private void ClearActiveSubscription(Interest _interest, string _messageHeader)
        {
            string subscriptionKey = _messageHeader + " ~ " + _interest.m_messageFunction.Method;

            switch (_interest.m_persistence)
            {
                case SubscriptionPersistence.Endless:
                    {
                        m_endlessActiveSubscriptions.Remove(subscriptionKey);
                        break;
                    }
                case SubscriptionPersistence.SingleUse:
                    {
                        m_singleActiveSubscriptions.Remove(subscriptionKey);
                        break;
                    }
                case SubscriptionPersistence.SoloFrame:
                    {
                        m_soloActiveSubscriptions.Remove(subscriptionKey);
                        m_prevSoloActiveSubscriptions.Remove(subscriptionKey);
                        break;
                    }
                default:
                    break;
            }
        }

        /// <summary>Clears all subscriptions to run-time Messages.</summary>
        private void ClearActiveSubscriptions()
        {
            ClearEndlessSubscription();
            ClearSoloSubscriptions();
            ClearSingleSubscriptions();
        }

        /// <summary>Clears all endless active subscriptions to run-time Messages.</summary>
        private void ClearEndlessSubscription()
        {
            if (m_useEndless)
            {
                //Unsubscribe all endless subscriptions
                foreach (KeyValuePair<string, ActiveSubscription>
                    subscription in m_endlessActiveSubscriptions)
                {
                    //If LocalNoticeBoard is null, subscription is Global
                    if (subscription.Value.m_noticeBoard)
                    {
                        subscription.Value.m_noticeBoard.UnsubscribeToMessage(
                            subscription.Value.m_dynamicMessageHeader,
                            subscription.Value.m_subscriberFunction,
                            OnRemoveSubscriberList);
                    }
                    else if(GlobalNoticeBoard.s_instance != null)
                    {
                        GlobalNoticeBoard.s_instance.UnsubscribeToMessage(
                            subscription.Value.m_staticMessageHeader,
                            subscription.Value.m_subscriberFunction);
                    }
                }
                m_endlessActiveSubscriptions.Clear();
            }
        }

        /// <summary>Clears all Solo active subscriptions to run-time Messages.</summary>
        private void ClearSoloSubscriptions()
        {
            if (m_useSolo)
            {
                //Unsubscribe all current solo subscriptions
                foreach (KeyValuePair<string, ActiveSubscription>
                    subscription in m_soloActiveSubscriptions)
                {
                    //If LocalNoticeBoard is null, subscription is Global
                    if (subscription.Value.m_noticeBoard)
                    {
                        subscription.Value.m_noticeBoard.UnsubscribeToMessage(
                            subscription.Value.m_dynamicMessageHeader,
                            subscription.Value.m_subscriberFunction,
                            OnRemoveSubscriberList);
                    }
                    else
                    {
                        GlobalNoticeBoard.s_instance.UnsubscribeToMessage(
                            subscription.Value.m_staticMessageHeader,
                            subscription.Value.m_subscriberFunction);
                    }
                }

                //Unsubscribe all previous solo subscriptions
                foreach (KeyValuePair<string, ActiveSubscription>
                    subscription in m_prevSoloActiveSubscriptions)
                {
                    //If LocalNoticeBoard is null, subscription is Global
                    if (subscription.Value.m_noticeBoard)
                    {
                        subscription.Value.m_noticeBoard.UnsubscribeToMessage(
                            subscription.Value.m_dynamicMessageHeader,
                            subscription.Value.m_subscriberFunction,
                            OnRemoveSubscriberList);
                    }
                    else
                    {
                        GlobalNoticeBoard.s_instance.UnsubscribeToMessage(
                            subscription.Value.m_staticMessageHeader,
                            subscription.Value.m_subscriberFunction);
                    }
                }
                m_soloActiveSubscriptions.Clear();
                m_prevSoloActiveSubscriptions.Clear();
            }
        }

        /// <summary>Clears all Single active subscriptions to run-time Messages.</summary>
        private void ClearSingleSubscriptions()
        {
            if (m_useSingle)
            {
                //Unsubscribe all single subscriptions
                foreach (KeyValuePair<string,
                    ActiveSubscription> subscription in m_singleActiveSubscriptions)
                {
                    //If LocalNoticeBoard is null, subscription is Global
                    if (subscription.Value.m_noticeBoard)
                    {
                        subscription.Value.m_noticeBoard.UnsubscribeToMessage(
                            subscription.Value.m_dynamicMessageHeader,
                            subscription.Value.m_subscriberFunction,
                            OnRemoveSubscriberList);
                    }
                    else
                    {
                        GlobalNoticeBoard.s_instance.UnsubscribeToMessage(
                            subscription.Value.m_staticMessageHeader, subscription.Value.m_subscriberFunction);
                    }
                }
                m_singleActiveSubscriptions.Clear();
            }
        }

        /// <summary>Removes relevant ActiveSubscription and triggers remove functions.</summary>
        private void ClearRemovedSubscriptions(MPac_SubscriberListRemoved _package)
        {
            //Checks whether MessageHeader's prefix is of interest
            string prefix = MessageHelper.GetPrefix(_package.MessageHeader);
            if (m_prefixesOfInterest.ContainsKey(prefix))
            {
                //Checks whether Interest matches existing ActiveSubscription 
                foreach (Interest interest in m_prefixesOfInterest[prefix])
                {
                    ActiveSubscription subscription = GetActiveSubscription(interest.m_persistence,
                        _package.MessageHeader, interest.m_messageFunction);

                    //If it exists, clears ActiveSubscription
                    if (subscription != null)
                    {
                        if (subscription.m_dynamicMessageHeader == _package.MessageHeader)
                        {
                            interest.m_removeFunction(_package);
                            ClearActiveSubscription(interest, _package.MessageHeader);
                        }
                    }
                }
            }

            //Checks whether Messageheader matches existing ActiveSubscription
            if (m_dynamicMessagesOfInterest.ContainsKey(_package.MessageHeader))
            {
                //If it exists, clears ActiveSubscription
                foreach (Interest interest in m_dynamicMessagesOfInterest[_package.MessageHeader])
                {
                    interest.m_removeFunction(_package);
                    ClearActiveSubscription(interest, _package.MessageHeader);
                }
            }
        }
        #endregion

        #region Message Functions
        /// <summary>Receiver for "DynamicMessageCreated" message</summary>
        public void OnDynamicMessageCreated(object _package = null)
        {
            //Checks whether package is valid before casting
            if (_package != null)
            {
                MPac_DynamicMessageCreated package = (MPac_DynamicMessageCreated)_package;

                //Checks whether client is interested in message
                if(CheckInterestList(package.MessagePrefix, package.MessageHeader))
                {
                    SubscribeToDynamicMessage(package);
                }
            }
        }

        /// <summary>Receiver for "RemoveSubList" message</summary>
        public void OnRemoveSubscriberList(object _package = null)
        {
            //Checks whether package is valid before casting
            if (_package != null)
            {
                MPac_SubscriberListRemoved package = (MPac_SubscriberListRemoved)_package;
                ClearRemovedSubscriptions(package);
            }
        }
        #endregion

        #region Subscription Management Functions
        /// <summary>Determines whether owner is interested in this messages.</summary>
        private bool CheckInterestList(string _prefix, string _message)
        {
            return m_prefixesOfInterest.ContainsKey(_prefix) 
                || m_dynamicMessagesOfInterest.ContainsKey(_message);
        }

        /// <summary>Subscribe every function on prefix or message list to MessageHeader on LocalNoticeBoard.</summary>
        private void SubscribeToDynamicMessage(MPac_DynamicMessageCreated _messagePackage)
        {
            foreach (Interest interest in GetInterests(_messagePackage.MessagePrefix, _messagePackage.MessageHeader))
            {
                string subscriptionKey = _messagePackage.MessageHeader + " ~ " + interest.m_messageFunction.Method;

                _messagePackage.NoticeBoard.SubscribeToMessage(_messagePackage.MessageHeader,
                    interest.m_messageFunction, OnRemoveSubscriberList);

                //Adds active subscription to relevant list
                switch (interest.m_persistence)
                {
                    case SubscriptionPersistence.Endless:
                        {
                            m_endlessActiveSubscriptions.Add(subscriptionKey, new ActiveSubscription(
                                _messagePackage.NoticeBoard, _messagePackage.MessageHeader, interest.m_messageFunction,
                                interest.m_unsubscribeFunction));
                            break;
                        }
                    case SubscriptionPersistence.SingleUse:
                        {
                            m_singleActiveSubscriptions.Add(subscriptionKey, new ActiveSubscription(
                                _messagePackage.NoticeBoard, _messagePackage.MessageHeader, interest.m_messageFunction,
                                interest.m_unsubscribeFunction));
                            m_hasSingle = true;
                            break;
                        }
                    case SubscriptionPersistence.SoloFrame:
                        {
                            m_soloActiveSubscriptions.Add(subscriptionKey, new ActiveSubscription(
                                _messagePackage.NoticeBoard, _messagePackage.MessageHeader, interest.m_messageFunction,
                                interest.m_unsubscribeFunction));
                            m_hasSolo = true;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        }

        /// <summary>Subscribe every function on global list to MessageHeader on GlobalNoticeBoard.</summary>
        private void SubscribeGlobalMessage(MessageHeader _header, Interest _interest)
        {
            string subscriptionKey = _header.ToString() + " ~ " + _interest.m_messageFunction.Method;
            GlobalNoticeBoard.s_instance.SubscribeToMessage(_header, _interest.m_messageFunction);

            //Adds active subscription to relevant list
            switch (_interest.m_persistence)
            {
                case SubscriptionPersistence.Endless:
                    {
                        m_endlessActiveSubscriptions.Add(subscriptionKey,
                            new ActiveSubscription(_header, _interest.m_messageFunction,
                            _interest.m_unsubscribeFunction));
                        break;
                    }
                case SubscriptionPersistence.SingleUse:
                    {
                        m_singleActiveSubscriptions.Add(subscriptionKey,
                            new ActiveSubscription(_header, _interest.m_messageFunction,
                            _interest.m_unsubscribeFunction));
                        m_hasSingle = true;
                        break;
                    }
                case SubscriptionPersistence.SoloFrame:
                    {
                        m_soloActiveSubscriptions.Add(subscriptionKey, 
                            new ActiveSubscription(_header, _interest.m_messageFunction,
                            _interest.m_unsubscribeFunction));
                        m_hasSolo = true;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>Handle logic after subscription has triggered.</summary>
        public void SubscriptionTriggered(string _messageHeader, SubscriberFunction _messageFunction)
        {
            //If single use, unsubscribe ActiveSubscription
            UnsubscribeSingleSubscription(_messageHeader, _messageFunction);
        }

        /// <summary>Unsubscribe previous frame solo subscriptions.</summary>
        private void UnsubscribeSoloSubscriptions()
        {
            if (m_useSolo && m_hasSolo)
            {
                //Unsubscribe previous solo subscriptions from LocalNoticeBoards
                foreach (KeyValuePair<string, ActiveSubscription>
                    subscription in m_prevSoloActiveSubscriptions)
                {
                    if (subscription.Value.m_noticeBoard)
                    {
                        subscription.Value.m_noticeBoard.UnsubscribeToMessage(
                            subscription.Value.m_dynamicMessageHeader,
                            subscription.Value.m_subscriberFunction,
                            OnRemoveSubscriberList);

                        //Creates LocalNoticeBoard unsubscription package
                        MPac_UnsubscribedLocalMessage package = new MPac_UnsubscribedLocalMessage(
                            subscription.Value.m_dynamicMessageHeader,
                            "Local Message: " + subscription.Value.m_dynamicMessageHeader
                            + " ~Unsubscribed", subscription.Value.m_dynamicMessageHeader);
                        subscription.Value.m_unsubscribeFunction?.Invoke(package);
                    }
                    else
                    {
                        GlobalNoticeBoard.s_instance.UnsubscribeToMessage(
                            subscription.Value.m_staticMessageHeader,
                            subscription.Value.m_subscriberFunction);


                        //Creates GlobalNoticeBoard unsubscription package
                        MPac_UnsubscribedGlobalMessage package = new MPac_UnsubscribedGlobalMessage(
                            subscription.Value.m_staticMessageHeader,
                             "Global Message: " + subscription.Value.m_staticMessageHeader.ToString()
                            + " ~Unsubscribed", subscription.Value.m_staticMessageHeader.ToString());
                        subscription.Value.m_unsubscribeFunction?.Invoke(package);
                    }
                }
                m_prevSoloActiveSubscriptions.Clear();
                
                //Transfer current buffer to previous buffer
                foreach (KeyValuePair<string, ActiveSubscription> 
                    subscription in m_soloActiveSubscriptions)
                {
                    m_prevSoloActiveSubscriptions.Add(subscription.Key, subscription.Value);
                }
                m_soloActiveSubscriptions.Clear();

                if (m_prevSoloActiveSubscriptions.Count == 0)
                {
                    m_hasSolo = false;
                }
            }
        }

        /// <summary>Unsubscribe from single use subscription.</summary>
        private void UnsubscribeSingleSubscription(string _messageHeader, SubscriberFunction _messageFunction)
        {
            if (m_useSingle && m_hasSingle)
            {
                string subscriptionKey = _messageHeader + " ~ " + _messageFunction.Method;

                //If in single active subscriptions, unsubscribe from LocalNoticeBoard
                if (m_singleActiveSubscriptions.ContainsKey(subscriptionKey))
                {
                    if (m_singleActiveSubscriptions[subscriptionKey].m_noticeBoard)
                    {
                        m_singleActiveSubscriptions[subscriptionKey].m_noticeBoard.UnsubscribeToMessage(
                            m_singleActiveSubscriptions[subscriptionKey].m_dynamicMessageHeader, _messageFunction,
                            OnRemoveSubscriberList);
                    }
                    else
                    {
                        GlobalNoticeBoard.s_instance.UnsubscribeToMessage(
                            m_singleActiveSubscriptions[subscriptionKey].m_staticMessageHeader, _messageFunction);
                    }

                    MPac_UnsubscribedLocalMessage package = new MPac_UnsubscribedLocalMessage(
                        m_singleActiveSubscriptions[subscriptionKey].m_dynamicMessageHeader,
                        "Local Message: " + m_singleActiveSubscriptions[subscriptionKey].m_dynamicMessageHeader
                        + " ~Unsubscribed", m_singleActiveSubscriptions[subscriptionKey].m_dynamicMessageHeader);
                    m_singleActiveSubscriptions[subscriptionKey].m_unsubscribeFunction?.Invoke(package);
                    m_singleActiveSubscriptions.Remove(subscriptionKey);
                }

                if(m_singleActiveSubscriptions.Count == 0)
                {
                    m_hasSingle = false;
                }
            }
        }

        /// <summary>Unsubscribe from global subscription.</summary>
        private void UnsubscribeGlobalMessage(MessageHeader _messageHeader, Interest _interest)
        {
            string subscriptionKey = _messageHeader.ToString() + " ~ " + _interest.m_messageFunction.Method;

            GlobalNoticeBoard.s_instance.UnsubscribeToMessage(_messageHeader, _interest.m_messageFunction);

            //If UnsubscribeFunction, send unsub package
            MPac_UnsubscribedGlobalMessage package = new MPac_UnsubscribedGlobalMessage(
                _messageHeader, "Global Message: " + _messageHeader.ToString() + " ~Unsubscribed", 
                _messageHeader.ToString());
            _interest.m_unsubscribeFunction?.Invoke(package);


            //Removes active subscription to relevant list
            switch (_interest.m_persistence)
            {
                case SubscriptionPersistence.Endless:
                    {
                        m_endlessActiveSubscriptions.Remove(subscriptionKey);
                        break;
                    }
                case SubscriptionPersistence.SingleUse:
                    {
                        m_singleActiveSubscriptions.Remove(subscriptionKey);
                        break;
                    }
                case SubscriptionPersistence.SoloFrame:
                    {
                        m_soloActiveSubscriptions.Remove(subscriptionKey);
                        m_prevSoloActiveSubscriptions.Remove(subscriptionKey);
                        m_hasSolo = true;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        #endregion

        #region Add/Remove Functions
        /// <summary>Add single interest to prefix intersets.</summary>
        public void AddPrefixInterest(string _prefix, Interest _interest)
        {
            //If Prefix list doesn't exist, add new List
            if (!m_prefixesOfInterest.ContainsKey(_prefix))
            {
                List<Interest> intersets = new List<Interest>();
                intersets.Add(_interest);
                m_prefixesOfInterest.Add(_prefix, intersets);
            }
            else
            {
                m_prefixesOfInterest[_prefix].Add(_interest);
            }
        }

        /// <summary>Add multiple interest to prefix intersets.</summary>
        public void AddPrefixInterests(string _prefix, List<Interest> _interests)
        {
            if (!m_prefixesOfInterest.ContainsKey(_prefix))
            {
                m_prefixesOfInterest.Add(_prefix, _interests);
            }
            else
            {
                m_prefixesOfInterest[_prefix].AddRange(_interests);
            }
        }

        /// <summary>Remove single interest to from prefix MessageHeader key.</summary>
        public void RemovePrefixInterest(string _prefix, SubscriberFunction _function)
        {
            if (m_prefixesOfInterest.ContainsKey(_prefix))
            {
                m_prefixesOfInterest[_prefix].Remove(GetPrefixInterest(_prefix, _function));
            }
        }

        /// <summary>Remove all intersets under prefix MessageHeader key.</summary>
        public void RemovePrefix(string _prefix)
        {
            if (m_prefixesOfInterest.ContainsKey(_prefix))
            {
                m_prefixesOfInterest[_prefix].Clear();
                m_prefixesOfInterest.Remove(_prefix);
            }
        }

        /// <summary>Add single interest to dynamic intersets.</summary>
        public void AddDynamicInterest(string _messageHeader, Interest _interest)
        {
            //If MesageHeader list doesn't exist, add new List
            if (!m_dynamicMessagesOfInterest.ContainsKey(_messageHeader))
            {
                List<Interest> intersets = new List<Interest>();
                intersets.Add(_interest);
                m_dynamicMessagesOfInterest.Add(_messageHeader, intersets);
            }
            else
            {
                m_dynamicMessagesOfInterest[_messageHeader].Add(_interest);
            }
        }

        /// <summary>Add multiple interest to dynamic intersets.</summary>
        public void AddDynamicInterests(string _messageHeader, List<Interest> _interests)
        {
            if (!m_dynamicMessagesOfInterest.ContainsKey(_messageHeader))
            {
                m_dynamicMessagesOfInterest.Add(_messageHeader, _interests);
            }
            else
            {
                m_dynamicMessagesOfInterest[_messageHeader].AddRange(_interests);
            }
        }

        /// <summary>Remove single interest to from dynamic MessageHeader key.</summary>
        public void RemoveDynamicInterest(string _messageHeader, SubscriberFunction _function)
        {
            if (m_dynamicMessagesOfInterest.ContainsKey(_messageHeader))
            {
                m_dynamicMessagesOfInterest[_messageHeader].Remove(GetDynamicInterest(_messageHeader, _function));
            }
        }

        /// <summary>Remove all intersets under dynamic MessageHeader key.</summary>
        public void RemoveDynamicHeader(string _messageHeader)
        {
            if (m_dynamicMessagesOfInterest.ContainsKey(_messageHeader))
            {
                m_dynamicMessagesOfInterest[_messageHeader].Clear();
                m_dynamicMessagesOfInterest.Remove(_messageHeader);
            }
        }

        /// <summary>Add single interest to static intersets.</summary>
        public void AddStaticInterest(MessageHeader _messageHeader, Interest _interest)
        {
            //If Static list doesn't exist, add new List
            if (!m_staticMessagesOfInterest.ContainsKey(_messageHeader))
            {
                List<Interest> intersets = new List<Interest>();
                intersets.Add(_interest);
                m_staticMessagesOfInterest.Add(_messageHeader, intersets);
                SubscribeGlobalMessage(_messageHeader, _interest);
            }
            else
            {
                m_staticMessagesOfInterest[_messageHeader].Add(_interest);
                SubscribeGlobalMessage(_messageHeader, _interest);
            }
        }

        /// <summary>Add multiple interest to static intersets.</summary>
        public void AddStaticInterests(MessageHeader _messageHeader, List<Interest> _interests)
        {
            if (!m_staticMessagesOfInterest.ContainsKey(_messageHeader))
            {
                m_staticMessagesOfInterest.Add(_messageHeader, _interests);
                foreach (Interest interest in _interests)
                {
                    SubscribeGlobalMessage(_messageHeader, interest);
                }
            }
            else
            {
                m_staticMessagesOfInterest[_messageHeader].AddRange(_interests);
                foreach (Interest interest in _interests)
                {
                    SubscribeGlobalMessage(_messageHeader, interest);
                }
            }
        }

        /// <summary>Remove single interest to from static MessageHeader key.</summary>
        public void RemoveStaticInterest(MessageHeader _messageHeader, SubscriberFunction _function)
        {
            if (m_staticMessagesOfInterest.ContainsKey(_messageHeader))
            {
                Interest interest = GetStaticInterest(_messageHeader, _function);

                //Must unsubscribe from GlobalNoticeBoard to prevent dangling references
                UnsubscribeGlobalMessage(_messageHeader, interest);
                m_staticMessagesOfInterest[_messageHeader].Remove(interest);
            }
        }

        /// <summary>Remove all intersets under static MessageHeader key.</summary>
        public void RemoveStaticHeader(MessageHeader _messageHeader)
        {
            if (m_staticMessagesOfInterest.ContainsKey(_messageHeader))
            {
                //Must unsubscribe from GlobalNoticeBoard to prevent dangling references
                foreach (Interest interest in m_staticMessagesOfInterest[_messageHeader])
                {
                    UnsubscribeGlobalMessage(_messageHeader, interest);
                }

                m_staticMessagesOfInterest[_messageHeader].Clear();
                m_staticMessagesOfInterest.Remove(_messageHeader);
            }
        }
        #endregion

        #region Gettters/Setters
        /// <summary>Returns interests from prefix lists.</summary>
        private Interest GetPrefixInterest(string _prefix, SubscriberFunction _function)
        {
            Interest result = null;
            if(m_prefixesOfInterest.ContainsKey(_prefix))
            {
                foreach (Interest interest in m_prefixesOfInterest[_prefix])
                {
                    if(interest.m_messageFunction == _function)
                    {
                        result = interest;
                    }
                }
            }
            return result;
        }

        /// <summary>Returns interests from dynamic lists.</summary>
        private Interest GetDynamicInterest(string _messageHeader, SubscriberFunction _function)
        {
            Interest result = null;
            if (m_dynamicMessagesOfInterest.ContainsKey(_messageHeader))
            {
                foreach (Interest interest in m_dynamicMessagesOfInterest[_messageHeader])
                {
                    if (interest.m_messageFunction == _function)
                    {
                        result = interest;
                    }
                }
            }
            return result;
        }

        /// <summary>Returns interests from static lists.</summary>
        private Interest GetStaticInterest(MessageHeader _messageHeader, SubscriberFunction _function)
        {
            Interest result = null;
            if (m_staticMessagesOfInterest.ContainsKey(_messageHeader))
            {
                foreach (Interest interest in m_staticMessagesOfInterest[_messageHeader])
                {
                    if (interest.m_messageFunction == _function)
                    {
                        result = interest;
                    }
                }
            }
            return result;
        }

        /// <summary> Returns interests from either Prefix or MessageHeader lists.
        /// Prioritise MessageHeaders over Prefix Interest.</summary>
        private List<Interest> GetInterests(string _prefix, string _messageHeader)
        {
            List<Interest> results = new List<Interest>();
            if (m_dynamicMessagesOfInterest.ContainsKey(_messageHeader))
            {
                results.AddRange(m_dynamicMessagesOfInterest[_messageHeader]);
            }
            else if (m_prefixesOfInterest.ContainsKey(_prefix))
            {
                results.AddRange(m_prefixesOfInterest[_prefix]);
            }
            return results;
        }

        /// <summary>Gets ActiveSubscription from a specific Subscription List by persistance.</summary>
        private ActiveSubscription GetActiveSubscription(SubscriptionPersistence _persistence, string _messageHeader, SubscriberFunction _messageFunction)
        {
            ActiveSubscription result = null;

            switch (_persistence)
            {
                case SubscriptionPersistence.Endless:
                    {
                        result = GetEndlessSubscription(_messageHeader, _messageFunction);
                        break;
                    }
                case SubscriptionPersistence.SingleUse:
                    {
                        result = GetSingleSubscription(_messageHeader, _messageFunction);
                        break;
                    }
                case SubscriptionPersistence.SoloFrame:
                    {
                        result = GetSoloSubscription(_messageHeader, _messageFunction);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return result;
        }

        /// <summary>Gets Endless ActiveSubscription if SubscriptionKey exists.</summary>
        private ActiveSubscription GetEndlessSubscription(string _messageHeader, SubscriberFunction _messageFunction)
        {
            string subscriptionKey = _messageHeader + " ~ " + _messageFunction.Method;
            if (m_endlessActiveSubscriptions.ContainsKey(subscriptionKey))
            {
                return m_endlessActiveSubscriptions[subscriptionKey];
            }
            return null;
        }

        /// <summary>Gets Single ActiveSubscription if SubscriptionKey exists.</summary>
        private ActiveSubscription GetSingleSubscription(string _messageHeader, SubscriberFunction _messageFunction)
        {
            string subscriptionKey = _messageHeader + " ~ " + _messageFunction.Method;
            if (m_singleActiveSubscriptions.ContainsKey(subscriptionKey))
            {
                return m_singleActiveSubscriptions[subscriptionKey];
            }
            return null;
        }

        /// <summary>Gets Solo ActiveSubscription if SubscriptionKey exists.</summary>
        private ActiveSubscription GetSoloSubscription(string _messageHeader, SubscriberFunction _messageFunction)
        {
            string subscriptionKey = _messageHeader + " ~ " + _messageFunction.Method;
            if (m_soloActiveSubscriptions.ContainsKey(subscriptionKey))
            {
                return m_soloActiveSubscriptions[subscriptionKey];
            }
            return null;
        }
        #endregion
    }
}