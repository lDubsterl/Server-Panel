import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import styles from '../styles/MinecraftConsole.module.css';
const MinecraftServer = () => {
    return (
        <div className={styles["console-container"]}>
            <textarea className={styles["console-output"]} readOnly placeholder={styles["Console output..."]} />
            <div className={styles["console-input-container"]}>
                <input type="text" className={styles["console-input"]} placeholder="Enter command..." />
                <button className={styles["console-button"]}>Send</button>
            </div>
        </div>
    );
};

export default MinecraftServer;