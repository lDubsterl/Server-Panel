import { React, useState, useEffect } from 'react';
import styles from '../styles/Files.module.css';
import ApiConfig from '../services/api';

const Files = ({ settingsRef, serverType }) => {
    const [files, setFiles] = useState([]);
    const [path, setPath] = useState(`${serverType}/`);
    const [isEditing, setIsEditing] = useState(false);
    const [fileContent, setFileContent] = useState('');
    const [selectedFile, setSelectedFile] = useState(null);

    const getContent = (filePath) => {
        if (filePath.split('/').slice(-1) == 'server.properties') {
            settingsRef.current.click();
            return
        }
        ApiConfig.api.get(`${ApiConfig.genericServerController}/GetFolderContent?path=${filePath}`)
            .then(response => {
                if (filePath.slice(-1) === '/') {
                    setFiles(response.data);
                    setPath(filePath);
                }
                else
                    {
                        setIsEditing(true);
                        setFileContent(response.data);
                        setSelectedFile(filePath);
                    }
            });
    }

    useEffect(() => {
        getContent(path);
    }, []);

    const selectEntryType = (filePath) => {
        let type = 'file.png';
        let newFilename = filePath.split('/').slice(-1);
        if (filePath[filePath.length - 1] === '/') {
            type = 'folder.png';
            newFilename = filePath.split('/').slice(-2, -1);
        }
        return (
            <>
                <img src={type} width="20px" height="20px" style={{ marginRight: '4px' }} />
                {newFilename}
            </>
        );
    };

    const setPathString = () => {
        let pathParts = path.split('/').filter(part => part).slice(1);
        return pathParts.map((part, index) => {
            let currentPath = pathParts.slice(0, index + 1).join('/');
            return (
                <>
                    <span onClick={() => getContent(`${serverType}/${currentPath}/`)}>{part}</span>
                    <span>/</span>
                </>
            );
        });
    }

    return (
        <>
            <div className={styles["path-string"]}>
                <span onClick={() => getContent(`${serverType}/`)}>{serverType}/</span>
                {setPathString()}
            </div>
            {!isEditing ? (<div className={styles["files-container"]}>
                {files.map((file, index) => (
                    <button
                        key={index}
                        className={styles["file-button"]}
                        onClick={() => getContent(file)}
                    >{selectEntryType(file)}</button>
                ))}
            </div>) : <EditFileBox filePath={selectedFile} fileContent={fileContent} setIsEditing={setIsEditing}/>}
        </>
    );
}

const EditFileBox = ({filePath, fileContent, setIsEditing}) => {
    const [content, setFileContent] = useState('');

    const updateFile = () => {
        ApiConfig.api.patch(`${ApiConfig.genericServerController}/UpdateFile`, {path: filePath, content: content.split("\n")});
    };
    return (
        <>
            <textarea defaultValue={fileContent} onChange={(e) => setFileContent(e.target.value)}></textarea>
            <div style={{display: 'flex'}}>
                <button onClick={updateFile}>Сохранить</button>
                <button onClick={() => setIsEditing(false)}>Отмена</button>
            </div>
        </>
    );
}

export default Files;