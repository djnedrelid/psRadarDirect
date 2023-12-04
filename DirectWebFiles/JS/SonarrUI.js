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

	xmlHttp.open("GET","HWMonitorAjaxSonarr?PIN="+ UItok +"&nocacheseed="+nocacheseed,true);
	xmlHttp.onreadystatechange=function() {
		if (xmlHttp.readyState == 4) {
			document.getElementById('HWinfo').innerHTML = xmlHttp.responseText;
		}
	}
	xmlHttp.send(null);
}