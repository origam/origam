function createIFrame(frameID, overflowAssignment) {
    var bodyID = document.getElementsByTagName("body")[0];
    var newDiv = document.createElement('div');
    newDiv.id = frameID;
    newDiv.style.position ='absolute';
    newDiv.style.backgroundColor = '#FFFFFF';
    newDiv.style.border = '0px';
    newDiv.style.overflow = overflowAssignment;
    newDiv.style.display = 'none';
    bodyID.appendChild(newDiv);
}

function moveIFrame(frameID,iframeID,x,y,w,h,objectID) {
    var frameRef = document.getElementById(frameID);
    var swfObject = document.getElementById(objectID);
    frameRef.style.left = x + swfObject.offsetLeft + 'px';
    frameRef.style.top = y + swfObject.offsetTop + 'px';
    frameRef.style.width = w + 'px';
    frameRef.style.height = h + 'px';
    var iFrameRef = document.getElementById(iframeID);
    iFrameRef.width = w;
    iFrameRef.height = h;
}

function hideIFrame(frameID, iframeID) {
    var iframeRef = document.getElementById(iframeID);
    var iframeDoc;
    if (iframeRef.contentWindow) {
        iframeDoc = iframeRef.contentWindow.document;
    } else if (iframeRef.contentDocument) {
        iframeDoc = iframeRef.contentDocument;
    } else if (iframeRef.document) {
        iframeDoc = iframeRef.document;
    }
    if (iframeDoc){
        iframeDoc.body.style.width = '0';
        iframeDoc.body.style.height = '0';
    }
    document.getElementById(frameID).style.width = '0';
    document.getElementById(frameID).style.height = '0';
}

function showIFrame(frameID, iframeID, w, h) {
    var iframeRef = document.getElementById(iframeID);
    document.getElementById(frameID).style.display='block';
    var iframeDoc;
    if (iframeRef.contentWindow) {
        iframeDoc = iframeRef.contentWindow.document;
    } else if (iframeRef.contentDocument) {
        iframeDoc = iframeRef.contentDocument;
    } else if (iframeRef.document) {
        iframeDoc = iframeRef.document;
    }
    if (iframeDoc) {
        iframeDoc.body.style.width = w + 'px';
        iframeDoc.body.style.height = h + 'px';
    }
}

function hideDiv(frameID) {
    document.getElementById(frameID).style.display='none';
}

function showDiv(frameID) {
    document.getElementById(frameID).style.display = 'block';
}

function loadIFrame(frameID, iframeID, url, embedID, scrollPolicy) {
    document.getElementById(frameID).innerHTML
        = "<iframe id='" + iframeID + "' src='" + url + "' name='" + iframeID
        + "' onLoad='" + embedID + "." + frameID + "_load();' scrolling='"
        + scrollPolicy + "' frameborder='0'></iframe>";
}

function loadDivContent(frameID, iframeID, content) {
    document.getElementById(frameID).innerHTML
        = "<div id='" + iframeID + "' frameborder='0'>" + content + "</div>";
}

function callIFrameFunction(iframeID, functionName, args) {
    var iframeRef=document.getElementById(iframeID);
    var iframeWin;
    if (iframeRef.contentWindow) {
        iframeWin = iframeRef.contentWindow;
    } else if (iframeRef.contentDocument) {
        iframeWin = iframeRef.contentDocument.window;
    } else if (iframeRef.window) {
        iframeWin = iframeRef.window;
    }
    if (iframeWin.wrappedJSObject != undefined) {
        iframeWin = iframeDoc.wrappedJSObject;
    }
    return iframeWin[functionName](args);
}

function removeIFrame(frameID) {
    var iFrameDiv = document.getElementById(frameID);
    iFrameDiv.parentNode.removeChild(iFrameDiv);
}

var oldFrame=null;
function bringIFrameToFront(frameID) {
    var frameRef=document.getElementById(frameID);
    if (oldFrame!=frameRef) {
        if (oldFrame) {
            oldFrame.style.zIndex="99";
        }
        frameRef.style.zIndex="100";
        oldFrame = frameRef;
    }
}

function askForEmbedObjectId(randomString) {
    try {
        var embeds = document.getElementsByTagName('embed');
        for (var i = 0; i < embeds.length; i++) {
            var isTheGoodOne = embeds[i].checkObjectId(embeds[i].getAttribute('id'),randomString);
            if (isTheGoodOne) {
                return embeds[i].getAttribute('id');
            }
        }
        var objects = document.getElementsByTagName('object');
        for(i = 0; i < objects.length; i++) {
            var isTheGoodOne = objects[i].checkObjectId(objects[i].getAttribute('id'),randomString);
            if(isTheGoodOne) {
                return objects[i].getAttribute('id');
            }
        }
    } catch(e) {}
    return null;
}

function getBrowserMeasuredWidth(objectID) {
    return document.getElementById(objectID).offsetWidth;
}

function reloadIFrame(frameID) {
    document.getElementById(frameID).contentWindow.location.reload();
}


