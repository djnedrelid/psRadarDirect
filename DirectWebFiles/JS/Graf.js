// GrafJS.
var GrafJS = [];
var GrafMatrixBuffer = [];

function psRadarGrafSetup() {
	
	GrafCreate();	// CPU
	GrafLoad([
		["CPU %","#0066ff",0,0],
		["CPU C","#ff0000",0,0],
		["GPU %","#66cc00",0,0],
		["GPU C","#ffcc00",0,0],
		["RAM %","#cccccc",0,0]
	],0);
}

function GrafCreate() {
	GrafJS.push([
		1,		// GrafStepX
		null,null,	// GrafCanvas, GrafCtx (graf itself)
		null,null,	// GrafCanvasTL, GrafCtxTL (timeline)
		[],		// GrafMatrix
		[],		// GrafMatrixTL
		[],		// GrafCollection (cpu, gpu, etc).
		0,		// LastTimelineRecorded
		null,		// GrafInfoBox
		null,null	// GageCanvas, GageCtx
	]);
}

function GrafLoad(Grafs,GrafJSIndex) {
	
	// Prep Graf engine.
	PrepCanvases(GrafJSIndex);
	
	// Start engine.
	GrafEngine(GrafJSIndex);
}

function PrepCanvases(GrafJSIndex) {
	
	// Get the canvases.
	GrafJS[GrafJSIndex][1] = document.getElementById("Graf"+GrafJSIndex);
	GrafJS[GrafJSIndex][3] = document.getElementById("GrafTimeline"+GrafJSIndex);
	GrafJS[GrafJSIndex][10] = document.getElementById("GageBox"+GrafJSIndex);
	
	// Did we resize?
	var CurCanvasX = document.getElementById("Grafs").offsetWidth;
	if (CurCanvasX > GrafJS[GrafJSIndex][1].width || CurCanvasX < GrafJS[GrafJSIndex][1].width)
		GrafJS[GrafJSIndex][0] = GrafJS[GrafJSIndex][1].width - CurCanvasX;
	else
		GrafJS[GrafJSIndex][0] = 1;
	
	// Canvas properties and contexts.
	GrafJS[GrafJSIndex][1].width = CurCanvasX;
	GrafJS[GrafJSIndex][3].width = CurCanvasX;
	GrafJS[GrafJSIndex][10].width = CurCanvasX;
	GrafJS[GrafJSIndex][2] = GrafJS[GrafJSIndex][1].getContext("2d");
	GrafJS[GrafJSIndex][4] = GrafJS[GrafJSIndex][3].getContext("2d");
	GrafJS[GrafJSIndex][11] = GrafJS[GrafJSIndex][10].getContext("2d");
	GrafJS[GrafJSIndex][2].clearRect(-10,-10,GrafJS[GrafJSIndex][1].width+20,GrafJS[GrafJSIndex][1].height+20);
	GrafJS[GrafJSIndex][4].clearRect(-10,-10,GrafJS[GrafJSIndex][1].width+20,GrafJS[GrafJSIndex][1].height+20);
	GrafJS[GrafJSIndex][11].clearRect(-10,-10,GrafJS[GrafJSIndex][10].width+20,GrafJS[GrafJSIndex][10].height+20);
	
	// Default styling.
	GrafJS[GrafJSIndex][2].strokeStyle = "blue";
	GrafJS[GrafJSIndex][2].lineWidth = 2;
	GrafJS[GrafJSIndex][2].lineJoin = "round";
	GrafJS[GrafJSIndex][2].lineCap = "round";
	GrafJS[GrafJSIndex][2].translate(0.5,0.5);
	GrafJS[GrafJSIndex][2].font = "11px Verdana";
	GrafJS[GrafJSIndex][2].textBaseline = "middle";
	GrafJS[GrafJSIndex][4].font = "12px Verdana";
	GrafJS[GrafJSIndex][4].textAlign = "end";
	GrafJS[GrafJSIndex][4].textBaseline = "top";
}

function GrafEngine(GrafJSIndex) {
	
	// Handle resize.
	PrepCanvases(GrafJSIndex);
	
	// Local vars.
	var n,n2;
	var newX = GrafJS[GrafJSIndex][1].width;
	var lastX = GrafJS[GrafJSIndex][1].width;
	var XposRegistry = [["CPUP",0],["CPUC",0],["GPUP",0],["GPUC",0],["RAM",0]];
	var TimelineTime;
	var GrafColor = "";
	var GageXPos = 30;
	var GageValuePos;
	var GageColor;
	var PercentStepValue;
	var PercentStep;
	
	// Horizontal value legends and numbers.
	GrafJS[GrafJSIndex][2].beginPath();
	GrafJS[GrafJSIndex][2].strokeStyle = "#ccc";
	GrafJS[GrafJSIndex][2].lineWidth = 1;
	// 80.
	GrafJS[GrafJSIndex][2].moveTo(20,0);
	GrafJS[GrafJSIndex][2].lineTo(20,GrafJS[GrafJSIndex][1].height);
	GrafJS[GrafJSIndex][2].moveTo(20,20);
	GrafJS[GrafJSIndex][2].lineTo(GrafJS[GrafJSIndex][1].width,20);
	GrafJS[GrafJSIndex][2].fillText("80",2,20);
	// 60.
	GrafJS[GrafJSIndex][2].moveTo(20,40);
	GrafJS[GrafJSIndex][2].lineTo(GrafJS[GrafJSIndex][1].width,40);
	GrafJS[GrafJSIndex][2].fillText("60",2,40);
	// 40.
	GrafJS[GrafJSIndex][2].moveTo(20,60);
	GrafJS[GrafJSIndex][2].lineTo(GrafJS[GrafJSIndex][1].width,60);
	GrafJS[GrafJSIndex][2].fillText("40",2,60);
	// 40.
	GrafJS[GrafJSIndex][2].moveTo(20,80);
	GrafJS[GrafJSIndex][2].lineTo(GrafJS[GrafJSIndex][1].width,80);
	GrafJS[GrafJSIndex][2].fillText("20",2,80);
	GrafJS[GrafJSIndex][2].stroke();
	
	// Set back line thickness for graphs.
	GrafJS[GrafJSIndex][2].lineWidth = 2;
	
	// Prepare X for buffer. Only the client side can provide width adaption.
	// Or else we would have prepared this on the serverside.
	for (n=GrafMatrixBuffer.length-1; n>=0; n--) {
		
		// We collect 5 elements (CPUP+CPUC+GPUP+GPUC+RAM).
		// So we need to coordinate X as a set.
		
		if (GrafMatrixBuffer[n].Name == "CPU %") {
			if (XposRegistry[0][1] == 0) {
				XposRegistry[0][1] = 1;
				GrafMatrixBuffer[n].NewX = lastX;
				GrafMatrixBuffer[n].LastX = lastX-1;
			}
			
		} else if (GrafMatrixBuffer[n].Name == "CPU C") {
			if (XposRegistry[1][1] == 0) {
				XposRegistry[1][1] = 1;
				GrafMatrixBuffer[n].NewX = lastX;
				GrafMatrixBuffer[n].LastX = lastX-1;
			}
			
		} else if (GrafMatrixBuffer[n].Name == "GPU %") {
			if (XposRegistry[2][1] == 0) {
				XposRegistry[2][1] = 1;
				GrafMatrixBuffer[n].NewX = lastX;
				GrafMatrixBuffer[n].LastX = lastX-1;
			}
			
		} else if (GrafMatrixBuffer[n].Name == "GPU C") {
			if (XposRegistry[3][1] == 0) {
				XposRegistry[3][1] = 1;
				GrafMatrixBuffer[n].NewX = lastX;
				GrafMatrixBuffer[n].LastX = lastX-1;
			}
			
		} else if (GrafMatrixBuffer[n].Name == "RAM %") {
			if (XposRegistry[4][1] == 0) {
				XposRegistry[4][1] = 1;
				GrafMatrixBuffer[n].NewX = lastX;
				GrafMatrixBuffer[n].LastX = lastX-1;
			}
		}
		
		// Reset registry once a complete set has been moved.
		if (XposRegistry[0][1] == 1 && 
			XposRegistry[1][1] == 1 &&
			XposRegistry[2][1] == 1 &&
			XposRegistry[3][1] == 1 &&
			XposRegistry[4][1] == 1) {

				for (n2=0; n2<XposRegistry.length; n2++) 
					XposRegistry[n2][1] = 0;
			
				lastX--;
		}
	}
	
	// Clean up old data outside X bounds in the matrix buffer.
	for (n=0; n<GrafMatrixBuffer.length; n++) {
		if (GrafMatrixBuffer[n].LastX <= 22) {
			GrafMatrixBuffer.shift();
			n=0;
			continue;
		}
	}
	
	// Print GrafMatrix.
	for (n=0; n<GrafMatrixBuffer.length; n++) {
		
		// Update moving coordinates.
		// lastX -= GrafJS[GrafJSIndex][0];
		
		// Draw line.
		GrafJS[GrafJSIndex][2].beginPath();
		GrafJS[GrafJSIndex][2].strokeStyle = GrafMatrixBuffer[n].GrafColor;
		GrafJS[GrafJSIndex][2].moveTo(GrafMatrixBuffer[n].LastX, 100-GrafMatrixBuffer[n].LastY);
		GrafJS[GrafJSIndex][2].lineTo(GrafMatrixBuffer[n].NewX, 100-GrafMatrixBuffer[n].NewY);
		GrafJS[GrafJSIndex][2].stroke();
		
		// Draw Timeline
		GrafJS[GrafJSIndex][4].fillText(GrafMatrixBuffer[n].Time,GrafMatrixBuffer[n].NewX,2);
	}
	
	// Print Gages based on last given buffer values.
	for (n=GrafMatrixBuffer.length-1; n>=0; n--) {
		
		// Once all gages has gotten a value, we break out.
		if (XposRegistry[0][1] == 1 && 
			XposRegistry[1][1] == 1 &&
			XposRegistry[2][1] == 1 &&
			XposRegistry[3][1] == 1 &&
			XposRegistry[4][1] == 1) 
				break;
		
		// Update gage.
		GageValuePos = 0;
		GageColor = ((GrafMatrixBuffer[n].NewY) >= 80 ?"red":"green");
		PercentStepValue = Math.PI/100;
		PercentStep = 100-GrafMatrixBuffer[n].NewY;
		GrafJS[GrafJSIndex][11].font = "11px Verdana";
		GrafJS[GrafJSIndex][11].lineWidth = 5;
		
		// Percent handling.
		if ((100-PercentStep) < 10)
			GageValuePos = 2;
		else if ((100-PercentStep) < 100)
			GageValuePos = 6;
		else if ((100-PercentStep) == 100)
			GageValuePos = 10;
		
		// Register gages that has been created.
		if (GrafMatrixBuffer[n].Name == "CPU %") {
			GageXPos = 30;
			if (XposRegistry[0][1] == 0) 
				XposRegistry[0][1] = 1;
			
		} else if (GrafMatrixBuffer[n].Name == "CPU C") {
			GageXPos = 90;
			if (XposRegistry[1][1] == 0) 
				XposRegistry[1][1] = 1;
			
		} else if (GrafMatrixBuffer[n].Name == "GPU %") {
			GageXPos = 150;
			if (XposRegistry[2][1] == 0) 
				XposRegistry[2][1] = 1;

		} else if (GrafMatrixBuffer[n].Name == "GPU C") {
			GageXPos = 210;
			if (XposRegistry[3][1] == 0) 
				XposRegistry[3][1] = 1;
			
		} else if (GrafMatrixBuffer[n].Name == "RAM %") {
			GageXPos = 270;
			if (XposRegistry[4][1] == 0) 
				XposRegistry[4][1] = 1;
		}
		
		//BGcolor.
		GrafJS[GrafJSIndex][11].beginPath();
		GrafJS[GrafJSIndex][11].arc(GageXPos,20,15,Math.PI,(Math.PI*2));
		GrafJS[GrafJSIndex][11].strokeStyle = "#eee";
		GrafJS[GrafJSIndex][11].stroke();
		// Front color.
		GrafJS[GrafJSIndex][11].beginPath();
		GrafJS[GrafJSIndex][11].arc(GageXPos,20,15,Math.PI,(Math.PI*2)-PercentStepValue*PercentStep);
		GrafJS[GrafJSIndex][11].strokeStyle = GageColor;
		GrafJS[GrafJSIndex][11].stroke();
		// Name.
		GrafJS[GrafJSIndex][11].beginPath();
		GrafJS[GrafJSIndex][11].fillStyle = GrafMatrixBuffer[n].GrafColor;
		GrafJS[GrafJSIndex][11].fillText(GrafMatrixBuffer[n].Name,GageXPos-(GrafMatrixBuffer[n].Name.length*3),30);
		GrafJS[GrafJSIndex][11].stroke();
		// Value.
		GrafJS[GrafJSIndex][11].beginPath();
		GrafJS[GrafJSIndex][11].font = "10px Verdana";
		GrafJS[GrafJSIndex][11].fillStyle = "#000";
		GrafJS[GrafJSIndex][11].fillText(100-PercentStep,GageXPos-GageValuePos,20);
		GrafJS[GrafJSIndex][11].stroke();
	}
	
	// Loop it.
	setTimeout(GrafEngine,1000,GrafJSIndex);
}
// GrafJS.