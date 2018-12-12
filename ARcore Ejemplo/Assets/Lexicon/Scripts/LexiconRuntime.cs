// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Services.Conversation.v1;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using UnityEngine;

namespace Mixspace.Lexicon
{
    public class LexiconRuntime : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("A Lexicon Workspace asset.")]
        private LexiconWorkspace workspace;

        [SerializeField]
        [Tooltip("Whether the speech to text service is currently active.")]
        private bool speechToTextActive = true;

        [SerializeField]
        [Tooltip("Whether the conversation service is currently active.")]
        private bool conversationActive = true;

        [SerializeField]
        [Tooltip("The confidence value required for speech to text results to be processed (value between 0 and 1).")]
        private float speechConfidenceThreshold = 0.5f;

        [SerializeField]
        [Tooltip("Whether to use the custom language model for speech to text.")]
        private bool useCustomLanguageModel;

        [SerializeField]
        [Tooltip("Weight for the custom language model (value between 0 and 1).")]
        private float customLanguageModelWeight = 0.3f;

        [SerializeField]
        [Tooltip("The confidence value required for a keyword match (value between 0 and 1).")]
        private float keywordConfidenceThreshold = 0.3f;

        [SerializeField]
        [Tooltip("A list of keywords to listen for. Keywords can be added and removed at runtime.")]
        private List<string> keywords;

        [SerializeField]
        [Tooltip("The confidence value required for an intent to be processed (value between 0 and 1).")]
        private float intentConfidenceThreshold = 0.5f;

        [SerializeField]
        [Tooltip("Whether to allow multiple intent matches. Leave unchecked to only process the best intent match.")]
        private bool matchMultipleIntents;

        [SerializeField]
        [Tooltip("Microphone input levels below this value will be considered silence (value between 0 and 1).")]
        private float silenceThreshold = 0.01f;

        [SerializeField]
        [Tooltip("Adjustment for timestamp values (in seconds). Use to improve the alignment of uttered words with focus data.")]
        private float timestampOffset = 0.1f;

        [SerializeField]
        [Tooltip("Whether to use dwell position as the default focusPosition on entity matches.")]
        private bool useDwellPosition;

        /// <summary>
        /// A Lexicon Workspace asset.
        /// </summary>
        public LexiconWorkspace Workspace
        {
            get { return workspace; }
            set { workspace = value; }
        }

        /// <summary>
        /// Whether the speech to text service is currently active.
        /// </summary>
        public bool SpeechToTextActive
        {
            get { return speechToTextActive; }
            set { speechToTextActive = value; }
        }

        /// <summary>
        /// Whether the conversation service is currently active.
        /// </summary>
        public bool ConversationActive
        {
            get { return conversationActive; }
            set { conversationActive = value; }
        }

        /// <summary>
        /// The confidence value required for speech to text results to be processed (value between 0 and 1).
        /// </summary>
        public float SpeechConfidenceThreshold
        {
            get { return speechConfidenceThreshold; }
            set { speechConfidenceThreshold = value; }
        }

        /// <summary>
        /// Whether to use the custom language model for speech to text.
        /// </summary>
        public bool UseCustomLanguageModel
        {
            get { return useCustomLanguageModel; }
            set { useCustomLanguageModel = value; }
        }

        /// <summary>
        /// Weight for the custom language model (value between 0 and 1).
        /// </summary>
        public float CustomLanguageModelWeight
        {
            get { return customLanguageModelWeight; }
            set { customLanguageModelWeight = value; }
        }

        /// <summary>
        /// The confidence value required for a keyword match (value between 0 and 1).
        /// </summary>
        public float KeywordConfidenceThreshold
        {
            get { return keywordConfidenceThreshold; }
            set { keywordConfidenceThreshold = value; }
        }

        /// <summary>
        /// A list of keywords to listen for. Keywords can be added and removed at runtime.
        /// </summary>
        public List<string> Keywords
        {
            get { return keywords; }
        }

        /// <summary>
        /// The confidence value required for an intent to be processed (value between 0 and 1).
        /// </summary>
        public float IntentConfidenceThreshold
        {
            get { return intentConfidenceThreshold; }
            set { intentConfidenceThreshold = value; }
        }

        /// <summary>
        /// Whether to allow multiple intent matches. Leave unchecked to only process the best intent match.
        /// </summary>
        public bool MatchMultipleIntents
        {
            get { return matchMultipleIntents; }
            set { matchMultipleIntents = value; }
        }

        /// <summary>
        /// Microphone input levels below this value will be considered silence (value between 0 and 1).
        /// </summary>
        public float SilenceThreshold
        {
            get { return silenceThreshold; }
            set { silenceThreshold = value; }
        }

        /// <summary>
        /// Adjustment for timestamp values (in seconds). Use to improve the alignment of uttered words with focus data.
        /// </summary>
        public float TimestampOffset
        {
            get { return timestampOffset; }
            set { timestampOffset = value; }
        }

        /// <summary>
        /// Whether to use dwell position as the default focusPosition on entity matches.
        /// </summary>
        public bool UseDwellPosition
        {
            get { return useDwellPosition; }
            set { useDwellPosition = value; }
        }

        /// <summary>
        /// The currently active runtime.
        /// </summary>
        public static LexiconRuntime CurrentRuntime;

        public delegate void SpeechToTextResultHandler(LexiconSpeechResult speechResult);
        /// <summary>
        /// Callback for speech to text results.
        /// </summary>
        public static event SpeechToTextResultHandler OnSpeechToTextResults;

        public delegate void KeywordDetectedHandler(LexiconSpeechResult.KeywordResult keywordResult);
        /// <summary>
        /// Callback for keyword matches.
        /// </summary>
        public static event KeywordDetectedHandler OnKeywordDetected;

        public delegate void LexiconResultHandler(List<LexiconRuntimeResult> results);
        /// <summary>
        /// Callback for intent matches. Results array will be empty if no intents were matched.
        /// </summary>
        public static event LexiconResultHandler OnLexiconResults;

        public delegate void SpeechStatusUpdateHandler(LexiconSpeechStatus status);
        /// <summary>
        /// Callback for speech status changes.
        /// </summary>
        public static event SpeechStatusUpdateHandler OnSpeechStatusUpdated;

        private List<LexiconSpeechResult> finalSpeechResults = new List<LexiconSpeechResult>();

        private Coroutine startSpeechCoroutine;

        private bool awaitingSpeechCapture;

        private void OnValidate()
        {
            speechConfidenceThreshold = Mathf.Clamp01(speechConfidenceThreshold);
            customLanguageModelWeight = Mathf.Clamp01(customLanguageModelWeight);
            keywordConfidenceThreshold = Mathf.Clamp01(keywordConfidenceThreshold);
            intentConfidenceThreshold = Mathf.Clamp01(intentConfidenceThreshold);
            silenceThreshold = Mathf.Clamp01(silenceThreshold);
        }

        void OnEnable()
        {
            Debug.Log("Lexicon Runtime Start");

            if (workspace == null)
            {
                return;
            }

            if (workspace.UseWatsonSpeechToText)
            {
                workspace.WatsonSpeechToTextManager.ResponseHandlers += HandleWatsonSpeechToTextResponse;
                workspace.WatsonSpeechToTextManager.StatusChangeHandlers += HandleSpeechStatusChange;
                workspace.WatsonSpeechToTextManager.Initialize();

                if (speechToTextActive)
                {
                    StartSpeechService();
                }
            }

            if (workspace.UseWatsonConversation)
            {
                workspace.WatsonConversationManager.ResponseHandlers += HandleWatsonConversationResponse;
                workspace.WatsonConversationManager.Initialize();

                if (conversationActive)
                {
                    StartConversationService();
                }
            }

            CurrentRuntime = this;
        }

        void OnDisable()
        {
            Debug.Log("Lexicon Runtime Stop");

            if (workspace == null)
            {
                return;
            }

            if (workspace.UseWatsonSpeechToText)
            {
                workspace.WatsonSpeechToTextManager.ResponseHandlers -= HandleWatsonSpeechToTextResponse;
                workspace.WatsonSpeechToTextManager.StatusChangeHandlers -= HandleSpeechStatusChange;

                if (speechToTextActive)
                {
                    workspace.WatsonSpeechToTextManager.Stop();
                }
            }

            if (workspace.UseWatsonConversation)
            {
                workspace.WatsonConversationManager.ResponseHandlers -= HandleWatsonConversationResponse;
            }

            if (startSpeechCoroutine != null)
            {
                StopCoroutine(startSpeechCoroutine);
                startSpeechCoroutine = null;
            }
        }

        void Update()
        {
            if (workspace.UseWatsonSpeechToText)
            {
                if (speechToTextActive && !workspace.WatsonSpeechToTextManager.Active && startSpeechCoroutine == null)
                {
                    if (workspace.WatsonSpeechToTextManager.FailedToConnect)
                    {
                        startSpeechCoroutine = StartCoroutine(StartSpeechService(5.0f));
                    }
                    else
                    {
                        StartSpeechService();
                    }
                }
                else if (!speechToTextActive && workspace.WatsonSpeechToTextManager.Active && !workspace.WatsonSpeechToTextManager.Stopping)
                {
                    StopSpeechService(true);
                }
            }
        }

        /// <summary>
        /// Starts the speech to text service and microphone input.
        /// </summary>
        public void StartSpeechService()
        {
            if (workspace.UseWatsonSpeechToText && !workspace.WatsonSpeechToTextManager.Active)
            {
                workspace.WatsonSpeechToTextManager.UseCustomModel = useCustomLanguageModel;
                workspace.WatsonSpeechToTextManager.CustomizationWeight = customLanguageModelWeight;
                workspace.WatsonSpeechToTextManager.KeywordConfidence = keywordConfidenceThreshold;
                workspace.WatsonSpeechToTextManager.Keywords = keywords;
                workspace.WatsonSpeechToTextManager.SilenceThreshold = silenceThreshold;
                workspace.WatsonSpeechToTextManager.TimestampOffset = timestampOffset;
                workspace.WatsonSpeechToTextManager.Start();
                speechToTextActive = true;
            }
        }

        private IEnumerator StartSpeechService(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (speechToTextActive && !workspace.WatsonSpeechToTextManager.Active)
            {
                StartSpeechService();
            }

            startSpeechCoroutine = null;
        }

        /// <summary>
        /// Stops the speech to text service. Microphone input stops immediately. Socket stays open for last result if waitForResults is true.
        /// </summary>
        public void StopSpeechService(bool waitForResults = false)
        {
            if (workspace.UseWatsonSpeechToText && workspace.WatsonSpeechToTextManager.Active)
            {
                if (waitForResults)
                {
                    workspace.WatsonSpeechToTextManager.Stop(true);
                }
                else
                {
                    workspace.WatsonSpeechToTextManager.Stop();
                }
                speechToTextActive = false;
            }
        }

        /// <summary>
        /// Starts the conversation service.
        /// </summary>
        public void StartConversationService()
        {
            if (workspace.UseWatsonConversation)
            {
                workspace.WatsonConversationManager.AlternateIntents = matchMultipleIntents;
                conversationActive = true;
            }
        }

        /// <summary>
        /// Stops the conversation service.
        /// </summary>
        public void StopConversationService()
        {
            if (workspace.UseWatsonConversation)
            {
                conversationActive = false;
            }
        }

        /// <summary>
        /// Restarts the currently active services.
        /// </summary>
        public void Restart()
        {
            if (speechToTextActive)
            {
                StopSpeechService();
                StartSpeechService();
            }

            if (conversationActive)
            {
                StopConversationService();
                StartConversationService();
            }
        }

        /// <summary>
        /// Process string input. Use this to send a message to the conversation service.
        /// </summary>
        public void ProcessInput(string message)
        {
            if (workspace.UseWatsonConversation)
            {
                workspace.WatsonConversationManager.SendRequest(message);
            }
        }

        /// <summary>
        /// Captures a single phrase from speech to text. Starts and stops the microphone for one phrase.
        /// </summary>
        public void CaptureOnePhrase(float timeout = 3.0f)
        {
            OnSpeechToTextResults += CapturedPhrase;
            StartCoroutine(CaptureTimeout(timeout));
            StartSpeechService();
        }

        private void CapturedPhrase(LexiconSpeechResult speechResult)
        {
            awaitingSpeechCapture = false;

            if (speechResult.IsFinal)
            {
                OnSpeechToTextResults -= CapturedPhrase;
                StopSpeechService();
            }
        }

        private IEnumerator CaptureTimeout(float seconds)
        {
            awaitingSpeechCapture = true;

            yield return new WaitForSeconds(seconds);

            if (awaitingSpeechCapture)
            {
                awaitingSpeechCapture = false;
                OnSpeechToTextResults -= CapturedPhrase;
                StopSpeechService();
            }
        }

        private void HandleWatsonSpeechToTextResponse(SpeechRecognitionEvent response, float realtimeStart)
        {
            if (response.results.Length == 0)
            {
                Debug.LogError("SpeechRecognitionEvent has no results!");
                return;
            }

            if (response.results.Length > 1)
            {
                Debug.LogWarning("SpeechRecognitionEvent has multiple results!");
            }

            LexiconSpeechResult speechResult = CreateSpeechResult(response.results[0], realtimeStart);

            if (speechResult.IsFinal && speechResult.Confidence >= speechConfidenceThreshold)
            {
                if (workspace.UseWatsonConversation && conversationActive)
                {
                    // Send the final transcript to the conversation service for processing.
                    workspace.WatsonConversationManager.SendRequest(speechResult.Transcript);

                    // Cache the speech to text results to be matched with conversation results later.
                    finalSpeechResults.Add(speechResult);
                }
            }

            if (OnSpeechToTextResults != null)
            {
                // Share speech results with our delegates.
                OnSpeechToTextResults(speechResult);
            }

            if (speechResult.KeywordResults != null && OnKeywordDetected != null)
            {
                // Share keyword results with our delegates (called once for each detected keyword).
                foreach (LexiconSpeechResult.KeywordResult keywordResult in speechResult.KeywordResults)
                {
                    OnKeywordDetected(keywordResult);
                }
            }
        }

        private void HandleWatsonConversationResponse(MessageResponse response)
        {
            LexiconSpeechResult matchingSpeechResult = null;

            object text;
            string utterance = "";
            if (response.input.TryGetValue("text", out text))
            {
                utterance = (string)text;

                // Find the speech to text result that matches this conversation result.
                foreach (LexiconSpeechResult speechResult in finalSpeechResults)
                {
                    if (string.Equals(speechResult.Transcript, utterance, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingSpeechResult = speechResult;
                    }
                }

                if (matchingSpeechResult != null)
                {
                    // Remove the cached speech to text result.
                    finalSpeechResults.Remove(matchingSpeechResult);
                }
            }

            // Make a list of all entity/value pairs in this response.
            List<EntityPair> entityPairs = new List<EntityPair>();

            foreach (RuntimeEntity entity in response.entities)
            {
                entityPairs.Add(new EntityPair(entity.entity, entity.value));
            }

            List<LexiconRuntimeResult> runtimeResults = new List<LexiconRuntimeResult>();
            bool consumed = false;

            // Process each intent (there will only be one if matchMultipleIntents is false).
            // TODO: We may want to process entities even if there isn't an intent match.
            foreach (RuntimeIntent watsonIntent in response.intents)
            {
                if (watsonIntent.confidence < intentConfidenceThreshold)
                {
                    // Ignore intents that don't meet the confidence threshold.
                    continue;
                }

                List<LexiconIntent> matchingIntents = workspace.FindMatchingIntents(watsonIntent.intent, entityPairs);
                LexiconFocusManager focusManager = LexiconFocusManager.Instance;

                foreach (LexiconIntent intent in matchingIntents)
                {
                    LexiconRuntimeResult runtimeResult = new LexiconRuntimeResult();
                    runtimeResult.Intent = intent;
                    runtimeResult.Confidence = watsonIntent.confidence;
                    runtimeResult.Utterance = utterance;
                    runtimeResult.SpeechResult = matchingSpeechResult;

                    foreach (RuntimeEntity watsonEntity in response.entities)
                    {
                        // Find the entity and entityValue that match this intent.
                        LexiconEntity entity;
                        LexiconEntityValue entityValue = intent.FindEntityValue(watsonEntity.entity, watsonEntity.value, out entity);
                        if (entityValue != null)
                        {
                            LexiconEntityMatch entityMatch = new LexiconEntityMatch();
                            entityMatch.Entity = entity;
                            entityMatch.EntityValue = entityValue;
                            entityMatch.FirstCharacter = watsonEntity.location[0];
                            entityMatch.LastCharacter = watsonEntity.location[1] - 1; // Convert to index of last character.

                            if (matchingSpeechResult != null)
                            {
                                // Find the entity in the speech to text transcript and extract the timestamps.
                                LexiconSpeechResult.WordResult[] wordResults = matchingSpeechResult.GetWordsFromTranscriptPositions(entityMatch.FirstCharacter, entityMatch.LastCharacter);
                                if (wordResults.Length > 0)
                                {
                                    entityMatch.TimeStart = wordResults[0].TimeStart;
                                    entityMatch.TimeEnd = wordResults[wordResults.Length - 1].TimeEnd;
                                    entityMatch.RealtimeStart = wordResults[0].RealtimeStart;
                                    entityMatch.RealtimeEnd = wordResults[wordResults.Length - 1].RealtimeEnd;

                                    if (useDwellPosition)
                                    {
                                        FocusDwellPosition dwellPosition = focusManager.GetFocusData<FocusDwellPosition>(entityMatch.RealtimeStart, 0.2f);
                                        if (dwellPosition != null)
                                        {
                                            FocusPosition focusPosition = new FocusPosition();
                                            focusPosition.Timestamp = dwellPosition.Timestamp;
                                            focusPosition.Position = dwellPosition.Position;
                                            focusPosition.Normal = dwellPosition.Normal;

                                            entityMatch.FocusPosition = focusPosition;
                                        }
                                        else
                                        {
                                            entityMatch.FocusPosition = focusManager.GetFocusData<FocusPosition>(entityMatch.RealtimeStart);
                                        }
                                    }
                                    else
                                    {
                                        entityMatch.FocusPosition = focusManager.GetFocusData<FocusPosition>(entityMatch.RealtimeStart);
                                    }

                                    entityMatch.FocusSelection = focusManager.GetFocusData<FocusSelection>(entityMatch.RealtimeStart);
                                }
                            }

                            runtimeResult.EntityMatches.Add(entityMatch);
                        }
                    }

                    runtimeResult.EntityMatches.Sort((x, y) => x.FirstCharacter.CompareTo(y.FirstCharacter));

                    runtimeResults.Add(runtimeResult);

                    if (!consumed)
                    {
                        // If an action has consumed this result no other actions will be fired.
                        // But, we continue to process the intents for the global handler.
                        consumed = intent.Process(runtimeResult);
                    }
                }
            }

            if (OnLexiconResults != null)
            {
                // Share runtime results with our delegates.
                OnLexiconResults(runtimeResults);
            }
        }

        private LexiconSpeechResult CreateSpeechResult(SpeechRecognitionResult watsonResult, float realtimeStart)
        {
            if (watsonResult.alternatives.Length == 0)
            {
                return null;
            }

            LexiconSpeechResult speechResult = new LexiconSpeechResult();

            SpeechRecognitionAlternative bestAlternative = watsonResult.alternatives[0];

            speechResult.Transcript = bestAlternative.transcript.Trim();
            speechResult.IsFinal = watsonResult.final;
            speechResult.Confidence = (float)bestAlternative.confidence;
            speechResult.RealtimeStart = realtimeStart;
            speechResult.RealtimeEnd = -1;

            string[] words = speechResult.Transcript.Split(' ');
            int wordCount = words.Length;

            if (wordCount > 0)
            {
                speechResult.WordResults = new LexiconSpeechResult.WordResult[wordCount];

                for (int i = 0; i < wordCount; i++)
                {
                    speechResult.WordResults[i] = new LexiconSpeechResult.WordResult();
                    speechResult.WordResults[i].Word = words[i];
                }

                if (bestAlternative.Timestamps != null)
                {
                    if (bestAlternative.Timestamps.Length == wordCount)
                    {
                        for (int i = 0; i < wordCount; i++)
                        {
                            if (string.Equals(words[i], bestAlternative.Timestamps[i].Word, StringComparison.OrdinalIgnoreCase))
                            {
                                speechResult.WordResults[i].TimeStart = (float)bestAlternative.Timestamps[i].Start;
                                speechResult.WordResults[i].TimeEnd = (float)bestAlternative.Timestamps[i].End;
                                speechResult.WordResults[i].RealtimeStart = realtimeStart + speechResult.WordResults[i].TimeStart;
                                speechResult.WordResults[i].RealtimeEnd = realtimeStart + speechResult.WordResults[i].TimeEnd;
                            }
                            else
                            {
                                Debug.LogWarning("word: " + words[i] + " does not match timestamp word: " + bestAlternative.Timestamps[i].Word);
                            }
                        }

                        if (speechResult.WordResults.Length > 0)
                        {
                            speechResult.RealtimeEnd = speechResult.WordResults[speechResult.WordResults.Length - 1].RealtimeEnd;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("word count: " + wordCount + ", timestamp count: " + bestAlternative.Timestamps.Length);
                    }
                }

                if (bestAlternative.WordConfidence != null)
                {
                    if (bestAlternative.WordConfidence.Length == wordCount)
                    {
                        for (int i = 0; i < wordCount; i++)
                        {
                            if (string.Equals(words[i], bestAlternative.WordConfidence[i].Word, StringComparison.OrdinalIgnoreCase))
                            {
                                speechResult.WordResults[i].Confidence = (float)bestAlternative.WordConfidence[i].Confidence;
                            }
                            else
                            {
                                Debug.LogWarning("word: " + words[i] + " does not match confidence word: " + bestAlternative.WordConfidence[i].Word);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("word count: " + wordCount + ", confidence count: " + bestAlternative.WordConfidence.Length);
                    }
                }
            }

            if (watsonResult.keywords_result != null && watsonResult.keywords_result.keyword != null && watsonResult.keywords_result.keyword.Length > 0)
            {
                speechResult.KeywordResults = new LexiconSpeechResult.KeywordResult[watsonResult.keywords_result.keyword.Length];

                for (int i = 0; i < watsonResult.keywords_result.keyword.Length; i++)
                {
                    KeywordResult watsonKeywordResult = watsonResult.keywords_result.keyword[i];
                    LexiconSpeechResult.KeywordResult keywordResult = new LexiconSpeechResult.KeywordResult();

                    keywordResult.Keyword = watsonKeywordResult.keyword;
                    keywordResult.TranscriptText = watsonKeywordResult.normalized_text;
                    keywordResult.Confidence = (float)watsonKeywordResult.confidence;
                    keywordResult.TimeStart = (float)watsonKeywordResult.start_time;
                    keywordResult.TimeEnd = (float)watsonKeywordResult.end_time;
                    keywordResult.RealtimeStart = realtimeStart + keywordResult.TimeStart;
                    keywordResult.RealtimeEnd = realtimeStart + keywordResult.TimeEnd;

                    speechResult.KeywordResults[i] = keywordResult;
                }
            }

            if (watsonResult.word_alternatives != null && watsonResult.word_alternatives.Length > 0)
            {
                speechResult.AlternativeWordResults = new LexiconSpeechResult.WordAlternativeResults[watsonResult.word_alternatives.Length];

                for (int i = 0; i < watsonResult.word_alternatives.Length; i++)
                {
                    WordAlternativeResults watsonAlternativeResults = watsonResult.word_alternatives[i];
                    LexiconSpeechResult.WordAlternativeResults alternativeResults = new LexiconSpeechResult.WordAlternativeResults();

                    alternativeResults.Alternatives = new LexiconSpeechResult.WordAlternative[watsonAlternativeResults.alternatives.Length];
                    alternativeResults.TimeStart = (float)watsonAlternativeResults.start_time;
                    alternativeResults.TimeEnd = (float)watsonAlternativeResults.end_time;
                    alternativeResults.RealtimeStart = realtimeStart + alternativeResults.TimeStart;
                    alternativeResults.RealtimeEnd = realtimeStart + alternativeResults.TimeEnd;

                    for (int j = 0; j < watsonAlternativeResults.alternatives.Length; j++)
                    {
                        LexiconSpeechResult.WordAlternative alternative = new LexiconSpeechResult.WordAlternative();

                        alternative.Word = watsonAlternativeResults.alternatives[j].word;
                        alternative.Confidence = (float)watsonAlternativeResults.alternatives[j].confidence;

                        alternativeResults.Alternatives[j] = alternative;
                    }

                    speechResult.AlternativeWordResults[i] = alternativeResults;
                }
            }

            return speechResult;
        }

        private void HandleSpeechStatusChange(LexiconSpeechStatus status)
        {
            if (OnSpeechStatusUpdated != null)
            {
                OnSpeechStatusUpdated(status);
            }
        }
    }
}
