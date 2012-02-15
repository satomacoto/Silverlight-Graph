<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Graph</title>
    <style type="text/css">
    html, body {
	    height: 100%;
	    overflow: auto;
    }
    body {
	    padding: 0;
	    margin: 0;
    }
    </style>
    <script type="text/javascript" src="Silverlight.js"></script>
    <script type="text/javascript">
        function onSilverlightError(sender, args) {
            var appSource = "";
            if (sender != null && sender != 0) {
              appSource = sender.getHost().Source;
            }
            
            var errorType = args.ErrorType;
            var iErrorCode = args.ErrorCode;

            if (errorType == "ImageError" || errorType == "MediaError") {
              return;
            }

            var errMsg = "Unhandled Error in Silverlight Application " +  appSource + "\n" ;

            errMsg += "Code: "+ iErrorCode + "    \n";
            errMsg += "Category: " + errorType + "       \n";
            errMsg += "Message: " + args.ErrorMessage + "     \n";

            if (errorType == "ParserError") {
                errMsg += "File: " + args.xamlFile + "     \n";
                errMsg += "Line: " + args.lineNumber + "     \n";
                errMsg += "Position: " + args.charPosition + "     \n";
            }
            else if (errorType == "RuntimeError") {           
                if (args.lineNumber != 0) {
                    errMsg += "Line: " + args.lineNumber + "     \n";
                    errMsg += "Position: " +  args.charPosition + "     \n";
                }
                errMsg += "MethodName: " + args.methodName + "     \n";
            }

            throw new Error(errMsg);
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div id="silverlightControlHost" style="background:black;">
        <object id="slGraph" data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="500px" height="400px">
		  <param name="source" value="ClientBin/Graph.xap"/>
		  <param name="onError" value="onSilverlightError" />
		  <param name="background" value="white" />
		  <param name="minRuntimeVersion" value="3.0.40624.0" />
		  <param name="autoUpgrade" value="true" />
		  <param name="initParams" value="json=%7B'Nodes'%3A%5B%7B'Name'%3A'0'%7D%2C%7B'Name'%3A'1'%7D%5D%2C'Edges'%3A%5B%7B'Source'%3A'0'%2C'Target'%3A'1'%7D%5D%7D" />
		  <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=3.0.40624.0" style="text-decoration:none">
 			  <img src="http://go.microsoft.com/fwlink/?LinkId=108181" alt="Get Microsoft Silverlight" style="border-style:none"/>
		  </a>
	    </object><iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe></div>
    </form>
    <button type="button" title="Start" onclick="start();">Start</button>
    <button type="button" title="Stop" onclick="stop();">Stop</button>
    <button type="button" title="Add Node" onclick="addNode();">Add Node</button>
    <script type="text/javascript">
    var slcontrol = document.getElementById("slGraph");
    function start() {
        if (slcontrol) {
            slcontrol.Content.MainPage.Start();
        }
    }
    function stop() {
        if (slcontrol) {
            slcontrol.Content.MainPage.Stop();
            var width = slcontrol.Content.MainPage.GetWidth();
            var height = slcontrol.Content.MainPage.GetHeight();
            slcontrol.Content.MainPage.SetBounds(width, height);
        }
    }
    function addNode() {
    	if (slcontrol) {
            slcontrol.Content.MainPage.AddNode("foo");
    	}
    }
    </script>
</body>
</html>
