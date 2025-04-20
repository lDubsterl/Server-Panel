import React, { useEffect, useState } from "react";
import ApiConfig from "../../services/api";
import styles from "../../styles/Modal.module.css";

function parseTerrariaConfig(configText) {
    const lines = configText.split('\n');
    const parsed = [];
    let pendingOptions = null;
    let skipNextEntry = false;

    for (let line of lines) {
        const trimmed = line.trim();

        if (trimmed === '') {
            parsed.push({ type: 'raw', content: line });
            continue;
        }

        if (trimmed.startsWith('##')) {
            if (trimmed === '## off') {
                skipNextEntry = true;
                continue;
            }

            // parse options
            const comment = trimmed.slice(2).trim(); // remove "##"
            pendingOptions = comment.split('/').map(opt => {
                const optMatch = opt.match(/^([^\(]+)\(([^)]+)\)/);
                return optMatch
                    ? { value: optMatch[1].trim(), label: optMatch[2].trim() }
                    : { value: opt.trim(), label: opt.trim() };
            });

            continue;
        }

        if (trimmed.startsWith('#')) {
            parsed.push({ type: 'comment', content: line });
            continue;
        }

        const match = line.match(/^([^=]+)=([^\s#]*)(?:\s*#\s*(.+))?$/);
        if (match) {
            const key = match[1].trim();
            const value = match[2].trim();
            const comment = match[3]?.trim();

            const entry = {
                type: 'entry',
                key,
                default: value,
                comment,
                options: pendingOptions || null,
                hidden: skipNextEntry
            };

            parsed.push(entry);
            pendingOptions = null;
            skipNextEntry = false;
        } else {
            parsed.push({ type: 'raw', content: line });
        }
    }
    console.log(parsed);
    return parsed;
}

function stringifyTerrariaConfig(parsed) {
    const lines = [];

    for (const entry of parsed) {
        if (entry.type === 'comment' || entry.type === 'raw') {
            lines.push(entry.content);
        } else if (entry.type === 'entry') {
            if (entry.hidden) {
                lines.push('## off');
            }

            if (entry.options) {
                const optionLine = '## ' + entry.options
                    .map(opt => `${opt.value}(${opt.label})`)
                    .join('/');
                lines.push(optionLine);
            }

            let line = `${entry.key}=${entry.default}`;
            if (!entry.options && entry.comment) {
                line += ` # ${entry.comment}`;
            }

            lines.push(line);
        }
    }

    return lines;
}


const TerrariaModal = ({ onClose, onUpdate }) => {
    const [serverName, setServerName] = useState("");
    const [serverDescription, setServerDescription] = useState("");
    const [serverPassword, setServerPassword] = useState("");
    const [modlist, setModlist] = useState([]);
    const [modlistModalOpen, setModlistModalOpen] = useState(false);
    const [showAdvanced, setShowAdvanced] = useState(false);
    const [configFields, setConfigFields] = useState([]);

    useEffect(() => {
        ApiConfig.api.get(`${ApiConfig.genericServerController}/GetInitialConfig?serverType=3`)
            .then((res) => {
                const parsed = parseTerrariaConfig(res.data);
                setConfigFields(parsed);
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
                <form>
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
                                    position: 'relative', overflow: 'scroll',
                                    maxHeight: '200px', display: 'flex', flexDirection: 'column', gap: '5px'
                                }}>
                                    {configFields.map((item, idx) => {
                                        if (item.type !== 'entry' || item.hidden) return null;

                                        return (
                                            <div key={idx} style={{
                                                display: 'grid',
                                                gridTemplateColumns: '1fr 1.5fr',
                                                alignItems: 'center',
                                                justifyItems: 'start'
                                            }}>
                                                <label>{
                                                    (item.key == 'autocreate' && 'world size') ||
                                                    (item.key == 'secure' && 'cheat protection') ||
                                                    item.key}</label>
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
                    <button type="submit" onClick={createServer}>Сохранить</button>
                    <button type="button" onClick={closeModal}>Закрыть</button>
                </div>
                </form>
            </div>
        </div>
    );
}

export default TerrariaModal;