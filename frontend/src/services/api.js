import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const ApiConfig = {
	authenticationController: 'Authentication',
	genericServerController: 'ServerSelection',
	dstController: 'ServerSelection',
	minecraftController: 'ServerSelection',
	userController: 'User',
	api: axios.create({
		baseURL: 'http://localhost:5000/api/',
	})
}

ApiConfig.api.interceptors.request.use(config => {
	const token = localStorage.getItem('accessToken');
	if (token) {
		config.headers.Authorization = `Bearer ${token}`;
	}
	return config;
});

const parseJwt = (token) => {
	var base64Url = token.split('.')[1];
	var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
	var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
		return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
	}).join(''));

	return JSON.parse(jsonPayload);
}

export const getServerTypeNumber = (type) => {
	const serverTypes = {
	  Minecraft: 2,
	  Terraria: 3,
	  DoNotStarveTogether: 0
	};
  
	return serverTypes[type] != null ? serverTypes[type] : -1;
  };


// Перехватчик для обработки 401 ошибки
ApiConfig.api.interceptors.response.use(response => {
	return response; // Если все прошло успешно, просто возвращаем ответ
}, async error => {
	const originalRequest = error.config;

	// Проверяем, если ошибка 401 и запрос не был уже перезапущен
	if (error.response.status === 401 && !originalRequest._retry) {
		originalRequest._retry = true;

		try {
			const refreshToken = localStorage.getItem('refreshToken');
			let userId = parseJwt(localStorage.getItem('accessToken')).nameid;
			const response = await ApiConfig.api.post(`${ApiConfig.authenticationController}/Reauthorize`, { UserId: userId, JwtToken: refreshToken });

			const newAccessToken = response.data.data;

			// Сохраняем новый токен
			localStorage.setItem('accessToken', newAccessToken);
			localStorage.setItem('id', parseJwt(localStorage.getItem('accessToken')).nameid);
			// Обновляем Authorization заголовок в оригинальном запросе и повторно его отправляем
			originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
			return ApiConfig.api(originalRequest);
		} catch (refreshError) {
			console.error('Error refreshing token', refreshError);
			window.location.href = '/';
		}
	}

	return Promise.reject(error); // Если это не ошибка 401 или токен не удалось обновить
});


export default ApiConfig;
