import React, { useEffect, useState } from "react";
import ApiConfig from "../../services/api";
import styles from "../../styles/Modal.module.css";

const parseWorldgenConfig = (text) => {
	const result = {};
	const lines = text.split('\n');
	const paramRegex = /^\s*([\w_]+)\s*=\s*(.*?),?\s*(--.*)?$/;

	for (let line of lines) {
		line = line.trim();

		// Пропуск строк с фигурными скобками или override
		if (
			line.includes('{') ||
			line.includes('}') ||
			line.includes('override')
		) continue;

		// Пропуск комментариев
		if (line.startsWith('--')) continue;

		const match = line.match(paramRegex);
		if (match) {
			const key = match[1];
			const rawValue = match[2].trim();
			const comment = match[3];

			// Удаляем кавычки, если есть
			const value = rawValue.replace(/^"|"$/g, '');

			// Парсим массив возможных значений из комментария
			let options = [];
			if (comment) {
				const commentValues = comment.replace('--', '').trim();
				options = commentValues
					.split(',')
					.map(opt => opt.replace(/["']/g, '').trim())
					.filter(Boolean);
			}

			result[key] = [value, options];
		}
	}

	return result;
}

const stringifyWorldgenConfig = (fields) => {
	const lines = [];
	lines.push('return {');
	lines.push('\toverride_enabled = true,');

	if ('preset' in fields) {
		const [presetValue, presetOptions] = fields['preset'];
		const comment = presetOptions.length
			? ` -- ${presetOptions.map(opt => `"${opt}"`).join(', ')}`
			: '';
		lines.push(`\tpreset = ${presetValue},\t\t\t${comment}`);
	}

	lines.push('\toverrides = {');

	const keys = Object.keys(fields).filter(k => k !== 'preset');
	for (const key of keys) {
		const [value, options] = fields[key];
		const comment = options.length
			? ` -- ${options.map(opt => `"${opt}"`).join(', ')}`
			: '';
		lines.push(`\t\t${key} = "${value}",\t\t\t${comment}`);
	}

	lines.push('\t},');
	lines.push('}');
	return lines;
};



const DstModal = ({ onClose, onUpdate }) => {
	const [serverName, setServerName] = useState("");
	const [serverDescription, setServerDescription] = useState("");
	const [serverPassword, setServerPassword] = useState("");
	const [serverToken, setServerToken] = useState("");
	const [modlist, setModlist] = useState([]);
	const [modlistModalOpen, setModlistModalOpen] = useState(false);
	const [showAdvanced, setShowAdvanced] = useState(false);
	const [worldgenFields, setWorldgenFields] = useState([]);

	useEffect(() => {
		ApiConfig.api.get(`${ApiConfig.genericServerController}/GetInitialConfig?serverType=0`)
			.then((res) => {
				const parsed = parseWorldgenConfig(res.data);
				setWorldgenFields(parsed);
			});
	}, []);

	const handleWorldgenChange = (key, value) => {
		setWorldgenFields((prev) => ({
			...prev,
			[key]: [value, prev[key][1]],
		}));
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
			ServerToken: serverToken,
			Worldgen: stringifyWorldgenConfig(worldgenFields),
			Modlist: modlist
		};
		ApiConfig.api.put(ApiConfig.genericServerController + `/0`, createRequest)
			.then(() => {
				onUpdate();
				onClose();
			});
	};
	return (
		<div className={styles["modal-overlay"]}>
			<div className={styles["modal-content"]}>
				<form onSubmit={createServer}>
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
						<label className={styles["label-item"]}>
							Токен сервера:
							<input type="text" required placeholder="Токен сервера" value={serverToken}
								onChange={(e) => setServerToken(e.target.value)} />
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
										{Object.entries(worldgenFields).map(([key, [currentValue, options]]) => (
											<div key={key} style={{
												display: 'grid',
												gridTemplateColumns: '1fr 1.5fr',
												alignItems: 'center',
												justifyItems: 'start',
											}}>
												<label htmlFor={key}>
													{key.replace(/_/g, ' ')}
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
								</div>
							)}
						</div>
					</> : <>
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
						<button type="submit">Сохранить</button>
						<button type="button" onClick={closeModal}>Закрыть</button>
					</div>
				</form>
			</div>
		</div >
	);
}

export default DstModal;