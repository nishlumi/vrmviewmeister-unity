var FileImportPlugin = {
	FileImportCaptureClick: function() {
		if (!document.getElementById('FileImportInput')) {
			var fileInput = document.createElement('input');
			fileInput.setAttribute('type', 'file');
			fileInput.setAttribute('id', 'FileImport');
			fileInput.style.visibility = 'hidden';
			fileInput.onclick = function (event) {
				this.value = null;
			};
			fileInput.onchange = function (event) {
				SendMessage('Canvas', 'FileSelected', URL.createObjectURL(event.target.files[0]));
			}
			document.body.appendChild(fileInput);
		}
		var OpenFileDialog = function() {
			document.getElementById('FileImport').click();
			document.getElementById('unity-canvas').removeEventListener('click', OpenFileDialog);
		};
		document.getElementById('unity-canvas').addEventListener('click', OpenFileDialog, false);
	},
	sendObjectError : function (type, info) {
		var js = {type:"", info:"", errcd : -1};
		try {
			var jsinfo  = UTF8ToString(info).split("\t");
			
			js.type = type;
			js.info = jsinfo;
		}catch(e) {
		}finally {
			var cur = AppQueue.current();
			if (cur) {
				AppDB.temp.setItem(cur.key,js).then(function(value){
					AppQueue.execute(cur.key,cur,value);
				});
			}
		}
	},
	sendVRMInfo : function (thumbnail, size, type, info, licenseType, height, blendShape) {
		var binary = "";
		var js = {};
		try {
			js = JSON.parse(UTF8ToString(info));
			
			
			for (var i = 0; i < size; i++) {
				binary += String.fromCharCode(HEAPU8[thumbnail + i]);
			}
			var dataurl = "data:image/png;base64," + btoa(binary);
			
			//js["type"] = UTF8ToString(type);
			js["thumbnail"] = dataurl;
			js["height"] = UTF8ToString(height);
			js["license"] = UTF8ToString(licenseType);
			js["blendshape"] = UTF8ToString(blendShape);
			/*
			var js = {
				thumbnail : dataurl,
				title : (js.title),
				version: (js.version),
				author : (js.author),
				contact : UTF8ToString(contactinfo),
				reference : UTF8ToString(reference),
				licence : UTF8ToString(licence),
				height : UTF8ToString(height)
			};
			*/
		}catch(e) {
			js = {};
		}finally {
			//document.getElementById("savedata_unity2html").value = JSON.stringify(js);
			//document.getElementById("savebtn_unity2html").click();
			//AppDB.temp.setItem("sendobjectinfo",js).then(function(value){
			//	document.getElementById("savebtn_unity2html").click();
			//});
			var cur = AppQueue.current();
			if (cur) {
				AppDB.temp.setItem(cur.key,js).then(function(value){
					AppQueue.execute(cur.key,cur,value);
				});
			}
		}
	},
	sendOtherObjectInfo : function (type, info) {
		var binary = "";
		var js = {};
		try {
			js = JSON.parse(UTF8ToString(info));
			//js["type"] = UTF8ToString(type);
			
		}catch(e) {
			js = {};
		}finally {
			//document.getElementById("savedata_unity2html").value = JSON.stringify(js);
			//document.getElementById("savebtn_unity2html").click();
			//AppDB.temp.setItem("sendobjectinfo",js).then(function(value){
			//	document.getElementById("savebtn_unity2html").click();
			//});
			var cur = AppQueue.current();
			if (cur) {
				AppDB.temp.setItem(cur.key,js).then(function(value){
					AppQueue.execute(cur.key,cur,value);
				});
			}
		}
	},
	sendCapture : function(rawimg, size) {
		var binary = "";
		
		for (var i = 0; i < size; i++) {
			binary += String.fromCharCode(HEAPU8[rawimg + i]);
		}
		var dataurl = "data:image/png;base64," + btoa(binary);
		
		AppDB.capture.setItem(new Date().valueOf(), dataurl);
		/*
		var elms = document.getElementById("capture-area");
		var newheight = elms.clientHeight;
		var ratio = parseFloat(ID("unity-canvas").width / ID("unity-canvas").height);
		var newwidth = newheight * ratio;
		
		var img = document.createElement("img");
		img.src = dataurl;
		img.onload = function (evt) {
			var w = evt.target.width;
			var h = evt.target.height;
			if (w > h) {
				evt.target.classList.add("capture-image-grid-itemL");
			}else {
				evt.target.classList.add("capture-image-grid-itemP");
			}
		}
		//img.width = newwidth;
		//img.height = newheight;
		document.getElementById("capture-area").appendChild(img);
		*/
		
	},
	sendCaptureFree : function (rawimg, size) {
		
		var binary = "";
		
		for (var i = 0; i < size; i++) {
			binary += String.fromCharCode(HEAPU8[rawimg + i]);
		}
		var dataurl = "data:image/png;base64," + btoa(binary);
		
		var imgname = new Date().valueOf();
		AppDB.capture.setItem(imgname, dataurl);
		
		var mykey = "freecapture_unity2html";
		var str = UTF8ToString(imgname);
		
		AppDB.temp.setItem(mykey,str).then(function(value){
			AppQueue.fixedExecute(mykey,value);
		});
	},
	sendBackupTransform: function(thumbnail, size, info) {
		var binary = "";
		var js = {};
		try {
			js = JSON.parse(UTF8ToString(info));
			
			
			for (var i = 0; i < size; i++) {
				binary += String.fromCharCode(HEAPU8[thumbnail + i]);
			}
			var dataurl = "data:image/png;base64," + btoa(binary);
			
			js["thumbnail"] = dataurl;

		}catch(e) {
			js = {};
		}finally {
			var cur = AppQueue.current();
			if (cur) {
				AppDB.temp.setItem(cur.key,js).then(function(value){
					AppQueue.execute(cur.key,cur,value);
				});
			}
		}
	}
};
mergeInto(LibraryManager.library, FileImportPlugin);