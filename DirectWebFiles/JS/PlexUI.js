function HWMonitor(UItok) {
	var xmlHttp;
	if (window.XMLHttpRequest) {
		xmlHttp=new XMLHttpRequest();
	} else if (window.ActiveXObject) {
		xmlHttp=new ActiveXObject("Msxml2.XMLHTTP");
	} else {
		alert("Your browser does not support AJAX!");
		return false;
	}
	
	var currentTime = new Date();
	var nocacheseed = currentTime.getTime();

	xmlHttp.open("GET","HWMonitorAjax?PIN="+ UItok +"&nocacheseed="+nocacheseed,true);
	xmlHttp.onreadystatechange=function() {
		if (xmlHttp.readyState == 4) {
			var HWInfoArray = xmlHttp.responseText.split("GRAFSPLIT");
			document.getElementById('HWinfoSubtext').innerHTML = HWInfoArray[0];
			
			// Cancel updates if server reports broken agent connection.
			if (xmlHttp.responseText.indexOf('N/A') > -1 || xmlHttp.responseText.indexOf('No hardware info') > -1) {
				setTimeout(function(){HWMonitor(UItok);}, 2000);
				return;
			}
			
			// Push latest Graf buffer.
			GrafMatrixBuffer = JSON.parse(HWInfoArray[1]);
		
			setTimeout(function(){HWMonitor(UItok);}, 2000);
		}
	}
	xmlHttp.send(null);
	
	// Shut down the phone app if Internet connection goes down.
	// Remember, this is client side functionality.
	// Tested - works awesome! Left for reference.
	/*
	if (!navigator.onLine) {
		PlexitudeWWWInterface.toastMessage('Bad Internet Connection Detected.');
		PlexitudeWWWInterface.stopPlexitudeWWW();
	}
	*/
}

function PlexMonitor(UItok) {
	var xmlHttp;
	if (window.XMLHttpRequest) {
		xmlHttp=new XMLHttpRequest();
	} else if (window.ActiveXObject) {
		xmlHttp=new ActiveXObject("Msxml2.XMLHTTP");
	} else {
		alert("Your browser does not support AJAX!");
		return false;
	}
	
	var currentTime = new Date();
	var nocacheseed = currentTime.getTime();
	
	xmlHttp.open("GET","PlexMonitorAjax?PIN="+ UItok +"&nocacheseed="+nocacheseed,true);
	xmlHttp.onreadystatechange=function() {
		if (xmlHttp.readyState == 4) {
			document.getElementById('PlexContainer').innerHTML = xmlHttp.responseText;
			setTimeout(function(){PlexMonitor(UItok);}, 2000);
		}
	}
	xmlHttp.send(null);
}

function PlexMonitorTopLists(UItok) {
	var xmlHttp;
	if (window.XMLHttpRequest) {
		xmlHttp=new XMLHttpRequest();
	} else if (window.ActiveXObject) {
		xmlHttp=new ActiveXObject("Msxml2.XMLHTTP");
	} else {
		alert("Your browser does not support AJAX!");
		return false;
	}
	
	var currentTime = new Date();
	var nocacheseed = currentTime.getTime();
	
	xmlHttp.open("GET","PlexMonitorTopListsAjax?PIN="+ UItok +"&nocacheseed="+nocacheseed,true);
	xmlHttp.onreadystatechange=function() {
		if (xmlHttp.readyState == 4) {
			document.getElementById('PlexContainerTopLists').innerHTML = xmlHttp.responseText;
			setTimeout(function(){PlexMonitorTopLists(UItok);}, 60000);
		}
	}
	xmlHttp.send(null);
}