mergeInto(LibraryManager.library, {

  

	// GiveMePlayerData: function () {
    // 	myGameInstance.SendMessage('Yandex', 'SetName', player.getName());
    // 	myGameInstance.SendMessage('Yandex', 'SetPhoto', player.getPhoto("medium"));
  	// },

  	// RateGame: function () {  
    // 	ysdk.feedback.canReview()
    //     .then(({ value, reason }) => {
    //         if (value) {
    //             ysdk.feedback.requestReview()
    //                 .then(({ feedbackSent }) => {
    //                     console.log(feedbackSent);
    //                     if(feedback == true)
    //                     {
    //                       myGameInstance.SendMessage('LevelManager', 'hideRateGameWindow');
    //                       myGameInstance.SendMessage('MoneyManager', 'AddMoneyForGamerate');
    //                     }
                        

    //                 })
    //         } else {
    //             console.log(reason);
    //             ysdk.auth.openAuthDialog()
    //         }
    //     })
  	// },


	SaveExtern: function(date) {
    if(player){
      var dateString = UTF8ToString(date);
    var myobj = JSON.parse(dateString);
    player.setData(myobj);
    }
      },

  LoadExtern: function(){
    if(player){
      player.getData().then(_date => {
      console.log(_date);
      const myJSON = JSON.stringify(_date);
      myGameInstance.SendMessage('Progress', 'SetPlayerInfo', myJSON);
      console.log("Data is getting");
    });
    }
    
  },

  ShowIntersitialAdvExtern: function(){
    ysdk.adv.showFullscreenAdv({
      callbacks: {
        onClose: function(wasShown) {
          console.log("Adv closed");
          myGameInstance.SendMessage('AdvManager', 'StartTimer');
        },
        onError: function(error) {
          // some action on error
        }
      }
    })
  },


  ShowRewardedAdvExtern: function(){

    ysdk.adv.showRewardedVideo({
      callbacks: {
        onOpen: () => {
          console.log('Video ad open.');
          myGameInstance.SendMessage("Progress", "MuteGame");
        },
        onRewarded: () => {
          
        },
        onClose: () => {
          myGameInstance.SendMessage("AdvManager","GiveSecondWind");
          myGameInstance.SendMessage("Progress", "UnmuteGame");
          console.log('Video ad closed');
        }, 
        onError: (e) => {
          console.log('Error while open video ad:', e);
        }
      }
    })
  },

 	SetToLeaderboard : function(value){
    	ysdk.getLeaderboards()
      	.then(lb => {
          lb.setLeaderboardScore('BestScore', value);
      });
  	},

  
    ShowLeaderBoard : function()
    {  
      ysdk.getLeaderboards()
          .then(lb => {             
              lb.getLeaderboardEntries('BestScore', { quantityTop: 5, includeUser: true})
                  .then(res => {
                  console.log(res);
                  const JSONEntry = JSON.stringify(res);
                  myGameInstance.SendMessage('Leaderboard', 'BoardEntriesReady', JSONEntry);        
                  });
          });
    },

    CheckAuth: function()
    {    
      // initPlayer().then(_player => {
      //         if (_player.getMode() === 'lite') {
      //           myGameInstance.SendMessage('Leaderboard', 'OpenAuthAlert'); } 
      // }).catch(() => {myGameInstance.SendMessage('Leaderboard', 'OpenEntries') });
      initPlayer();
      if(player) 
        myGameInstance.SendMessage('Leaderboard', 'OpenEntries');    
      else
        myGameInstance.SendMessage('Leaderboard', 'OpenAuthAlert');  
    },

    Auth: function()
    {
      ysdk.auth.openAuthDialog();
      myGameInstance.SendMessage('Leaderboard', 'CloseAuthWindow');  
    },

    GetLang : function()
    {
      var lang = ysdk.environment.i18n.lang;
      var bufferSize = lengthBytesUTF8(lang) + 1;
      var buffer = _malloc(bufferSize);
      stringToUTF8(lang, buffer, bufferSize);
      return buffer;
    },

  });