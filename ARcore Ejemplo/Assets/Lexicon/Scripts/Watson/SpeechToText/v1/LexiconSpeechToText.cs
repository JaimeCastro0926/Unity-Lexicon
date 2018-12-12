/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

/* 
 * Mixspace Lexicon Changes:
 *   - Disabled Keep Alive routine.
 *   - Capture prefix audio and send to improve first-word detection.
 *   - Status update callback.
 */

//#define ENABLE_DEBUGGING

using System;
using System.Collections;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using MiniJSON;
using UnityEngine;

namespace Mixspace.Lexicon.Watson.SpeechToText.v1
{
    /// <summary>
    /// This class wraps the Watson Speech to Text service.
    /// <a href="http://www.ibm.com/watson/developercloud/speech-to-text.html">Speech to Text Service</a>
    /// </summary>
    public class LexiconSpeechToText : IWatsonService
    {
        #region Constants
        /// <summary>
        /// This ID is used to match up a configuration record with this service.
        /// </summary>
        private const string ServiceId = "SpeechToTextV1";
        /// <summary>
        /// How often to send a message to the web socket to keep it alive.
        /// </summary>
        private const float WsKeepAliveInterval = 20.0f;
        /// <summary>
        /// If no listen state is received after start is sent within this time, we will timeout
        /// and stop listening. 
        /// </summary>
        private const float ListenTimeout = 10.0f;
        /// <summary>
        /// How many recording AudioClips will we queue before we enter a error state.
        /// </summary>
        private const int MaxQueuedRecordings = 1000;
        /// <summary>
        /// Size of a clip in bytes that can be sent through the Recognize function.
        /// </summary>
        private const int MaxRecognizeClipSize = 4 * (1024 * 1024);
        #endregion

        #region Public Types
        /// <summary>
        /// This callback is used to return errors through the OnError property.
        /// </summary>
        /// <param name="error">A string containing the error message.</param>
        public delegate void ErrorEvent(string error);
        /// <summary>
        /// The delegate for loading a file.
        /// </summary>
        /// <param name="filename">The filename to load.</param>
        /// <returns>Should return a byte array of the file contents or null of failure.</returns>
        public delegate byte[] LoadFileDelegate(string filename);
        /// <summary>
        /// Set this property to overload the internal file loading of this class.
        /// </summary>
        public LoadFileDelegate LoadFile { get; set; }
        /// <summary>
        /// This callback is used to return the status through the OnStatusUpdate property.
        /// </summary>
        public delegate void StatusUpdate(LexiconSpeechStatus status);
        #endregion

        #region Private Data
        private OnRecognize _listenCallback = null;        // Callback is set by StartListening()    
        private OnRecognizeSpeaker _speakerLabelCallback = null;
        private WSConnector _listenSocket = null;          // WebSocket object used when StartListening() is invoked  
        private bool _listenActive = false;
        private bool _audioSent = false;
        private bool _isListening = false;
        private Queue<AudioData> _listenRecordings = new Queue<AudioData>();
        private int _keepAliveRoutine = 0;             // ID of the keep alive co-routine
        private DateTime _lastKeepAlive = DateTime.Now;
        private DateTime _lastStartSent = DateTime.Now;
        private string _recognizeModel = "en-US_BroadbandModel";   // ID of the model to use.
        private int _maxAlternatives = 1;              // maximum number of alternatives to return.
        private string[] _keywords = { "" };
        private float _keywordsThreshold = 0.5f;
        private float? _wordAlternativesThreshold = null;
        private bool _profanityFilter = true;
        private bool _smartFormatting = false;
        private bool _speakerLabels = false;
        private bool _timestamps = false;
        private bool _wordConfidence = false;
        private bool _detectSilence = true;            // If true, then we will try not to record silence.
        private float _silenceThreshold = 0.0f;         // If the audio level is below this value, then it's considered silent.
        private int _recordingHZ = -1;
        private int _inactivityTimeout = 60;
        private string _customization_id = null;
        private string _acoustic_customization_id = null;
        private float _customization_weight = 0.3f;
        private bool _streamMultipart = false;           //  If true sets `Transfer-Encoding` header of multipart request to `chunked`.
        private float _silenceDuration = 0.0f;
        private float _silenceCutoff = 1.0f;

        //private fsSerializer _serializer = new fsSerializer();
        private Credentials _credentials = null;
        private string _url = "https://stream.watsonplatform.net/speech-to-text/api";

        private LexiconSpeechStatus _currentStatus;

        private List<AudioData> _prefixClips = new List<AudioData>();
        private int _prefixClipCount = 10; // Each clip is 20 ms with current setup.
        private bool _stopSent = false;
        private float _timeOffset = 0.0f;
        private bool _sendStopAfterListening;
        #endregion

        #region Public Properties
        /// <summary>
        /// True if StartListening() has been called.
        /// </summary>
        public bool IsListening { get { return _isListening; } private set { _isListening = value; } }
        /// <summary>
        /// True if AudioData has been sent and we are recognizing speech.
        /// </summary>
        public bool AudioSent { get { return _audioSent; } }
        /// <summary>
        /// This delegate is invoked when an error occurs.
        /// </summary>
        public ErrorEvent OnError { get; set; }
        /// <summary>
        /// This property controls which recognize model we use when making recognize requests of the server.
        /// </summary>
        public string RecognizeModel
        {
            get { return _recognizeModel; }
            set
            {
                if (_recognizeModel != value)
                {
                    _recognizeModel = value;
                    StopListening();        // close any active connection when our model is changed.
                }
            }
        }
        /// <summary>
        /// Returns the maximum number of alternatives returned by recognize.
        /// </summary>
        public int MaxAlternatives { get { return _maxAlternatives; } set { _maxAlternatives = value; } }
        /// <summary>
        /// True to return timestamps of words with results.
        /// </summary>
        public bool EnableTimestamps { get { return _timestamps; } set { _timestamps = value; } }
        /// <summary>
        /// True to return word confidence with results.
        /// </summary>
        public bool EnableWordConfidence { get { return _wordConfidence; } set { _wordConfidence = value; } }
        /// <summary>
        /// If true, then we will get interim results while recognizing. The user will then need to check 
        /// the Final flag on the results.
        /// </summary>
        public bool EnableInterimResults { get; set; }
        /// <summary>
        /// If true, then we will try not to send silent audio clips to the server. This can save bandwidth
        /// when no sound is happening.
        /// </summary>
        public bool DetectSilence { get { return _detectSilence; } set { _detectSilence = value; } }
        /// <summary>
        /// A value from 1.0 to 0.0 that determines what is considered silence. If the absolute value of the 
        /// audio level is below this value then we consider it silence.
        /// </summary>
        public float SilenceThreshold
        {
            get { return _silenceThreshold; }
            set
            {
                if (value < 0f || value > 1f)
                {
                    throw new ArgumentOutOfRangeException("Silence threshold should be between 0.0f and 1.0f");
                }
                else
                {
                    _silenceThreshold = value;
                }
            }
        }
        /// <summary>
        /// Gets and sets the endpoint URL for the service.
        /// </summary>
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        /// <summary>
        /// Gets and sets the credentials of the service. Replace the default endpoint if endpoint is defined.
        /// </summary>
        public Credentials Credentials
        {
            get { return _credentials; }
            set
            {
                _credentials = value;
                if (!string.IsNullOrEmpty(_credentials.Url))
                {
                    _url = _credentials.Url;
                }
            }
        }

        /// <summary>
        /// NON-MULTIPART ONLY: Array of keyword strings to spot in the audio. Each keyword string can include one or more tokens. Keywords are spotted only in the final hypothesis, not in interim results. Omit the parameter or specify an empty array if you do not need to spot keywords.
        /// </summary>
        public string[] Keywords { get { return _keywords; } set { _keywords = value; } }
        /// <summary>
        /// NON-MULTIPART ONLY: Confidence value that is the lower bound for spotting a keyword. A word is considered to match a keyword if its confidence is greater than or equal to the threshold. Specify a probability between 0 and 1 inclusive. No keyword spotting is performed if you omit the parameter. If you specify a threshold, you must also specify one or more keywords.
        /// </summary>
        public float KeywordsThreshold { get { return _keywordsThreshold; } set { _keywordsThreshold = value; } }
        /// <summary>
        /// NON-MULTIPART ONLY: Confidence value that is the lower bound for identifying a hypothesis as a possible word alternative (also known as "Confusion Networks"). An alternative word is considered if its confidence is greater than or equal to the threshold. Specify a probability between 0 and 1 inclusive. No alternative words are computed if you omit the parameter.
        /// </summary>
        public float? WordAlternativesThreshold { get { return _wordAlternativesThreshold; } set { _wordAlternativesThreshold = value; } }
        /// <summary>
        /// NON-MULTIPART ONLY: If true (the default), filters profanity from all output except for keyword results by replacing inappropriate words with a series of asterisks. Set the parameter to false to return results with no censoring. Applies to US English transcription only.
        /// </summary>
        public bool ProfanityFilter { get { return _profanityFilter; } set { _profanityFilter = value; } }
        /// <summary>
        /// NON-MULTIPART ONLY: If true, converts dates, times, series of digits and numbers, phone numbers, currency values, and Internet addresses into more readable, conventional representations in the final transcript of a recognition request. If false (the default), no formatting is performed. Applies to US English transcription only.
        /// </summary>
        public bool SmartFormatting { get { return _smartFormatting; } set { _smartFormatting = value; } }
        /// <summary>
        /// NON-MULTIPART ONLY: Indicates whether labels that identify which words were spoken by which participants in a multi-person exchange are to be included in the response. If true, speaker labels are returned; if false (the default), they are not. Speaker labels can be returned only for the following language models: en-US_NarrowbandModel, en-US_BroadbandModel, es-ES_NarrowbandModel, es-ES_BroadbandModel, ja-JP_NarrowbandModel, and ja-JP_BroadbandModel. Setting speaker_labels to true forces the timestamps parameter to be true, regardless of whether you specify false for the parameter.
        /// </summary>
        public bool SpeakerLabels { get { return _speakerLabels; } set { _speakerLabels = value; } }
        /// <summary>
        /// NON-MULTIPART ONLY: The time in seconds after which, if only silence (no speech) is detected in submitted audio, the connection is closed with a 400 error. Useful for stopping audio submission from a live microphone when a user simply walks away. Use -1 for infinity.
        /// </summary>
        public int InactivityTimeout { get { return _inactivityTimeout; } set { _inactivityTimeout = value; } }
        /// <summary>
        /// Specifies the Globally Unique Identifier (GUID) of a custom language model that is to be used for all requests sent over the connection. The base model of the custom language model must match the value of the model parameter. By default, no custom language model is used. For more information, see https://console.bluemix.net/docs/services/speech-to-text/custom.html.
        /// </summary>
        public string CustomizationId { get { return _customization_id; } set { _customization_id = value; } }
        /// <summary>
        /// Specifies the Globally Unique Identifier (GUID) of a custom acoustic model that is to be used for all requests sent over the connection. The base model of the custom acoustic model must match the value of the model parameter. By default, no custom acoustic model is used. For more information, see https://console.bluemix.net/docs/services/speech-to-text/custom.html.
        /// </summary>
        public string AcousticCustomizationId { get { return _acoustic_customization_id; } set { _acoustic_customization_id = value; } }
        /// <summary>
        /// Specifies the weight the service gives to words from a specified custom language model compared to those from the base model for all requests sent over the connection. Specify a value between 0.0 and 1.0; the default value is 0.3. For more information, see https://console.bluemix.net/docs/services/speech-to-text/language-use.html#weight.
        /// </summary>
        public float CustomizationWeight { get { return _customization_weight; } set { _customization_weight = value; } }
        /// <summary>
        /// If true sets `Transfer-Encoding` request header to `chunked` causing the audio to be streamed to the service. By default, audio is sent all at once as a one-shot delivery. See https://console.bluemix.net/docs/services/speech-to-text/input.html#transmission.
        /// </summary>
        public bool StreamMultipart { get { return _streamMultipart; } set { _streamMultipart = value; } }

        /// <summary>
        /// This delegate is invoked when the status changes.
        /// </summary>
        public StatusUpdate OnStatusUpdate { get; set; }
        /// <summary>
        /// Gets or sets the current status.
        /// </summary>
        public LexiconSpeechStatus CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                if (_currentStatus != value)
                {
                    _currentStatus = value;
                    if (OnStatusUpdate != null)
                    {
                        OnStatusUpdate(_currentStatus);
                    }
                }
            }
        }
        /// <summary>
        /// Time offset for the last call to OnListen (e.g. if buffered audio is sent)
        /// </summary>
        public float TimeOffset { get { return _timeOffset; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Speech to Text constructor.
        /// </summary>
        /// <param name="credentials">The service credentials.</param>
        public LexiconSpeechToText(Credentials credentials)
        {
            if (credentials.HasCredentials() || credentials.HasWatsonAuthenticationToken())
            {
                Credentials = credentials;
            }
            else
            {
                throw new WatsonException("Please provide a username and password or authorization token to use the Speech to Text service. For more information, see https://github.com/watson-developer-cloud/unity-sdk/#configuring-your-service-credentials");
            }
        }
        #endregion

        #region Sessionless - Streaming
        /// <summary>
        /// This callback object is used by the Recognize() and StartListening() methods.
        /// </summary>
        /// <param name="results">The ResultList object containing the results.</param>
        public delegate void OnRecognize(SpeechRecognitionEvent results);

        /// <summary>
        /// This callback object is used by the RecognizeSpeaker() method.
        /// </summary>
        /// <param name="speakerRecognitionEvent">Array of speaker label results.</param>
        public delegate void OnRecognizeSpeaker(SpeakerRecognitionEvent speakerRecognitionEvent);

        /// <summary>
        /// This starts the service listening and it will invoke the callback for any recognized speech.
        /// OnListen() must be called by the user to queue audio data to send to the service. 
        /// StopListening() should be called when you want to stop listening.
        /// </summary>
        /// <param name="callback">All recognize results are passed to this callback.</param>
        /// <param name="speakerLabelCallback">Speaker label goes through this callback if it arrives separately from recognize result.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public bool StartListening(OnRecognize callback, OnRecognizeSpeaker speakerLabelCallback = null)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (_isListening)
                return false;
            if (!CreateListenConnector())
                return false;

            _isListening = true;
            _listenCallback = callback;
            if (speakerLabelCallback != null)
                _speakerLabelCallback = speakerLabelCallback;
            //_keepAliveRoutine = Runnable.Run(KeepAlive());
            _lastKeepAlive = DateTime.Now;
            _sendStopAfterListening = false;

            return true;
        }

        /// <summary>
        /// This function should be invoked with the AudioData input after StartListening() method has been invoked.
        /// The user should continue to invoke this function until they are ready to call StopListening(), typically
        /// microphone input is sent to this function.
        /// </summary>
        /// <param name="clip">A AudioData object containing the AudioClip and max level found in the clip.</param>
        /// <returns>True if audio was sent or enqueued, false if audio was discarded.</returns>
        public bool OnListen(AudioData clip)
        {
            bool audioSentOrEnqueued = false;
            _timeOffset = 0;

            if (_isListening)
            {
                if (_recordingHZ < 0)
                {
                    _recordingHZ = clip.Clip.frequency;
                    SendStart();
                }

                // If silence persists for _silenceCutoff seconds, send stop and discard clips until audio resumes
                if (DetectSilence && clip.MaxLevel < _silenceThreshold)
                {
                    _silenceDuration += clip.Clip.length;
                }
                else
                {
                    _silenceDuration = 0.0f;
                }

                if (!DetectSilence || _silenceDuration < _silenceCutoff)
                {
                    if (_stopSent)
                    {
                        // Send some clips of ambient sound leading up to the audio, to improve first word recognition
                        if (_listenActive)
                        {
                            //Debug.Log("Sending " + _prefixClips.Count + " prefix clips");
                            foreach (AudioData prefixClip in _prefixClips)
                            {
                                _listenSocket.Send(new WSConnector.BinaryMessage(AudioClipUtil.GetL16(prefixClip.Clip)));
                                _timeOffset -= prefixClip.Clip.length;
                            }
                        }
                        else
                        {
                            //Debug.Log("Queuing " + _prefixClips.Count + " prefix clips");
                            foreach (AudioData prefixClip in _prefixClips)
                            {
                                _listenRecordings.Enqueue(clip);
                                _timeOffset -= prefixClip.Clip.length;
                            }
                        }
                        _prefixClips.Clear();
                        _stopSent = false;
                    }

                    if (_listenActive)
                    {
                        CurrentStatus = LexiconSpeechStatus.Sending;
                        _listenSocket.Send(new WSConnector.BinaryMessage(AudioClipUtil.GetL16(clip.Clip)));
                        _audioSent = true;
                        audioSentOrEnqueued = true;
                    }
                    else
                    {
                        CurrentStatus = LexiconSpeechStatus.WaitingForServer;
                        // we have not received the "listening" state yet from the server, so just queue
                        // the audio clips until that happens.
                        _listenRecordings.Enqueue(clip);
                        audioSentOrEnqueued = true;

                        // check the length of this queue and do something if it gets too full.
                        if (_listenRecordings.Count > MaxQueuedRecordings)
                        {
                            Log.Error("SpeechToText.OnListen()", "Recording queue is full.");

                            StopListening();
                            if (OnError != null)
                                OnError("Recording queue is full.");
                        }
                    }
                }
                else if (_audioSent)
                {
                    CurrentStatus = LexiconSpeechStatus.Silence;
                    //Debug.Log("Send stop");
                    SendStop();
                    _audioSent = false;
                    _stopSent = true;
                    _prefixClips.Clear();
                }
                else
                {
                    // Buffer some of the ambient audio for when the user starts speaking again
                    _prefixClips.Add(clip);

                    if (_prefixClips.Count > _prefixClipCount)
                    {
                        _prefixClips.RemoveAt(0);
                    }
                }

                // After sending start, we should get into the listening state within the amount of time specified
                // by LISTEN_TIMEOUT. If not, then stop listening and record the error.
                if (!_listenActive && (DateTime.Now - _lastStartSent).TotalSeconds > ListenTimeout)
                {
                    Log.Error("SpeechToText.OnListen()", "Failed to enter listening state.");

                    StopListening();
                    if (OnError != null)
                        OnError("Failed to enter listening state.");
                }
            }

            return audioSentOrEnqueued;
        }

        /// <summary>
        /// Invoke this function stop this service from listening.
        /// </summary>
        /// <returns>Returns true on success, false on failure.</returns>
        public bool StopListening()
        {
            if (!_isListening)
                return false;

            _isListening = false;
            CloseListenConnector();

            if (_keepAliveRoutine != 0)
            {
                Runnable.Stop(_keepAliveRoutine);
                _keepAliveRoutine = 0;
            }

            _listenRecordings.Clear();
            _prefixClips.Clear();
            _listenCallback = null;
            _recordingHZ = -1;

            Debug.Log("Stop Listening");
            CurrentStatus = LexiconSpeechStatus.Stopped;

            return true;
        }

        private bool CreateListenConnector()
        {
            if (_listenSocket == null)
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(CustomizationId))
                    queryParams["customization_id"] = CustomizationId;
                if (!string.IsNullOrEmpty(AcousticCustomizationId))
                    queryParams["acoustic_customization_id"] = AcousticCustomizationId;
                if (!string.IsNullOrEmpty(CustomizationId))
                    queryParams["customization_weight"] = CustomizationWeight.ToString();

                string parsedParams = "";

                foreach (KeyValuePair<string, string> kvp in queryParams)
                {
                    parsedParams += string.Format("&{0}={1}", kvp.Key, kvp.Value);
                }

                _listenSocket = WSConnector.CreateConnector(Credentials, "/v1/recognize", "?model=" + WWW.EscapeURL(_recognizeModel) + parsedParams);
                if (_listenSocket == null)
                {
                    return false;
                }
                else
                {
#if ENABLE_DEBUGGING
                    Log.Debug("SpeechToText.CreateListenConnector()", "Created listen socket. Model: {0}, parsedParams: {1}", WWW.EscapeURL(_recognizeModel), parsedParams);
#endif
                }

                _listenSocket.OnMessage = OnListenMessage;
                _listenSocket.OnClose = OnListenClosed;
            }

            return true;
        }

        private void CloseListenConnector()
        {
            if (_listenSocket != null)
            {
                _listenSocket.Close();
                _listenSocket = null;
            }
        }

        private void SendStart()
        {
            if (_listenSocket == null)
                throw new WatsonException("SendStart() called with null connector.");

            Dictionary<string, object> start = new Dictionary<string, object>();
            start["action"] = "start";
            start["content-type"] = "audio/l16;rate=" + _recordingHZ.ToString() + ";channels=1;";
            start["inactivity_timeout"] = InactivityTimeout;
            start["interim_results"] = EnableInterimResults;
            start["keywords"] = Keywords;
            start["keywords_threshold"] = KeywordsThreshold;
            start["max_alternatives"] = MaxAlternatives;
            start["profanity_filter"] = ProfanityFilter;
            start["smart_formatting"] = SmartFormatting;
            start["speaker_labels"] = SpeakerLabels;
            start["timestamps"] = EnableTimestamps;
            if (WordAlternativesThreshold != null)
                start["word_alternatives_threshold"] = WordAlternativesThreshold;
            start["word_confidence"] = EnableWordConfidence;

            _listenSocket.Send(new WSConnector.TextMessage(Json.Serialize(start)));
#if ENABLE_DEBUGGING
            Log.Debug("SpeechToText.SendStart()", "SendStart() with the following params: {0}", Json.Serialize(start));
#endif
            _lastStartSent = DateTime.Now;
        }

        public void SendStop()
        {
            if (_listenSocket == null)
                throw new WatsonException("SendStart() called with null connector.");

            if (_listenActive)
            {
                Dictionary<string, string> stop = new Dictionary<string, string>();
                stop["action"] = "stop";

                _listenSocket.Send(new WSConnector.TextMessage(Json.Serialize(stop)));
                _lastStartSent = DateTime.Now;     // sending stop, will send the listening state again..
                _listenActive = false;
            }
            else
            {
                _sendStopAfterListening = true;
            }
        }

        // This keeps the WebSocket connected when we are not sending any data.
        private IEnumerator KeepAlive()
        {
            while (_listenSocket != null)
            {
                yield return null;

                if ((DateTime.Now - _lastKeepAlive).TotalSeconds > WsKeepAliveInterval)
                {
                    //  Temporary clip to use for KeepAlive
                    //  TODO: Generate small sound clip to send to the service to keep alive.
                    //AudioClip _keepAliveClip = Resources.Load<AudioClip>("highHat");

#if ENABLE_DEBUGGING
                    Log.Debug("SpeechToText.KeepAlive()", "Sending keep alive.");
#endif
                    //_listenSocket.Send(new WSConnector.BinaryMessage(AudioClipUtil.GetL16(_keepAliveClip)));
                    //_keepAliveClip = null;

                    Debug.Log("Sending " + _prefixClips.Count + " prefix clips for keep alive");
                    foreach (AudioData prefixClip in _prefixClips)
                    {
                        _listenSocket.Send(new WSConnector.BinaryMessage(AudioClipUtil.GetL16(prefixClip.Clip)));
                    }
                    _prefixClips.Clear();

                    _lastKeepAlive = DateTime.Now;
                }
            }
            Log.Debug("SpeechToText.KeepAlive()", "KeepAlive exited.");
        }

        private void OnListenMessage(WSConnector.Message msg)
        {
            if (msg is WSConnector.TextMessage)
            {
                WSConnector.TextMessage tm = (WSConnector.TextMessage)msg;

                IDictionary json = Json.Deserialize(tm.Text) as IDictionary;
                if (json != null)
                {
                    if (json.Contains("results"))
                    {
                        SpeechRecognitionEvent results = ParseRecognizeResponse(json);
                        if (results != null)
                        {
                            //// when we get results, start listening for the next block ..
                            //if (results.HasFinalResult())
                                //Log.Debug("SpeechToText.OnListenMessage()", "final json response: {0}", tm.Text);
                            //    SendStart();

                            if (_listenCallback != null)
                                _listenCallback(results);
                            else
                                StopListening();            // automatically stop listening if our callback is destroyed.
                        }
                        else
                            Log.Error("SpeechToText.OnListenMessage()", "Failed to parse results: {0}", tm.Text);
                    }
                    else if (json.Contains("state"))
                    {
                        string state = (string)json["state"];

#if ENABLE_DEBUGGING
                        Log.Debug("SpeechToText.OnListenMessage()", "Server state is {0}", state);
#endif
                        if (state == "listening")
                        {
                            if (_isListening)
                            {
                                if (!_listenActive)
                                {
                                    _listenActive = true;

                                    //Debug.Log("Listening, sending " + _listenRecordings.Count + " queued clips");

                                    bool hasAudio = _listenRecordings.Count > 0;

                                    // send all pending audio clips ..
                                    while (_listenRecordings.Count > 0)
                                    {
                                        AudioData clip = _listenRecordings.Dequeue();
                                        _listenSocket.Send(new WSConnector.BinaryMessage(AudioClipUtil.GetL16(clip.Clip)));
                                        _audioSent = true;
                                    }

                                    // We may have received a stop command while waiting for the listening state.
                                    if (_sendStopAfterListening && hasAudio)
                                    {
                                        SendStop();
                                    }
                                }
                            }
                        }

                    }
                    else if (json.Contains("speaker_labels"))
                    {
                        SpeakerRecognitionEvent speakerRecognitionEvent = ParseSpeakerRecognitionResponse(json);
                        if (speakerRecognitionEvent != null)
                        {
                            _speakerLabelCallback(speakerRecognitionEvent);
                        }
                    }
                    else if (json.Contains("error"))
                    {
                        string error = (string)json["error"];
                        Log.Error("SpeechToText.OnListenMessage()", "Error: {0}", error);

                        StopListening();
                        if (OnError != null)
                            OnError(error);
                    }
                    else
                    {
                        Log.Warning("SpeechToText.OnListenMessage()", "Unknown message: {0}", tm.Text);
                    }
                }
                else
                {
                    Log.Error("SpeechToText.OnListenMessage()", "Failed to parse JSON from server: {0}", tm.Text);
                }
            }
        }

        private void OnListenClosed(WSConnector connector)
        {
#if ENABLE_DEBUGGING
            Log.Debug("SpeechToText.OnListenClosed()", "OnListenClosed(), State = {0}", connector.State.ToString());
#endif
            WSConnector.ConnectionState state = connector.State;

            _listenActive = false;
            StopListening();

            if (state == WSConnector.ConnectionState.DISCONNECTED)
            {
                if (OnError != null)
                    OnError("Disconnected from server.");
            }
        }

        private SpeakerRecognitionEvent ParseSpeakerRecognitionResponse(IDictionary resp)
        {
            if (resp == null)
                return null;

            try
            {
                List<SpeakerLabelsResult> results = new List<SpeakerLabelsResult>();
                IList iresults = resp["speaker_labels"] as IList;

                if (iresults == null)
                    return null;

                foreach (var r in iresults)
                {
                    IDictionary iresult = r as IDictionary;
                    if (iresult == null)
                        continue;

                    SpeakerLabelsResult result = new SpeakerLabelsResult();
                    result.confidence = (double)iresult["confidence"];
                    result.final = (bool)iresult["final"];
                    result.from = (double)iresult["from"];
                    result.to = (double)iresult["to"];
                    result.speaker = (Int64)iresult["speaker"];

                    results.Add(result);
                }
                SpeakerRecognitionEvent speakerRecognitionEvent = new SpeakerRecognitionEvent();
                speakerRecognitionEvent.speaker_labels = results.ToArray();
                return (speakerRecognitionEvent);
            }
            catch (Exception e)
            {
                Log.Error("SpeechToText.ParseSpeakerRecognitionResponse()", "ParseSpeakerRecognitionResponse exception: {0}", e.ToString());
                return null;
            }
        }

        private SpeechRecognitionEvent ParseRecognizeResponse(IDictionary resp)
        {
            if (resp == null)
                return null;

            try
            {
                List<SpeechRecognitionResult> results = new List<SpeechRecognitionResult>();
                IList iresults = resp["results"] as IList;
                if (iresults == null)
                    return null;

                foreach (var r in iresults)
                {
                    IDictionary iresult = r as IDictionary;
                    if (iresults == null)
                        continue;

                    SpeechRecognitionResult result = new SpeechRecognitionResult();
                    result.final = (bool)iresult["final"];

                    IList iwordAlternatives = iresult["word_alternatives"] as IList;
                    if (iwordAlternatives != null)
                    {

                        List<WordAlternativeResults> wordAlternatives = new List<WordAlternativeResults>();
                        foreach (var w in iwordAlternatives)
                        {
                            IDictionary iwordAlternative = w as IDictionary;
                            if (iwordAlternative == null)
                                continue;

                            WordAlternativeResults wordAlternativeResults = new WordAlternativeResults();
                            if (iwordAlternative.Contains("start_time"))
                                wordAlternativeResults.start_time = (double)iwordAlternative["start_time"];
                            if (iwordAlternative.Contains("end_time"))
                                wordAlternativeResults.end_time = (double)iwordAlternative["end_time"];
                            if (iwordAlternative.Contains("alternatives"))
                            {
                                List<WordAlternativeResult> wordAlternativeResultList = new List<WordAlternativeResult>();
                                IList iwordAlternativeResult = iwordAlternative["alternatives"] as IList;
                                if (iwordAlternativeResult == null)
                                    continue;

                                foreach (var a in iwordAlternativeResult)
                                {
                                    WordAlternativeResult wordAlternativeResult = new WordAlternativeResult();
                                    IDictionary ialternative = a as IDictionary;
                                    if (ialternative.Contains("word"))
                                        wordAlternativeResult.word = (string)ialternative["word"];
                                    if (ialternative.Contains("confidence"))
                                        wordAlternativeResult.confidence = (double)ialternative["confidence"];
                                    wordAlternativeResultList.Add(wordAlternativeResult);
                                }

                                wordAlternativeResults.alternatives = wordAlternativeResultList.ToArray();
                            }

                            wordAlternatives.Add(wordAlternativeResults);
                        }

                        result.word_alternatives = wordAlternatives.ToArray();
                    }

                    IList ialternatives = iresult["alternatives"] as IList;
                    if (ialternatives != null)
                    {

                        List<SpeechRecognitionAlternative> alternatives = new List<SpeechRecognitionAlternative>();
                        foreach (var a in ialternatives)
                        {
                            IDictionary ialternative = a as IDictionary;
                            if (ialternative == null)
                                continue;

                            SpeechRecognitionAlternative alternative = new SpeechRecognitionAlternative();
                            alternative.transcript = (string)ialternative["transcript"];
                            if (ialternative.Contains("confidence"))
                                alternative.confidence = (double)ialternative["confidence"];

                            if (ialternative.Contains("timestamps"))
                            {
                                IList itimestamps = ialternative["timestamps"] as IList;

                                TimeStamp[] timestamps = new TimeStamp[itimestamps.Count];
                                for (int i = 0; i < itimestamps.Count; ++i)
                                {
                                    IList itimestamp = itimestamps[i] as IList;
                                    if (itimestamp == null)
                                        continue;

                                    TimeStamp ts = new TimeStamp();
                                    ts.Word = (string)itimestamp[0];
                                    ts.Start = (double)itimestamp[1];
                                    ts.End = (double)itimestamp[2];
                                    timestamps[i] = ts;
                                }

                                alternative.Timestamps = timestamps;
                            }
                            if (ialternative.Contains("word_confidence"))
                            {
                                IList iconfidence = ialternative["word_confidence"] as IList;

                                WordConfidence[] confidence = new WordConfidence[iconfidence.Count];
                                for (int i = 0; i < iconfidence.Count; ++i)
                                {
                                    IList iwordconf = iconfidence[i] as IList;
                                    if (iwordconf == null)
                                        continue;

                                    WordConfidence wc = new WordConfidence();
                                    wc.Word = (string)iwordconf[0];
                                    wc.Confidence = (double)iwordconf[1];
                                    confidence[i] = wc;
                                }

                                alternative.WordConfidence = confidence;
                            }

                            alternatives.Add(alternative);
                        }

                        result.alternatives = alternatives.ToArray();
                    }

                    IDictionary iKeywords = iresult["keywords_result"] as IDictionary;
                    if (iKeywords != null)
                    {
                        result.keywords_result = new KeywordResults();
                        List<KeywordResult> keywordResults = new List<KeywordResult>();
                        foreach (string keyword in Keywords)
                        {
                            if (iKeywords[keyword] != null)
                            {
                                IList iKeywordList = iKeywords[keyword] as IList;
                                if (iKeywordList == null)
                                    continue;

                                foreach (var k in iKeywordList)
                                {
                                    IDictionary iKeywordDictionary = k as IDictionary;
                                    KeywordResult keywordResult = new KeywordResult();
                                    keywordResult.keyword = keyword;
                                    keywordResult.confidence = (double)iKeywordDictionary["confidence"];
                                    keywordResult.end_time = (double)iKeywordDictionary["end_time"];
                                    keywordResult.start_time = (double)iKeywordDictionary["start_time"];
                                    keywordResult.normalized_text = (string)iKeywordDictionary["normalized_text"];
                                    keywordResults.Add(keywordResult);
                                }
                            }
                        }
                        result.keywords_result.keyword = keywordResults.ToArray();
                    }

                    results.Add(result);
                }

                return new SpeechRecognitionEvent(results.ToArray());
            }
            catch (Exception e)
            {
                Log.Error("SpeechToText.ParseRecognizeResponse()", "ParseJsonResponse exception: {0}", e.ToString());
                return null;
            }
        }
        #endregion

        #region IWatsonService interface
        /// <exclude />
        public string GetServiceID()
        {
            return ServiceId;
        }
        #endregion
    }
}
