import {React, useState} from 'react';
import styles from '../styles/MinecraftSettings.module.css'

const GroupedInput = ({ name, inputType, oldValue }) => {
    return (
        <div className={styles["parameter-container"]}>
            <div className={styles["input-container"]}>
                <label>
                    {name}
                </label>
                {
                    inputType != "slider" ? <input type={inputType}></input> :
                        <CheckboxSlider />
                }
            </div>
            <span style = {{backgroundColor: '#2d3943', color: '#aaa'}}>
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
        <div className = {styles["parameters-container"]}>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест2"} inputType={"slider"} oldValue={"кек=4"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
            <GroupedInput name = {"Тест"} inputType={"text"} oldValue={"кек=3"}/>
        </div>
    );
}

export default MinecraftSettings;