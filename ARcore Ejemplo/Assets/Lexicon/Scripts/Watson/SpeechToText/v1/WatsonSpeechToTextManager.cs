// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using Mixspace.Lexicon.Watson.SpeechToText.v1;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonSpeechToTextManager
    {
        [SerializeField]
        private string url = "https://stream.watsonplatform.net/speech-to-text/api";

        [SerializeField]
        private string model = "en-US_BroadbandModel";

        [SerializeField]
        private bool createCustomModel;

        [SerializeField]
        private string customizationId;

        [SerializeField]
        private string syncStatus;

        [SerializeField]
        private bool isSyncing;

        [SerializeField]
        private bool isTraining;

        [SerializeField]
        private string lastSyncTime;

        [NonSerialized]
        private bool useCustomModel;

        [NonSerialized]
        private float customizationWeight = 0.3f;

        [NonSerialized]
        private List<string> keywords = new List<string>();

        [NonSerialized]
        private float keywordConfidence = 0.3f;

        [NonSerialized]
        private float timestampOffset = 0.1f;

        [NonSerialized]
        private float silenceThreshold = 0.01f;

        [NonSerialized]
        private bool failedToConnect;

        [NonSerialized]
        private float currentLevel;

        public string Username
        {
            get
            {
                if (Application.isEditor)
                {
                    return StringUtility.Base64Decode(PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonSpeechToTextUsername));
                }
                else
                {
                    LexiconConfig config = (LexiconConfig)Resources.Load(LexiconConstants.Files.ConfigFile);
                    return ((config != null) ? StringUtility.Base64Decode(config.WatsonSpeechToTextUsername) : "");
                }
            }
        }

        public string Password
        {
            get
            {
                if (Application.isEditor)
                {
                    return StringUtility.Base64Decode(PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonSpeechToTextPassword));
                }
                else
                {
                    LexiconConfig config = (LexiconConfig)Resources.Load(LexiconConstants.Files.ConfigFile);
                    return ((config != null) ? StringUtility.Base64Decode(config.WatsonSpeechToTextPassword) : "");
                }
            }
        }

        public string Url
        {
            get { return url; }
        }

        public string Model
        {
            get { return model; }
        }

        public bool CreateCustomModel
        {
            get { return createCustomModel; }
        }

        public string CustomizationId
        {
            get { return customizationId; }
            set { customizationId = value; }
        }

        public string SyncStatus
        {
            get { return syncStatus; }
            set { syncStatus = value; }
        }

        public bool IsSyncing
        {
            get { return isSyncing; }
            set { isSyncing = value; }
        }

        public bool IsTraining
        {
            get { return isTraining; }
            set { isTraining = value; }
        }

        public string LastSyncTime
        {
            get { return lastSyncTime; }
            set { lastSyncTime = value; }
        }

        public bool UseCustomModel
        {
            get { return useCustomModel; }
            set { useCustomModel = value; }
        }

        public float CustomizationWeight
        {
            get { return customizationWeight; }
            set { customizationWeight = value; }
        }

        public List<string> Keywords
        {
            get { return keywords; }
            set { keywords = value; }
        }

        public float KeywordConfidence
        {
            get { return keywordConfidence; }
            set { keywordConfidence = value; }
        }

        public float TimestampOffset
        {
            get { return timestampOffset; }
            set { timestampOffset = value; }
        }

        public float SilenceThreshold
        {
            get { return silenceThreshold; }
            set { silenceThreshold = value; }
        }

        public bool FailedToConnect
        {
            get { return failedToConnect; }
        }

        public float CurrentLevel
        {
            get { return currentLevel; }
        }

        public LexiconSpeechToText SpeechToText
        {
            get { return speechToText; }
        }

        public delegate void OnSpeechToTextResponse(SpeechRecognitionEvent response, float realtimeStart);
        public OnSpeechToTextResponse ResponseHandlers;

        public delegate void OnSpeechStatusChange(LexiconSpeechStatus status);
        public OnSpeechStatusChange StatusChangeHandlers;

        private LexiconSpeechToText speechToText;

        private int recordingRoutine = 0;
        private string microphoneID = null;
        private AudioClip recording = null;
        private int recordingBufferSize = 1;
        private int recordingHZ = 22050;
        private int sampleSegments = 50;

        private bool calibrating;
        private float levelSum;
        private int levelSamples;

        [NonSerialized]
        private bool isStreaming;
        [NonSerialized]
        private float realtimeStart = -1;
        [NonSerialized]
        private bool awaitingFinal;
        [NonSerialized]
        private bool stopAfterFinal;

        public void Initialize()
        {
            LogSystem.InstallDefaultReactors();

            Credentials credentials = new Credentials(Username, Password, url);

            speechToText = new LexiconSpeechToText(credentials);
        }

        public void Start()
        {
            //Debug.Log("isStreaming: " + isStreaming + ", realtimeStart: " + realtimeStart);

            if (speechToText != null)
            {
                isStreaming = false;
                awaitingFinal = false;
                stopAfterFinal = false;

                speechToText.OnStatusUpdate += OnStatusChange;
                Active = true;

                StartRecording();
            }
        }

        public void Stop(bool waitForFinal = false)
        {
            StopRecording();

            if (waitForFinal && awaitingFinal)
            {
                stopAfterFinal = true;
            }
            else
            {
                speechToText.StopListening();
                if (speechToText != null)
                {
                    speechToText.OnStatusUpdate -= OnStatusChange;
                }
            }
        }

        public void StartCalibrating()
        {
            levelSum = 0.0f;
            levelSamples = 0;
            calibrating = true;
        }

        public float StopCalibrating()
        {
            calibrating = false;
            return levelSum / levelSamples;
        }

        public bool Active
        {
            get { return speechToText.IsListening; }
            set
            {
                if (value && !speechToText.IsListening)
                {
                    speechToText.RecognizeModel = model;

                    if (useCustomModel && !string.IsNullOrEmpty(customizationId))
                    {
                        Debug.Log("Watson Speech to Text: Using Custom Language Model");
                        speechToText.CustomizationId = customizationId;
                        speechToText.CustomizationWeight = customizationWeight;
                    }
                    else
                    {
                        speechToText.CustomizationId = null;
                        Debug.Log("Watson Speech to Text: Using " + model);
                    }

                    Debug.Log("Silence Threshold: " + silenceThreshold);

                    speechToText.DetectSilence = true;
                    speechToText.SilenceThreshold = silenceThreshold;
                    speechToText.EnableInterimResults = true;
                    speechToText.EnableTimestamps = true;
                    speechToText.EnableWordConfidence = true;
                    speechToText.MaxAlternatives = 3;
                    speechToText.SpeakerLabels = false;
                    speechToText.OnError = OnError;

                    if (keywords.Count > 0)
                    {
                        speechToText.KeywordsThreshold = keywordConfidence;
                        speechToText.Keywords = keywords.ToArray();
                    }
                    else
                    {
                        string[] emptyArray = { "" };
                        speechToText.Keywords = emptyArray;
                    }

                    failedToConnect = false;
                    speechToText.StartListening(OnRecognize);
                }
                else if (!value && speechToText.IsListening)
                {
                    speechToText.StopListening();
                    isStreaming = false;
                }
            }
        }

        public bool Stopping
        {
            get { return stopAfterFinal; }
        }

        private void StartRecording()
        {
            if (recordingRoutine == 0)
            {
                UnityObjectUtil.StartDestroyQueue();
                recordingRoutine = Runnable.Run(RecordingHandler());
            }
        }

        private void StopRecording()
        {
            if (recordingRoutine != 0)
            {
                //speechToText.StopListening();
                Microphone.End(microphoneID);

                Runnable.Stop(recordingRoutine);
                recordingRoutine = 0;

                if (!failedToConnect)
                {
                    speechToText.SendStop();
                }

                isStreaming = false;
            }
        }

        private void OnError(string error)
        {
            Active = false;
            isStreaming = false;
            awaitingFinal = false;

            if (error.Equals("Disconnected from server."))
            {
                Debug.LogError("Speech to Text: " + error + " Check your internet connection");
                failedToConnect = true;
                Stop();
            }
            else
            {
                Debug.Log("Speech to Text: " + error);
            }
        }

        private IEnumerator RecordingHandler()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogError("No microphone connected");
            }
            else
            {
                Debug.Log("Microphone: " + Microphone.devices[0]);
            }

            //  Start recording
            recording = Microphone.Start(microphoneID, true, recordingBufferSize, recordingHZ);
            yield return null;

            if (recording == null)
            {
                StopRecording();
                yield break;
            }

            //  Current sample segment number
            int sampleSegmentNum = 0;

            //  Size of the sample segment in samples
            int sampleSegmentSize = recording.samples / sampleSegments;

            //  Init samples
            float[] samples = null;

            while (recordingRoutine != 0 && recording != null)
            {
                float startTime = Time.realtimeSinceStartup;

                //  Get the mic position
                int microphonePosition = Microphone.GetPosition(microphoneID);
                if (microphonePosition > recording.samples || !Microphone.IsRecording(microphoneID))
                {
                    Debug.LogError("Microphone disconnected");

                    StopRecording();
                    yield break;
                }

                int sampleStart = sampleSegmentSize * sampleSegmentNum;
                int sampleEnd = sampleSegmentSize * (sampleSegmentNum + 1);

                int count = 0;
                //If the write position is past the end of the sample segment or if write position is before the start of the sample segment
                while (microphonePosition > sampleEnd || microphonePosition < sampleStart)
                {
                    count++;
                    //  Init samples
                    samples = new float[sampleSegmentSize];
                    //  Write data from recording into samples starting from the sampleSegmentStart
                    recording.GetData(samples, sampleStart);

                    //  Create AudioData and use the samples we just created
                    AudioData record = new AudioData();
                    record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                    record.Clip = AudioClip.Create("Recording", sampleSegmentSize, recording.channels, recordingHZ, false);
                    record.Clip.SetData(samples, 0);

                    currentLevel = record.MaxLevel;

                    if (calibrating)
                    {
                        levelSum += record.MaxLevel;
                        levelSamples++;
                    }

                    if (!Active && record.MaxLevel > speechToText.SilenceThreshold)
                    {
                        Debug.Log("Autostart Speech to Text");
                        Active = true;
                    }

                    //  Send the newly created AudioData to the service
                    bool sent = speechToText.OnListen(record);

                    if (sent)
                    {
                        if (!isStreaming)
                        {
                            realtimeStart = startTime - record.Clip.length + speechToText.TimeOffset - timestampOffset; // TODO: Add this instead?
                            isStreaming = true;
                            awaitingFinal = true;
                        }
                    }
                    else
                    {
                        isStreaming = false;
                    }

                    //  Iterate or reset sampleSegmentNum
                    if (sampleSegmentNum < sampleSegments - 1)
                    {
                        sampleSegmentNum++;
                    }
                    else
                    {
                        sampleSegmentNum = 0;
                    }

                    sampleStart = sampleSegmentSize * sampleSegmentNum;
                    sampleEnd = sampleSegmentSize * (sampleSegmentNum + 1);
                }

                yield return 0;
            }

            yield break;
        }

        private void OnRecognize(SpeechRecognitionEvent result)
        {
            if (result != null && result.results.Length > 0)
            {
                if (result.results[0].final)
                {
                    //Debug.Log("Speech Output: " + result.results[0].alternatives[0].transcript);

                    awaitingFinal = false;
                    if (stopAfterFinal)
                    {
                        Stop();
                        stopAfterFinal = false;
                    }
                }

                if (ResponseHandlers != null)
                {
                    ResponseHandlers(result, realtimeStart);
                }
            }
        }

        private void OnStatusChange(LexiconSpeechStatus status)
        {
            if (StatusChangeHandlers != null)
            {
                StatusChangeHandlers(status);
            }
        }
    }
}
