import React, { useState } from 'react';
import styles from "../styles/TabButtons.module.css";

const TabButtons = ({selectedButton, setSelected}) => {
    const [hoveredButton, setHovered] = useState(null);

    const onHovered = (event) => {
        let actualId = event.target.id.replace(/H$/, '');
        if (selectedButton != event.target.id)
            document.getElementById(actualId)
                .classList
                .add(styles["hovered"]);
        setHovered(actualId);

    };
    const onHoverExit = () => {
        if (hoveredButton)
            document.getElementById(hoveredButton)
                .classList
                .remove(styles["hovered"]);
        setHovered(null);
    };

    const onSelected = (event) => {
        document.getElementById(selectedButton).style.opacity = 0;
        document.getElementById(event.target.id).style.opacity = 1;

        document.getElementById(event.target.id.replace(/H$/, ''))
            .classList.remove(styles["hovered"]);

        setSelected(event.target.id);
    }

    return (
        <>
            <div className={styles["button-container-hidden"]}>
                <button
                    id="consoleBtnH"
                    className={styles["hover-button-hidden"]}
                    onMouseEnter={onHovered}
                    onMouseLeave={onHoverExit}
                    onClick={onSelected}
                >Консоль</button>
                <button
                    id="filesBtnH"
                    className={styles["hover-button-hidden"]}
                    onMouseEnter={onHovered}
                    onMouseLeave={onHoverExit}
                    onClick={onSelected}
                >Файлы</button>
                <button
                    id="settingsBtnH"
                    className={styles["hover-button-hidden"]}
                    onMouseEnter={onHovered}
                    onMouseLeave={onHoverExit}
                    onClick={onSelected}
                >Настройки</button>
            </div>
            <div className={styles["button-container"]}>
                <button
                    id="consoleBtn"
                    className={styles["hover-button"]}
                >Консоль</button>
                <button
                    id="filesBtn"
                    className={styles["hover-button"]}
                >Файлы</button>
                <button
                    id="settingsBtn"
                    className={styles["hover-button"]}
                >Настройки</button>
            </div>
        </>
    );
};

export default TabButtons;