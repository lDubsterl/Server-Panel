import { useState, useEffect } from "react";
import ApiConfig from "../../services/api";
import styles from "../../styles/Modal.module.css";


const MinecraftModal = ({ onClose, onUpdate }) => {
    const [serverVersion, setServerVersion] = useState("");
    const [versionsList, setVersionsList] = useState([]);

    useEffect(() => {
        ApiConfig.api.get(`${ApiConfig.genericServerController}/GetInitialConfig?serverType=2`)
            .then((res) => {
                setVersionsList(res.data);
                if (res.data.length > 0) {
                    setServerVersion(res.data[0]);
                }
            });
    }, []);

    const createServer = () => {

        ApiConfig.api.put(ApiConfig.genericServerController + `/2?serverVersion=${serverVersion}.jar`)
            .then(() => {
                onClose();
                onUpdate();
            });
    };

    return (
        <div className={styles["modal-overlay"]}>
            <div className={styles["modal-content"]}>
                <form onSubmit={createServer}>
                    <div className={styles["label-item"]} style={{paddingTop: '10px'}}>
                        <label>Версия сервера</label>
                        <select onChange={(e) => setServerVersion(e.target.value)}
                            value={serverVersion}>
                            {
                                versionsList.map((version) => {
                                    return (<option key={version} value={version}>{version}</option>);
                                })
                            }
                        </select>
                    </div>
                    <div className={styles["modal-actions"]}>
                        <button type="submit">Сохранить</button>
                        <button type="button" onClick={onClose}>Закрыть</button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default MinecraftModal;