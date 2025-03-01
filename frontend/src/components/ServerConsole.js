import styles from "../styles/MinecraftConsole.module.css";

const ServerConsole = () => {
    return (
        <div className={styles["console-container"]}>
            <textarea className={styles["console-output"]} readOnly placeholder={styles["Console output..."]} />
            <div className={styles["console-input-container"]}>
                <input type="text" className={styles["console-input"]} placeholder="Enter command..." />
                <button className={styles["console-button"]}>Отправить</button>
            </div>
            <div className={styles["additional-features-container"]}>
                <button className={styles["start-button"]}>Запустить сервер</button>
                <h4>Адрес сервера:</h4>
            </div>
        </div>
    );
}

export default ServerConsole;