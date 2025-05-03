import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from '../styles/ServerSelection.module.css';
import ApiConfig, { getServerTypeNumber } from '../services/api';

const PanelItem = ({ serverType, onRemove, name, onUpdate }) => {
    const [serverStatus, setServerStatus] = useState();
    const [isLoaded, setIsLoaded] = useState(false);

    const navigate = useNavigate();

    useEffect(() => {
        ApiConfig.api.get(`${ApiConfig.genericServerController}` +
            `/GetServerStatus?ServerType=${serverType}`)
            .then(response => {
                setServerStatus(response.data);
                setIsLoaded(true);
            });
    }, [])

    if (!isLoaded) return;

    return (
        <div className={styles["server-item"]} onClick={() => navigate(`${name == "Don't starve together" ? "DoNotStarveTogether" : name}`)}>
            <h4 style={{ margin: 0 }}>{name}</h4> {/* Заголовок */}
            <span style={{ cursor: 'pointer' }}>Статус: {serverStatus.isStarted ? " запущен" : " остановлен"}</span> {/* Статус */}
            {serverType > 1 ? <p style={{ margin: 0, cursor: 'pointer' }}>Адрес: {serverStatus.address}</p> : <p style={{ margin: 0 }}></p>}
            <span className="material-symbols-outlined" style={{ fontSize: '24px', justifySelf: 'end' }} onClick={(e) => {
                e.stopPropagation();
                onRemove();
            }}>close</span>
        </div>
    );
}

export default PanelItem;