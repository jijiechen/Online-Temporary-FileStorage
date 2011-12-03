<%@ Page Language="VB" AutoEventWireup="false" ValidateRequest="false"  CodeFile="Destination.aspx.vb" Inherits="destination" %>
<!doctype html>
<html>
<head>
<meta http-equiv="Content-Type" content="text/html;charset=utf-8" />
<title>File Storage</title>
<style>
div{margin:10px 0;}
tr:hover{background-color:#dfdfdf}
</style>
</head>
<body>
<div id="upload">
<h1>Add</h1>
<form action="destination.aspx" method="post" enctype="multipart/form-data"><input type="file" name="file" id="file" accept="*/*" /></form>
<p></p>
</div>
<div id="uploaded">
<h1>Added</h1>
<table id="RecentlyFiles" style="border-collapse:collapse;">
<%=FileListStr%>
</table>
</div>
<script>
	(function () {

		function out(txt) {
			document.getElementsByTagName('p')[0].innerHTML = txt;
		}
		function sendBinary(str, file) {
			var formData, boundary, xhr = new XMLHttpRequest();

			xhr.open("POST", "Destination.aspx?ajax=true", true);
			xhr.setRequestHeader("Content-Length", file.size);
			out("Uploading...");

			xhr.upload.addEventListener("progress", function (e) {
				if (e.lengthComputable) {
					var percentage = Math.round((e.loaded * 100) / e.total);
					out("Uploading..." + percentage + "%");
				}
			}, false);
			xhr.upload.addEventListener("load", function (e) {
				out("Done");
				setTimeout(function () {
					location.reload();
				}, 1000);
			}, false);
			xhr.upload.addEventListener("error", function (e) {
				out("Error when uploading");
			}, false);

//			if (xhr.sendAsBinary) {
//				boundary = "---------------------------Ajax" + String(Number(new Date)).substr(0, 5) + 'Upload';
//				xhr.setRequestHeader("Content-Type", "multipart/form-data, boundary=" + boundary);
//				var requestBody = "--" + boundary + "\r\n" +
//					'Content-Disposition: form-data; name="file"; filename="' + file.name + '"\r\n' +
//					"Content-Type: Application/Stream-Otect\r\n\r\n" +
//					str + "\r\n--" + boundary + "--\r\n";
//				xhr.sendAsBinary(requestBody);
//			} else if (typeof FormData != "undefined") {
				formData = new FormData();
				formData.append('file', file);
				xhr.send(formData);
//			} else {
//				out('Can not send the file by XmlHttpRequest');
//			}
		}
		function uploadFile(data) {
			if (data.files && data.files.length) {
				var file = data.files[0], fr = new FileReader();
				try {
					out('Initializing...');
					fr.readAsBinaryString(file);
					fr.onload = function () {
						sendBinary(this.result, file);
					}
				} catch (err) {
					return out("Error when reading file");
				}
			}
		}
		document.ondragover = function (e) { e.preventDefault() };
		document.ondrop = function (e) {
			e.stopPropagation();
			e.preventDefault();
			uploadFile(e.dataTransfer);
		};
		var isIE = navigator.userAgent.toLowerCase().indexOf('msie') > -1;
		if (isIE) {
			out("Try Chrome or Firefox to get a better experience");
		}
		document.getElementsByTagName('input')[0].onchange = function () {
			if (isIE) {
				document.getElementsByTagName("FORM")[0].submit();
			} else {
				uploadFile(this);
			}
		};
	})();
</script>
</body>
</html>