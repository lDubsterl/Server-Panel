import { React, useState, useEffect } from 'react';
import styles from '../../styles/MinecraftSettings.module.css';
import ApiConfig from '../../services/api';

const GroupedInput = ({ name, inputType, listKey, values, style, onChange }) => {
    return (
        <div className={[styles["parameter-container"], style].join(' ')}>
            <label style={{ paddingLeft: '5px', gridRow: '1', gridColumn: '1' }}>
                {name}
            </label>
            {inputType == "slider" && <CheckboxSlider boolKey={listKey} values={values} onChange={onChange} />}
            {inputType == "text" && <TextInput textKey={listKey} values={values} onChange={onChange} />}
            <span style={{ fontSize: '12px', color: '#aaa', paddingLeft: '5px', gridRow: '2', gridColumn: '1 / 2', fontWeight: 'bold' }}>
                {listKey}={(values && values[listKey]) || ""}
            </span>
        </div>
    );
}

const CheckboxSlider = ({ boolKey, values, onChange }) => {
    const [isOn, setIsOn] = useState((values && values[boolKey] === 'true') || false);

    return (
        <label className={styles["toggle-switch"]}>
            <input
                type="checkbox"
                checked={isOn}
                onChange={() => {
                    setIsOn(!isOn);
                    onChange(boolKey, (!isOn).toString());
                }}
            />
            <span className={styles["slider"]}></span>
        </label>
    );
}

const TextInput = ({ textKey, values, onChange }) => {
    return (
        <input type="text"
            style={{ gridRow: 'span 2', gridColumn: '2', alignSelf: 'center', justifySelf: 'end', width: '100%' }}
            onBlur={(e) => onChange(textKey, e.target.value)} defaultValue={(values && values[textKey]) || ""}></input>
    );
}

const formatSettings = (settingsObj) => {
    return Object.entries(settingsObj).map(([listKey, value]) => `${listKey}=${value}`);
};

const parseSettings = (settingsArray) => {
    return settingsArray.reduce((acc, line) => {
        const [listKey, value] = line.split("=");
        acc[listKey] = value;
        return acc;
    }, {});
};

const MinecraftSettings = () => {
    const [oldValues, setOldValues] = useState(null);
    const [newValues, setNewValues] = useState(null);
    const [isAnyModified, setIsAnyModified] = useState(false);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        ApiConfig.api.get(`${ApiConfig.genericServerController}/GetSettings`, {
            params: { type: "Minecraft" }
        })
            .then(response => {
                setNewValues(parseSettings(response.data));
                setOldValues(parseSettings(response.data));
                setLoading(false);
            })
    }, []);

    useEffect(() => {
        if (!oldValues || !newValues) return;
    
        setIsAnyModified(
            Object.keys(oldValues).some((key) => newValues[key] !== oldValues[key])
        );
    }, [newValues]);

    const handleChange = (listKey, newValue) => {
        setNewValues((prev) => ({ ...prev, [listKey]: newValue }));
    };

    const save = () => {
        ApiConfig.api.patch(`${ApiConfig.minecraftController}/UpdateFile`, {path: 'Minecraft/server.properties', content: formatSettings(newValues)})
            .then(() => {
                console.log("save complete");
                setOldValues(newValues);
                setIsAnyModified(false);
            });
    }
    if (loading) return(
        <div className={styles["parameters-container"]}>Загрузка</div>
    );
    return (
        <>
            <div className={styles["parameters-container"]}>
                <GroupedInput name={"Имя мира"} inputType={"text"}
                    listKey={"level-name"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Семя мира"} inputType={"text"}
                    listKey={"level-seed"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Тип мира"} inputType={"text"}
                    listKey={"level-type"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Максимум игроков"} inputType={"text"}
                    listKey={"max-players"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Режим игры"} inputType={"text"}
                    listKey={"gamemode"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Сбрасывать режим игры"} inputType={"slider"}
                    listKey={"force-gamemode"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Сложность"} inputType={"text"}
                    listKey={"difficulty"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Хардкор"} inputType={"slider"}
                    listKey={"hardcore"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Генерация структур"} inputType={"slider"}
                    listKey={"generate-structures"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Полёт"} inputType={"slider"}
                    listKey={"allow-flight"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Незер"} inputType={"slider"}
                    listKey={"allow-nether"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"PVP"} inputType={"slider"}
                    listKey={"pvp"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Животные"} inputType={"slider"}
                    listKey={"spawn-animals"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Монстры"} inputType={"slider"}
                    listKey={"spawn-monsters"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Жители"} inputType={"slider"}
                    listKey={"spawn-npcs"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Командные блоки"} inputType={"slider"}
                    listKey={"enable-command-block"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Защита спауна"} inputType={"text"}
                    listKey={"spawn-protection"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Пиратский"} inputType={"slider"}
                    listKey={"online-mode"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Дальность симуляции"} inputType={"text"}
                    listKey={"simulation-distance"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Дальность прорисовки"} inputType={"text"}
                    listKey={"view-distance"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Белый список"} inputType={"slider"}
                    listKey={"white-list"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Принудительный белый список"} inputType={"slider"}
                    listKey={"enforce-whitelist"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Уровень разрешений функций"} inputType={"text"}
                    listKey={"function-permission-level"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Уровень разрешений оператора"} inputType={"text"}
                    listKey={"op-permission-level"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Принудительная защита профиля"} inputType={"slider"}
                    listKey={"enforce-secure-profile"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Онлайн статус"} inputType={"slider"}
                    listKey={"enable-status"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Скрыть онлайн игроков"} inputType={"slider"}
                    listKey={"hide-online-players"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Обязательный ресурс пак"} inputType={"slider"}
                    listKey={"require-resource-pack"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Транслировать консоль операторам"} inputType={"slider"}
                    listKey={"broadcast-console-to-ops"} values={newValues} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Описание сервера"} inputType={"text"}
                    listKey={"motd"} values={newValues} style={styles.b} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Максимальный размер мира"} inputType={"text"}
                    listKey={"max-world-size"} values={newValues} style={styles.b} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Ресурс пак"} inputType={"text"}
                    listKey={"resource-pack"} values={newValues} style={styles.b} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Сообщение при необходимости ресурс пака"} inputType={"text"}
                    listKey={"resource-pack-promt"} values={newValues} style={styles.b} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"SHA-1 Ресурс пака"} inputType={"text"}
                    listKey={"resource-pack-sha1"} values={newValues} style={styles.b} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Максимальное количество возобновлений в цепочке"} inputType={"text"}
                    listKey={"max-chained-neighbor-updates"} values={newValues} style={styles.b} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Диапазон трансляции сущностей"} inputType={"text"}
                    listKey={"entity-broadcast-range-percentage"} values={newValues} style={styles.b} onChange={(listKey, val) => handleChange(listKey, val)} />
                <GroupedInput name={"Настройки генератора"} inputType={"text"}
                    listKey={"generator-settings"} values={newValues} style={styles.b} onChange={(listKey, val) => handleChange(listKey, val)} />
            </div>
            <div style={{ paddingTop: '10px' }}>
                <button className={styles["save-button"]} disabled={!isAnyModified} onClick={save}>Сохранить</button>
            </div>
        </>
    );
}

export default MinecraftSettings;