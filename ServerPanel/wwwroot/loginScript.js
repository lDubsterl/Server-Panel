
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
        })
        .catch(error => console.log(error));
    sessionStorage.setItem("id", serverResponse.id);
    sessionStorage.setItem("token", serverResponse.access_token);
}

function sendCommandPOST(){
    let command = document.getElementById('text-e521').value;
    let id = sessionStorage["id"];
    let token = sessionStorage["token"];

    const config = {
        headers: { Authorization: `Bearer ${token}` }
    };

    const url = `/api/ServerSelection/${id}/minecraft/panel/general`;

    const commandToSend = {
        "id": id,
        "command": command,
    };

    axios.post(url, commandToSend, config)
        .then(response => console.log(response.data))
        .catch(error => console.log(error));
}

