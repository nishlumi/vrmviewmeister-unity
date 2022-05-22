var UIControllFromOuterPlugin = {
	ChangeBendSliderHTML : function (value) {
		document.getElementById("sli_kansetu").value = value;
		document.getElementById("sli_kansetu").click();
	},
	ChangeActiveAvatarSelection : function (name, type) {
		var vname = UTF8ToString(name);
		var vtype = UTF8ToString(type);
		

		var mykey = "selectavatar_unity2html";
		
		AppDB.temp.setItem(mykey,{name : vname, type : vtype}).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
		

	},
	AfterActivateAvatar : function (name) {
		var vname = UTF8ToString(name);
		//ID("unity_receiveevent").value = "selectavatar_html2unity";
		
		var mykey = "selectavatar_html2unity";
			
		AppDB.temp.setItem(mykey,vname).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
		
	},
	ChangeTransformOnUpdate : function (val) {
		var mykey = "transform_unity2html";
		
		var str = UTF8ToString(val);
		var js = JSON.parse(str);
		AppDB.temp.setItem(mykey,js).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
	},
	ChangeDirectionalLightTransformOnUpdate : function (val) {
		var mykey = "dlight_transform_unity2html";
		
		var str = UTF8ToString(val);
		var js = JSON.parse(str);
		AppDB.temp.setItem(mykey,js).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
	},
	SendPlayingAnimationInfoOnUpdate : function (val) {
		var mykey = "playinganima_unity2html";
		
		//var str = UTF8ToString(val);
		//var js = JSON.parse(str);
		AppDB.temp.setItem(mykey,val).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
	},
	SendPlayingAnimationInfoOnPause : function (val) {
		var mykey = "pauseanima_unity2html";
		
		//var str = UTF8ToString(val);
		//var js = JSON.parse(str);
		AppDB.temp.setItem(mykey,val).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
	},
	SendPlayingAnimationInfoOnComplete : function (val) {
		var mykey = "completeanima_unity2html";
		
		//var str = UTF8ToString(val);
		//var js = JSON.parse(str);
		AppDB.temp.setItem(mykey,val).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
	},
	SendPreviewingOtherObjectAnimationEnd : function (val) {
		var mykey = "previewend_otherobjectAnimation_unity2html";
		
		var str = UTF8ToString(val);
		
		AppDB.temp.setItem(mykey,str).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
	},
	SendPreviewingEffectEnd : function (val) {
		var mykey = "previewend_effect_unity2html";
		
		var str = UTF8ToString(val);
		
		AppDB.temp.setItem(mykey,str).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
	},
	SendPlayingPreviewAnimationInfoOnComplete : function (val) {
		var mykey = "finishPreviewFrame_unity2html";
		
		AppDB.temp.setItem(mykey,val).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
	},
	SendPlayEndAudio : function (val) {
		var mykey = "finishAudio_unity2html";
		
		var str = UTF8ToString(val);

		AppDB.temp.setItem(mykey,str).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
	},
	IntervalLoadingProject : function (val) {
		var mykey =  "intervalLoadingProject_unity2html";
		
		AppDB.temp.setItem(mykey,val).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
	},
	ReceiveObjectVal : function (val) {
		var str = UTF8ToString(val);
		var js = JSON.parse(str);
		var cur = AppQueue.current();
		if (cur) {
			AppDB.temp.setItem(cur.key,js).then(function(value){
				AppQueue.execute(cur.key,cur,value);
			});
		}
		
	},
	ReceiveStringVal : function (val) {
		var str = UTF8ToString(val);
		
		var cur = AppQueue.current();
		if (cur) {
			AppDB.temp.setItem(cur.key,str).then(function(value){
				AppQueue.execute(cur.key,cur,value);
			});
		}
	},
	ReceiveIntVal : function (val) {
		var cur = AppQueue.current();
		if (cur) {
			AppDB.temp.setItem(cur.key,val).then(function(value){
				AppQueue.execute(cur.key,cur,value);
			});
		}
		
	},
	ReceiveFloatVal : function (val) {
		var cur = AppQueue.current();
		if (cur) {
			AppDB.temp.setItem(cur.key,val).then(function(value){
				AppQueue.execute(cur.key,cur,value);
			});
		}
	}
};
mergeInto(LibraryManager.library, UIControllFromOuterPlugin);