import { React, useState } from 'react';
import styles from '../styles/MinecraftSettings.module.css'

const GroupedInput = ({ name, inputType, oldValue, style }) => {
    return (
        <div className={[styles["parameter-container"], style].join(' ')}>
            <label style={{ paddingLeft: '5px', gridRow: '1', gridColumn: '1' }}>
                {name}
            </label>
            {
                inputType != "slider" ? <input type={inputType}
                    style={{ gridRow: 'span 2', gridColumn: '2', alignSelf: 'center', justifySelf: 'end' }}></input> :
                    <CheckboxSlider />
            }
            <span style={{ fontSize: '12px', color: '#aaa', paddingLeft: '5px', gridRow: '2', gridColumn: '1 / 2' }}>
                {oldValue}
            </span>
        </div>
    );
}

const CheckboxSlider = () => {
    const [isOn, setIsOn] = useState(false);

    return (
        <label className={styles["toggle-switch"]}>
            <input
                type="checkbox"
                checked={isOn}
                onChange={() => setIsOn(!isOn)}
            />
            <span className={styles["slider"]}></span>
        </label>
    );
}

const MinecraftSettings = () => {
    return (
        <div className={styles["parameters-container"]}>
            <GroupedInput name={"Полёт"} inputType={"slider"} oldValue={"кек=3"} />
            <GroupedInput name={"Незер"} inputType={"slider"} oldValue={"кек=4"} />
            <GroupedInput name={"Транслировать консоль операторам"} inputType={"slider"} oldValue={"кек=3"} />
            <GroupedInput name={"Сложность"} inputType={"text"} oldValue={"кек=3"} />
            <GroupedInput name={"Командные блоки"} inputType={"slider"} oldValue={"кек=3"} />
            <GroupedInput name={"Онлайн статус"} inputType={"slider"} oldValue={"кек=3"} />
            <GroupedInput name={"Принудительная защита профиля"} inputType={"slider"} oldValue={"кек=3"} />
            <GroupedInput name={"Принудительный белый список"} inputType={"slider"} oldValue={"кек=3"} />
            <GroupedInput name={"Сбрасывать режим игры"} inputType={"slider"} oldValue={"кек=3"} />
            <GroupedInput name={"Режим игры"} inputType={"text"} oldValue={"кек=3"} />
            <GroupedInput name={"Генерация структур"} inputType={"slider"} oldValue={"кек=3"} />
            <GroupedInput name={"Настройки генератора"} inputType={"text"} oldValue={"кек=3"} />
            <GroupedInput name={"Хардкор"} inputType={"slider"} oldValue={"кек=3"} />
            <GroupedInput name={"Скрыть онлайн игроков"} inputType={"slider"} oldValue={"кек=3"} />
            <GroupedInput name={"Имя мира"} inputType={"text"} oldValue={"кек=3"} />
            <GroupedInput name={"Семя мира"} inputType={"text"} oldValue={"кек=3"} />
            <GroupedInput name={"Тип мира"} inputType={"text"} oldValue={"кек=3"} />
            <GroupedInput name={"Максимум игроков"} inputType={"text"} oldValue={"кек=3"} style={styles.b} />
            <GroupedInput name={"Максимальное количество возобновлений в цепочке"} inputType={"text"} oldValue={"кек=3"} style={styles.b} />
            <GroupedInput name={"Уровень разрешений функций"} inputType={"text"} oldValue={"кек=3"} style={styles.b} />
            <GroupedInput name={"Диапазон трансляции сущностей"} inputType={"text"} oldValue={"кек=3"} style={styles.b} />
        </div>
    );
}

export default MinecraftSettings;