/**
 * Copyright 2023 Google LLC
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     https://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

// ==================================================
// Begin YouTube Playables integration section 
// ==================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace YTGameSDK
{
    // This code is designed to help communication between YT Game JS SDK and Unity.
    public class YTGameWrapper : MonoBehaviour
    {
        // connect external call
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GetYTGameSDKVersion();

        [DllImport("__Internal")]
        private static extern void SendYTGameScore(int score);
    
        [DllImport("__Internal")]
        private static extern void SendFirstFrameReady();
    
        [DllImport("__Internal")]
        private static extern void SendGameReady();
    
        [DllImport("__Internal")]
        private static extern int SaveGameData(string gameSaveData);
    
        [DllImport("__Internal")]
        private static extern void LoadGameData();
    
        [DllImport("__Internal")]
        private static extern void SendGameError(string errorMessage);
    
        [DllImport("__Internal")]
        private static extern void SendGameWarning(string warningMessage);
    
        [DllImport("__Internal")]
        private static extern bool CheckIsAudioEnabled();
    
        [DllImport("__Internal")]
        private static extern void SetCallbackOnAudioEnabledChange();
    
        [DllImport("__Internal")]
        private static extern void SetCallbackOnGamePause();
    
        [DllImport("__Internal")]
        private static extern void SetCallbackOnGameResume();

        [DllImport("__Internal")]
        private static extern bool CheckInPlayablesEnv();
        
        [DllImport("__Internal")]
        private static extern int RequestAnInterstitialAd();
#endif

        // Used to set callbacks for when the audio changes on YT's end
        public delegate void OnYTGameAudioChange(bool isAudioEnabled);
        protected OnYTGameAudioChange callbackOnYTGameAudioChange;

        // Used to set callbacks for when YT Game pause happens
        public delegate void OnYTGamePause();
        protected OnYTGamePause callbackOnYTGamePause;

        // Used to set callbacks for when YT Game resume happens
        public delegate void OnYTGameResume();
        protected OnYTGameResume callbackOnYTGameResume;

        // Used to set callbacks for when YT Game resume happens
        public delegate void OnYTGameLoadSave(string data);
        protected OnYTGameLoadSave callbackOnYTGameLoadSave;

        public bool DontDestroyOnSceneChange = true;

        private void Awake()
        {
            if (DontDestroyOnSceneChange) GameObject.DontDestroyOnLoad(this.gameObject);
        }

        #region YouTube Game SDK
        // Receive The YouTube Playables SDK version.
        public string GetTheYTGameSDKVersion()
        {
            string theSDKVersion = "";
#if UNITY_WEBGL && !UNITY_EDITOR
            theSDKVersion = GetYTGameSDKVersion();
#endif
            return theSDKVersion;
        }

        // The score object the game sends to YouTube.
        // The score value expressed as an integer.
        public void SendGameScore(int score)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SendYTGameScore(score);
#endif
        }

        // Notifies YouTube that the game�s first frame is ready to be shown.
        // Note: The game MUST call this API. Otherwise, the game isn�t shown on YouTube.
        public void SendGameFirstFrameReady()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SendFirstFrameReady();
#endif
        }

        // Notifies YouTube that the game is ready for players to interact with.
        public void SendGameIsReady()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SendGameReady();
#endif
        }

        // Saves game data to the YouTube cloud in the form of a serialized string.
        // @return a Promise that completes when saving succeeded and rejects with an SdkError when failed.
        // Please note: on success return status == 0, if it returns anything else something went wrong.
        public int SendGameSaveData(string gameSaveData)
        {
            int status = 1;
#if UNITY_WEBGL && !UNITY_EDITOR
            status = SaveGameData(gameSaveData);
            if (status > 0){
                Debug.Log("YT Game save data failed, error code: " + status.ToString());
            }
#endif
            return status;
        }

        // Loads game data from the YouTube cloud in the form of a serialized string.
        // @return a Promise that completes when loading succeeded and rejects with an SdkError when failed.
        // Also sets a callback function for your loaded save data
        public void LoadGameSaveData(OnYTGameLoadSave onLoadSaveCallback)
        {
            callbackOnYTGameLoadSave = onLoadSaveCallback;

#if UNITY_WEBGL && !UNITY_EDITOR
            LoadGameData();
#endif
        }

        // After triggering load game save data, this callback will be triggered from YT JS.
        // Note: The game must call LoadGameSaveData before this will trigger
        public void ReceiveOnLoadSaveEvent(string data)
        {
            callbackOnYTGameLoadSave(data);
        }

        // Logs an error to YouTube.Aggregate information will be accessible on the YouTube Playables health page.
        // Note: This API is best-effort and rate-limited which can result in data loss.
        public void SendYTGameError(string errorMessage)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SendGameError(errorMessage);
#endif
        }

        // Logs a warning to YouTube.Aggregate information will be accessible on the YouTube Playables health webpage.
        // Note: This API is best-effort and rate-limited which can result in data loss.
        public void SendYTGameWarning(string warningMessage)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SendGameWarning(warningMessage);
#endif
        }

        // Returns whether the game audio is enabled in the YouTube settings.
        // Note: The game SHOULD use this API to initialize the game audio state.
        public bool IsYTGameAudioEnabled()
        {
            bool isEnabled = true;
#if UNITY_WEBGL && !UNITY_EDITOR
            isEnabled = CheckIsAudioEnabled();
#endif
            return isEnabled;
        }

        // Sets a callback to be triggered when the audio settings change event is fired from YouTube.
        // Note: The game MUST use this API to update the game audio state.
        public void SetOnAudioEnabledChangeCallback(OnYTGameAudioChange audioCallback)
        {
            callbackOnYTGameAudioChange = audioCallback;
#if UNITY_WEBGL && !UNITY_EDITOR
            // Sets up the callback to receive when YT audio settings change 
            SetCallbackOnAudioEnabledChange();
#endif
        }

        // After setting a callback this is triggered from YT JS when receiving an audio event change.
        // Note: The game must call SetOnAudioEnabledChangeCallback before this to receive changes
        // @param isAudioEnabled whether the audio is enabled in the YouTube settings.
        // Receive callback from OnAudioEnabled in YT Game SDK JS
        public void ReceiveOnAudioEnabledChange(string isAudioEnabled)
        {
            bool isEnabled = (isAudioEnabled == "true" || isAudioEnabled == "True");
            callbackOnYTGameAudioChange(isEnabled);
        }

        // Sets a callback to be triggered when a pause game event is fired from YouTube.
        // Note: You have a short window to save any state before your game gets evicted.
        public void SetOnPauseCallback(OnYTGamePause onPauseCallback)
        {
            callbackOnYTGamePause = onPauseCallback;
#if UNITY_WEBGL && !UNITY_EDITOR
            // Sets up the callback to receive when YT pause happens 
            SetCallbackOnGamePause();
#endif
        }

        // After setting a callback this is triggered from YT JS when receiving an on Pause event.
        // Note: The game must call SetOnPauseCallback before this to receive changes
        public void ReceiveOnPauseEvent()
        {
            callbackOnYTGamePause();
        }

        // Sets a callback to be triggered when a resume game event is fired from YouTube.
        // @return a function to unset the callback.
        public void SetOnResumeCallback(OnYTGameResume onResumeCallback)
        {
            callbackOnYTGameResume = onResumeCallback;
#if UNITY_WEBGL && !UNITY_EDITOR
            // Sets up the callback to receive when YT pause happens 
            SetCallbackOnGameResume();
#endif
        }

        // After setting a callback this is triggered from YT JS when receiving an on Resume event.
        // Note: The game must call SetOnResumeCallback before this to receive changes
        public void ReceiveOnResumeEvent()
        {
            callbackOnYTGameResume();
        }

        // Returns whether the game is currently loaded in a proper Playables Environment.
        // Used to enusre calls like save data and send score are done in a Playables Env.
        public bool InPlayablesEnv()
        {
            bool isPlayables = true;
#if UNITY_WEBGL && !UNITY_EDITOR
            isPlayables = CheckInPlayablesEnv();
#endif
            return isPlayables;
        }
        
        // Requests an Interestitial Ad to be shown. This call makes no guarantees about whether the ad was shown.
        // @return a Promise that an ad is shown successfully. Note: that there will be many times when an Ad is 
        //  not available due something like a delay between ads. So no action is needed, unless its successful and 
        //  you want to adjust anything in your game.
        // Please note: on success return status == 0, if it returns anything else something went wrong.
        public int RequestInterstitialAd()
        {
            int status = 1;
#if UNITY_WEBGL && !UNITY_EDITOR
            status = RequestAnInterstitialAd();
            if (status > 0){
                // status == 0 means the add was successful. Depending on how often your game calls this event status 
                //  code may be something else but that is expected due to ad limits.
                Debug.Log("No Ad shown or Ad request failed, error code: " + status.ToString());
            }
#endif
            return status;
        }
        #endregion
    }
}
