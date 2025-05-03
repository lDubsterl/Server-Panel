import { useState, useEffect } from "react";
import { parseTranslations, parseWorldgenConfig, stringifyWorldgenConfig } from "../Functions";
import ApiConfig from "../../services/api";
import styles from "../../styles/Settings.module.css";

const DstSettings = () => {
    const [configFields, setConfigFields] = useState([]);
    const [oldConfigFields, setOldConfigFields] = useState([]);
    const [translation, setTranslation] = useState([]);
    const [isAnyModified, setIsAnyModified] = useState(false);

    useEffect(() => {
        ApiConfig.api.get(`${ApiConfig.genericServerController}/GetSettings?type=0`)
            .then((res) => {
                const parsed = parseWorldgenConfig(res.data);
                setConfigFields(parsed);
                setOldConfigFields(parsed);
                parseTranslations("dst_localization.txt", setTranslation);
            });
    }, []);

    useEffect(() => {
        if (!oldConfigFields || !configFields) return;

        const oldFull = JSON.stringify(oldConfigFields);
        const editedFull = JSON.stringify(configFields);
        setIsAnyModified(oldFull !== editedFull);
    }, [configFields, oldConfigFields]);

    const handleWorldgenChange = (key, value) => {
        setConfigFields((prev) => ({
            ...prev,
            [key]: [value, prev[key][1]],
        }));
    };

    const updateConfig = () => {
        ApiConfig.api.patch(`${ApiConfig.genericServerController}/UpdateFile`, {
            Path: 'DoNotStarveTogether/DST/Master/worldgenoverride.lua',
            Content: stringifyWorldgenConfig(configFields)
        })
            .then(() => setOldConfigFields(configFields));
    }

    return (
        <div style={{
            position: 'relative', height: '76vh',
            display: 'flex', flexDirection: 'column', gap: '5px'
        }}>
            <div style={{overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: '5px', zIndex: '3'}}>
                {Object.entries(configFields).map(([key, [currentValue, options]]) => (
                    <div key={key} className={styles["parameter-container"]}>
                        <label htmlFor={key}>
                            {translation[key] || key}
                        </label>
                        <select id={key} name={key} value={currentValue}
                            onChange={(e) => handleWorldgenChange(key, e.target.value)}>
                            {options.map((option) => (
                                <option key={option} value={option}>
                                    {option}
                                </option>
                            ))}
                        </select>
                    </div>
                ))}
            </div>
            <button className={styles.saveBtn} onClick={updateConfig} disabled={!isAnyModified}>Сохранить изменения</button>
        </div>);
};

export default DstSettings;