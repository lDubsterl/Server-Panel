import axios from 'axios';
import { useNavigate } from 'react-router-dom';

export const ApiConfig = {
	authenticationController: 'Authentication',
	genericServerController: 'ServerSelection',
	dstController: 'ServerSelection',
	minecraftController: 'ServerSelection',
	userController: 'User',
	api: axios.create({
		baseURL: '/api/',
	})
}

ApiConfig.api.interceptors.request.use(config => {
	const token = localStorage.getItem('accessToken');
	if (token) {
		config.headers.Authorization = `Bearer ${token}`;
	}
	return config;
});

export const parseJwt = (token) => {
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
	return response;
}, async error => {
	const originalRequest = error.config;

	if (error.response?.status !== 401) return Promise.reject(error);
	if (originalRequest._retry)
		return Promise.reject(error);

	originalRequest._retry = true;

	try {
		const refreshToken = localStorage.getItem('refreshToken');
		const accessToken = localStorage.getItem('accessToken');

		const userId = parseJwt(accessToken).nameid;

		const response = await ApiConfig.api.post(
			`${ApiConfig.authenticationController}/Reauthorize`,
			{ UserId: userId, JwtToken: refreshToken }
		);

		const newAccessToken = response.data.data;

		localStorage.setItem('accessToken', newAccessToken);
		localStorage.setItem('id', parseJwt(newAccessToken).nameid);

		originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
		return ApiConfig.api(originalRequest);
	} catch (refreshError) {
		console.error('Ошибка при обновлении токена:', refreshError);

		localStorage.removeItem('accessToken');
		localStorage.removeItem('refreshToken');
		localStorage.removeItem('id');

		window.location.href = '/';
		return Promise.reject(refreshError);
	}
});


export default ApiConfig;
