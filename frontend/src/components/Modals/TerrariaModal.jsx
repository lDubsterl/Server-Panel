import React, { useEffect, useState } from "react";
import ApiConfig from "../../services/api";
import styles from "../../styles/Modal.module.css";
import {parseTranslations, stringifyTerrariaConfig, parseTerrariaConfig} from "../Functions";

const TerrariaModal = ({ onClose, onUpdate }) => {
    const [serverName, setServerName] = useState("");
    const [serverDescription, setServerDescription] = useState("");
    const [serverPassword, setServerPassword] = useState("");
    const [modlist, setModlist] = useState([]);
    const [modlistModalOpen, setModlistModalOpen] = useState(false);
    const [showAdvanced, setShowAdvanced] = useState(false);
    const [configFields, setConfigFields] = useState([]);
    const [translation, setTranslation] = useState([]);

    useEffect(() => {
        ApiConfig.api.get(`${ApiConfig.genericServerController}/GetInitialConfig?serverType=3`)
            .then((res) => {
                const parsed = parseTerrariaConfig(res.data);
                setConfigFields(parsed);
                parseTranslations("terraria_localization.txt", setTranslation);
            });
    }, []);

    const handleConfigChange = (key, newValue) => {
        const updated = configFields.map(item => {
            if (item.type === 'entry' && item.key === key) {
                return { ...item, default: newValue };
            }
            return item;
        });
        setConfigFields(updated);
    };

    const closeModal = () => {
        if (modlistModalOpen)
            setModlistModalOpen(false);
        else onClose();
    }

    const createServer = () => {
        let pathIndex = configFields.findIndex(item => item.key == 'world');
        let path = configFields[pathIndex].default;
        configFields[pathIndex].default = path.replace('Terraria.wld', serverName + '.wld');

        let nameIndex = configFields.findIndex(item => item.key == 'worldname');
        let name = configFields[nameIndex].default;
        configFields[nameIndex].default = serverName;

        let createRequest = {
            ServerName: serverName,
            ServerDescription: serverDescription,
            ServerPassword: serverPassword,
            Config: stringifyTerrariaConfig(configFields),
            Modlist: modlist
        };
        ApiConfig.api.put(ApiConfig.genericServerController + `/3`, createRequest)
            .then(() => {
                onClose();
                onUpdate();
            });
    };
    return (
        <div className={styles["modal-overlay"]}>
            <div className={styles["modal-content"]}>
                <form onSubmit={createServer}>
                {!modlistModalOpen ? <>
                    <h2>Настройки сервера</h2>
                    <label className={styles["label-item"]}>
                        Название сервера:
                        <input type="text" placeholder="Название сервера" value={serverName}
                            onChange={(e) => setServerName(e.target.value)} />
                    </label>
                    <label className={styles["label-item"]}>
                        Описание сервера:
                        <input type="text" autoComplete="false" placeholder="Описание сервера" value={serverDescription}
                            onChange={(e) => setServerDescription(e.target.value)} />
                    </label>
                    <label className={styles["label-item"]}>
                        Пароль сервера:
                        <input type="text" autoComplete="false" placeholder="Пароль сервера" value={serverPassword}
                            onChange={(e) => setServerPassword(e.target.value)} />
                    </label>
                    <div>
                        <button type="button" className={styles["additional-content-button"]}
                            onClick={() => setShowAdvanced(!showAdvanced)}>
                            <span className="material-symbols-outlined">
                                {showAdvanced ? 'keyboard_arrow_up' : 'keyboard_arrow_down'}
                            </span>
                            {showAdvanced ? 'Скрыть дополнительные настройки' : 'Показать дополнительные настройки'}
                        </button>

                        {showAdvanced && (
                            <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                                <div className={styles["label-item"]}>
                                    <label>Список модов</label>
                                    <button type="button" onClick={() => setModlistModalOpen(true)}>
                                        Редактировать
                                    </button>
                                </div>
                                <div style={{
                                    position: 'relative',
                                    maxHeight: '200px', display: 'flex', flexDirection: 'column', gap: '5px'
                                }}>
                                    {configFields.map((item, idx) => {
                                        if (item.type !== 'entry' || item.hidden) return null;

                                        return (
                                            <div key={idx} style={{
                                                display: 'grid',
                                                gridTemplateColumns: '1fr 1fr',
                                                alignItems: 'center',
                                                justifyItems: 'start'
                                            }}>
                                                <label>{translation[item.key] || item.key}</label>
                                                {item.options ? (
                                                    <select
                                                        value={item.default}
                                                        onChange={(e) => handleConfigChange(item.key, e.target.value)}
                                                    >
                                                        {item.options.map(opt => (
                                                            <option key={opt.value} value={opt.value}>
                                                                {opt.label}
                                                            </option>
                                                        ))}
                                                    </select>
                                                ) : item.key === 'maxplayers' ? (
                                                    <input
                                                        type="number"
                                                        min={1}
                                                        max={255}
                                                        value={item.default}
                                                        onChange={(e) => handleConfigChange(item.key, e.target.value)}
                                                        onBlur={(e) => {
                                                            let val = parseInt(e.target.value, 10);
                                                            if (val < 1) val = 1;
                                                            if (val > 255) val = 255;
                                                            handleConfigChange(item.key, val.toString());
                                                        }}
                                                    />
                                                ) : (
                                                    <input
                                                        type="text"
                                                        value={item.default}
                                                        onChange={(e) => handleConfigChange(item.key, e.target.value)}
                                                    />
                                                )}
                                            </div>

                                        );
                                    })}
                                </div>
                            </div>
                        )}
                    </div></> : <>
                    <h2 style={{ margin: 0 }}>Список модов</h2>
                    <h4 style={{ margin: 0 }}> Введите id мода (число в ссылке на мод из steam workshop) для добавления мода</h4>
                    <div style={{ display: 'flex', flexDirection: 'column' }}>
                        {modlist.map((mod, index) => (
                            <div className={styles["modlist-item"]} key={index}>
                                <input value={mod} type='number' style={{ flex: 1 }}
                                    onChange={(e) => {
                                        const updated = [...modlist];
                                        updated[index] = e.target.value;
                                        setModlist(updated);
                                    }} />
                                <span className="material-symbols-outlined" style={{ cursor: 'pointer' }} onClick={() => {
                                    const updated = modlist.filter((_, i) => i !== index);
                                    setModlist(updated);
                                }}>close</span>
                            </div>
                        ))}
                        <button type="button" className="material-symbols-outlined" onClick={() => setModlist([...modlist, ""])}>
                            add
                        </button>
                    </div></>}
                <div className={styles["modal-actions"]}>
                    <button type="submit">Сохранить</button>
                    <button type="button" onClick={closeModal}>Закрыть</button>
                </div>
                </form>
            </div>
        </div>
    );
}

export default TerrariaModal;