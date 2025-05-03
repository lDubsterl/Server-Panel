import React, { useState, useEffect } from 'react';
import { Routes, Route, useNavigate, Navigate } from 'react-router-dom';
import Server from './Pages/Server';
import Navbar from './components/Navbar';
import ApiConfig from './services/api';
import './App.css';
import ServerSelection from './Pages/ServerSelection';
import UserProfile from './Pages/UserProfile';
import HomePage from './Pages/HomePage';
import Login from './Pages/Login';
import Register from './Pages/Register';

const App = () => {
  const [isAuthenticated, setIsAuthenticated] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (localStorage['accessToken'] || localStorage['refreshToken'])
      ApiConfig.api.get('/Authentication/Verify')
        .then(response => {
          setIsAuthenticated(true);
        })
        .catch((err) => setIsAuthenticated(false));
    else
      setIsAuthenticated(false);
  }, []);

  const handleLogout = () => {
    ApiConfig.api.post('/Authentication/LogOut')
      .then(() => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('id');
        setIsAuthenticated(false);
        navigate('/');
      })
      .catch();
  };

  if (isAuthenticated == null)
    return;

  return (
    <div className="App">
      <Navbar isAuthenticated={isAuthenticated} onLogout={handleLogout} />
      <Routes>
        <Route path='/' element={<HomePage isAuthenticated={isAuthenticated} />} />
        <Route path="/login" element={<Login />} />
        <Route path='/register' element={<Register />} />
        {isAuthenticated ? <>
          <Route path=":id" element={<ServerSelection />} />
          <Route path=':id/:serverTypeName' element={<Server />} />
          <Route path=':id/profile' element={<UserProfile />} />
          <Route path='*' element={<h1>404 - Страница не найдена</h1>} />
        </> : <Route path='*' element={<Navigate to='/login' replace />} />}
      </Routes>
    </div>
  );
}

export default App;
