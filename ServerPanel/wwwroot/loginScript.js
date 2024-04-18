
function authorizationPOST() {
    const url = "https://localhost:5001/authorization";
    const user = {
        "email": document.getElementById("email-a0e8").value,
        "password": document.getElementById("name-a0e8").value,
    };

    let serverResponse;


    axios.post(url, user)
        .then(response => {
            console.log(response.data);
            serverResponse = response.data;
            sessionStorage.setItem("id", serverResponse.id);
            sessionStorage.setItem("token", serverResponse.access_token);
            return true;
        })
        .catch(error => {
            console.log(error);
            return false;
        });
    return false;
}

function sendCommandPOST(url, id, command) {
    let token = sessionStorage["token"];

    const config = {
        headers: { Authorization: `Bearer ${token}` }
    };

    const commandToSend = {
        "id": id,
        "command": command,
    };

    let btn = document.getElementById("startBtn");

    axios.post(url, commandToSend, config)
        .then(response => {
            console.log(response.data);
            if (command == "stop") {
                btn.textContent = "Запуск сервера";
                btn.onclick = function () { startServerGET(url, id) };
            }
        })
        .catch(error => console.log(error));
}

function startServerGET(url, id) {
    let token = sessionStorage["token"];

    const config = {
        headers: { Authorization: `Bearer ${token}` }
    };
    let btn = document.getElementById("startBtn");
    axios.get(url, config)
        .then(response => {
            console.log(response.data);
            btn.textContent = "Остановка сервера";
            btn.onclick = function () { sendCommandPOST(url, id, "stop") };
        })
        .catch(error => console.log(error));
}

function startMinecraftServer() {
    let id = sessionStorage["id"];
    const url = `/api/ServerSelection/${id}/minecraft/panel/general?id=${id}`;
    document.getElementById("Minecraft").value = "";
    startServerGET(url, id);
}

function stopMinecraftServer() {
    let id = sessionStorage["id"];
    const url = `/api/ServerSelection/${id}/minecraft/panel/general`;
    sendCommandPOST(url, id, "stop");
}

function sendCommandToMinecraft() {
    let id = sessionStorage["id"];
    let command = document.getElementById('text-e521').value;
    document.getElementById('text-e521').value = "";
    const url = `/api/ServerSelection/${id}/minecraft/panel/general`;
    sendCommandPOST(url, id, command);
}