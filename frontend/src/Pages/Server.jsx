import React, { useState, useEffect, useRef } from 'react';
import { useParams } from 'react-router-dom';
import styles from '../styles/ServerPage.module.css';
import ServerConsole from '../components/ServerConsole';
import TabButtons from '../components/TabButtons';
import MinecraftSettings from '../components/Settings/MinecraftSettings';
import Files from '../components/Files';
import { getServerTypeNumber } from '../services/api';
import Mods from '../components/Mods';
import TerrariaSettings from '../components/Settings/TerrariaSettings';
import DstSettings from '../components/Settings/DstSettings';

const Server = () => {
    const [selectedButton, setSelected] = useState('consoleBtnH');
    const { serverTypeName } = useParams();

    const settingsRef = useRef(null);

    let serverType = getServerTypeNumber(serverTypeName);

    useEffect(() => { document.getElementById(selectedButton).style.opacity = 1; }, []);

    return (
        <div className={[styles["background"], styles[serverTypeName]].join(' ')}>
            <div className={styles.wrapper}>
                <TabButtons selectedButton={selectedButton} setSelected={setSelected} reference={settingsRef} serverType={serverType} />
                <div className={styles["content"]}>
                    {selectedButton === 'consoleBtnH' && <ServerConsole serverType={serverType} />}
                    {selectedButton === 'filesBtnH' && <Files settingsRef={settingsRef} serverTypeName={serverTypeName} />}
                    {selectedButton === 'modsBtnH' && <Mods serverType={serverType} />}
                    {selectedButton === 'settingsBtnH' && (
                        serverType === 2 && <MinecraftSettings /> ||
                        serverType === 3 && <TerrariaSettings /> ||
                        serverType === 0 && <DstSettings />)}
                </div>
            </div>
        </div>
    );
};

export default Server;