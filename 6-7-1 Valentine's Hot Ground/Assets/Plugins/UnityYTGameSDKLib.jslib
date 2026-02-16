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

mergeInto(LibraryManager.library, {
    GetYTGameSDKVersion: function () {
		var sdkVersion = ytgame.SDK_VERSION;
		var bufferSize = lengthBytesUTF8(sdkVersion) + 1;
		var strBuffer = _malloc(bufferSize);
		stringToUTF8(sdkVersion, strBuffer, bufferSize);
		return strBuffer;
    },
	SendYTGameScore: function (score) {
		ytgame.engagement.sendScore({value: score});
	},
	SendFirstFrameReady: function () {
		ytgame.game.firstFrameReady();
	},
	SendGameReady: function () {
		ytgame.game.gameReady();
	},
	SaveGameData: function (playableSaveData) {
		ytgame.game.saveData(UTF8ToString(playableSaveData)).then(() => {
			// Handle data save success.
		  	return 0;
		}, (e) => {
		  	// console.error(e);
	  		// Send an error to YouTube
		  	ytgame.health.logError();
			
		  	// Handle data save failure.
		  	return 1;
		});
	},
	LoadGameData: function () {
		ytgame.game.loadData().then((data) => {
			unityGameInstance.SendMessage("YTGameWrapper", "ReceiveOnLoadSaveEvent", data.toString());
		});
	},
	SendGameError: function (errorMessage) {
		console.error(errorMessage);
		// Send an error to YouTube
		ytgame.health.logError();
	},
	SendGameWarning: function (warningMessage) {
		console.debug(warningMessage);
		// Send an warning to YouTube
		ytgame.health.logWarning();
	},
	CheckIsAudioEnabled: function () {
		var isEnabled = Boolean(ytgame.system.isAudioEnabled());
		return isEnabled;
	},
	SetCallbackOnAudioEnabledChange: function () {
		const unsetCallback = ytgame.system.onAudioEnabledChange((isAudioEnabled) => {
			unityGameInstance.SendMessage("YTGameWrapper", "ReceiveOnAudioEnabledChange", isAudioEnabled.toString());
		 });
	},
	SetCallbackOnGamePause: function () {
		const unsetCallback = ytgame.system.onPause(() => {
			unityGameInstance.SendMessage("YTGameWrapper", "ReceiveOnPauseEvent");
		 });
	},
	SetCallbackOnGameResume: function () {
		const unsetCallback = ytgame.system.onResume(() => {
			unityGameInstance.SendMessage("YTGameWrapper", "ReceiveOnResumeEvent");
		 });
	},
	CheckInPlayablesEnv: function () {
		var isPlayables = Boolean(typeof ytgame !== 'undefined' && ytgame.IN_PLAYABLES_ENV);
		return isPlayables;
	},
	RequestAnInterstitialAd: function () {
		ytgame.ads.requestInterstitialAd().then(() => {
			// Ad request successful, pause and mute calls should also be sent to your game.
		  	return 0;
		}, (e) => {
		  	// Ad not shown as there is a delay or none are available. Other failures will be picked up by YT.
		  	return 1;
		});
	},
});
