import { useState, useEffect } from "react";
import { parseTranslations, parseTerrariaConfig, stringifyTerrariaConfig } from "../Functions";
import ApiConfig from "../../services/api";
import styles from "../../styles/Settings.module.css";

const TerrariaSettings = () => {
    const [configFields, setConfigFields] = useState([]);
    const [oldConfigFields, setOldConfigFields] = useState([]);
    const [translation, setTranslation] = useState([]);
    const [isAnyModified, setIsAnyModified] = useState(false);

    useEffect(() => {
        ApiConfig.api.get(`${ApiConfig.genericServerController}/GetSettings?type=3`)
            .then((res) => {
                const parsed = parseTerrariaConfig(res.data);
                setConfigFields(parsed);
                setOldConfigFields(parsed);
                parseTranslations("terraria_localization.txt", setTranslation);
            });
    }, []);

    useEffect(() => {
        if (!oldConfigFields || !configFields) return;

        const oldFull = JSON.stringify(oldConfigFields);
        const editedFull = JSON.stringify(configFields);
        setIsAnyModified(oldFull !== editedFull);
    }, [configFields]);

    const handleConfigChange = (key, newValue) => {
        const updated = configFields.map(item => {
            if (item.type === 'entry' && item.key === key) {
                return { ...item, default: newValue };
            }
            return item;
        });
        setConfigFields(updated);
    };

    const updateConfig = () => {
        ApiConfig.api.get(`${ApiConfig.genericServerController}/UpdateFile`, {
            Path: 'Terraria/serverconfig.txt',
            Content: stringifyTerrariaConfig(configFields)
        })
        .then(() => setOldConfigFields(configFields));
    }

    return (
        <div style={{
            position: 'relative',
            display: 'flex', flexDirection: 'column', gap: '5px'
        }}>
            {configFields.map((item, idx) => {
                if (item.type !== 'entry' || item.hidden) return null;

                return (
                    <div key={idx} className={styles["parameter-container"]}>
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
            <button className={styles.saveBtn} onClick={updateConfig} disabled={!isAnyModified}>Сохранить изменения</button>
        </div>);
};

export default TerrariaSettings;