# RconClient
A simple, dependency injection-friendly .NET Client for RCON connections.

Nuget: https://www.nuget.org/packages/Rcon.Client/

## Usage
To instance a client, you can construct one using the `RconClient(string serverAddress, int port)` constructor. This will create the client, and store the connection information for use.

Connections using the client are not created until right before initial command execution, and the connection will remain open until the client is disposed, or if the connection is externally closed. This allows you to use the same TCP connection for multiple commands:

    using (var client = new RconClient("192.168.0.15", 25575))
    {
	    var authCommand = RconCommand.Auth("password");
	    var myCommand = RconCommand.ServerCommand("/gamemode creative");
		
		var authResponse = client.ExecuteCommand(authCommand);
		var cmdResponse = client.ExecuteCommand(myCommand);
    }
