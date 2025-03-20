import React, { useState, useEffect, useRef } from 'react';
import styles from '../styles/ServerPage.module.css';
import ServerConsole from '../components/ServerConsole';
import TabButtons from '../components/TabButtons';
import MinecraftSettings from '../components/MinecraftSettings';
import Files from '../components/Files';

const MinecraftServer = () => {
    const [selectedButton, setSelected] = useState('consoleBtnH');

    useEffect(() => { document.getElementById(selectedButton).style.opacity = 1; }, []);
    const settingsRef = useRef(null);

    return (
        <div className={styles["background"]}>
            <div className={styles.wrapper}>
                <TabButtons selectedButton={selectedButton} setSelected={setSelected} ref={settingsRef}/>
                <div className={styles["content"]}>
                    {selectedButton === 'consoleBtnH' && <ServerConsole />}
                    {selectedButton === 'filesBtnH' && <Files settingsRef={settingsRef} serverType={'Minecraft'}/>}
                    {selectedButton === 'settingsBtnH' && <MinecraftSettings />}
                </div>
            </div>
        </div>
    );
};

export default MinecraftServer;