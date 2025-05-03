import styles from "../../styles/Modal.module.css";

const SelectionModal = ({ onClose, AddServer, serverStates }) => {
    return (
        <div className={styles["modal-overlay"]}>
            <div className={styles["modal-content"]} style={{paddingTop: '30px'}}>
                {!serverStates[1] && <button onClick={() => AddServer(2)}>Minecraft</button>}
                {!serverStates[0] &&<button onClick={() => AddServer(0)}>Don't starve together</button>}
                {!serverStates[2] &&<button onClick={() => AddServer(3)}>Terraria</button>}
                <div className={styles["modal-actions"]}>
                    <button onClick={onClose}>Закрыть</button>
                </div>
            </div>
        </div>
    );
}

export default SelectionModal;