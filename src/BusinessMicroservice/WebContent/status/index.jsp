<%@ page import="java.net.*" %> 
<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
<title>saasi</title>
</head>
<body>
<%
	String hostname, serverAddress;
	hostname = "error";
	serverAddress = "error";
	try {
		InetAddress inetAddress;
		inetAddress = InetAddress.getLocalHost();
		hostname = inetAddress.getHostName();
		serverAddress = inetAddress.toString();
	} catch (UnknownHostException e) {

		e.printStackTrace();
	}
%>
	<h1>Hello</h1>
	Business Microservices is working...
	<hr/>
	<div>
		Request served by: <%=serverAddress %>  (<%=hostname %>)
	</div>
</body>
</html>