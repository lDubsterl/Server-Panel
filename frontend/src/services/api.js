import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const api = axios.create({
	baseURL: 'https://localhost:7104/api/',
});

api.interceptors.request.use(config => {
	const token = localStorage.getItem('accessToken');
	if (token) {
		config.headers.Authorization = `Bearer ${token}`;
	}
	return config;
});

// Перехватчик для обработки 401 ошибки
api.interceptors.response.use(response => {
	return response; // Если все прошло успешно, просто возвращаем ответ
}, async error => {
	//const navigate = useNavigate();
	const originalRequest = error.config;

	// Проверяем, если ошибка 401 и запрос не был уже перезапущен
	if (error.response.status === 401 && !originalRequest._retry) {
		originalRequest._retry = true;

		try {
			const refreshToken = localStorage.getItem('refreshToken');
			let userId = localStorage.getItem('userId');
			const response = await api.post('Authentication/GetNewAccessToken', { userId, Token: refreshToken });

			const newAccessToken = response.data.data;

			// Сохраняем новый токен
			localStorage.setItem('accessToken', newAccessToken);

			// Обновляем Authorization заголовок в оригинальном запросе и повторно его отправляем
			originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
			return api(originalRequest);
		} catch (refreshError) {
			console.error('Error refreshing token', refreshError);
			//navigate('/login');
		}
	}

	return Promise.reject(error); // Если это не ошибка 401 или токен не удалось обновить
});


export default api;
