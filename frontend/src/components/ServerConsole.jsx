import { React, useState, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import ApiConfig from '../services/api';
import { HubConnectionBuilder } from '@microsoft/signalr';

import styles from "../styles/MinecraftConsole.module.css";

const ServerConsole = ({ serverType }) => {
    const [stopped, setStopped] = useState(true);
    const [address, setAddress] = useState('сервер не запущен');
    const [logs, setLogs] = useState([]);
    const [commandToSend, setCommand] = useState('');
    const [gameConnectionId, setConnectionId] = useState(null);
    const [activeInput, setActiveInput] = useState(null);

    const connectionRef = useRef(null);
    const containerNameRef = useRef('');

    const navigate = useNavigate();
    const { id } = useParams();

    const EstablishWebSocketConnection = () => {
        if (connectionRef.current) return connectionRef.current;

        const newConnection = new HubConnectionBuilder()
            .withUrl(`http://localhost:5000/console?containerName=${containerNameRef.current}&serverType=${serverType}`)
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

        newConnection.on('ServerStopped', () => {
            newConnection.stop()
                .then(() => console.log("Web socket session terminated"));
            setStopped(true);
            setAddress('сервер не запущен');
            connectionRef.current = null;
        });

        newConnection.on('Error', (response) => {
            console.error(response.data);
        })

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
    const sendMessage = (msg, serverType) => {
        if (connectionRef.current) {
            connectionRef.current.invoke('ReceiveCommandFromCLient', gameConnectionId, serverType, msg || commandToSend)
                .then(() => {
                    console.log('Command sent!');
                    setLogs((prevLogs) => [...prevLogs, msg || commandToSend]);
                    setCommand('');
                })
                .catch((err) => {
                    if (!msg)
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
        ApiConfig.api.get(`${ApiConfig.genericServerController}/GetServerStatus?serverType=${serverType}`)
            .then((response) => {
                if (response.data.isStarted) {
                    setStopped(!response.data.isStarted);
                    setAddress(response.data.address);
                    containerNameRef.current = response.data.containerName;
                    EstablishWebSocketConnection();
                    setLoaded(true);
                }
            })
            .catch((error) => {
                if (error.status == 400)
                    navigate(`/${id}`);
            });
    }, []);

    const changeServerStatus = (serverType, stopCommand) => {
        if (stopped) {
            ApiConfig.api.get(`${ApiConfig.genericServerController}/${serverType}?serverType=${serverType}`)
                .then((response) => {
                    setStopped(!stopped);
                    setAddress(response.data.address);
                    containerNameRef.current = response.data.containerName;
                    EstablishWebSocketConnection();
                });
        }
        else {
            sendMessage(stopCommand, serverType);
        }
    }

    let stopCmd = 'stop';
    if (serverType === 3)
        stopCmd = 'exit';

    return (
        <div className={styles["console-container"]}>
            <textarea className={styles["console-output"]} readOnly value={logs.join('\n')} />
            {serverType === 0 && <>
                <div className={styles["console-input-container"]}>
                    <input type="text" className={styles["console-input"]}
                        value={activeInput === 'master' ? commandToSend : ''}
                        onFocus={() => setActiveInput('master')}
                        onChange={(e) => setCommand(e.target.value)}
                        placeholder="Master server" />
                    <button className={styles["console-button"]} onClick={() => sendMessage(null, 0)}>Отправить</button>
                </div>
                <div className={styles["console-input-container"]}>
                    <input type="text" className={styles["console-input"]}
                        value={activeInput === 'caves' ? commandToSend : ''}
                        onFocus={() => setActiveInput('caves')}
                        onChange={(e) => setCommand(e.target.value)}
                        placeholder="Caves" />
                    <button className={styles["console-button"]} onClick={() => sendMessage(null, 1)}>Отправить</button>
                </div>
                <div className={styles["additional-features-container"]}>
                    <button className={styles["start-button"]} onClick={() => changeServerStatus(0, 'c_shutdown()')}>
                        {stopped ? 'Запустить сервер' : 'Остановить сервер'}
                    </button>
                </div>
            </>}
            {serverType >= 2 && <>
                <div className={styles["console-input-container"]}>
                    <input type="text" className={styles["console-input"]} onChange={(e) => setCommand(e.target.value)}
                        value={commandToSend} placeholder="Enter command..." />
                    <button className={styles["console-button"]} onClick={() => sendMessage(null, serverType)}>
                        Отправить
                    </button>
                </div>
                <div className={styles["additional-features-container"]}>
                    <button className={styles["start-button"]} onClick={() => changeServerStatus(serverType, stopCmd)}>
                        {stopped ? 'Запустить сервер' : 'Остановить сервер'}
                    </button>
                    <h4>Адрес сервера: {address}</h4>
                </div>
            </>}
        </div>
    );
}
export default ServerConsole;