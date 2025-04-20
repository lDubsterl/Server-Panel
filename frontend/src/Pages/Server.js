import React, { useState, useEffect, useRef } from 'react';
import { useParams } from 'react-router-dom';
import styles from '../styles/ServerPage.module.css';
import ServerConsole from '../components/ServerConsole';
import TabButtons from '../components/TabButtons';
import MinecraftSettings from '../components/MinecraftSettings';
import Files from '../components/Files';
import {getServerTypeNumber} from '../services/api';

const Server = () => {
    const [selectedButton, setSelected] = useState('consoleBtnH');
    const {serverTypeName} = useParams();
    
    const settingsRef = useRef(null);
    
    let serverType = getServerTypeNumber(serverTypeName);

    useEffect(() => { document.getElementById(selectedButton).style.opacity = 1; }, []);

    return (
        <div className={styles["background"]}>
            <div className={styles.wrapper}>
                <TabButtons selectedButton={selectedButton} setSelected={setSelected} ref={settingsRef}/>
                <div className={styles["content"]}>
                    {selectedButton === 'consoleBtnH' && <ServerConsole serverType={serverType}/>}
                    {selectedButton === 'filesBtnH' && <Files settingsRef={settingsRef} serverType={serverType}/>}
                    {selectedButton === 'settingsBtnH' && <MinecraftSettings />}
                </div>
            </div>
        </div>
    );
};

export default Server;