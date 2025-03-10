import { React, useState, useEffect, useRef } from 'react';
import api from '../services/api';
import { HubConnectionBuilder } from '@microsoft/signalr';

import styles from "../styles/MinecraftConsole.module.css";

const ServerConsole = () => {
    const [stopped, setStopped] = useState(true);
    const [address, setAddress] = useState('сервер не запущен');

    const [logs, setLogs] = useState([]);
    const [commandToSend, setCommand] = useState('');

    const [gameConnectionId, setConnectionId] = useState(null);

    const connectionRef = useRef(null);
    const containerNameRef = useRef('');

    const EstablishWebSocketConnection = () => {
        if (connectionRef.current) return connectionRef.current;

        const newConnection = new HubConnectionBuilder()
            .withUrl(`http://localhost:5000/console?containerName=${containerNameRef.current}`)
            .configureLogging('information') // Включение логирования
            .build();
        newConnection.on('InfoReceive', (obj) => {
            let data = JSON.parse(obj);
            if (data.connectionId)
                setConnectionId(data.connectionId);
            else
                console.log(obj);
        })
        newConnection.on('Receive', (message) => {
            setLogs((prevMessages) => [...prevMessages, message]);
        });

        newConnection.start()
            .then(() => {
                console.log('Connected to SignalR Hub');
            })
            .catch((err) => {
                console.error('Error while establishing connection: ', err);
            });

        connectionRef.current = newConnection;
    }

    useEffect(() => {
        setLogs((prevLogs) => prevLogs.slice(-200));;
    }, [logs.length]);

    // Функция для отправки сообщения
    const sendMessage = (msg) => {
        if (connectionRef.current) {
            connectionRef.current.invoke('ReceiveCommandFromCLient', gameConnectionId, msg || commandToSend)
                .then(() => {
                    console.log('Command sent!');
                    setLogs((prevLogs) => [...prevLogs, msg || commandToSend]);
                    setCommand('');
                    if (msg || commandToSend == 'stop') {
                        setStopped(true);
                        setAddress('сервер не запущен');
                    }
                })
                .catch((err) => {
                    console.error('Error sending message: ', err);
                });
        }
    };

    useEffect(() => {
        return () => {
            if (connectionRef.current) {
                console.log('Component unmounting, stopping SignalR connection...');
                connectionRef.current.stop()
                    .then(() => console.log('SignalR connection stopped.'))
                    .catch((err) => console.error('Error stopping connection:', err));
            }
        };
    }, []);

    useEffect(() => {
        api.get(`ServerSelection/${localStorage.getItem("userId")}/GetServerStatus`)
            .then((response) => {
                if (response.data.isStarted) {
                    setStopped(!response.data.isStarted);
                    setAddress(response.data.address);
                    containerNameRef.current = response.data.containerName;
                    EstablishWebSocketConnection();
                }
            })
    }, []);

    const changeServerStatus = () => {
        if (stopped) {
            api.get(`ServerSelection/${localStorage.getItem("userId")}/StartMinecraftServer`)
                .then((response) => {
                    setStopped(!stopped);
                    setAddress(response.data.address);
                    containerNameRef.current = response.data.containerName;
                    EstablishWebSocketConnection();
                });
        }
        else {
            sendMessage('stop');
        }
    }

    return (
        <div className={styles["console-container"]}>
            <textarea className={styles["console-output"]} readOnly value={logs.join('\n')} />
            <div className={styles["console-input-container"]}>
                <input type="text" className={styles["console-input"]} onChange={(e) => setCommand(e.target.value)}
                    value={commandToSend} placeholder="Enter command..." />
                <button className={styles["console-button"]} onClick={() => sendMessage(null)}>Отправить</button>
            </div>
            <div className={styles["additional-features-container"]}>
                <button className={styles["start-button"]} onClick={changeServerStatus}>{stopped ? 'Запустить сервер' : 'Остановить сервер'}</button>
                <h4>Адрес сервера: {address}</h4>
            </div>
        </div>
    );
}

export default ServerConsole;