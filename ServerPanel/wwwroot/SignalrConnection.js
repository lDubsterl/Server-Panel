const hubConnection = new signalR.HubConnectionBuilder()
	.withUrl("/console")
	.build();

hubConnection.on("Send", function (message, id, consoleType) {
	let div = document.getElementById(consoleType);
	div.value += message;
});

hubConnection.start();
