import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from '../styles/ServerSelection.module.css';
import ApiConfig, { getServerTypeNumber } from '../services/api';

const PanelItem = ({ serverType, onRemove, name , onUpdate}) => {
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
            <h4 style={{ margin: '0' }}>{name}</h4>
            <span style={{ margin: '0' }}>Статус:{serverStatus.isStarted ? " запущен" : " остановлен"}</span>
            {serverType > 1 && serverStatus.isStarted && <p style={{ margin: '0' }}>Адрес: {serverStatus.address}</p>}
            <span className="material-symbols-outlined" style={{ fontSize: '24px' }} onClick={(e) => {
                e.stopPropagation();
                onRemove();
                }}>close</span>
        </div>
    );
}

export default PanelItem;