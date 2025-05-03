import React, { useState, useEffect } from 'react';
import styles from '../styles/ServerSelection.module.css';
import ApiConfig, { getServerTypeNumber } from '../services/api';
import PanelItem from '../components/PanelItem';
import DstModal from '../components/Modals/DstModal';
import TerrariaModal from '../components/Modals/TerrariaModal';
import SelectionModal from '../components/Modals/SelectionModal';
import MinecraftModal from '../components/Modals/MinecraftModal';

const ServerSelection = () => {
    const [serverList, setServers] = useState([false, false, false]);
    const [modalType, setModalType] = useState(null);
    const [selectionModalShowed, showSelectionModal] = useState(false);

    const apiServersUrl = (serverTypeNumber) => `${ApiConfig.genericServerController}` +
        `/${serverTypeNumber}?ServerType=${serverTypeNumber}`;

    const fetchServerList = () => {
        ApiConfig.api.get(`${ApiConfig.userController}/GetServers`)
            .then(response => {
                setServers([response.data.dst, response.data.minecraft, response.data.terraria]);
            });
    };

    useEffect(() => {
        fetchServerList();
    }, []);

    const RemoveServer = (serverTypeNumber) => {
        ApiConfig.api.delete(apiServersUrl(serverTypeNumber))
            .then(fetchServerList);
    };

    const AddServer = (type) => {
        setModalType(type);
        showSelectionModal(false);
    }

    const panels = [
        serverList[0] && <PanelItem key="0" serverType={0} name={"Don't starve together"}
            onRemove={() => RemoveServer(0, 0)} />,
        serverList[1] && <PanelItem key="2" serverType={2} name={"Minecraft"}
            onRemove={() => RemoveServer(2)} />,
        serverList[2] && <PanelItem key="3" serverType={3} name={"Terraria"}
            onRemove={() => RemoveServer(3)} />,
    ].filter(Boolean);

    return (
        <div className={styles["background"]}>
            <div className={styles.content}>
                {panels.length > 0 ? <>
                    {panels}
                    <span className="material-symbols-outlined"
                        style={{ fontSize: '36px' }} onClick={() => showSelectionModal(true)}>add</span>
                </> : <>
                    <p>На данный момент у вас нет серверов.
                        Выберите интересующий вас сервер для его создания:</p>
                    <button onClick={() => AddServer(2)}>Minecraft</button>
                    <button onClick={() => AddServer(0)}>Don't starve together</button>
                    <button onClick={() => AddServer(3)}>Terraria</button>
                </>}
            </div>
            {modalType === 0 && <DstModal onClose={() => setModalType(null)} onUpdate={fetchServerList} />}
            {modalType === 2 && <MinecraftModal onClose={() => setModalType(null)} onUpdate={fetchServerList} />}
            {modalType === 3 && <TerrariaModal onClose={() => setModalType(null)} onUpdate={fetchServerList} />}
            {selectionModalShowed && <SelectionModal AddServer={(type) => {AddServer(type)}} onClose={() => showSelectionModal(false)} serverStates={serverList} />}
        </div>
    );
}

export default ServerSelection;