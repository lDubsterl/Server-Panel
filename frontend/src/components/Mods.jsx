import { useState, useEffect } from "react";
import ApiConfig from "../services/api";
import styles from "../styles/ModList.module.css";

const Mods = ({ serverType }) => {
    const [loadingMessage, setMessage] = useState("Загрузка");
    const [modList, setModList] = useState([]);
    const [editedModList, setEditedModList] = useState([]);
    const [isAnyModified, setIsAnyModified] = useState(false);
    const [newMods, setNewMods] = useState([]);
    const [refreshTrigger, setRefreshTrigger] = useState(false);

    useEffect(() => {
        ApiConfig.api.post(ApiConfig.genericServerController + `/GetSetModList`, {
            ServerType: serverType,
            Modlist: null
        })
            .then((res) => {
                setMessage(null);
                setModList(res.data);
                setEditedModList(res.data);
            })
            .catch((error) => {
                if (error.response.status == 404)
                    setMessage("Просмотр модов будет доступен после первого перезапуска сервера");
            })
    }, [refreshTrigger]);

    useEffect(() => {
        if (!editedModList || !modList) return;

        const oldFull = JSON.stringify(modList);
        const editedFull = JSON.stringify(editedModList.concat(newMods));
        setIsAnyModified(oldFull !== editedFull);
    }, [editedModList, newMods]);

    const updateMods = () => {
        ApiConfig.api.post(ApiConfig.genericServerController + `/GetSetModList`, {
            ServerType: serverType,
            Modlist: editedModList.concat(newMods)
        })
            .then(() => setRefreshTrigger(!refreshTrigger));
    };

    if (loadingMessage)
        return loadingMessage;

    return (<>
        <div className={styles["list-container"]}>
            {editedModList.map((item, index) => {
                return (
                    <div key={index} className={styles["mod-item"]}>
                        <label>{item.modName}</label>
                        <div style={{ display: 'flex' }}>
                            <label className={styles["toggle-switch"]}>
                                <input
                                    type="checkbox"
                                    checked={item.status}
                                    onChange={() => {
                                        const updatedList = [...editedModList];
                                        updatedList[index] = {
                                            ...item,
                                            status: !item.status
                                        };
                                        setEditedModList(updatedList);
                                    }}
                                />
                                <span className={styles["slider"]}></span>
                            </label>
                            <span className="material-symbols-outlined" style={{ cursor: 'pointer' }} onClick={() => {
                                const updated = editedModList.filter((_, i) => i !== index);
                                setEditedModList(updated);
                            }}>close</span>
                        </div>
                    </div>
                );
            })}
            {newMods.map((mod, index) => {
                return (
                    <div key={index} className={styles["mod-item"]}>
                        <input value={mod.modName} type='number' style={{ flex: 1 }}
                            onChange={(e) => {
                                const updated = [...newMods];
                                updated[index].modName = e.target.value;
                                setNewMods(updated);
                            }} />
                        <span className="material-symbols-outlined" style={{ cursor: 'pointer' }} onClick={() => {
                            const updated = newMods.filter((_, i) => i !== index);
                            setNewMods(updated);
                        }}>close</span>
                    </div>
                );
            })}
        </div>
        <div className={styles["edit-actions"]}>
            <button onClick={() => setNewMods([...newMods, { modName: "", status: false }])}>Добавить мод</button>
            <button onClick={updateMods} disabled={!isAnyModified}>Сохранить изменения</button>
        </div>
    </>);
};

export default Mods;