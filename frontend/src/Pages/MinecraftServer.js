import React, { useState, useEffect } from 'react';
import styles from '../styles/ServerPage.module.css';
import ServerConsole from '../components/ServerConsole';
import TabButtons from '../components/TabButtons';
import MinecraftSettings from '../components/MinecraftSettings';

const MinecraftServer = () => {
    const [selectedButton, setSelected] = useState('consoleBtnH');

    useEffect(() => { document.getElementById(selectedButton).style.opacity = 1; }, []);

    return (
        <div className={styles["background"]}>
            <div className={styles.wrapper}>
                <TabButtons selectedButton={selectedButton} setSelected={setSelected} />
                <div className={styles["content"]}>
                    {selectedButton === 'consoleBtnH' && <ServerConsole />}
                    {selectedButton === 'settingsBtnH' && <MinecraftSettings />}
                </div>
            </div>
        </div>
    );
};

export default MinecraftServer;